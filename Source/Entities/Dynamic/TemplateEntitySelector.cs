using Celeste.Mod.BalintHelper.Utils;
using Celeste.Mod.Entities;
using DynamicInstructions.Instructions;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.BalintHelper.Entities.Dynamic
{
    [CustomEntity("BalintHelper/TemplateEntitySelector")]
    public class TemplateEntitySelector : Entity, IDisposable
    {
        private readonly Vector2 _nodePos;
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
        public TemplateEntitySelector(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            _templateType ??= MiscUtils.GetTypeFromCurrentDomain("Celeste.Mod.auspicioushelper.Template")!;
            if (_templateType is null)
            {
                throw new MissingMemberException($"failed to spawn {nameof(TemplateEntitySelector)} as it could not find the relevant auspicioushelper types");
            }
            if (data.Nodes is null || data.Nodes.Length < 1)
            {
                throw new ArgumentException($"missing node in {nameof(TemplateEntitySelector)}", nameof(data));
            }
            _nodePos = data.Nodes[0];

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
            this.Dispose();
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);

            if (string.IsNullOrWhiteSpace(target))
            {
                RemoveSelf();
                return;
            }
        }

        public bool InvalidOrProcessed(Entity e)
        {
            return e == this || _processed.Contains(e) || _templates.Contains(e);
        }

        public bool TryGetVariable(string? name, [MaybeNullWhen(false)]out object value)
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
                FindTemplates();
                if (_templates.Count < 1)
                {
                    RemoveSelf();
                    return;
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
            foreach (var template in _templates)
            {
                AuspiciousTemplateInterop.registerEntity(template, e);
            }
        }

        private void FindTemplates()
        {

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
            _templates.Clear();
            _processed.Clear();
        }
    }
}