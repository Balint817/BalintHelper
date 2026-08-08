using Monocle;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Celeste.Mod.BalintHelper.Entities.Dynamic
{
    [Tracked(false)]
    public sealed class StaticVariableController : Entity
    {
        private readonly Dictionary<string, object?> _staticVariables = [];
        private ReadOnlyDictionary<string, object?>? _staticVariablesReadOnly;
        public ReadOnlyDictionary<string, object?>? StaticVariables => _staticVariablesReadOnly ??= new(_staticVariables);
        public StaticVariableController()
        {
            Tag = Tags.Global | Tags.Persistent | Tags.TransitionUpdate;
        }
        public static StaticVariableController GetOrCreate(Scene scene)
        {
            var instance = scene.Tracker.GetEntity<StaticVariableController>();
            if (instance == null)
            {
                instance = [];
                scene.Add(instance);
            }
            return instance;
        }
        public override void Added(Scene scene)
        {
            var existing = scene.Tracker.GetEntity<StaticVariableController>();
            if (existing != null)
            {
                throw new InvalidOperationException($"tried to add {nameof(StaticVariableController)} when one already exists");
            }

            base.Added(scene);
        }
        public class Info
        {
            public readonly string Name;
            public Info(string name)
            {
                Name = name;
            }
            public void SetValue(object? value)
            {
                var controller = GetOrCreate(Engine.Scene);
                controller._staticVariables[Name] = value;
            }
            public object? GetValue()
            {
                var controller = GetOrCreate(Engine.Scene);
                if (!controller._staticVariables.TryGetValue(Name, out var value))
                {
                    throw new InvalidProgramException($"attempted to read undefined static variable {Name}");
                }
                return value;
            }
            internal bool IsDefined()
            {
                var controller = GetOrCreate(Engine.Scene);
                return controller._staticVariables.ContainsKey(Name);
            }
        }
    }
}
