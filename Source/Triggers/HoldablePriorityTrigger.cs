
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using System;

namespace Celeste.Mod.BalintHelper.Triggers
{
    public enum HoldableSelectMode
    {
        LowestId,
        HighestId,
        Newest,
        Oldest,
        Closest,
        Furthest,
        ClosestFacing,
        FurthestFacing,
    }

    [Flags]
    public enum HoldableSelectFlags
    {
        None = 0,
        DisableTheoFreeze = 1 << 0,
    }

    [CustomEntity("BalintHelper/HoldablePriorityTrigger")]
    [Tracked(false)]
    public class HoldablePriorityTrigger : Trigger
    {

        public readonly HoldableSelectMode Mode;
        public readonly HoldableSelectFlags Flags;

        public HoldablePriorityTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            Mode = data.Enum("mode", HoldableSelectMode.LowestId);
            Flags = data.Bool("disableTheoFreeze", false)
                    ? HoldableSelectFlags.DisableTheoFreeze
                    : HoldableSelectFlags.None;

        }
    }
}
