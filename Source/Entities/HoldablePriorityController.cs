using Celeste.Mod.BalintHelper.Triggers;
using Celeste.Mod.BalintHelper.Utils;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.BalintHelper.Entities
{
    [Tracked(false)]
    public class HoldablePriorityController : Entity
    {
        public static HoldablePriorityController GetOrCreate(Scene scene)
        {
            return scene.GetOrCreateTrackedSingleton<HoldablePriorityController>();
        }
        public override void Added(Scene scene)
        {
            this.DuplicateCheck(scene);

            base.Added(scene);
        }
        public HoldablePriorityController()
        {
            Tag = Tags.Persistent | Tags.Global;
        }
        public Holdable? GetTarget(Player player)
        {
            var triggers = player.Scene.Tracker
                .GetEntities<HoldablePriorityTrigger>()
                .Cast<HoldablePriorityTrigger>()
                .Where(t => t.PlayerIsInside)
                .GroupBy(t => t.Priority)
                .OrderBy(group => group.Key)
                .SelectMany(group => group)
                .ToList();

            if (triggers.Count == 0)
            {
                return null!;
            }

            var mode = triggers[^1].Mode;
            var flags = triggers.Aggregate(HoldableSelectFlags.None,
                            (acc, t) => acc | t.Flags);

            if (flags.HasFlag(HoldableSelectFlags.DisableTheoFreeze))
            {
                var current = player.Holding;
                if (current != null)
                {
                    return current;
                }
            }

            var candidates = GetPickupCandidates(player);
            if (candidates.Count == 0)
            {
                return null!;
            }

            return SelectBy(player, candidates, mode);
        }

        private static List<Holdable> GetPickupCandidates(Player player)
        {
            return [.. player.Scene.Tracker
                .GetComponents<Holdable>()
                .Cast<Holdable>()
                .Where(h => player.Holding != h && h.cannotHoldTimer <= 0 && h.Check(player))];
        }

        private readonly MRUSet<Holdable> holdableOrder = [];

        public override void Update()
        {
            // track which holdables the player grabs
            if (Scene?.Tracker?.GetEntity<Player>() is not { } player)
            {
                return;
            }
            if (player.Holding is { } holdable)
            {
                holdableOrder.Add(holdable);
            }
        }

        private Holdable? SelectBy(Player player, List<Holdable> candidates,
                                  HoldableSelectMode mode)
        {
            if (candidates.Count == 0)
            {
                return null;
            }
            return mode switch
            {
                HoldableSelectMode.HighestId => candidates.Last(),
                HoldableSelectMode.Newest => OrderedHoldablesOrDefault(candidates, false),
                HoldableSelectMode.Oldest => OrderedHoldablesOrDefault(candidates, true),
                HoldableSelectMode.Closest => candidates.MinBy(h => CenterDist(player, h))!,
                HoldableSelectMode.Furthest => candidates.MaxBy(h => CenterDist(player, h))!,
                HoldableSelectMode.ClosestFacing => candidates.MinBy(h => FacingDist(player, h))!,
                HoldableSelectMode.FurthestFacing => candidates.MaxBy(h => FacingDist(player, h))!,
                // vanilla behavior
                _ => candidates.First(),
            };
        }

        private Holdable OrderedHoldablesOrDefault(List<Holdable> candidates, bool oldest)
        {
            var filteredHoldableOrder = holdableOrder.Intersect(candidates).ToList();
            if (filteredHoldableOrder.Count > 0)
            {
                if (!oldest)
                {
                    return filteredHoldableOrder.Last();
                }
                if (filteredHoldableOrder.Count != candidates.Count)
                {
                    // count the one that was never grabbed as the "oldest" one
                    return candidates.Except(filteredHoldableOrder).First();
                }
                return filteredHoldableOrder.First();
            }
            // vanilla behavior
            return candidates.First();
        }

        // Center-to-center distance.
        private static float CenterDist(Player player, Holdable h) =>
            Vector2.Distance(player.Center, h.Entity.Center);

        // Distance measured from the edge of the player's hitbox in the facing direction.
        private static float FacingDist(Player player, Holdable h)
        {
            float edgeX = player.Facing == Facings.Right
                ? player.Right
                : player.Left;
            var edgePoint = new Vector2(edgeX, player.CenterY);
            return Vector2.Distance(edgePoint, h.Entity.Center);
        }
    }
}
