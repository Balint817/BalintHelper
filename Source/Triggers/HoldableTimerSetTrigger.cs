using System;
using System.Collections.Generic;
using System.Reflection;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.BalintHelper.Triggers
{
    /// <summary>
    /// BalintHelper/HoldableTimerSetTrigger
    ///
    /// Sets the 'cannotHoldTimer' of targeted Holdable entities to an arbitrary number.
    /// 
    /// Properties (set in Lönn):
    ///   value             - float  (default 0f) Value to set the cannotHoldTimer to.
    ///   entityTypes       - string (default "TheoCrystal") Comma-separated type names or entity IDs.
    ///   playerTriggerMode - string (Never, OnEntry, OnLeave, EntryOrLeave, Stay)
    ///   entityTriggerMode - string (Never, OnEntry, OnLeave, EntryOrLeave, Stay)
    ///   targetingMode     - string (Inside, Outside, Everywhere)
    ///   global            - bool   (default false) If true, ignores triggers and applies every frame.
    /// </summary>
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
        private readonly string entityTypesRaw;

        private readonly TriggerModes playerTriggerMode;
        private readonly TriggerModes entityTriggerMode;
        private readonly TargetingModes targetingMode;
        private readonly bool isGlobal;

        private readonly HashSet<string> managedTypeNames = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<int> managedEntityIds = new HashSet<int>();

        private HashSet<Entity> insideLastFrame = new HashSet<Entity>();
        public HoldableTimerSetTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            timerValue = data.Float("value", 0f);
            entityTypesRaw = data.Attr("entityTypes", "TheoCrystal");

            playerTriggerMode = data.Enum("playerTriggerMode", TriggerModes.Never);
            entityTriggerMode = data.Enum("entityTriggerMode", TriggerModes.Never);
            targetingMode = data.Enum("targetingMode", TargetingModes.Inside);
            isGlobal = data.Bool("global", false);

            ParseManagedEntityFilters();
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
                        shouldFire = true;
                }
                else
                {
                    if (entityTriggerMode == TriggerModes.Stay)
                        shouldFire = true;
                }
            }

            foreach (var entity in insideLastFrame)
            {
                if (!insideThisFrame.Contains(entity))
                {
                    if (entityTriggerMode == TriggerModes.OnLeave || entityTriggerMode == TriggerModes.EntryOrLeave)
                        shouldFire = true;
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
        private void ParseManagedEntityFilters()
        {
            managedTypeNames.Clear();
            managedEntityIds.Clear();

            var tokens = entityTypesRaw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var raw in tokens)
            {
                var token = raw.Trim();
                if (token.Length == 0) continue;

                if (int.TryParse(token, out int entityId))
                    managedEntityIds.Add(entityId);
                else
                    managedTypeNames.Add(token);
            }
        }
        private bool IsManagedEntity(Entity entity)
        {
            var type = entity.GetType();
            bool byType = managedTypeNames.Contains(entity.SourceData?.Name ?? "") || managedTypeNames.Contains(type.Name);
            if (byType) return true;

            if (managedEntityIds.Count > 0 && entity.SourceData?.ID is int entityId)
                return managedEntityIds.Contains(entityId);

            return false;
        }

        private IEnumerable<Entity> GetManagedHoldables()
        {
            if (Scene == null) yield break;

            foreach (Entity entity in Scene.Entities)
            {
                if (entity.Get<Holdable>() == null)
                    continue;

                if (IsManagedEntity(entity))
                    yield return entity;
            }
        }
    }
}