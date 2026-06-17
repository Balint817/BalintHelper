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
            Each
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
        private readonly string entityTypesRaw;

        private readonly HashSet<string> managedTypeNames = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<int> managedEntityIds = new HashSet<int>();

        // Cached sets to track found entities for TheoModes.Each without causing GC allocations every frame
        private readonly HashSet<string> foundTypeNames = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<int> foundEntityIds = new HashSet<int>();

        private float drawHeight;
        private float drawHeightMoveSpeed;
        private bool open;
        private float holdingWaitTimer = HoldingWaitTime;
        private bool lockState;

        private readonly bool needsPlayer;

        public CustomTheoGate(EntityData data, Vector2 offset)
            : base(data.Position + offset, 8f, data.Height, safe: true)
        {
            closedHeight = data.Height;
            theoMode = data.Enum("theoMode", TheoModes.Any);
            entityTypesRaw = data.Attr("entityTypes", "TheoCrystal");
            needsPlayer = data.Bool("needsPlayer", false);
            ParseManagedEntityFilters();

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

            if (needsPlayer)
            {
                var player = Scene.Tracker.GetEntity<Player>();
                if (Vector2.DistanceSquared(holdingCheckFrom, player.Center) >= maxDistanceSq)
                {
                    return false;
                }
            }

            bool foundRelevantHoldable = false;

            if (theoMode == TheoModes.Each)
            {
                foundTypeNames.Clear();
                foundEntityIds.Clear();
            }

            foreach (Entity entity in GetManagedHoldables())
            {
                if (entity.X > X + 10f)
                {
                    continue;
                }

                foundRelevantHoldable = true;
                bool isNearby = Vector2.DistanceSquared(holdingCheckFrom, entity.Center) < maxDistanceSq;

                if (theoMode == TheoModes.Any)
                {
                    if (isNearby)
                    {
                        return true;
                    }
                }
                else if (theoMode == TheoModes.All)
                {
                    if (!isNearby)
                    {
                        return false;
                    }
                }
                else if (theoMode == TheoModes.Each)
                {
                    if (isNearby)
                    {
                        if (entity.SourceData?.ID is int entityId && managedEntityIds.Contains(entityId))
                        {
                            foundEntityIds.Add(entityId);
                        }

                        string sourceName = entity.SourceData?.Name ?? "";
                        if (managedTypeNames.Contains(sourceName))
                        {
                            foundTypeNames.Add(sourceName);
                        }
                        else if (managedTypeNames.Contains(entity.GetType().Name))
                        {
                            foundTypeNames.Add(entity.GetType().Name);
                        }

                        // Early exit optimization if all required types and IDs are found
                        if (foundTypeNames.Count == managedTypeNames.Count && foundEntityIds.Count == managedEntityIds.Count)
                        {
                            return true;
                        }
                    }
                }
            }

            // If no relevant holdables exist in the active area, open the gate (vanilla behavior fallback)
            if (!foundRelevantHoldable)
            {
                return true;
            }

            // If we are checking "All", and we didn't return false in the loop above, all relevant entities are nearby
            if (theoMode == TheoModes.All)
            {
                return true;
            }

            // If "Each" didn't early exit, do a final evaluation
            if (theoMode == TheoModes.Each)
            {
                return foundTypeNames.Count == managedTypeNames.Count && foundEntityIds.Count == managedEntityIds.Count;
            }

            return false;
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

        private void ParseManagedEntityFilters()
        {
            managedTypeNames.Clear();
            managedEntityIds.Clear();

            var tokens = entityTypesRaw
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var raw in tokens)
            {
                var token = raw.Trim();
                if (token.Length == 0)
                    continue;

                if (int.TryParse(token, out int entityId))
                    managedEntityIds.Add(entityId);
                else
                    managedTypeNames.Add(token);
            }
        }

        private bool IsManagedEntity(Entity entity)
        {
            var type = entity.GetType();
            bool byType = managedTypeNames.Contains(entity.SourceData?.Name ?? "") || managedTypeNames.Contains(type.Name);
            if (byType)
                return true;

            if (managedEntityIds.Count > 0 && entity.SourceData?.ID is int entityId)
                return managedEntityIds.Contains(entityId);

            return false;
        }

        private IEnumerable<Entity> GetManagedHoldables()
        {
            if (Scene == null)
                yield break;

            foreach (Entity entity in Scene.Entities)
            {
                if (entity.Get<Holdable>() == null)
                    continue;

                if (IsManagedEntity(entity))
                    yield return entity;
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