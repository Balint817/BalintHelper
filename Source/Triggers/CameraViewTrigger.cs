using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.BalintHelper.Triggers
{
    [CustomEntity("BalintHelper/CameraViewTrigger")]
    public class CameraViewTrigger : Trigger
    {
        private bool triggered;
        private bool triggeredOnce;

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
                RemoveSelf();
                Logger.Warn("BalintHelper", $"{nameof(CameraViewTrigger)} with empty flag at {Position}");
            }
        }

        public override void Update()
        {
            base.Update();

            if (onlyOnce && triggeredOnce && !resetFlag)
            {
                return;
            }

            Level level = SceneAs<Level>();
            if (level is null)
            {
                return;
            }

            bool binoOk = !needsBino || level.IsInLookout();
            bool playerSource = triggerOnPlayer && PlayerIsInside;
            bool cameraSource = CameraCheck(level);
            bool activeNow = (playerSource || cameraSource) && binoOk;

            if (activeNow)
            {
                TryFire(level);
            }
            else if (triggered)
            {
                triggered = false;
                if (resetFlag)
                {
                    level.Session?.SetFlag(flag, false);
                    if (onlyOnce)
                    {
                        RemoveSelf();
                    }
                }
            }
        }

        private bool CameraCheck(Level level)
        {
            Rectangle cameraRect = new Rectangle(
                (int)level.Camera.X,
                (int)level.Camera.Y,
                320, 180
            );
            return CollideRect(cameraRect);
        }

        private void TryFire(Level level)
        {
            if (triggered || (onlyOnce && triggeredOnce))
            {
                return;
            }
            triggered = true;
            triggeredOnce = true;
            level.Session?.SetFlag(flag, true);

            if (onlyOnce && !resetFlag)
            {
                RemoveSelf();
            }
        }
    }
}