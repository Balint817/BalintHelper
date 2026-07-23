using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System.Reflection;

namespace Celeste.Mod.BalintHelper.Triggers
{
    [CustomEntity("BalintHelper/DashCooldownSetTrigger")]
    public class DashCooldownSetTrigger : Trigger
    {
        private static readonly FieldInfo DashCooldownTimerField =
            typeof(Player).GetField(
                "dashCooldownTimer",
                BindingFlags.Instance | BindingFlags.NonPublic
            )!;

        private readonly float value;
        private readonly bool resetOnEnter;
        private readonly bool resetOnStay;
        private readonly bool resetOnLeave;
        private readonly int maxUses;

        private int uses = 0;
        private bool CanActivate => maxUses <= 0 || uses < maxUses;

        public DashCooldownSetTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            value = data.Float("value", defaultValue: 0f);
            if (value < 0)
            {
                value = 0;
            }
            resetOnEnter = data.Bool("resetOnEnter", defaultValue: true);
            resetOnStay = data.Bool("resetOnStay", defaultValue: false);
            resetOnLeave = data.Bool("resetOnLeave", defaultValue: false);
            maxUses = data.Int("maxUses", defaultValue: 0);
        }


        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (resetOnEnter)
            {
                TrySet(player);
            }
        }

        public override void OnStay(Player player)
        {
            base.OnStay(player);
            if (resetOnStay)
            {
                TrySet(player);
            }
        }

        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            if (resetOnLeave)
            {
                TrySet(player);
            }
        }


        private void TrySet(Player player)
        {
            if (!CanActivate)
            {
                return;
            }

            var current = GetDashCooldown(player);

            // don't reapply same value
            if (current == value)
            {
                return;
            }

            SetDashCooldown(player, value);
            uses++;
        }

        private static float GetDashCooldown(Player player)
        {
            if (DashCooldownTimerField == null)
            {
                return 0f;
            }

            return (float)DashCooldownTimerField.GetValue(player)!;
        }

        private static void SetDashCooldown(Player player, float value)
        {
            DashCooldownTimerField?.SetValue(player, value);
        }
    }
}