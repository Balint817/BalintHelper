using Celeste.Mod.Entities;
using Celeste.Mod.Registry;
using DynamicInstructions.Instructions;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Linq;

namespace Celeste.Mod.BalintHelper.Triggers
{
    [CustomEntity("BalintHelper/SpawnEntityTrigger")]
    public class SpawnEntityTrigger : Trigger
    {
        public enum TriggerModes
        {
            OnPlayerEntry,
            Automatically
        }

        private readonly string entityName;
        private readonly TriggerModes triggerMode;
        private readonly string flag;

        private bool triggered;

        public SpawnEntityTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            entityName = data.Attr("entityName", "").Trim();
            triggerMode = data.Enum("triggerMode", TriggerModes.OnPlayerEntry);
            flag = data.Attr("flag", "").Trim();

            if (string.IsNullOrEmpty(entityName))
            {
                throw new ArgumentException($"'entityName' for {nameof(SpawnEntityTrigger)} cannot be empty!", nameof(data));
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            if (triggerMode == TriggerModes.Automatically)
            {
                TryTrigger();
            }
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);

            if (triggerMode == TriggerModes.OnPlayerEntry)
            {
                TryTrigger();
            }
        }

        private void TryTrigger()
        {
            if (triggered)
            {
                return;
            }

            if (!ShouldRun())
            {
                return;
            }

            triggered = true;
            SpawnEntity(entityName);
            RemoveSelf();
        }

        private bool ShouldRun()
        {
            if (string.IsNullOrWhiteSpace(flag))
            {
                return true;
            }

            var level = SceneAs<Level>();
            if (level == null)
            {
                return false;
            }

            bool inverted = flag.StartsWith('!');
            string flagName = inverted ? flag[1..] : flag;

            if (string.IsNullOrWhiteSpace(flagName))
            {
                return true;
            }

            bool flagState = level.Session.GetFlag(flagName);
            return inverted ? !flagState : flagState;
        }

        private void SpawnEntity(string entityName)
        {
            var knownTypes = EntityRegistry.GetKnownTypesFromSid(entityName).ToHashSet();

            if (knownTypes.Count == 0)
            {
                knownTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(TypeNameCodec.GetLoadableTypes)
                    .Where(type => type.IsAssignableTo(typeof(Entity)) && type.Name == entityName)
                    .ToHashSet();
            }

            if (knownTypes.Count == 0)
            {
                knownTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(TypeNameCodec.GetLoadableTypes)
                    .Where(type => type.IsAssignableTo(typeof(Entity)) && type.FullName == entityName)
                    .ToHashSet();
            }

            if (knownTypes.Count < 1)
            {
                throw new ArgumentException($"No types found for '{entityName}' in {nameof(SpawnEntityTrigger)}", nameof(entityName));
            }

            if (knownTypes.Count > 1)
            {
                throw new ArgumentException($"'{entityName}' in {nameof(SpawnEntityTrigger)} is ambiguous between {string.Join(", ", knownTypes.Select(t => t.FullName))}", nameof(entityName));
            }


            var type = knownTypes.First();

            // todo: support constructors with parameters
            Scene.Add(Activator.CreateInstance(type, []) as Entity);
        }
    }
}