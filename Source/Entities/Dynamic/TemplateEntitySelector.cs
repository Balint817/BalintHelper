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
    public class TemplateEntitySelector : Entity, IDisposable
    {
        private readonly Vector2? _nodePos;
        private readonly HashSet<Entity> _processed = [];
        private static Type _templateType = null!;
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
            EntityVariable,
        }
        public readonly string? activeChannel;
        public readonly string? outputChannel;
        public readonly float outputIncrement;
        public readonly string target;
        public readonly TargetMode targetMode;
        public readonly RunMode runMode;
        private readonly List<Entity> _currentTargets = [];
        public readonly Type? cachedType;
        private readonly HashSet<Entity> _templates = [];

        private TemplateSelectorChildComponent? _tcomp;
        public TemplateEntitySelector(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            if (!AuspiciousChannelInterop.IsImported || !AuspiciousTemplateInterop.IsImported)
            {
                throw new InvalidOperationException("cannot do template interop because auspicioushelper is not loaded!");
            }
            _templateType ??= MiscUtils.GetTypeFromCurrentDomain("Celeste.Mod.auspicioushelper.Template")!;
            if (_templateType is null)
            {
                throw new MissingMemberException($"failed to spawn {nameof(TemplateEntitySelector)} as it could not find the relevant auspicioushelper types");
            }
            _nodePos = data.FirstNodeNullable();

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
                //if (cachedType.IsAssignableTo(_templateType) || cachedType.IsAssignableTo(typeof(TemplateEntitySelector)))
                //{
                //    throw new ArgumentException($"invalid ", nameof(data));
                //}
            }
        }
        public override void Removed(Scene scene)
        {
            Dispose();
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);

            if (string.IsNullOrWhiteSpace(target))
            {
                Logger.Warn("TemplateEntitySelector", "No target was provided, removing!");
                RemoveSelf();
                return;
            }
        }

        public bool InvalidOrProcessed(Entity e)
        {
            return e == this || _processed.Contains(e) || _templates.Contains(e);
        }

        public bool TryGetVariable(string? name, [MaybeNullWhen(false)] out object value)
        {
            if (string.IsNullOrEmpty(name))
            {
                value = null;
                return false;
            }
            return DynamicMethodController
                .GetOrCreate(Scene)
                .Interpreter
                .GlobalVariables
                .TryGetValue(name!, out value);
        }
        public void RefreshTargets()
        {
            void AddTargetsOfTypes(params IEnumerable<Type> types)
            {
                foreach (var e in Scene.Entities)
                {
                    if (!types.Any(t => t.IsAssignableFrom(e.GetType())))
                    {
                        continue;
                    }
                    _currentTargets.Add(e);
                }
            }
            _currentTargets.Clear();
            switch (targetMode)
            {
                case TargetMode.Type:
                    {
                        AddTargetsOfTypes(cachedType!);
                    }
                    break;
                case TargetMode.TypeVariable:
                    {
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
                    }
                    break;
                case TargetMode.EntityVariable:
                    {
                        if (TryGetVariable(target, out var value))
                        {
                            if (value is Entity e)
                            {
                                _currentTargets.Add(e);
                            }
                            else if (value is IEnumerable<Entity> iterable)
                            {
                                _currentTargets.AddRange(iterable);
                            }
                        }
                    }
                    break;
                default:
                    throw new InvalidOperationException($"invalid targetMode {(int)targetMode}");
            }

            _currentTargets.RemoveAll(InvalidOrProcessed);
        }
        public override void Update()
        {
            if (_templates.Count < 1)
            {
                _tcomp ??= this.Get<TemplateSelectorChildComponent>();
                if (_tcomp is null)
                {
                    FindTemplates();
                    if (_templates.Count < 1)
                    {
                        Logger.Warn("TemplateEntitySelector", "No template was found, removing!");
                        RemoveSelf();
                        return;
                    }
                }
                else
                {
                    _templates.Add(_tcomp.parent!);
                }
            }

            if (!string.IsNullOrEmpty(activeChannel))
            {
                if (AuspiciousChannelInterop.readChannel(activeChannel) == 0)
                {
                    return;
                }
            }

            RefreshTargets();

            if (_currentTargets.Count == 0)
            {
                return;
            }

            var shouldRemove = false;

            switch (runMode)
            {
                case RunMode.First:
                    {
                        RegisterEntityAndIncrementOutput(_currentTargets[0]);
                        shouldRemove = true;
                    }
                    break;
                case RunMode.All:
                    {
                        foreach (Entity e in _currentTargets)
                        {
                            RegisterEntityAndIncrementOutput(e);
                        }
                    }
                    break;
                default:
                    throw new InvalidOperationException($"invalid runMode {(int)runMode}");
            }

            if (shouldRemove)
            {
                RemoveSelf();
                return;
            }
        }
        private void RegisterEntityAndIncrementOutput(Entity e)
        {
            if (!string.IsNullOrEmpty(outputChannel) && outputIncrement != 0 && float.IsFinite(outputIncrement))
            {
                var currentValue = AuspiciousChannelInterop.readChannel(outputChannel);
                AuspiciousChannelInterop.setChannel(outputChannel, currentValue + outputIncrement);
            }
            _processed.Add(e);
            if (_tcomp is not null)
            {
                Vector2 virtLoc = _tcomp._lastKnownVirtLoc;
                _tcomp._offsets[e] = e.Position - virtLoc;
                _tcomp.RegisterEntity(e);
            }
            else
            {
                foreach (var template in _templates)
                {
                    AuspiciousTemplateInterop.registerEntity(template, e);
                }
            }
        }

        private void FindTemplates()
        {
            if (_nodePos is not { } nodePos)
            {
                Logger.Warn("TemplateEntitySelector", "No node position was provided, and we're not inside a template!");
                return;
            }
            foreach (var e in Scene.Entities)
            {
                if (e.SourceData is null)
                {
                    continue;
                }
                var type = e.GetType();

                if (!type.IsAssignableTo(_templateType))
                {
                    continue;
                }

                if (e.SourceData.Position != _nodePos)
                {
                    continue;
                }

                _templates.Add(e);
                break;
            }

        }
        public void Dispose()
        {
            _templates?.Clear();
            _processed?.Clear();
            _tcomp?.Dispose();
            _tcomp = null;
        }

        public class TemplateSelectorChildComponent : Component, IDisposable
        {
            internal readonly Dictionary<Entity, Vector2> _offsets = [];
            private TemplateEntitySelector _selectorEntity;
            internal Vector2 _lastKnownVirtLoc; // cached from the last SetOffsetCB
            internal sealed class EntityState
            {
                internal sealed class PosInfo
                {
                    public Vector2 Offset;
                    public Vector2 LastSetPosition;
                    public PosInfo(Entity e, Vector2 virtLoc)
                    {
                        Offset = e.Position - virtLoc;
                        LastSetPosition = e.Position;
                    }
                }
                public PosInfo? Pos;
                internal sealed class StatusInfo
                {
                    public bool OwnVisible;
                    public bool OwnCollidable;
                    public bool OwnActive;
                    public bool LastAppliedVisible;
                    public bool LastAppliedCollidable;
                    public bool LastAppliedActive;
                    public StatusInfo(Entity e)
                    {
                        LastAppliedVisible = OwnVisible = e.Visible;
                        LastAppliedCollidable = OwnCollidable = e.Collidable;
                        LastAppliedActive = OwnActive = e.Active;
                    }
                }
                public StatusInfo? Status;
            }

            internal readonly Dictionary<Entity, EntityState> _states = [];
            void FilterRemoved()
            {
                _selectorEntity._processed.RemoveWhere(x => x?.Scene is null);
            }
            void EnsureOffset(Entity e, EntityState state, Vector2 virtLoc)
            {
                state.Pos ??= new(e, virtLoc);
            }
            void EnsureState(Entity e, EntityState state)
            {
                state.Status ??= new(e);
            }
            void ForceRefreshState(Entity e, EntityState state)
            {
                state.Status = new(e);
            }
            EntityState GetOrCreateState(Entity e)
            {
                if (!_states.TryGetValue(e, out var state))
                {
                    _states[e] = state = new();
                }
                return state;
            }
            internal void PollAndApplyStatus(Entity e)
            {
                var state = GetOrCreateState(e);
                EnsureState(e, state);
                var status = state.Status!;

                // Drift detection
                if (e.Visible != status.LastAppliedVisible) status.OwnVisible = e.Visible;
                if (e.Collidable != status.LastAppliedCollidable) status.OwnCollidable = e.Collidable;
                if (e.Active != status.LastAppliedActive) status.OwnActive = e.Active;

                bool wantVisible = ParentVisible && status.OwnVisible;
                bool wantCollidable = ParentCollidable && status.OwnCollidable;
                bool wantActive = ParentActive && status.OwnActive;

                if (e.Visible != wantVisible) e.Visible = wantVisible;
                if (e.Collidable != wantCollidable) e.Collidable = wantCollidable;
                if (e.Active != wantActive) e.Active = wantActive;

                status.LastAppliedVisible = e.Visible;
                status.LastAppliedCollidable = e.Collidable;
                status.LastAppliedActive = e.Active;
            }
            public TemplateSelectorChildComponent(Entity ent) : base(false, false)
            {
                Entity = ent;
                if (ent is not TemplateEntitySelector t)
                {
                    throw new ArgumentException($"invalid entity type {ent.GetType().FullName} for {nameof(TemplateSelectorChildComponent)}", nameof(ent));
                }
                _selectorEntity = t;

                this.SetOffsetCB = (Vector2 templateVirtualLoc) => {
                    _lastKnownVirtLoc = templateVirtualLoc;
                    FilterRemoved();
                    foreach (var e in _selectorEntity._processed)
                    {
                        var state = GetOrCreateState(e);
                        EnsureOffset(e, state, templateVirtualLoc);
                        EnsureState(e, state);
                    }
                };

                this.RepositionCB = (Vector2 nloc, Vector2 liftspeed) => {
                    FilterRemoved();
                    foreach (var e in _selectorEntity._processed)
                    {
                        var state = GetOrCreateState(e);
                        EnsureOffset(e, state, _lastKnownVirtLoc);
                        var pos = state.Pos!;

                        // detect independent movement and dont discard it!
                        if (e.Position != pos.LastSetPosition)
                        {
                            pos.Offset += e.Position - pos.LastSetPosition;
                        }

                        Vector2 target = nloc + pos.Offset;

                        if (e is Platform p)
                            p.MoveTo(target, liftspeed);
                        else
                            e.Position = target;

                        pos.LastSetPosition = target;
                    }
                };

                // I have no clue how I plan to make this work

                this.ChangeStatusCB = (int vis, int col, int act) =>
                {
                    FilterRemoved();
                    foreach (var e in _selectorEntity._processed)
                        PollAndApplyStatus(e);
                };

                this.DestroyCB = (bool allowParticles) => {
                    //FilterRemoved(); // RemoveSelf already has a Scene != null check.
                    foreach (var e in _selectorEntity._processed)
                        e.RemoveSelf();
                    _selectorEntity.RemoveSelf();
                };
            }

            public override void Update()
            {
                foreach (var e in _selectorEntity._processed)
                    PollAndApplyStatus(e);
            }
            //This is a reference to the template's parent and should not be changed
            internal Entity parent = null;
            //Called when this is added to a template. Parent will be non-null before this function.
            //<IMPORTANT> This is called before Entity.Added is called. For maximum compatibility,
            //make sure that AddSelf returns all associated entities to this one before you return from it.
            public Action<Scene> AddTo = null;
            //This function should add your entity and any entities it makes to the provided list.
            public Action<List<Entity>> AddSelf = null;


            //Called when your entity repositions; First parameter is the new location, second parameter is the liftspeed.
            //If you have definied SetOffset, the location will be the location of the template; if you have not, I try to
            //guess a location based on your original entity's location!
            public Action<Vector2, Vector2> RepositionCB = null;
            public Action<Vector2> SetOffsetCB = null;

            //Called when the template changes visibility, collidability and active status (in order).
            //0 means no change, 1 means set to true, -1 means set to false. 
            //Note that this is the parent collidability; your component should only be actually collidable
            //if it's normal logic would have it be collidable AND the last value from these was 1.
            public Action<int, int, int> ChangeStatusCB = null;
            //You can also read from these parameters to get the current status
            public bool ParentVisible = true;
            public bool ParentCollidable = true;
            public bool ParentActive = true;

            //Called when the template this entity is a part of is destroyed. Parameter is true if particles/debris
            //should be used. Should remove the current entity and any children
            public Action<bool> DestroyCB = null;
            public void TriggerParent() => AuspiciousTemplateInterop.triggerTemplate(parent, Entity);
            //call this when your solids are hit please <3
            public DashCollisionResults RegisterDashhit(Player p, Vector2 dir) => AuspiciousTemplateInterop.registerDashhit(parent, p, dir);
            public void RegisterEntity(Entity e) => AuspiciousTemplateInterop.registerEntity(parent, e);
            public Vector2 getParentLiftspeed() => AuspiciousTemplateInterop.getTemplateLiftspeed(parent);

            public void Dispose()
            {
                _offsets?.Clear();
                _states?.Clear();
                this.DestroyCB = null!;
                this.ChangeStatusCB = null!;
                this.SetOffsetCB = null!;
                this.RepositionCB = null!;
                this._selectorEntity = null!;
            }
        }
    }
}