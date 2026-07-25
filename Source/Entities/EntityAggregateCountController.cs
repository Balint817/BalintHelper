using Celeste.Mod.BalintHelper.Triggers;
using Celeste.Mod.Entities;
using Monocle;
using System;
using System.Linq;

namespace Celeste.Mod.BalintHelper.Entities
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
            {
                return;
            }

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
}