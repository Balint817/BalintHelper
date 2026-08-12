using Celeste.Mod.BalintHelper.Entities;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.BalintHelper.Triggers
{

    [CustomEntity("BalintHelper/EntityAggregateCountTrigger")]
    [Tracked]
    public class EntityAggregateCountTrigger : Trigger
    {
        internal readonly string counterId;
        private readonly EntityAggregateCountController.AggregateMode aggregateMode;

        private EntityAggregateCountController? controller;

        public int CurrentCount { get; private set; }

        public EntityAggregateCountTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            counterId = data.Attr("counterId", "entityCount");
            aggregateMode = data.Enum("aggregateMode", EntityAggregateCountController.AggregateMode.Maximum);
            Add(new EntityTypeFilterComponent(data.Attr("entityTypes", "")));
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
                foreach (Entity entity in Get<EntityTypeFilterComponent>().GetMatches())
                {
                    if (entity == this)
                    {
                        continue;
                    }
                    if (entity.Collider == null)
                    {
                        continue;
                    }
                    if (CollideCheck(entity))
                    {
                        newCount++;
                    }
                }
            }

            if (newCount != CurrentCount)
            {
                CurrentCount = newCount;
                UpdateCounter();
            }
        }

        public void UpdateCounter()
        {
            controller?.UpdateCounter();
        }
    }
}