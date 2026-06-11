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
                BindingFlags.Instance | BindingFlags.Public);

        private static readonly FieldInfo TheoCrystalSpeedField =
            typeof(TheoCrystal).GetField("Speed",
                BindingFlags.Instance | BindingFlags.Public);

        // ── Built-in fallback particle types ─────────────────────────────────────
        // These are created lazily so GFX.Game is already loaded at that point.
        private static ParticleType s_ReturnLineFallback;
        private static ParticleType s_ExplodeFallback;
        private static ParticleType s_BreakFallback;

        private static ParticleType GetReturnLineFallback() => s_ReturnLineFallback ??= new ParticleType
        {
            Source = GFX.Game["particles/blob"],
            Color = Calc.HexToColor("7fffff"),
            Color2 = Calc.HexToColor("ffffff"),
            ColorMode = ParticleType.ColorModes.Fade,
            FadeMode = ParticleType.FadeModes.Late,
            Size = 0.5f,
            SizeRange = 0.25f,
            SpeedMin = 5f,
            SpeedMax = 20f,
            LifeMin = 0.3f,
            LifeMax = 0.6f,
            DirectionRange = (float)Math.PI * 2f
        };

        private static ParticleType GetExplodeFallback() => s_ExplodeFallback ??= new ParticleType
        {
            Source = GFX.Game["particles/blob"],
            Color = Calc.HexToColor("7fffff"),
            Color2 = Calc.HexToColor("ffffff"),
            ColorMode = ParticleType.ColorModes.Fade,
            FadeMode = ParticleType.FadeModes.Late,
            Size = 0.8f,
            SizeRange = 0.3f,
            SpeedMin = 40f,
            SpeedMax = 100f,
            LifeMin = 0.4f,
            LifeMax = 0.8f,
            DirectionRange = (float)Math.PI * 2f
        };

        // "Break" particles: white shards, no atlas particle needed – we define sizes directly.
        private static ParticleType GetBreakFallback() => s_BreakFallback ??= new ParticleType
        {
            Color = Color.White,
            Color2 = Calc.HexToColor("aaaaaa"),
            ColorMode = ParticleType.ColorModes.Blink,
            FadeMode = ParticleType.FadeModes.Late,
            Size = 0.6f,
            SizeRange = 0.3f,
            SpeedMin = 30f,
            SpeedMax = 80f,
            Acceleration = Vector2.UnitY * 80f,
            LifeMin = 0.4f,
            LifeMax = 0.9f,
            DirectionRange = (float)Math.PI * 2f
        };

        // ── Configuration ─────────────────────────────────────────────────────────
        private readonly string spriteNormalPath;
        private readonly string spriteBrokenPath;
        private readonly float returnDelay;
        private readonly bool instantReturnInBounds;
        private readonly float maxDistance;
        private readonly string entityTypesRaw;
        private readonly bool breakable;
        private readonly float brokenDisableDuration;
        private readonly bool showReturnLine;

        // Atlas paths for custom particle sprites (empty = use fallback)
        private readonly string particleReturnAtlas;
        private readonly string particleExplodeAtlas;
        private readonly string particleBreakAtlas;

        private readonly string soundTeleport;
        private readonly string soundBreak;
        private readonly string soundRepair;

        // Lazily built from atlas paths (or fallback) on first use
        private ParticleType _ptReturnLine;
        private ParticleType _ptExplode;
        private ParticleType _ptBreak;

        private ParticleType PtReturnLine => _ptReturnLine ??= BuildParticleType(particleReturnAtlas, GetReturnLineFallback());
        private ParticleType PtExplode => _ptExplode ??= BuildParticleType(particleExplodeAtlas, GetExplodeFallback());
        private ParticleType PtBreak => _ptBreak ??= BuildParticleType(particleBreakAtlas, GetBreakFallback());

        // ── Runtime state ─────────────────────────────────────────────────────────
        private Image spriteNormalImg;
        private Image spriteBrokenImg;

        private bool isBroken = false;
        private float brokenTimer = 0f;

        /// <summary>The entity this pedestal currently owns while resting on it.</summary>
        private Entity claimedEntity = null;

        // returnTimers[entity] = seconds remaining before that entity teleports back.
        private readonly Dictionary<Entity, float> returnTimers = new Dictionary<Entity, float>();

        // Previous-frame holder per entity (Player or null).
        // Stored as Entity? to avoid the CS0029 "cannot convert Player to Component" error:
        // Holdable.Holder is typed as Player, not Component.
        private readonly Dictionary<Entity, Player> prevHolder = new Dictionary<Entity, Player>();

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

            // Read atlas paths; empty string means "use built-in fallback"
            particleReturnAtlas = data.Attr("particleReturn", "");
            particleExplodeAtlas = data.Attr("particleExplode", "");
            particleBreakAtlas = data.Attr("particleBreak", "");

            soundTeleport = data.Attr("soundTeleport", "event:/game/05_mirror_temple/crystaltheo_appear");
            soundBreak = data.Attr("soundBreak", "event:/game/05_mirror_temple/crystaltheo_break_free");
            soundRepair = data.Attr("soundRepair", "event:/game/general/strawberry_get");

            // Sprites are added immediately; GFX.Game is available at ctor time.
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

            OnDashCollide = HandleDash;
            Tag = Tags.TransitionUpdate;
        }

        // ── Scene lifecycle ───────────────────────────────────────────────────────

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Collidable = breakable;

            var candidates = GetManagedEntities();
            if (candidates.Count > 0)
                candidates[0].Depth = Depth + 1;
        }

        // ── Update ────────────────────────────────────────────────────────────────

        public override void Update()
        {
            base.Update();

            // Broken repair countdown
            if (isBroken)
            {
                if (brokenDisableDuration > 0f)
                {
                    brokenTimer -= Engine.DeltaTime;
                    if (brokenTimer <= 0f) Repair();
                }
                return;
            }

            var candidates = GetManagedEntities();

            // ── Detect hold / release transitions ────────────────────────────────
            foreach (var entity in candidates)
            {
                var holdable = entity.Get<Holdable>();
                if (holdable == null) continue;

                // Holdable.Holder is Player (not Component), so we store Player?.
                Player currentHolder = holdable.Holder;
                prevHolder.TryGetValue(entity, out Player lastHolder);
                prevHolder[entity] = currentHolder;

                bool isHeld = currentHolder != null;
                bool wasHeld = lastHolder != null;

                if (isHeld)
                {
                    returnTimers.Remove(entity);
                    if (claimedEntity == entity) claimedEntity = null;
                    continue;
                }

                if (wasHeld && !isHeld)
                {
                    var target = FindBestPedestal(entity, null);
                    if (target == null) continue;

                    float delay = returnDelay;

                    if (delay > 0f && instantReturnInBounds
                        && target.CollidePoint(entity.Center))
                        delay = 0f;

                    if (maxDistance > 0f
                        && Vector2.Distance(entity.Position, target.Position) > maxDistance)
                        continue;

                    if (delay <= 0f)
                        TeleportEntityTo(entity, target);
                    else if (!returnTimers.ContainsKey(entity))
                        returnTimers[entity] = delay;
                }
            }

            // ── Process return timers ─────────────────────────────────────────────
            var expired = new List<Entity>();

            foreach (var kvp in returnTimers.ToList())
            {
                var entity = kvp.Key;

                if (!candidates.Contains(entity))
                {
                    expired.Add(entity);
                    continue;
                }

                float remaining = kvp.Value - Engine.DeltaTime;

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

            // ── Snap claimed entity to pedestal ───────────────────────────────────
            if (claimedEntity != null)
            {
                var holdable = claimedEntity.Get<Holdable>();
                bool isHeld = holdable?.Holder != null;
                if (!isHeld && !returnTimers.ContainsKey(claimedEntity))
                {
                    claimedEntity.Position = SnapPosition(this);
                    (claimedEntity as Actor)?.ZeroRemainderX();
                    (claimedEntity as Actor)?.ZeroRemainderY();
                    TheoCrystalOnPedestalProp?.SetValue(claimedEntity, true);
                }
            }
        }

        // ── Dash handler ──────────────────────────────────────────────────────────

        private DashCollisionResults HandleDash(Player player, Vector2 direction)
        {
            if (!breakable || isBroken) return DashCollisionResults.NormalCollision;

            isBroken = true;
            Collidable = false;
            brokenTimer = brokenDisableDuration > 0f ? brokenDisableDuration : float.MaxValue;

            if (claimedEntity != null)
            {
                TheoCrystalOnPedestalProp?.SetValue(claimedEntity, false);
                claimedEntity = null;
            }

            spriteNormalImg.Visible = false;
            spriteBrokenImg.Visible = true;

            EmitParticleBurst(PtBreak, Center, 14);
            (Scene as Level)?.Flash(Color.White * 0.5f);
            Celeste.Freeze(0.05f);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
            Audio.Play(soundBreak, Position);

            return DashCollisionResults.Rebound;
        }

        private void Repair()
        {
            isBroken = false;
            Collidable = breakable;

            spriteNormalImg.Visible = true;
            spriteBrokenImg.Visible = false;

            Audio.Play(soundRepair, Position);
        }

        // ── Teleport ──────────────────────────────────────────────────────────────

        private void TeleportEntityTo(Entity entity, CustomPedestal target)
        {
            foreach (var ped in Scene.Tracker
                         .GetEntities<CustomPedestal>()
                         .Cast<CustomPedestal>())
            {
                if (ped.claimedEntity == entity)
                    ped.claimedEntity = null;
            }

            target.claimedEntity = entity;
            entity.Position = SnapPosition(target);

            (entity as Actor)?.ZeroRemainderX();
            (entity as Actor)?.ZeroRemainderY();

            // Zero speed via reflection for TheoCrystal (public field)
            TheoCrystalSpeedField?.SetValue(entity, Vector2.Zero);

            // For other holdable types, also attempt a public Speed field
            if (entity is not TheoCrystal)
            {
                var speedField = entity.GetType().GetField("Speed",
                    BindingFlags.Instance | BindingFlags.Public);
                speedField?.SetValue(entity, Vector2.Zero);
            }

            TheoCrystalOnPedestalProp?.SetValue(entity, true);

            EmitParticleBurst(PtExplode, entity.Center, 16);
            Audio.Play(soundTeleport, entity.Position);
        }

        // ── Pedestal selection ────────────────────────────────────────────────────

        private CustomPedestal FindBestPedestal(Entity entity, CustomPedestal exclude)
        {
            var all = Scene.Tracker
                .GetEntities<CustomPedestal>()
                .Cast<CustomPedestal>()
                .Where(p => !p.isBroken && p != exclude)
                .OrderBy(p => Vector2.DistanceSquared(entity.Position, p.Position))
                .ToList();

            foreach (var p in all)
                if (p.claimedEntity == entity) return p;

            foreach (var p in all)
            {
                if (p.claimedEntity != null) continue;
                if (maxDistance > 0f
                    && Vector2.Distance(entity.Position, p.Position) > maxDistance)
                    continue;
                return p;
            }

            return null;
        }

        private CustomPedestal ResolveConflict(Entity entity)
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

        private List<Entity> GetManagedEntities()
        {
            var result = new List<Entity>();
            var tokens = entityTypesRaw
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var raw in tokens)
            {
                var token = raw.Trim();
                if (int.TryParse(token, out int entityId))
                {
                    foreach (var e in Scene.Entities)
                    {
                        if (e.Get<Holdable>() == null) continue;
                        var idField = e.GetType().GetField("entityID",
                            BindingFlags.Instance | BindingFlags.NonPublic)
                            ?? e.GetType().GetField("ID",
                            BindingFlags.Instance | BindingFlags.Public);
                        if (idField?.GetValue(e) is EntityID eid && eid.ID == entityId)
                            result.Add(e);
                    }
                }
                else
                {
                    foreach (var e in Scene.Entities)
                    {
                        if (e.GetType().Name == token
                            && e.Get<Holdable>() != null
                            && !result.Contains(e))
                            result.Add(e);
                    }
                }
            }

            return result;
        }

        // ── Visual helpers ────────────────────────────────────────────────────────

        private static Vector2 SnapPosition(CustomPedestal pedestal)
            => pedestal.Position + new Vector2(0f, -32f);

        private void EmitReturnLine(Vector2 from, CustomPedestal target)
        {
            var particles = (Scene as Level)?.Particles;
            if (particles == null) return;

            const int steps = 5;
            var to = SnapPosition(target);
            for (int i = 0; i <= steps; i++)
            {
                var pos = Vector2.Lerp(from, to, i / (float)steps);
                particles.Emit(PtReturnLine, 1, pos, Vector2.One * 2f);
            }
        }

        private void EmitParticleBurst(ParticleType type, Vector2 pos, int count)
        {
            (Scene as Level)?.Particles.Emit(type, count, pos, Vector2.One * 6f);
        }

        /// <summary>
        /// Builds a ParticleType that uses a custom atlas sprite when <paramref name="atlasPath"/>
        /// is non-empty and exists in GFX.Game, otherwise returns <paramref name="fallback"/>.
        /// The returned type copies all settings from <paramref name="fallback"/> and only
        /// overrides the Source texture, so speed/life/colour ranges stay consistent.
        /// </summary>
        private static ParticleType BuildParticleType(string atlasPath, ParticleType fallback)
        {
            if (string.IsNullOrWhiteSpace(atlasPath) || !GFX.Game.Has(atlasPath))
                return fallback;

            // Clone the fallback settings, swap only the source sprite.
            return new ParticleType(fallback)
            {
                Source = GFX.Game[atlasPath]
            };
        }
    }
}
