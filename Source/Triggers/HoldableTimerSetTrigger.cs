using Celeste.Mod.BalintHelper.Utils;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.BalintHelper.Triggers
{
    [CustomEntity("BalintHelper/HoldableTimerSetTrigger")]
    public class HoldableTimerSetTrigger : Trigger
    {
        public enum TriggerModes
        {
            Never,
            OnEntry,
            OnLeave,
            EntryOrLeave,
            Stay
        }

        public enum TargetingModes
        {
            Inside,
            Outside,
            Everywhere
        }

        private static readonly FieldInfo HoldableCannotHoldTimerField =
            typeof(Holdable).GetField(
                "cannotHoldTimer",
                BindingFlags.Instance | BindingFlags.NonPublic
            )!;

        private readonly float timerValue;

        private readonly TriggerModes playerTriggerMode;
        private readonly TriggerModes entityTriggerMode;
        private readonly TargetingModes targetingMode;
        private readonly bool isGlobal;

        private readonly EntityTypeFilter managedEntities;

        private HashSet<Entity> insideLastFrame = new HashSet<Entity>();
        public HoldableTimerSetTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            timerValue = data.Float("value", 0f);
            managedEntities = new EntityTypeFilter(data.Attr("entityTypes", "TheoCrystal,ExtendedVariantMode/TheoCrystal"));

            playerTriggerMode = data.Enum("playerTriggerMode", TriggerModes.Never);
            entityTriggerMode = data.Enum("entityTriggerMode", TriggerModes.Never);
            targetingMode = data.Enum("targetingMode", TargetingModes.Inside);
            isGlobal = data.Bool("global", false);
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (playerTriggerMode == TriggerModes.OnEntry || playerTriggerMode == TriggerModes.EntryOrLeave)
            {
                FireTrigger();
            }
        }

        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            if (playerTriggerMode == TriggerModes.OnLeave || playerTriggerMode == TriggerModes.EntryOrLeave)
            {
                FireTrigger();
            }
        }

        public override void OnStay(Player player)
        {
            base.OnStay(player);
            if (playerTriggerMode == TriggerModes.Stay)
            {
                FireTrigger();
            }
        }

        public override void Update()
        {
            base.Update();

            if (isGlobal)
            {
                FireTrigger();
                return;
            }

            if (entityTriggerMode == TriggerModes.Never)
            {
                return;
            }

            bool shouldFire = false;
            HashSet<Entity> insideThisFrame = new HashSet<Entity>();

            foreach (var entity in GetManagedHoldables())
            {
                if (CollideCheck(entity))
                {
                    insideThisFrame.Add(entity);
                }
            }

            foreach (var entity in insideThisFrame)
            {
                if (!insideLastFrame.Contains(entity))
                {
                    if (entityTriggerMode == TriggerModes.OnEntry || entityTriggerMode == TriggerModes.EntryOrLeave)
                    {
                        shouldFire = true;
                    }
                }
                else
                {
                    if (entityTriggerMode == TriggerModes.Stay)
                    {
                        shouldFire = true;
                    }
                }
            }

            foreach (var entity in insideLastFrame)
            {
                if (!insideThisFrame.Contains(entity))
                {
                    if (entityTriggerMode == TriggerModes.OnLeave || entityTriggerMode == TriggerModes.EntryOrLeave)
                    {
                        shouldFire = true;
                    }
                }
            }

            insideLastFrame = insideThisFrame;

            if (shouldFire)
            {
                FireTrigger();
            }
        }

        private void FireTrigger()
        {
            foreach (var entity in GetManagedHoldables())
            {
                bool apply = false;

                switch (targetingMode)
                {
                    case TargetingModes.Inside:
                        apply = CollideCheck(entity);
                        break;
                    case TargetingModes.Outside:
                        apply = !CollideCheck(entity);
                        break;
                    case TargetingModes.Everywhere:
                        apply = true;
                        break;
                }

                if (apply)
                {
                    var holdable = entity.Get<Holdable>();
                    if (holdable != null)
                    {
                        HoldableCannotHoldTimerField?.SetValue(holdable, timerValue);
                    }
                }
            }
        }
        private bool IsManagedEntity(Entity entity)
        {
            return managedEntities.Matches(entity);
        }

        private IEnumerable<Entity> GetManagedHoldables()
        {
            if (Scene == null)
            {
                yield break;
            }

            foreach (Entity entity in Scene.Entities)
            {
                if (entity.Get<Holdable>() == null)
                {
                    continue;
                }

                if (IsManagedEntity(entity))
                {
                    yield return entity;
                }
            }
        }
    }
}