using Celeste.Mod.BalintHelper.Source.Utils;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

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

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (resetFlag)
            {
                scene.TrySetFlag(flag, false);
            }
        }

        public override void Update()
        {
            base.Update();

            if (onlyOnce && triggeredOnce && !resetFlag)
            {
                return;
            }

            var level = SceneAs<Level>();
            if (level is null)
            {
                return;
            }

            var binoOk = !needsBino || level.IsInLookout();
            // don't use PlayerIsInside to avoid Update order issues (Player might call OnEntry AFTER we've already updated)
            // which might cause us to fire a frame late (or not fire at all if the interaction lasted for one frame)
            var playerSource = triggerOnPlayer && CollideCheck<Player>();
            var cameraSource = CameraCheck(level);
            var activeNow = (playerSource || cameraSource) && binoOk;

            if (activeNow)
            {
                TryFire(level);
            }
            else if (triggered)
            {
                triggered = false;
                if (resetFlag)
                {
                    Scene.TrySetFlag(flag, false);
                    if (onlyOnce)
                    {
                        RemoveSelf();
                    }
                }
            }
        }

        private bool CameraCheck(Level level)
        {
            var cameraRect = new Rectangle(
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
            level.TrySetFlag(flag, true);

            if (onlyOnce && !resetFlag)
            {
                RemoveSelf();
            }
        }
    }
}