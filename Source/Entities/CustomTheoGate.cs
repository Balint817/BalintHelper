using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.BalintHelper.Entities
{
    [CustomEntity("BalintHelper/CustomTheoGate")]
    [Tracked(false)]
    public class CustomTheoGate : Solid
    {
        public enum TheoModes
        {
            Any,
            All
        }

        private const float HoldingWaitTime = 0.2f;
        private const float HoldingOpenDistSq = 4096f;
        private const float HoldingCloseDistSq = 6400f;
        private const int MinDrawHeight = 4;

        private readonly int closedHeight;
        private readonly Sprite sprite;
        private readonly Shaker shaker;
        private readonly TheoModes theoMode;
        private readonly Vector2 holdingCheckFrom;

        private float drawHeight;
        private float drawHeightMoveSpeed;
        private bool open;
        private float holdingWaitTimer = HoldingWaitTime;
        private bool lockState;

        public CustomTheoGate(EntityData data, Vector2 offset)
            : base(data.Position + offset, 8f, data.Height, safe: true)
        {
            closedHeight = data.Height;
            theoMode = data.Enum("theoMode", TheoModes.Any);

            Add(sprite = GFX.SpriteBank.Create("templegate_theo"));
            sprite.X = Collider.Width / 2f;
            sprite.Play("idle");

            Add(shaker = new Shaker(on: false));

            Depth = -9000;
            holdingCheckFrom = Position + new Vector2(Width / 2f, closedHeight / 2f);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);

            if (TheoIsNearby())
            {
                StartOpen();
            }

            if (Collider is Hitbox hitbox)
            {
                hitbox.Width = 16f;
            }

            drawHeight = Math.Max(MinDrawHeight, Height);
        }

        public void Open()
        {
            Audio.Play("event:/game/05_mirror_temple/gate_theo_open", Position);
            holdingWaitTimer = HoldingWaitTime;
            drawHeightMoveSpeed = 200f;
            drawHeight = Height;
            shaker.ShakeFor(0.2f, removeOnFinish: false);
            SetHeight(0);
            sprite.Play("open");
            open = true;
        }

        public void StartOpen()
        {
            SetHeight(0);
            drawHeight = MinDrawHeight;
            open = true;
        }

        public void Close()
        {
            Audio.Play("event:/game/05_mirror_temple/gate_theo_close", Position);
            holdingWaitTimer = HoldingWaitTime;
            drawHeightMoveSpeed = 300f;
            drawHeight = Math.Max(MinDrawHeight, Height);
            shaker.ShakeFor(0.2f, removeOnFinish: false);
            SetHeight(closedHeight);
            sprite.Play("hit");
            open = false;
        }

        public bool TheoIsNearby()
        {
            if (Scene == null)
            {
                return true;
            }

            float maxDistanceSq = open ? HoldingCloseDistSq : HoldingOpenDistSq;
            bool foundRelevantCrystal = false;

            foreach (Entity entity in Scene.Tracker.GetEntities<TheoCrystal>())
            {
                if (entity is not TheoCrystal crystal)
                {
                    continue;
                }

                if (crystal.X > X + 10f)
                {
                    continue;
                }

                foundRelevantCrystal = true;
                bool isNearby = Vector2.DistanceSquared(holdingCheckFrom, crystal.Center) < maxDistanceSq;

                if (theoMode == TheoModes.Any)
                {
                    if (isNearby)
                    {
                        return true;
                    }
                }
                else if (!isNearby)
                {
                    return false;
                }
            }

            if (!foundRelevantCrystal)
            {
                return true;
            }

            return theoMode == TheoModes.All;
        }

        private void SetHeight(int height)
        {
            if (height < Collider.Height)
            {
                Collider.Height = height;
                return;
            }

            float y = Y;
            int currentHeight = (int)Collider.Height;
            if (Collider.Height < 64f)
            {
                Y -= 64f - Collider.Height;
                Collider.Height = 64f;
            }

            MoveVExact(height - currentHeight);
            Y = y;
            Collider.Height = height;
        }

        public override void Update()
        {
            base.Update();

            if (holdingWaitTimer > 0f)
            {
                holdingWaitTimer -= Engine.DeltaTime;
            }
            else if (!lockState)
            {
                if (open && !TheoIsNearby())
                {
                    Close();
                    CollideFirst<Player>(Position + new Vector2(8f, 0f))?.Die(Vector2.Zero);
                }
                else if (!open && TheoIsNearby())
                {
                    Open();
                }
            }

            float targetDrawHeight = Math.Max(MinDrawHeight, Height);
            if (drawHeight != targetDrawHeight)
            {
                lockState = true;
                drawHeight = Calc.Approach(drawHeight, targetDrawHeight, drawHeightMoveSpeed * Engine.DeltaTime);
            }
            else
            {
                lockState = false;
            }
        }
        public override void Render()
        {
            Vector2 shakeOffset = new Vector2(Math.Sign(shaker.Value.X), 0f);

            Draw.Rect(X - 2f, Y - 8f, 14f, 10f, Color.Black);

            if (drawHeight <= sprite.Height)
            {
                sprite.DrawSubrect(
                    shakeOffset,
                    new Rectangle(
                        0,
                        (int)(sprite.Height - drawHeight),
                        (int)sprite.Width,
                        (int)drawHeight
                    )
                );
            }
            else
            {
                float oldScaleY = sprite.Scale.Y;

                sprite.Scale.Y = drawHeight / sprite.Height;
                sprite.RenderPosition += shakeOffset;
                sprite.Render();

                sprite.Scale.Y = oldScaleY;
            }
        }
    }
}
