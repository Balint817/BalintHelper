using Celeste.Mod.BalintHelper.Utils;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.BalintHelper.Triggers
{
    [CustomEntity("BalintHelper/EntityAggregateCountController")]
    [Tracked]
    public class EntityAggregateCountController : Entity
    {
        public enum AggregateMode
        {
            Minimum,
            Maximum,
            Sum
        }

        public string CounterId { get; }
        public AggregateMode Mode { get; }
        public EntityAggregateCountController(string counterId, AggregateMode mode)
        {
            CounterId = counterId;
            Mode = mode;
            Tag = Tags.Global;
        }

        public void UpdateCounter()
        {
            if (Scene is not Level level)
                return;

            int value = 0;
            var triggers = level.Tracker.GetEntities<EntityAggregateCountTrigger>().Cast<EntityAggregateCountTrigger>().Where(x => x.counterId == CounterId).ToArray();
            if (triggers.Length > 0)
            {
                value = Mode switch
                {
                    AggregateMode.Minimum => triggers.Min(t => t.CurrentCount),
                    AggregateMode.Maximum => triggers.Max(t => t.CurrentCount),
                    AggregateMode.Sum => triggers.Sum(t => t.CurrentCount),
                    _ => triggers.Max(t => t.CurrentCount)
                };
                Holdable t = null;
                t.SlowFall
            }

            level.Session.SetCounter(CounterId, value);
        }

        public static EntityAggregateCountController GetOrCreate(Scene scene, string counterId, AggregateMode mode)
        {
            foreach (Entity entity in scene.Tracker.GetEntities<EntityAggregateCountController>())
            {
                if (entity is EntityAggregateCountController controller && controller.CounterId == counterId)
                {
                    if (controller.Mode != mode)
                    {
                        throw new ArgumentException($"Inconsistent aggregation method! The existing aggregation method for {nameof(EntityAggregateCountController)}({counterId}) was {controller.Mode}, but {mode} was requested!", nameof(mode));
                    }
                    return controller;
                }
            }

            EntityAggregateCountController created = new(counterId, mode);
            scene.Add(created);
            return created;
        }
    }

    [CustomEntity("BalintHelper/EntityAggregateCountTrigger")]
    [Tracked]
    public class EntityAggregateCountTrigger : Trigger
    {
        internal readonly string counterId;
        private readonly EntityAggregateCountController.AggregateMode aggregateMode;
        private readonly EntityTypeFilter managedEntities;

        private EntityAggregateCountController? controller;

        public int CurrentCount { get; private set; }

        public EntityAggregateCountTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            counterId = data.Attr("counterId", "entityCount");
            aggregateMode = data.Enum("aggregateMode", EntityAggregateCountController.AggregateMode.Maximum);
            managedEntities = new EntityTypeFilter(data.Attr("entityTypes", ""));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            controller = EntityAggregateCountController.GetOrCreate(scene, counterId, aggregateMode);
            controller.UpdateCounter();
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
        }

        public override void Update()
        {
            base.Update();

            int newCount = 0;

            if (Scene != null)
            {
                foreach (Entity entity in Scene.Entities)
                {
                    if (entity == this)
                        continue;

                    if (entity.Collider == null)
                        continue;

                    if (!managedEntities.Matches(entity))
                        continue;

                    if (CollideCheck(entity))
                        newCount++;
                }
            }

            if (newCount != CurrentCount)
            {
                CurrentCount = newCount;
                controller?.UpdateCounter();
            }
        }
    }
}