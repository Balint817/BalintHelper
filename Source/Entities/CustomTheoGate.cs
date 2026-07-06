using System;
using System.Collections.Generic;
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
            All,
            Each,
            None
        }

        public enum GateDirection
        {
            Down,
            Up,
            Left,
            Right
        }

        public enum PlayerMode
        {
            // Gate behaviour is independent of player proximity.
            Ignored,
            // Gate can only open if a player is within range (original needsPlayer = true).
            Required,
            // Gate closes (and stays closed) if a player is within range.
            Repels
        }

        private const float HoldingWaitTime = 0.2f;
        private const float HoldingOpenDistSq = 4096f;
        private const float HoldingCloseDistSq = 4096f;
        private const int MinDrawLength = 4;
        private const int GateThickness = 16;
        private const int OpenThickness = 2;

        private readonly int closedLength;
        private readonly Sprite sprite;
        private readonly Shaker shaker;
        private readonly TheoModes theoMode;
        private readonly string entityTypesRaw;
        private readonly GateDirection direction;
        private readonly PlayerMode playerMode;

        private readonly Vector2 basePosition;

        private readonly Vector2 closedCenter;

        private readonly HashSet<string> managedTypeNames = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<int> managedEntityIds = new HashSet<int>();
        private readonly HashSet<string> foundTypeNames = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<int> foundEntityIds = new HashSet<int>();

        private float drawLength;
        private float drawLengthMoveSpeed;
        private bool open;
        private float holdingWaitTimer = HoldingWaitTime;
        private bool lockState;

        public CustomTheoGate(EntityData data, Vector2 offset)
            : base(
                data.Position + offset,
                IsHorizontalDir(data.Enum("direction", GateDirection.Down))
                    ? Math.Max(data.Height, GateThickness)
                    : GateThickness,
                IsHorizontalDir(data.Enum("direction", GateDirection.Down))
                    ? GateThickness
                    : Math.Max(data.Height, GateThickness),
                safe: true)
        {
            basePosition = data.Position + offset;
            direction = data.Enum("direction", GateDirection.Down);
            closedLength = Math.Max(data.Height, GateThickness);
            theoMode = data.Enum("theoMode", TheoModes.Any);
            entityTypesRaw = data.Attr("entityTypes", "TheoCrystal");
            playerMode = data.Enum("playerMode", PlayerMode.Ignored);
            closedCenter = CalcClosedCenter(basePosition, closedLength, direction);

            ParseManagedEntityFilters();

            Add(sprite = GFX.SpriteBank.Create("templegate_theo"));
            sprite.Play("idle");
            ConfigureSpriteForDirection();

            Add(shaker = new Shaker(on: false));

            Depth = -9000;
            drawLength = closedLength;
        }

        private static bool IsHorizontalDir(GateDirection d) =>
            d == GateDirection.Left || d == GateDirection.Right;

        private bool IsHorizontal => IsHorizontalDir(direction);

        private int CurrentGateLength =>
            IsHorizontal ? (int)Collider.Width : (int)Collider.Height;

        private Vector2 GateCenter => closedCenter;

        private static Vector2 CalcClosedCenter(Vector2 pos, int len, GateDirection dir)
        {
            return IsHorizontalDir(dir)
                ? pos + new Vector2(len * 0.5f, GateThickness * 0.5f)
                : pos + new Vector2(GateThickness * 0.5f, len * 0.5f);
        }

        private void ConfigureSpriteForDirection()
        {
            sprite.Rotation = 0f;

            switch (direction)
            {
                case GateDirection.Down:
                default:
                    sprite.Position = new Vector2(GateThickness / 2f, 0f);
                    break;

                case GateDirection.Up:
                    sprite.Rotation = MathHelper.Pi;
                    sprite.Position = new Vector2(GateThickness / 2f, closedLength);
                    break;

                case GateDirection.Right:
                    sprite.Rotation = -MathHelper.PiOver2;
                    sprite.Position = new Vector2(0f, GateThickness / 2f);
                    break;

                case GateDirection.Left:
                    sprite.Rotation = MathHelper.PiOver2;
                    sprite.Position = new Vector2(closedLength, GateThickness / 2f);
                    break;
            }
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);

            if (TheoIsNearby())
            {
                StartOpen();
            }
            else
            {
                ApplyClosedCollider();
                drawLength = closedLength;
                open = false;
            }
        }

        public void Open()
        {
            Audio.Play("event:/game/05_mirror_temple/gate_theo_open", Position);
            holdingWaitTimer = HoldingWaitTime;
            drawLengthMoveSpeed = 200f;
            drawLength = closedLength;
            shaker.ShakeFor(0.2f, removeOnFinish: false);
            ApplyOpenCollider();
            sprite.Play("open");
            open = true;
        }

        public void StartOpen()
        {
            ApplyOpenCollider();
            drawLength = MinDrawLength;
            sprite.Play("open");
            open = true;
        }

        public void Close()
        {
            Audio.Play("event:/game/05_mirror_temple/gate_theo_close", Position);
            holdingWaitTimer = HoldingWaitTime;
            drawLengthMoveSpeed = 300f;
            drawLength = Math.Max(MinDrawLength, CurrentGateLength);
            shaker.ShakeFor(0.2f, removeOnFinish: false);
            ApplyClosedCollider();
            sprite.Play("hit");
            open = false;
        }

        private void ApplyClosedCollider()
        {
            Position = basePosition;
            if (IsHorizontal)
            {
                Collider.Width = closedLength;
                Collider.Height = GateThickness;
            }
            else
            {
                Collider.Width = GateThickness;
                Collider.Height = closedLength;
            }
        }

        private void ApplyOpenCollider()
        {
            switch (direction)
            {
                case GateDirection.Down:
                default:
                    Position = basePosition;
                    Collider.Width = GateThickness;
                    Collider.Height = OpenThickness;
                    break;

                case GateDirection.Up:
                    Position = basePosition + new Vector2(0f, closedLength - OpenThickness);
                    Collider.Width = GateThickness;
                    Collider.Height = OpenThickness;
                    break;

                case GateDirection.Right:
                    Position = basePosition;
                    Collider.Width = OpenThickness;
                    Collider.Height = GateThickness;
                    break;

                case GateDirection.Left:
                    Position = basePosition + new Vector2(closedLength - OpenThickness, 0f);
                    Collider.Width = OpenThickness;
                    Collider.Height = GateThickness;
                    break;
            }
        }

        private bool IsEntityWithinGateRange(Entity entity, float maxDistanceSq)
        {
            Vector2 delta = entity.Center - GateCenter;

            float longAxisLimit = (float)Math.Sqrt(maxDistanceSq);
            float shortAxisLimit = Math.Max(closedLength * 1f, 8f);

            // For a vertical gate the long axis is Y; for horizontal it is X.
            float horizontalLimit = IsHorizontal ? shortAxisLimit : longAxisLimit;
            float verticalLimit = IsHorizontal ? longAxisLimit : shortAxisLimit;

            return Math.Abs(delta.X) <= horizontalLimit
                && Math.Abs(delta.Y) <= verticalLimit;
        }

        public bool TheoIsNearby()
        {
            if (Scene == null)
                return true;

            float maxDistanceSq = open ? HoldingCloseDistSq : HoldingOpenDistSq;

            Player playerEntity = Scene.Tracker.GetEntity<Player>();

            if (playerMode == PlayerMode.Required)
            {
                // Gate cannot open unless a player is within range.
                if (playerEntity == null || !IsEntityWithinGateRange(playerEntity, maxDistanceSq))
                    return false;
            }
            else if (playerMode == PlayerMode.Repels)
            {
                // Gate closes (returns false) if a player is within range.
                if (playerEntity != null && IsEntityWithinGateRange(playerEntity, maxDistanceSq))
                    return false;
            }

            bool foundRelevantHoldable = false;

            if (theoMode == TheoModes.Each)
            {
                foundTypeNames.Clear();
                foundEntityIds.Clear();
            }

            foreach (Entity entity in GetManagedHoldables())
            {
                foundRelevantHoldable = true;
                bool isNearby = IsEntityWithinGateRange(entity, maxDistanceSq);

                if (theoMode == TheoModes.None)
                {
                    // Any entity nearby means we should NOT open.
                    if (isNearby) return false;
                }
                else if (theoMode == TheoModes.Any)
                {
                    if (isNearby) return true;
                }
                else if (theoMode == TheoModes.All)
                {
                    if (!isNearby) return false;
                }
                else // Each
                {
                    if (isNearby)
                    {
                        if (entity.SourceData?.ID is int entityId && managedEntityIds.Contains(entityId))
                            foundEntityIds.Add(entityId);

                        string sourceName = entity.SourceData?.Name ?? "";
                        if (managedTypeNames.Contains(sourceName))
                            foundTypeNames.Add(sourceName);
                        else if (managedTypeNames.Contains(entity.GetType().Name))
                            foundTypeNames.Add(entity.GetType().Name);

                        if (foundTypeNames.Count == managedTypeNames.Count &&
                            foundEntityIds.Count == managedEntityIds.Count)
                            return true;
                    }
                }
            }

            if (!foundRelevantHoldable) return true;
            if (theoMode == TheoModes.None) return true;  // No managed entity was nearby -> open
            if (theoMode == TheoModes.All) return true;
            if (theoMode == TheoModes.Each)
            {
                return foundTypeNames.Count == managedTypeNames.Count &&
                       foundEntityIds.Count == managedEntityIds.Count;
            }
            return false;
        }

        private void SetGateSize(int size)
        {
            size = Math.Max(0, size);
            if (IsHorizontal) SetWidth(size);
            else SetHeight(size);
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

        private void SetWidth(int width)
        {
            if (width < Collider.Width)
            {
                Collider.Width = width;
                return;
            }

            float x = X;
            int currentWidth = (int)Collider.Width;

            if (Collider.Width < 64f)
            {
                X -= 64f - Collider.Width;
                Collider.Width = 64f;
            }

            MoveHExact(width - currentWidth);
            X = x;
            Collider.Width = width;
        }

        private void KillPlayerOnClose()
        {
            var player = Scene?.Tracker.GetEntity<Player>();
            if (player == null || !CollideCheck(player))
                return;

            Vector2 killDir = direction switch
            {
                GateDirection.Up => -Vector2.UnitY,
                GateDirection.Left => -Vector2.UnitX,
                GateDirection.Right => Vector2.UnitX,
                _ => Vector2.Zero,
            };

            player.Die(killDir);
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
                bool nearby = TheoIsNearby();
                if (open && !nearby)
                {
                    Close();
                    KillPlayerOnClose();
                }
                else if (!open && nearby)
                {
                    Open();
                }
            }

            float targetDrawLength = open ? MinDrawLength : closedLength;
            if (drawLength != targetDrawLength)
            {
                lockState = true;
                drawLength = Calc.Approach(drawLength, targetDrawLength, drawLengthMoveSpeed * Engine.DeltaTime);
            }
            else
            {
                lockState = false;
            }
        }

        private void ParseManagedEntityFilters()
        {
            managedTypeNames.Clear();
            managedEntityIds.Clear();

            foreach (string raw in entityTypesRaw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string token = raw.Trim();
                if (token.Length == 0) continue;

                if (int.TryParse(token, out int entityId))
                    managedEntityIds.Add(entityId);
                else
                    managedTypeNames.Add(token);
            }
        }

        private bool IsManagedEntity(Entity entity)
        {
            Type type = entity.GetType();
            bool byType = managedTypeNames.Contains(entity.SourceData?.Name ?? "")
                       || managedTypeNames.Contains(type.Name);
            if (byType) return true;

            if (managedEntityIds.Count > 0 && entity.SourceData?.ID is int entityId)
                return managedEntityIds.Contains(entityId);

            return false;
        }

        private IEnumerable<Entity> GetManagedHoldables()
        {
            if (Scene == null) yield break;
            foreach (Entity entity in Scene.Entities)
            {
                if (entity.Get<Holdable>() != null && IsManagedEntity(entity))
                    yield return entity;
            }
        }

        private void RenderCapRect()
        {
            switch (direction)
            {
                case GateDirection.Down:
                default:
                    // Cap above the gate top, full width
                    Draw.Rect(basePosition.X, basePosition.Y - 8f, GateThickness, 10f, Color.Black);
                    break;

                case GateDirection.Up:
                    // Cap below the gate bottom, full width
                    Draw.Rect(basePosition.X, basePosition.Y + closedLength - 2f, GateThickness, 10f, Color.Black);
                    break;

                case GateDirection.Right:
                    // Cap left of the gate left edge, full height
                    Draw.Rect(basePosition.X - 8f, basePosition.Y, 10f, GateThickness, Color.Black);
                    break;

                case GateDirection.Left:
                    // Cap right of the gate right edge, full height
                    Draw.Rect(basePosition.X + closedLength - 2f, basePosition.Y, 10f, GateThickness, Color.Black);
                    break;
            }
        }

        public override void Render()
        {
            RenderCapRect();

            Vector2 savedEntityPos = Position;
            Position = basePosition;

            Vector2 shakeOffset = new Vector2(Math.Sign(shaker.Value.X), 0f);

            if (drawLength <= sprite.Height)
            {
                // Clip from the tail (bottom of texture), keep the head (top of texture).
                sprite.DrawSubrect(
                    shakeOffset,
                    new Rectangle(
                        0,
                        (int)(sprite.Height - drawLength),
                        (int)sprite.Width,
                        (int)drawLength
                    )
                );
            }
            else
            {
                // Scale along texture-Y (= world length axis due to rotation).
                float oldScaleY = sprite.Scale.Y;
                Vector2 oldRenderPos = sprite.RenderPosition;

                sprite.Scale.Y = drawLength / sprite.Height;
                sprite.RenderPosition += shakeOffset;
                sprite.Render();

                sprite.RenderPosition = oldRenderPos;
                sprite.Scale.Y = oldScaleY;
            }

            Position = savedEntityPos;
        }
    }
}