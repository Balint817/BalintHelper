using Celeste.Mod.BalintHelper.Utils;
using Celeste.Mod.Entities;
using DynamicInstructions.Instructions;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Celeste.Mod.BalintHelper.Entities.Dynamic
{
    [CustomEntity("BalintHelper/TemplateEntitySelector")]
    public sealed class TemplateEntitySelector : Entity, IDisposable
    {
        private readonly HashSet<Entity> _processed = [];
        private readonly List<Entity> _currentTargets = [];
        private TemplateSelectorChildComponent? _tcomp;

        public enum RunMode
        {
            None,
            First,
            All
        }
        public enum TargetMode
        {
            None,
            Type,
            TypeVariable,
            EntityVariable
        }

        public readonly string? activeChannel;
        public readonly string? outputChannel;
        public readonly float outputIncrement;
        public readonly string target;
        public readonly TargetMode targetMode;
        public readonly RunMode runMode;
        public readonly Type? cachedType;

        public TemplateEntitySelector(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            activeChannel = data.Attr("activeChannel");
            outputChannel = data.Attr("outputChannel");
            target = data.Attr("target");
            outputIncrement = data.Float("outputIncrement", 1f);
            targetMode = data.Enum("targetMode", TargetMode.None);
            runMode = data.Enum("runMode", RunMode.None);

            if (targetMode is TargetMode.Type)
            {
                cachedType = TypeNameCodec.ParseType(target, AppDomain.CurrentDomain.GetAssemblies())
                    ?? throw new ArgumentException($"failed to get type {target}", nameof(data));
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            if (string.IsNullOrWhiteSpace(target))
            {
                Logger.Warn("TemplateEntitySelector", "No target, removing.");
                RemoveSelf();
                return;
            }
        }

        public override void Removed(Scene scene)
        {
            Dispose();
        }

        public bool InvalidOrProcessed(Entity entity)
        {
            return entity == this || entity == _tcomp?.parent || _processed.Contains(entity);
        }

        public bool TryGetVariable(string? name, [MaybeNullWhen(false)] out object value)
        {
            if (string.IsNullOrEmpty(name)) { value = null; return false; }
            return DynamicMethodController.GetOrCreate(Scene).Interpreter.GlobalVariables.TryGetValue(name!, out value);
        }

        public void RefreshTargets()
        {
            void AddTargetsOfTypes(params IEnumerable<Type> types)
            {
                foreach (var e in Scene.Entities)
                {
                    if (types.Any(t => t.IsAssignableFrom(e.GetType())))
                    {
                        _currentTargets.Add(e);
                    }
                }
            }
            _currentTargets.Clear();
            switch (targetMode)
            {
                case TargetMode.Type:
                    AddTargetsOfTypes(cachedType!);
                    break;
                case TargetMode.TypeVariable:
                    if (TryGetVariable(target, out var value))
                    {
                        if (value is Type t)
                        {
                            AddTargetsOfTypes(t);
                        }
                        else if (value is IEnumerable<Type> iterable)
                        {
                            AddTargetsOfTypes(iterable);
                        }
                    }
                    break;
                case TargetMode.EntityVariable:
                    if (TryGetVariable(target, out var value2))
                    {
                        if (value2 is Entity e)
                        {
                            _currentTargets.Add(e);
                        }
                        else if (value2 is IEnumerable<Entity> iterable)
                        {
                            _currentTargets.AddRange(iterable);
                        }
                    }
                    break;
                default:
                    throw new InvalidOperationException($"invalid targetMode {(int)targetMode}");
            }
            _currentTargets.RemoveAll(InvalidOrProcessed);
        }

        private bool _loggedMissingComponent;
        private bool hasRunOnce;

        public override void Update()
        {
            _tcomp ??= Get<TemplateSelectorChildComponent>();
            if (_tcomp is null)
            {
                if (!_loggedMissingComponent)
                {
                    Logger.Warn("TemplateEntitySelector", "No component attached at all. Not inside a template via customClarify.");
                    _loggedMissingComponent = true;
                }
                RemoveSelf();
                return;
            }

            base.Update();

            if (hasRunOnce)
            {
                return;
            }

            if (!string.IsNullOrEmpty(activeChannel) && AuspiciousChannelInterop.readChannel(activeChannel) == 0)
            {
                return;
            }

            RefreshTargets();
            if (_currentTargets.Count == 0)
            {
                return;
            }

            switch (runMode)
            {
                case RunMode.First:
                    RegisterEntityAndIncrementOutput(_currentTargets[0]);
                    hasRunOnce = true;
                    break;
                case RunMode.All:
                    foreach (Entity e in _currentTargets)
                    {
                        RegisterEntityAndIncrementOutput(e);
                    }

                    break;
                default:
                    throw new InvalidOperationException($"invalid runMode {(int)runMode}");
            }
        }

        private void RegisterEntityAndIncrementOutput(Entity entity)
        {
            if (!string.IsNullOrEmpty(outputChannel) && outputIncrement != 0 && float.IsFinite(outputIncrement))
            {
                var currentValue = AuspiciousChannelInterop.readChannel(outputChannel);
                AuspiciousChannelInterop.setChannel(outputChannel, currentValue + outputIncrement);
            }

            _tcomp!.NewEntity(entity);
        }

        public void Dispose()
        {
            _processed?.Clear();
            _currentTargets?.Clear();
            _tcomp?.Dispose();
            _tcomp = null;
        }

        public sealed class TemplateSelectorChildComponent : Component, IDisposable
        {
            internal sealed class PosInfo
            {
                public Vector2 Offset;
                public Vector2 LastSetPosition;
            }

            internal sealed class StatusInfo
            {
                public bool Visible;
                public bool Collidable;
                public bool Active;
                public StatusInfo(Entity e)
                {
                    Visible = e.Visible;
                    Collidable = e.Collidable;
                    Active = e.Active;
                }
            }

            internal readonly Dictionary<Entity, PosInfo> _positions = [];
            internal readonly Dictionary<Entity, StatusInfo> _statuses = [];
            private TemplateEntitySelector _selectorEntity;
            internal Vector2 _lastKnownVirtLoc;

            public TemplateSelectorChildComponent(Entity ent) : base(true, false)
            {
                Entity = ent;
                _selectorEntity = (TemplateEntitySelector)ent;

                AddTo = s =>
                {
                    s.Add(_selectorEntity);
                    Logger.Info("TemplateEntitySelector", $"AddTo fired, parent={parent}");
                };

                SetOffsetCB = loc =>
                {
                    _lastKnownVirtLoc = loc;
                    foreach (var e in _selectorEntity._processed)
                    {
                        if (!_positions.ContainsKey(e))
                        {
                            _positions[e] = new PosInfo { Offset = e.Position - loc, LastSetPosition = e.Position };
                        }
                    }
                };

                RepositionCB = (nloc, liftspeed) =>
                {
                    _selectorEntity._processed.RemoveWhere(x => x?.Scene is null);

                    foreach (var e in _selectorEntity._processed)
                    {
                        if (!_positions.TryGetValue(e, out var pos))
                        {
                            continue;
                        }

                        // Fold any independent movement since our last write
                        // into the offset, instead of discarding it.
                        if (e.Position != pos.LastSetPosition)
                        {
                            pos.Offset += e.Position - pos.LastSetPosition;
                        }

                        Vector2 target = nloc + pos.Offset;

                        if (e is Platform p)
                        {
                            p.MoveTo(target, liftspeed);
                        }
                        else if (e is Solid s)
                        {
                            s.MoveTo(target, liftspeed);
                        }
                        else
                        {
                            e.Position = target;
                        }

                        pos.LastSetPosition = target;
                    }
                };

                ChangeStatusCB = (vis, col, act) =>
                {
                    foreach (var e in _selectorEntity._processed)
                    {
                        if (!_statuses.TryGetValue(e, out var ogSt))
                        {
                            Logger.Error("TemplateEntitySelector", $"No original status captured for {e.GetType().Name}! THIS IS A BUG!");
                            continue;
                        }
                        if (vis != 0)
                        {
                            e.Visible = ParentVisible && ogSt.Visible;
                        }

                        if (col != 0)
                        {
                            e.Collidable = ParentCollidable && ogSt.Collidable;
                        }

                        if (act != 0)
                        {
                            e.Active = ParentActive && ogSt.Active;
                        }
                    }
                };

                DestroyCB = (particles) =>
                {
                    foreach (var e in _selectorEntity._processed)
                    {
                        e.RemoveSelf();
                    }

                    _selectorEntity.RemoveSelf();
                };
            }

            internal void CaptureOffsetNow(Entity e)
            {
                if (!_positions.ContainsKey(e))
                {
                    _positions[e] = new PosInfo
                    {
                        Offset = e.Position - _lastKnownVirtLoc,
                        LastSetPosition = e.Position
                    };
                    Logger.Info("TemplateEntitySelector", $"Captured offset {_positions[e].Offset} for {e.GetType().Name} at registration (virtLoc={_lastKnownVirtLoc})");
                }
            }

            public Entity parent = null!;
            public Action<Scene> AddTo = null!;
            public Action<List<Entity>> AddSelf = null!;
            public Action<Vector2, Vector2> RepositionCB = null!;
            public Action<Vector2> SetOffsetCB = null!;
            public Action<int, int, int> ChangeStatusCB = null!;
            public bool ParentVisible = true;
            public bool ParentCollidable = true;
            public bool ParentActive = true;
            public Action<bool> DestroyCB = null!;
            public DashCollisionResults RegisterDashhit(Player p, Vector2 dir) => AuspiciousTemplateInterop.registerDashhit(parent, p, dir);
            public void RegisterEntity(Entity e) => AuspiciousTemplateInterop.registerEntity(parent, e);
            internal void NewEntity(Entity entity)
            {
                Logger.Info("TemplateEntitySelector", $"Registering {entity.GetType().Name} at {entity.Position} to parent {parent}");
                _selectorEntity._processed.Add(entity);
                _statuses.Add(entity, new(entity));
                RegisterEntity(entity);
                CaptureOffsetNow(entity);
                RegisterDashColliders(entity);
            }

            private void RegisterDashColliders(Entity e)
            {
                if (IsAuspiciousTemplate(e))
                {
                    return;
                }
                if (e is Solid solid && e is not DreamBlock)
                {
                    var original = solid.OnDashCollide;
                    solid.OnDashCollide = (player, vector) =>
                    {
                        var result = original?.Invoke(player, vector);
                        var registerResult = RegisterDashhit(player, vector);
                        return result ?? registerResult;
                    };
                }
                else if (e is Platform platform)
                {
                    var original = platform.OnDashCollide;
                    platform.OnDashCollide = (player, vector) =>
                    {
                        var result = original?.Invoke(player, vector);
                        var registerResult = RegisterDashhit(player, vector);
                        return result ?? registerResult;
                    };
                }
            }

            public override void Update()
            {
                foreach (var e in _selectorEntity._processed.ToArray())
                {
                    if (e.IsGone(Scene))
                    {
                        _selectorEntity._processed.Remove(e);
                    }
                }
            }

            private static Type? _auspiciousTemplateType;

            private static bool IsAuspiciousTemplate(Entity e)
            {
                _auspiciousTemplateType ??= MiscUtils.GetTypeFromCurrentDomain("Celeste.Mod.auspicioushelper.Template");
                return _auspiciousTemplateType is not null && e.GetType().IsAssignableTo(_auspiciousTemplateType);
            }

            public void Dispose()
            {
                parent = null!;
                AddTo = null!;
                AddSelf = null!;
                RepositionCB = null!;
                SetOffsetCB = null!;
                ChangeStatusCB = null!;
                DestroyCB = null!;
                _selectorEntity = null!;
                _positions?.Clear();
                _statuses?.Clear();
            }
        }
    }
}