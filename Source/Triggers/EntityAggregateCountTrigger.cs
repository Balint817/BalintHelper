using Celeste.Mod.BalintHelper.Entities;
using Celeste.Mod.BalintHelper.Utils;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Linq;

namespace Celeste.Mod.BalintHelper.Triggers
{

    [CustomEntity("BalintHelper/EntityAggregateCountTrigger")]
    [Tracked]
    public class EntityAggregateCountTrigger : Trigger
    {
        public enum AggregateMode
        {
            Minimum,
            Maximum,
            Sum
        }

        public readonly string CounterId;
        public readonly AggregateMode Mode;
        private readonly EntityTypeFilter managedEntities;

        public int CurrentCount { get; private set; }
        private bool firstUpdate = true;

        public EntityAggregateCountTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            CounterId = data.Attr("counterId", "entityCount");
            Mode = data.Enum("aggregateMode", AggregateMode.Maximum);
            managedEntities = new EntityTypeFilter(data.Attr("entityTypes", ""));
        }
        public void UpdateCounter()
        {
            firstUpdate = false;
            var level = SceneAs<Level>();

            var triggers = level.Tracker.GetEntities<EntityAggregateCountTrigger>().Cast<EntityAggregateCountTrigger>().Where(x => x.CounterId == CounterId).ToArray();
            var inconsistent = triggers.FirstOrDefault(x => x.Mode != Mode);
            if (inconsistent != null)
            {
                throw new ArgumentException($"Inconsistent aggregation method! The existing aggregation method for {CounterId} was {Mode}, but one of the triggers requested {inconsistent.Mode}!", nameof(Mode));
            }

            int value = Mode switch
            {
                AggregateMode.Minimum => triggers.Min(t => t.CurrentCount),
                AggregateMode.Maximum => triggers.Max(t => t.CurrentCount),
                AggregateMode.Sum => triggers.Sum(t => t.CurrentCount),
                _ => triggers.Max(t => t.CurrentCount)
            };

            level.Session.SetCounter(CounterId, value);
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
        }

        public override void Update()
        {
            base.Update();

            int newCount = 0;

            // always at least one (itself)
            var controller = Scene.Tracker.GetEntities<EntityAggregateCountTrigger>().Cast<EntityAggregateCountTrigger>().First(c => c.CounterId == CounterId);

            foreach (Entity entity in Scene.Entities)
            {
                if (entity == this)
                {
                    continue;
                }

                if (entity.Collider == null)
                {
                    continue;
                }

                if (!managedEntities.Matches(entity))
                {
                    continue;
                }

                if (CollideCheck(entity))
                {
                    newCount++;
                }
            }

            if (newCount != CurrentCount || controller.firstUpdate)
            {
                CurrentCount = newCount;
                controller.UpdateCounter();
            }
        }
    }
}