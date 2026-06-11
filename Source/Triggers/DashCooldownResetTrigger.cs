using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Reflection;

namespace Celeste.Mod.BalintHelper.Triggers
{
    /// <summary>
    /// BalintHelper/DashCooldownSetTrigger
    ///
    /// Sets the player's internal dash cooldown timer (dashCooldownTimer)
    /// to a configured value when any of the configured events occurs.
    ///
    /// Properties (set in Lönn):
    ///   value         – float (default 0f)   Value to set dash cooldown timer to
    ///   resetOnEnter  – bool  (default true) Reset when player enters the trigger area
    ///   resetOnStay   – bool  (default false) Reset every frame the player is inside
    ///   resetOnLeave  – bool  (default false) Reset when player exits the trigger area
    ///   maxUses       – int   (default 0)    Maximum number of times the trigger can fire
    ///                                        (0 = unlimited, non-persistent: resets on room revisit or respawn)
    /// </summary>
    [CustomEntity("BalintHelper/DashCooldownSetTrigger")]
    public class DashCooldownSetTrigger : Trigger
    {
        // ── Reflection cache ──────────────────────────────────────────────────────
        private static readonly FieldInfo DashCooldownTimerField =
            typeof(Player).GetField(
                "dashCooldownTimer",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

        // ── Per-instance config ───────────────────────────────────────────────────
        private readonly float value;
        private readonly bool resetOnEnter;
        private readonly bool resetOnStay;
        private readonly bool resetOnLeave;
        private readonly int maxUses;

        private int uses = 0;
        private bool canActivate => maxUses <= 0 || uses < maxUses;

        // ── Constructor ───────────────────────────────────────────────────────────
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

        // ── Event hooks ───────────────────────────────────────────────────────────

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (resetOnEnter)
                TrySet(player);
        }

        public override void OnStay(Player player)
        {
            base.OnStay(player);
            if (resetOnStay)
                TrySet(player);
        }

        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            if (resetOnLeave)
                TrySet(player);
        }

        // ── Core logic ────────────────────────────────────────────────────────────

        private void TrySet(Player player)
        {
            if (!canActivate)
                return;

            float current = GetDashCooldown(player);

            // Optional optimization: don't reapply same value
            if (current == value)
                return;

            SetDashCooldown(player, value);
            uses++;
        }

        // ── Reflection helpers ────────────────────────────────────────────────────

        private static float GetDashCooldown(Player player)
        {
            if (DashCooldownTimerField == null) return 0f;
            return (float)DashCooldownTimerField.GetValue(player);
        }

        private static void SetDashCooldown(Player player, float value)
        {
            DashCooldownTimerField?.SetValue(player, value);
        }
    }
}