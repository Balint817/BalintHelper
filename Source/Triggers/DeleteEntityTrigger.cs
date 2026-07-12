using Celeste.Mod.BalintHelper.Utils;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.BalintHelper.Triggers
{
    [CustomEntity("BalintHelper/DeleteEntityTrigger")]
    public class DeleteEntityTrigger : Trigger
    {
        public enum TargetingModes
        {
            Inside,
            Outside,
            Everywhere
        }

        private readonly TargetingModes targetingMode;
        private readonly EntityTypeFilter managedEntities;
        private readonly string flag;

        public DeleteEntityTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            targetingMode = data.Enum("targetingMode", TargetingModes.Inside);
            managedEntities = new EntityTypeFilter(data.Attr("entityTypes", ""));
            flag = data.Attr("flag", "");
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);

            if (!ShouldRun())
            {
                RemoveSelf();
                return;
            }

            List<Entity> toRemove = new List<Entity>();

            foreach (Entity entity in scene.Entities)
            {
                if (entity == this)
                {
                    continue;
                }

                if (!MatchesFilters(entity))
                {
                    continue;
                }

                bool matchesTargeting = targetingMode switch
                {
                    TargetingModes.Inside => CollideCheck(entity),
                    TargetingModes.Outside => !CollideCheck(entity),
                    TargetingModes.Everywhere => true,
                    _ => false
                };

                if (matchesTargeting)
                {
                    toRemove.Add(entity);
                }
            }

            foreach (Entity entity in toRemove)
            {
                entity.RemoveSelf();
            }

            RemoveSelf();
        }

        private bool ShouldRun()
        {
            if (string.IsNullOrWhiteSpace(flag))
            {
                return true;
            }

            Level level = SceneAs<Level>();
            if (level == null)
            {
                return false;
            }

            bool inverted = flag[0] == '!';
            string flagName = inverted ? flag[1..] : flag;

            if (string.IsNullOrWhiteSpace(flagName))
            {
                return true;
            }

            bool value = level.Session.GetFlag(flagName);
            return inverted ? !value : value;
        }

        private bool MatchesFilters(Entity entity)
        {
            if (!managedEntities.Any)
            {
                return true;
            }

            return managedEntities.Matches(entity);
        }
    }
}