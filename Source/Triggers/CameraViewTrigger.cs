using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.BalintHelper.Triggers
{
    [CustomEntity("BalintHelper/CameraViewTrigger")]
    public class CameraViewTrigger : Trigger
    {
        private bool _triggered;
        private bool triggeredOnce;
        private bool triggered
        {
            get
            {
                return _triggered;
            }
            set
            {
                _triggered = value;
                triggeredOnce = triggeredOnce || value;
            }
        }
        private readonly bool triggerOnPlayer;
        private readonly bool onlyOnce;
        private readonly string flag;
        private readonly bool resetFlag;
        private readonly bool needsBino;

        public CameraViewTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            triggerOnPlayer = data.Bool("triggerOnPlayer", false);
            onlyOnce = data.Bool("onlyOnce", true);
            flag = data.Attr("flag");
            resetFlag = data.Bool("resetFlag", false);
            needsBino = data.Bool("needsBino", true);

            if (string.IsNullOrWhiteSpace(flag))
            {
                triggered = true;
                RemoveSelf();
                Logger.Warn("BalintHelper", $"{nameof(CameraViewTrigger)} with empty flag at {Position}");
                return;
            }
        }

        public override void Update()
        {
            //no components to update
            //base.Update();

            if (triggerOnPlayer && PlayerIsInside)
                return;

            if (CameraCheck())
            {
                TryFire();
            }
            else
            {
                triggered = false;
                ResetFlagIfNeeded();
            }
        }

        private bool CameraCheck()
        {
            var level = SceneAs<Level>();
            if (level is null)
            {
                return false;
            }
            Rectangle cameraRect = new Rectangle(
                (int)level.Camera.X,
                (int)level.Camera.Y,
                320, 180
            );

            return CollideRect(cameraRect);
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (triggerOnPlayer)
            {
                TryFire();
            }
        }
        private void ResetFlagIfNeeded()
        {
            if (!resetFlag || triggered)
            {
                return;
            }
            var level = SceneAs<Level>();
            if (level is null)
            {
                return;
            }
            level.Session?.SetFlag(flag, false);
            if (onlyOnce)
            {
                RemoveSelf();
            }
        }
        public override void OnStay(Player player)
        {
            base.OnStay(player);
            if (triggerOnPlayer)
            {
                TryFire();
            }
        }
        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            if (triggerOnPlayer)
            {
                triggered = CameraCheck() && (!needsBino || SceneAs<Level>()?.IsInLookout() == true);
                ResetFlagIfNeeded();
            }
        }

        private void TryFire()
        {
            if (triggered || (onlyOnce && triggeredOnce))
            {
                return;
            }
            if (needsBino && SceneAs<Level>()?.IsInLookout() != true)
            {
                return;
            }
            triggered = true;


            var level = SceneAs<Level>();
            if (level is null)
            {
                return;
            }
            level.Session?.SetFlag(flag, true);


            if (onlyOnce && !resetFlag)
            {
                RemoveSelf();
            }
        }
    }
}
