using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.BalintHelper.Entities
{
    /// <summary>
    /// BalintHelper/CustomPedestal
    ///
    /// A fully-customisable replacement for TheoCrystalPedestal that supports
    /// multiple pedestals per map, configurable return timers, breakability,
    /// particle trails, and any Holdable entity type.
    ///
    /// Lönn properties:
    ///   spriteNormal           – string  Atlas path for intact state sprite
    ///                            (default "characters/theoCrystal/pedestal")
    ///   spriteBroken           – string  Atlas path for broken state sprite
    ///                            (default "characters/theoCrystal/pedestal")
    ///   returnDelay            – float   Seconds after release before teleport back (default 0)
    ///   instantReturnInBounds  – bool    Teleport immediately when released inside
    ///                            the pedestal collider (default false)
    ///   maxDistance            – float   Max range from pedestal for auto-return
    ///                            (<=0 = infinite, default 0)
    ///   entityTypes            – string  Comma-separated entity type names and/or
    ///                            numeric Lönn entity IDs to manage (default "TheoCrystal")
    ///   breakable              – bool    Allow dashing into pedestal to break it (default false)
    ///   brokenDisableDuration  – float   Seconds pedestal stays broken (<=0 = forever, default 0)
    ///   showReturnLine         – bool    Emit particle trail during return countdown (default true)
    ///   particleReturn         – string  GFX.Game atlas path for trail particle sprite
    ///                            (blank = built-in cyan blob)
    ///   particleExplode        – string  GFX.Game atlas path for teleport burst particle sprite
    ///                            (blank = built-in cyan blob)
    ///   particleBreak          – string  GFX.Game atlas path for break debris particle sprite
    ///                            (blank = built-in white shard)
    ///   particleRepair         – string  GFX.Game atlas path for repair burst particle sprite
    ///                            (blank = built-in cyan blob)
    ///   soundTeleport          – string  FMOD event for teleport
    ///   soundBreak             – string  FMOD event for break
    ///   soundRepair            – string  FMOD event for repair
    /// </summary>
    [CustomEntity("BalintHelper/CustomPedestal")]
    [Tracked]
    public class CustomPedestal : Solid
    {
        // ── Reflection: TheoCrystal.OnPedestal, Speed ────────────────────────────
        private static readonly PropertyInfo TheoCrystalOnPedestalProp =
            typeof(TheoCrystal).GetProperty("OnPedestal",
                BindingFlags.Instance | BindingFlags.Public)!;

        private static readonly FieldInfo TheoCrystalSpeedField =
            typeof(TheoCrystal).GetField("Speed",
                BindingFlags.Instance | BindingFlags.Public)!;

        // ── Built-in fallback particle types (lazy) ───────────────────────────────
        private static ParticleType? s_ReturnLineFallback;
        private static ParticleType? s_ExplodeFallback;
        private static ParticleType? s_BreakFallback;

        private static ParticleType GetReturnLineFallback() => s_ReturnLineFallback ??= new ParticleType
        {
            Source = GFX.Game["particles/blob"],
            Color = Calc.HexToColor("7fffff"),
            Color2 = Calc.HexToColor("ffffff"),
            ColorMode = ParticleType.ColorModes.Fade,
            FadeMode = ParticleType.FadeModes.Late,
            Size = 0.2f,
            SizeRange = 0.05f,
            SpeedMin = 0.1f,
            SpeedMax = 2f,
            LifeMin = 0.03f,
            LifeMax = 0.06f,
            DirectionRange = (float)Math.PI * 2f
        };

        private static ParticleType GetExplodeFallback() => s_ExplodeFallback ??= new ParticleType
        {
            Source = GFX.Game["particles/blob"],
            Color = Calc.HexToColor("7fffff"),
            Color2 = Calc.HexToColor("ffffff"),
            ColorMode = ParticleType.ColorModes.Fade,
            FadeMode = ParticleType.FadeModes.Late,
            Size = 0.6f,
            SizeRange = 0.2f,
            SpeedMin = 25f,
            SpeedMax = 70f,
            LifeMin = 0.3f,
            LifeMax = 0.6f,
            DirectionRange = (float)Math.PI * 2f
        };

        private static ParticleType GetBreakFallback() => s_BreakFallback ??= new ParticleType
        {
            Color = Color.White,
            Color2 = Calc.HexToColor("aaaaaa"),
            ColorMode = ParticleType.ColorModes.Blink,
            FadeMode = ParticleType.FadeModes.Late,
            Size = 0.5f,
            SizeRange = 0.2f,
            SpeedMin = 25f,
            SpeedMax = 65f,
            Acceleration = Vector2.UnitY * 80f,
            LifeMin = 0.3f,
            LifeMax = 0.7f,
            DirectionRange = (float)Math.PI * 2f
        };

        // ── Configuration ─────────────────────────────────────────────────────────
        private readonly string spriteNormalPath;
        private readonly string spriteBrokenPath;
        private readonly float returnDelay;
        private readonly bool instantReturnInBounds;
        private readonly float maxDistance;
        private readonly string entityTypesRaw;
        private readonly HashSet<string> managedTypeNames = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<int> managedEntityIds = new HashSet<int>();
        private readonly Dictionary<Type, FieldInfo> entityIdFieldCache = new Dictionary<Type, FieldInfo>();
        private readonly List<Entity> managedEntities = new List<Entity>();
        private readonly bool breakable;
        private readonly float brokenDisableDuration;
        private readonly bool showReturnLine;

        private readonly string particleReturnAtlas;
        private readonly string particleExplodeAtlas;
        private readonly string particleBreakAtlas;
        private readonly string particleRepairAtlas;

        private readonly string soundTeleport;
        private readonly string soundBreak;
        private readonly string soundRepair;

        // Lazily resolved particle types
        private ParticleType? _ptReturnLine;
        private ParticleType? _ptExplode;
        private ParticleType? _ptBreak;
        private ParticleType? _ptRepair;

        private ParticleType PtReturnLine => _ptReturnLine ??= BuildParticleType(particleReturnAtlas, GetReturnLineFallback());
        private ParticleType PtExplode => _ptExplode ??= BuildParticleType(particleExplodeAtlas, GetExplodeFallback());
        private ParticleType PtBreak => _ptBreak ??= BuildParticleType(particleBreakAtlas, GetBreakFallback());
        private ParticleType PtRepair => _ptRepair ??= BuildParticleType(particleRepairAtlas, GetExplodeFallback());

        // ── Runtime state ─────────────────────────────────────────────────────────
        private Image spriteNormalImg;
        private Image spriteBrokenImg;

        private bool isBroken = false;
        private float brokenTimer = 0f;

        /// <summary>The entity this pedestal currently owns while resting on it.</summary>
        public Entity? ClaimedEntity { get; private set; } = null;

        // returnTimers[entity] = seconds remaining before that entity teleports back.
        // Shared across ALL pedestals via the first (authority) pedestal's dictionary.
        // Non-authority pedestals delegate everything to the authority.
        private readonly Dictionary<Entity, float> returnTimers = new Dictionary<Entity, float>();

        // ── Constructor ───────────────────────────────────────────────────────────
        public CustomPedestal(EntityData data, Vector2 offset)
            : base(data.Position + offset, 32f, 32f, safe: false)
        {
            spriteNormalPath = data.Attr("spriteNormal", "characters/theoCrystal/pedestal");
            spriteBrokenPath = data.Attr("spriteBroken", "characters/theoCrystal/pedestal");
            returnDelay = Math.Max(0f, data.Float("returnDelay", 0f));
            instantReturnInBounds = data.Bool("instantReturnInBounds", false);
            maxDistance = data.Float("maxDistance", 0f);
            entityTypesRaw = data.Attr("entityTypes", "TheoCrystal");
            breakable = data.Bool("breakable", false);
            brokenDisableDuration = data.Float("brokenDisableDuration", 0f);
            showReturnLine = data.Bool("showReturnLine", true);

            ParseManagedEntityFilters();

            particleReturnAtlas = data.Attr("particleReturn", "");
            particleExplodeAtlas = data.Attr("particleExplode", "");
            particleBreakAtlas = data.Attr("particleBreak", "");
            particleRepairAtlas = data.Attr("particleRepair", "");

            soundTeleport = data.Attr("soundTeleport", "event:/game/05_mirror_temple/crystaltheo_appear");
            soundBreak = data.Attr("soundBreak", "event:/game/05_mirror_temple/crystaltheo_break_free");
            soundRepair = data.Attr("soundRepair", "event:/game/09_core/iceblock_reappear");

            spriteNormalImg = new Image(GFX.Game[spriteNormalPath]);
            spriteNormalImg.JustifyOrigin(0.5f, 1f);
            Add(spriteNormalImg);

            spriteBrokenImg = new Image(GFX.Game[spriteBrokenPath]);
            spriteBrokenImg.JustifyOrigin(0.5f, 1f);
            spriteBrokenImg.Visible = false;
            Add(spriteBrokenImg);

            EnableAssistModeChecks = false;
            Depth = 8998;
            Collider.Position = new Vector2(-16f, -64f);
            Collidable = false;
            AllowStaticMovers = false;

            Tag = Tags.TransitionUpdate;
        }

        // ── Scene lifecycle ───────────────────────────────────────────────────────

        public override void Awake(Scene scene)
        {
            base.Awake(scene);

            RefreshManagedEntities();
            if (managedEntities.Count > 0)
                managedEntities[0].Depth = Depth + 1;
        }

        // ── Authority check ───────────────────────────────────────────────────────

        /// <summary>
        /// Returns true if this pedestal is the "authority" — the first non-broken
        /// one in scene order. Only the authority runs entity management logic each
        /// frame; all others just handle their own broken-repair timer and snap.
        /// </summary>
        private bool IsAuthority()
        {
            var all = Scene.Tracker.GetEntities<CustomPedestal>();
            // Scene.Tracker ordering matches EntityList insertion order.
            // We want the first pedestal in the list that is still in the scene.
            foreach (Entity e in all)
            {
                if (e is CustomPedestal p)
                    return p == this; // First one found = authority
            }
            return true;
        }

        // ── Update ────────────────────────────────────────────────────────────────

        public override void Update()
        {
            base.Update();

            TryBreakFromDash();

            // ── Broken repair countdown (every pedestal handles its own) ──────────
            if (isBroken)
            {
                if (brokenDisableDuration > 0f)
                {
                    brokenTimer -= Engine.DeltaTime;
                    if (brokenTimer <= 0f) Repair();
                }

                // Snap logic still runs even when broken: if we recover mid-frame,
                // claimed entity will be snapped next frame by authority.
                return;
            }

            // ── Non-authority pedestals: only snap their own claimed entity ───────
            if (!IsAuthority())
            {
                SnapClaimed();
                return;
            }

            // ── Authority: full entity management ────────────────────────────────
            RefreshManagedEntities();
            var candidates = managedEntities;

            foreach (var entity in candidates)
            {
                var holdable = entity.Get<Holdable>();
                if (holdable == null) continue;

                bool isHeld = holdable.Holder != null;

                if (isHeld)
                {
                    // Entity is being carried — cancel any pending return timer
                    // and release whichever pedestal currently claims it.
                    returnTimers.Remove(entity);
                    ReleaseClaim(entity);
                    continue;
                }

                // Track all matching, non-held entities continuously.
                // Skip entities already claimed by a non-broken pedestal or already queued.
                var claimingPedestal = FindClaimingPedestal(entity);
                if (claimingPedestal != null && !claimingPedestal.isBroken)
                    continue;

                if (returnTimers.ContainsKey(entity))
                    continue;

                var target = FindBestPedestal(entity, null);
                if (target == null) continue;

                float delay = returnDelay;

                // Instant-in-bounds override
                if (delay > 0f && instantReturnInBounds
                    && target.CollidePoint(entity.Center))
                    delay = 0f;

                // Max-distance check — skip return entirely if too far
                if (maxDistance > 0f
                    && Vector2.Distance(entity.Position, target.Position) > maxDistance)
                    continue;

                if (delay <= 0f)
                    TeleportEntityTo(entity, target);
                else
                    returnTimers[entity] = delay;
            }

            // ── Remove timers for entities that are no longer candidates ─────────
            // (e.g. left the room)
            var candidateSet = new HashSet<Entity>(candidates);
            var expired = new List<Entity>();

            // ── Process return timers ─────────────────────────────────────────────
            foreach (var kvp in returnTimers.ToList())
            {
                var entity = kvp.Key;

                if (!candidateSet.Contains(entity))
                {
                    expired.Add(entity);
                    continue;
                }

                float remaining = kvp.Value - Engine.DeltaTime;

                // Emit return-line particles
                if (showReturnLine)
                {
                    var lineTarget = FindBestPedestal(entity, null);
                    if (lineTarget != null)
                        EmitReturnLine(entity.Center, lineTarget);
                }

                if (remaining <= 0f)
                {
                    var target = ResolveConflict(entity);
                    if (target != null)
                        TeleportEntityTo(entity, target);
                    expired.Add(entity);
                }
                else
                {
                    returnTimers[entity] = remaining;
                }
            }
            foreach (var e in expired) returnTimers.Remove(e);

            // ── Snap this pedestal's own claimed entity ───────────────────────────
            SnapClaimed();
        }

        // ── Snap helper ───────────────────────────────────────────────────────────

        private void SnapClaimed()
        {
            if (ClaimedEntity == null || isBroken) return;

            var holdable = ClaimedEntity.Get<Holdable>();
            bool isHeld = holdable?.Holder != null;

            if (!isHeld && !returnTimers.ContainsKey(ClaimedEntity))
            {
                ClaimedEntity.Position = SnapPosition(this);
                (ClaimedEntity as Actor)?.ZeroRemainderX();
                (ClaimedEntity as Actor)?.ZeroRemainderY();
                TheoCrystalOnPedestalProp?.SetValue(ClaimedEntity, true);
            }
        }

        // ── Dash handler ──────────────────────────────────────────────────────────
        private void TryBreakFromDash()
        {
            if (!breakable || isBroken || Scene == null)
                return;

            Player player = Scene.Tracker.GetEntity<Player>();
            if (player == null)
                return;

            if (player.DashAttacking && CollideCheck(player))
            {
                Break(player.DashDir);
            }
        }
        private void Break(Vector2 direction)
        {
            isBroken = true;
            brokenTimer = brokenDisableDuration > 0f ? brokenDisableDuration : float.MaxValue;


            if (ClaimedEntity != null)
            {
                TheoCrystalOnPedestalProp?.SetValue(ClaimedEntity, false);

                const float baseDirectionMultiplier = 150f;
                const float verticalSpeedOffset = 0.1f;
                const float verticalSpeedMultiplier = 150f;

                if (ClaimedEntity is TheoCrystal theo)
                {
                    theo.Speed += direction * baseDirectionMultiplier;
                    theo.Speed.Y = (direction.Y - verticalSpeedOffset) * verticalSpeedMultiplier;
                }
                else
                {
                    // TODO: optimize this, this should be a one-time thing when we initially resolve entity types (and that way we can also null-check & field type check (Vector2) and skip this step for unsupported entities)
                    FieldInfo speedField = ClaimedEntity.GetType().GetField(
                        "Speed",
                        BindingFlags.Instance | BindingFlags.Public
                    );

                    if (speedField != null && speedField.FieldType == typeof(Vector2))
                    {
                        Vector2 speed = (Vector2)speedField.GetValue(ClaimedEntity)!;
                        speed += direction * baseDirectionMultiplier;
                        speed.Y = (direction.Y - verticalSpeedOffset) * verticalSpeedMultiplier;
                        speedField.SetValue(ClaimedEntity, speed);
                    }
                }

                ClaimedEntity = null;
            }

            spriteNormalImg.Visible = false;
            spriteBrokenImg.Visible = true;

            EmitParticleBurst(PtBreak, Center, 8);
            (Scene as Level)?.Flash(Color.White * 0.5f);
            Celeste.Freeze(0.05f);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
            Audio.Play(soundBreak, Position);
        }

        private void Repair()
        {
            isBroken = false;

            spriteNormalImg.Visible = true;
            spriteBrokenImg.Visible = false;

            EmitParticleBurst(PtRepair, Center, 8);
            Audio.Play(soundRepair, Position);
        }

        // ── Teleport ──────────────────────────────────────────────────────────────

        private void TeleportEntityTo(Entity entity, CustomPedestal target)
        {
            ReleaseClaim(entity);

            target.ClaimedEntity = entity;
            entity.Position = SnapPosition(target);

            (entity as Actor)?.ZeroRemainderX();
            (entity as Actor)?.ZeroRemainderY();

            TheoCrystalSpeedField?.SetValue(entity, Vector2.Zero);

            if (entity is not TheoCrystal)
            {
                var speedField = entity.GetType().GetField("Speed",
                    BindingFlags.Instance | BindingFlags.Public);
                speedField?.SetValue(entity, Vector2.Zero);
            }

            TheoCrystalOnPedestalProp?.SetValue(entity, true);

            EmitParticleBurst(PtExplode, entity.Center, 8);
            Audio.Play(soundTeleport, entity.Position);
        }

        private CustomPedestal? FindClaimingPedestal(Entity entity)
        {
            foreach (var ped in Scene.Tracker
                         .GetEntities<CustomPedestal>()
                         .Cast<CustomPedestal>())
            {
                if (ped.ClaimedEntity == entity)
                    return ped;
            }

            return null;
        }

        /// <summary>Removes this entity's claim from whichever pedestal holds it.</summary>
        private void ReleaseClaim(Entity entity)
        {
            var ped = FindClaimingPedestal(entity);
            if (ped != null)
                ped.ClaimedEntity = null;
        }

        // ── Pedestal selection ────────────────────────────────────────────────────

        private CustomPedestal? FindBestPedestal(Entity entity, CustomPedestal? exclude)
        {
            var all = Scene.Tracker
                .GetEntities<CustomPedestal>()
                .Cast<CustomPedestal>()
                .Where(p => !p.isBroken && p != exclude)
                .OrderBy(p => Vector2.DistanceSquared(entity.Position, p.Position))
                .ToList();

            // Prefer pedestal that already claims this entity
            foreach (var p in all)
                if (p.ClaimedEntity == entity) return p;

            // Nearest unclaimed within maxDistance
            foreach (var p in all)
            {
                if (p.ClaimedEntity != null) continue;
                if (maxDistance > 0f
                    && Vector2.Distance(entity.Position, p.Position) > maxDistance)
                    continue;
                return p;
            }

            return null;
        }

        private CustomPedestal? ResolveConflict(Entity entity)
        {
            var target = FindBestPedestal(entity, null);
            if (target == null) return null;

            float myTimer = returnTimers.TryGetValue(entity, out float mt) ? mt : 0f;

            foreach (var kvp in returnTimers)
            {
                if (kvp.Key == entity) continue;

                var theirTarget = FindBestPedestal(kvp.Key, null);
                if (theirTarget != target) continue;

                float theirTimer = kvp.Value;
                bool iWin;

                if (Math.Abs(myTimer - theirTimer) > 0.01f)
                    iWin = myTimer < theirTimer;
                else
                {
                    float myDist = Vector2.Distance(entity.Position, target.Position);
                    float theirDist = Vector2.Distance(kvp.Key.Position, target.Position);
                    if (Math.Abs(myDist - theirDist) > 1f)
                        iWin = myDist < theirDist;
                    else
                        iWin = entity.GetHashCode() > kvp.Key.GetHashCode();
                }

                if (!iWin)
                    return FindBestPedestal(entity, target);
            }

            return target;
        }

        // ── Entity collection ─────────────────────────────────────────────────────

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

        private void RefreshManagedEntities()
        {
            managedEntities.Clear();

            if (Scene == null)
                return;

            foreach (var e in Scene.Entities)
            {
                if (e.Get<Holdable>() == null)
                    continue;

                bool byType = managedTypeNames.Contains(e.GetType().Name);
                bool byId = false;

                if (!byType && managedEntityIds.Count > 0)
                {
                    var type = e.GetType();
                    if (!entityIdFieldCache.TryGetValue(type, out FieldInfo idField))
                    {
                        idField = type.GetField("entityID", BindingFlags.Instance | BindingFlags.NonPublic)
                            ?? type.GetField("ID", BindingFlags.Instance | BindingFlags.Public);
                        entityIdFieldCache[type] = idField;
                    }

                    if (idField?.GetValue(e) is EntityID eid)
                        byId = managedEntityIds.Contains(eid.ID);
                }

                if (byType || byId)
                    managedEntities.Add(e);
            }
        }

        // ── Visual helpers ────────────────────────────────────────────────────────

        private static Vector2 SnapPosition(CustomPedestal pedestal)
            => pedestal.Position + new Vector2(0f, -32f);

        // Emit one particle every other step (3 points instead of 6) for a lighter trail
        private void EmitReturnLine(Vector2 from, CustomPedestal target)
        {
            var particles = (Scene as Level)?.Particles;
            if (particles == null) return;

            var to = SnapPosition(target);
            var length = (to - from).Abs().Length();
            var steps = (int)MathF.Ceiling(length / 8); // 1 tile == 8 pixels

            for (int i = 0; i <= steps; i++)
            {
                var pos = Vector2.Lerp(from, to, i / (float)steps);
                particles.Emit(PtReturnLine, 1, pos, Vector2.One * 1.5f);
            }
        }

        private void EmitParticleBurst(ParticleType type, Vector2 pos, int count)
        {
            (Scene as Level)?.Particles.Emit(type, count, pos, Vector2.One * 4f);
        }

        private static ParticleType BuildParticleType(string atlasPath, ParticleType fallback)
        {
            if (string.IsNullOrWhiteSpace(atlasPath) || !GFX.Game.Has(atlasPath))
                return fallback;

            return new ParticleType(fallback)
            {
                Source = GFX.Game[atlasPath]
            };
        }
    }
}
