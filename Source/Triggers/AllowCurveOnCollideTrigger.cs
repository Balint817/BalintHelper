using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Reflection;

namespace Celeste.Mod.BalintHelper.Triggers
{
    [CustomEntity("BalintHelper/AllowCurveOnCollideTrigger")]
    [Tracked]
    public class AllowCurveOnCollideTrigger : Trigger
    {
        private readonly string flag;
        private readonly bool global;
        private readonly bool invert;
        public bool IsTriggered { get; private set; }
        public AllowCurveOnCollideTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            flag = data.Attr("flag", "");
            global = data.Bool("global", false);
            if (!string.IsNullOrEmpty(flag)) 
            {
                invert = flag[0] == '!';
                flag = flag[1..];
            }
        }

        public override void Update()
        {
            if (global)
            {
                CheckTrigger();
            }
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (global)
            {
                return;
            }
            CheckTrigger();
        }

        public override void OnStay(Player player)
        {
            base.OnStay(player);
            if (global)
            {
                return;
            }
            CheckTrigger();
        }

        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            if (global)
            {
                return;
            }
            IsTriggered = false;
        }

        private void CheckTrigger()
        {
            if (string.IsNullOrEmpty(flag))
            {
                IsTriggered = true;
                return;
            }

            IsTriggered = (SceneAs<Level>()?.Session.GetFlag(flag) is { } flagState) && (flagState ^ invert);
        }
    }
}