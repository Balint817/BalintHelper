using Celeste.Mod.BalintHelper.Utils;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.BalintHelper.Triggers
{
    [CustomEntity("BalintHelper/ThrowTimerSetTrigger")]
    public class ThrowTimerSetTrigger : Trigger
    {
        public enum TriggerModes
        {
            OnEntry,
            OnLeave,
            EntryOrLeave,
            Stay
        }

        private readonly float timerValue;

        private readonly TriggerModes playerTriggerMode;
        private readonly bool onlyOnce;
        private readonly bool waitForSuccess;

        private readonly EntityTypeFilter managedEntities;

        //p.minHoldTimer
        public ThrowTimerSetTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            timerValue = data.Float("value", 0f);
            managedEntities = new EntityTypeFilter(data.Attr("entityTypes", "TheoCrystal,ExtendedVariantMode/TheoCrystal"));

            playerTriggerMode = data.Enum("playerTriggerMode", TriggerModes.Stay);
            onlyOnce = data.Bool("onlyOnce", true);
            waitForSuccess = data.Bool("waitForSuccess", true);
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (playerTriggerMode == TriggerModes.OnEntry || playerTriggerMode == TriggerModes.EntryOrLeave || playerTriggerMode == TriggerModes.Stay)
            {
                FireTrigger(player);
            }
        }

        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            if (playerTriggerMode == TriggerModes.OnLeave || playerTriggerMode == TriggerModes.EntryOrLeave)
            {
                FireTrigger(player);
            }
        }

        public override void OnStay(Player player)
        {
            base.OnStay(player);
            if (playerTriggerMode == TriggerModes.Stay)
            {
                FireTrigger(player);
            }
        }
        private void FireTrigger(Player player)
        {
            var match = player.Holding is { } holdable && managedEntities.Matches(holdable.Entity);
            if (match)
            {
                player.minHoldTimer = timerValue;
            }
            if (onlyOnce && (!waitForSuccess || match))
            {
                RemoveSelf();
            }
        }
    }
}