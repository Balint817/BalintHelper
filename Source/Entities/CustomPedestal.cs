using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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
    /// </summary>
    [CustomEntity("BalintHelper/CustomPedestal")]
    [Tracked]
    public class CustomPedestal : Solid
    {
        // ── Particle defaults ─────────────────────────────────────────────────────
        private const string DefaultReturnParticleColorA = "7fffff";
        private const string DefaultReturnParticleColorB = "ffffff";
        private const string DefaultExplodeParticleColorA = "7fffff";
        private const string DefaultExplodeParticleColorB = "ffffff";
        private const string DefaultBreakParticleColorA = "ffffff";
        private const string DefaultBreakParticleColorB = "aaaaaa";
        private const string DefaultRepairParticleColorA = "7fffff";
        private const string DefaultRepairParticleColorB = "ffffff";

        private static readonly string[] RepairParticleAtlases = { "particles/smoke0", "particles/smoke1", "particles/smoke2", "particles/smoke3" };
        private static readonly string[] ExplodeParticleAtlases = { "particles/zappysmoke00", "particles/zappysmoke01", "particles/zappysmoke02", "particles/zappysmoke03" };

        // ── Configuration ─────────────────────────────────────────────────────────
        private readonly string spriteNormalPath;
        private readonly string spriteBrokenPath;
        private readonly float returnDelay;
        private readonly bool instantReturnInBounds;
        private readonly float maxDistance;
        private readonly string entityTypesRaw;
        private readonly HashSet<string> managedTypeNames = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<int> managedEntityIds = new HashSet<int>();
        private readonly List<Entity> managedEntities = new List<Entity>();
        private readonly Dictionary<Type, FieldInfo?> speedFieldInfos = new Dictionary<Type, FieldInfo?>();
        private readonly bool breakable;
        private readonly float brokenDisableDuration;
        private readonly bool showReturnLine;

        private readonly Color returnParticleColorA;
        private readonly Color returnParticleColorB;
        private readonly Color explodeParticleColorA;
        private readonly Color explodeParticleColorB;
        private readonly Color breakParticleColorA;
        private readonly Color breakParticleColorB;
        private readonly Color repairParticleColorA;
        private readonly Color repairParticleColorB;

        private readonly string soundTeleport;
        private readonly string soundBreak;
        private readonly string soundRepair;

        // Lazily resolved particle types
        private ParticleType? _ptReturnLine;
        private ParticleType? _ptExplode;
        private ParticleType? _ptBreak;
        private ParticleType? _ptRepair;

        private ParticleType PtReturnLine => _ptReturnLine ??= CreateReturnParticleType();
        private ParticleType PtExplode => _ptExplode ??= CreateExplodeParticleType();
        private ParticleType PtBreak => _ptBreak ??= CreateBreakParticleType();
        private ParticleType PtRepair => _ptRepair ??= CreateRepairParticleType();

        // ── Runtime state ─────────────────────────────────────────────────────────
        private sealed class SharedReturnState
        {
            public readonly Dictionary<Entity, float> ReturnTimers = new Dictionary<Entity, float>();
            public readonly Dictionary<Entity, CustomPedestal> ReturnTargets = new Dictionary<Entity, CustomPedestal>();
        }

        private static readonly ConditionalWeakTable<Scene, SharedReturnState> sharedReturnStates = new ConditionalWeakTable<Scene, SharedReturnState>();

        private Image spriteNormalImg;
        private Image spriteBrokenImg;

        private SilentFloatingDebris? explosionTrackerDebris; // Reference to tracked debris child

        private readonly bool startBroken;
        private bool isBroken = false;
        private float brokenTimer = 0f;

        /// <summary>The entity this pedestal currently owns while resting on it.</summary>
        public Entity? ClaimedEntity { get; private set; } = null;

        private readonly SharedReturnState detachedReturnState = new SharedReturnState();
        private Dictionary<Entity, float> returnTimers => GetSharedReturnState().ReturnTimers;
        private Dictionary<Entity, CustomPedestal> returnTargets => GetSharedReturnState().ReturnTargets;

        private readonly bool canDash;
        private readonly bool canExplode;

        private readonly bool canGrab;
        private static readonly FieldInfo HoldableCannotHoldTimer =
            typeof(Holdable).GetField("cannotHoldTimer",
                BindingFlags.Instance | BindingFlags.NonPublic)!;
        private static readonly FieldInfo HoldableCannotHoldDelay =
            typeof(Holdable).GetField("cannotHoldDelay",
                BindingFlags.Instance | BindingFlags.NonPublic)!;

        // ── Constructor ───────────────────────────────────────────────────────────
        public CustomPedestal(EntityData data, Vector2 offset)
            : base(data.Position + offset, 32f, 32f, safe: false)
        {
            spriteNormalPath = data.Attr("spriteNormal", "characters/theoCrystal/pedestal");
            spriteBrokenPath = data.Attr("spriteBroken", "objects/pedestal/damaged");
            returnDelay = Math.Max(0f, data.Float("returnDelay", 2.0f));
            instantReturnInBounds = data.Bool("instantReturnInBounds", true);
            maxDistance = data.Float("maxDistance", 0f);
            entityTypesRaw = data.Attr("entityTypes", "TheoCrystal");
            breakable = data.Bool("breakable", false);
            brokenDisableDuration = data.Float("brokenDisableDuration", 5.0f);
            showReturnLine = data.Bool("showReturnLine", true);
            canDash = data.Bool("canDash", false);
            canExplode = data.Bool("canExplode", false);
            canGrab = data.Bool("canGrab", true);

            ParseManagedEntityFilters();

            returnParticleColorA = ReadColor(data, "returnParticleColorA", DefaultReturnParticleColorA);
            returnParticleColorB = ReadColor(data, "returnParticleColorB", DefaultReturnParticleColorB);
            explodeParticleColorA = ReadColor(data, "explodeParticleColorA", DefaultExplodeParticleColorA);
            explodeParticleColorB = ReadColor(data, "explodeParticleColorB", DefaultExplodeParticleColorB);
            breakParticleColorA = ReadColor(data, "breakParticleColorA", DefaultBreakParticleColorA);
            breakParticleColorB = ReadColor(data, "breakParticleColorB", DefaultBreakParticleColorB);
            repairParticleColorA = ReadColor(data, "repairParticleColorA", DefaultRepairParticleColorA);
            repairParticleColorB = ReadColor(data, "repairParticleColorB", DefaultRepairParticleColorB);

            soundTeleport = data.Attr("soundTeleport", "event:/game/01_forsaken_city/birdbros_thrust");
            soundBreak = data.Attr("soundBreak", "event:/game/05_mirror_temple/crystaltheo_break_free");
            soundRepair = data.Attr("soundRepair", "event:/game/09_core/iceblock_reappear");

            startBroken = data.Bool("startBroken", false);

            var visibilityFlag = startBroken && breakable;

            spriteNormalImg = new Image(GFX.Game[spriteNormalPath]);
            spriteNormalImg.JustifyOrigin(0.5f, 1f);
            spriteNormalImg.Visible = !visibilityFlag;
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

        public override void Added(Scene scene)
        {
            base.Added(scene);

            // Align position & scale exactly with this entity's collider dimensions
            explosionTrackerDebris = new SilentFloatingDebris(Position + Collider.Position, (int)Width, (int)Height);
            explosionTrackerDebris.OnExploded += OnDebrisExploded;
            scene.Add(explosionTrackerDebris);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);

            if (breakable && startBroken)
                ApplyBrokenState();

            if (explosionTrackerDebris != null)
                explosionTrackerDebris.Collidable = !isBroken;

            RefreshManagedEntities();
            foreach (var entity in managedEntities)
            {
                if (entity.Depth <= Depth)
                {
                    entity.Depth = Depth + 1;
                }
            }
        }

        public override void Removed(Scene scene)
        {
            if (explosionTrackerDebris != null)
            {
                explosionTrackerDebris.OnExploded -= OnDebrisExploded;
                scene.Remove(explosionTrackerDebris);
                explosionTrackerDebris = null;
            }
            base.Removed(scene);
        }

        // ── Authority check ───────────────────────────────────────────────────────

        private SharedReturnState GetSharedReturnState()
            => Scene == null
                ? detachedReturnState
                : sharedReturnStates.GetValue(Scene, static _ => new SharedReturnState());

        private bool IsAuthority() => GetAuthorityPedestal() == this;

        private CustomPedestal? GetAuthorityPedestal()
        {
            if (Scene == null)
                return null;

            CustomPedestal? fallback = null;
            foreach (var entity in Scene.Tracker.GetEntities<CustomPedestal>())
            {
                if (entity is not CustomPedestal pedestal)
                    continue;

                fallback ??= pedestal;
                if (!pedestal.isBroken)
                    return pedestal;
            }

            return fallback;
        }

        // ── Update ────────────────────────────────────────────────────────────────

        public override void Update()
        {
            base.Update();

            // Continuously keep the debris bounds locked to the pedestal bounds
            if (explosionTrackerDebris != null)
            {
                explosionTrackerDebris.Position = Position + Collider.Position;
            }

            TryBreakFromDash();

            // ── Broken repair countdown (every pedestal handles its own) ──────────
            if (isBroken)
            {
                if (brokenDisableDuration > 0f)
                {
                    brokenTimer -= Engine.DeltaTime;
                    if (brokenTimer <= 0f) Repair();
                }
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
            var eligibleEntities = new List<Entity>();

            foreach (var entity in candidates)
            {
                var holdable = entity.Get<Holdable>();
                if (holdable == null)
                    continue;

                if (holdable.Holder != null)
                {
                    returnTimers.Remove(entity);
                    returnTargets.Remove(entity);
                    ReleaseClaim(entity);
                    continue;
                }

                var claimingPedestal = FindClaimingPedestal(entity);
                if (claimingPedestal != null)
                {
                    if (claimingPedestal.isBroken)
                    {
                        if (claimingPedestal.ClaimedEntity != null)
                        {
                            SetHoldableTimer(claimingPedestal.ClaimedEntity.Get<Holdable>(), 0);
                        }
                        claimingPedestal.ClaimedEntity = null;
                    }
                    else
                    {
                        returnTimers.Remove(entity);
                        returnTargets.Remove(entity);
                        continue;
                    }
                }

                eligibleEntities.Add(entity);
            }

            var eligibleSet = new HashSet<Entity>(eligibleEntities);
            foreach (var entity in returnTimers.Keys.ToList())
            {
                if (!eligibleSet.Contains(entity))
                {
                    returnTimers.Remove(entity);
                    returnTargets.Remove(entity);
                }
            }

            var assignments = AssignTargets(eligibleEntities);

            foreach (var entity in eligibleEntities)
            {
                if (!assignments.TryGetValue(entity, out var target))
                {
                    returnTimers.Remove(entity);
                    returnTargets.Remove(entity);
                    continue;
                }

                bool instantInBounds;
                float delay = GetTargetDelay(entity, target, out instantInBounds);
                bool targetChanged = !returnTargets.TryGetValue(entity, out var previousTarget) || previousTarget != target;

                if (delay <= 0f)
                {
                    TeleportEntityTo(entity, target, !instantInBounds);
                    continue;
                }

                if (targetChanged || !returnTimers.ContainsKey(entity))
                {
                    returnTargets[entity] = target;
                    returnTimers[entity] = delay;
                }
            }

            var expired = new List<Entity>();
            foreach (var kvp in returnTimers.ToList())
            {
                var entity = kvp.Key;
                if (!assignments.TryGetValue(entity, out var timedTarget))
                {
                    expired.Add(entity);
                    continue;
                }

                bool instantInBounds;
                GetTargetDelay(entity, timedTarget, out instantInBounds);
                if (instantInBounds)
                {
                    TeleportEntityTo(entity, timedTarget, false);
                    expired.Add(entity);
                    continue;
                }

                if (timedTarget.showReturnLine)
                    EmitReturnLine(entity.Center, timedTarget);

                float remaining = kvp.Value - Engine.DeltaTime;
                if (remaining <= 0f)
                {
                    TeleportEntityTo(entity, timedTarget);
                    expired.Add(entity);
                }
                else
                {
                    returnTimers[entity] = remaining;
                }
            }

            foreach (var entity in expired)
            {
                returnTimers.Remove(entity);
                returnTargets.Remove(entity);
            }

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
            }

            // dunno how this would be null here but just to be safe
            if (!canGrab && holdable != null)
            {
                SetHoldableTimer(holdable);
            }
        }

        private void SetHoldableTimer(Holdable? holdable, float delay)
        {
            if (holdable == null) return;
            HoldableCannotHoldTimer.SetValue(holdable, delay);
        }

        private void SetHoldableTimer(Holdable? holdable)
        {
            if (holdable == null) return;
            SetHoldableTimer(holdable, (float)HoldableCannotHoldDelay.GetValue(holdable)!);
        }

        // ── Break & Explosion Handlers ────────────────────────────────────────────

        private void OnDebrisExploded(Vector2 from)
        {
            if (!isBroken && breakable && canExplode)
            {
                // Calculate directional push vectors relative to explosion source center
                Vector2 pushDirection = (SnapPosition(this) - from).SafeNormalize(Vector2.UnitY);
                Break(pushDirection);
            }
        }

        private void TryBreakFromDash()
        {
            if (!breakable || !canDash || isBroken || Scene == null)
                return;

            Player player = Scene.Tracker.GetEntity<Player>();
            if (player == null)
                return;

            if (player.DashAttacking && player.DashDir != Vector2.Zero && CollideCheck(player))
            {
                Break(player.DashDir);
            }
        }
        private void Break(Vector2 direction)
        {
            ApplyBrokenState();

            if (explosionTrackerDebris != null)
                explosionTrackerDebris.Collidable = false;

            if (ClaimedEntity != null)
            {
                const float baseDirectionMultiplier = 150f;
                const float verticalSpeedOffset = 0.1f;
                const float verticalSpeedMultiplier = 150f;

                if (speedFieldInfos.TryGetValue(ClaimedEntity.GetType(), out var speedField))
                {
                    if (speedField != null && speedField.FieldType == typeof(Vector2))
                    {
                        Vector2 speed = (Vector2)speedField.GetValue(ClaimedEntity)!;
                        speed += direction * baseDirectionMultiplier;
                        speed.Y = (direction.Y - verticalSpeedOffset) * verticalSpeedMultiplier;
                        speedField.SetValue(ClaimedEntity, speed);
                    }
                }

                SetHoldableTimer(ClaimedEntity.Get<Holdable>(), 0);
                ClaimedEntity = null;
            }

            EmitParticleBurst(PtBreak, Center, 20);
            (Scene as Level)?.Flash(Color.White * 0.25f);
            Celeste.Freeze(0.05f);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
            Audio.Play(soundBreak, SnapPosition(this));
        }

        private void ApplyBrokenState()
        {
            isBroken = true;

            brokenTimer = brokenDisableDuration > 0f ? brokenDisableDuration : float.MaxValue;

            spriteNormalImg.Visible = false;
            spriteBrokenImg.Visible = true;
        }

        private void Repair()
        {
            isBroken = false;

            if (explosionTrackerDebris != null)
                explosionTrackerDebris.Collidable = true;

            spriteNormalImg.Visible = true;
            spriteBrokenImg.Visible = false;

            EmitParticleFamilyBurst(PtRepair, RepairParticleAtlases, 20);
            Audio.Play(soundRepair, SnapPosition(this));
        }

        // ── Teleport ──────────────────────────────────────────────────────────────

        private void TeleportEntityTo(Entity entity, CustomPedestal target, bool playEffects = true)
        {
            returnTimers.Remove(entity);
            returnTargets.Remove(entity);
            ReleaseClaim(entity);

            target.ClaimedEntity = entity;
            entity.Position = SnapPosition(target);
            SetHoldableTimer(target.Get<Holdable>(), 0);

            (entity as Actor)?.ZeroRemainderX();
            (entity as Actor)?.ZeroRemainderY();

            if (speedFieldInfos.TryGetValue(entity.GetType(), out var speedField))
            {
                speedField?.SetValue(entity, Vector2.Zero);
            }

            if (playEffects)
            {
                target.EmitParticleFamilyBurst(PtExplode, ExplodeParticleAtlases, 15);
                Audio.Play(soundTeleport, SnapPosition(target));
            }
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

        private void ReleaseClaim(Entity entity)
        {
            var ped = FindClaimingPedestal(entity);
            if (ped != null && ped.ClaimedEntity != null)
            {
                SetHoldableTimer(ped.ClaimedEntity.Get<Holdable>(), 0);
                ped.ClaimedEntity = null;
            }
        }

        // ── Pedestal selection ────────────────────────────────────────────────────

        private Dictionary<Entity, CustomPedestal> AssignTargets(IEnumerable<Entity> entities)
        {
            var assignments = new Dictionary<Entity, CustomPedestal>();
            var pedestalOwners = new Dictionary<CustomPedestal, Entity>();

            foreach (var entity in entities.OrderBy(GetStableId))
            {
                TryAssignTarget(entity, assignments, pedestalOwners, new HashSet<CustomPedestal>());
            }

            return assignments;
        }

        private bool TryAssignTarget(
            Entity entity,
            Dictionary<Entity, CustomPedestal> assignments,
            Dictionary<CustomPedestal, Entity> pedestalOwners,
            HashSet<CustomPedestal> excluded)
        {
            foreach (var pedestal in GetCandidatePedestals(entity))
            {
                if (excluded.Contains(pedestal))
                    continue;

                if (!pedestalOwners.TryGetValue(pedestal, out var currentOwner))
                {
                    pedestalOwners[pedestal] = entity;
                    assignments[entity] = pedestal;
                    return true;
                }

                if (currentOwner == entity)
                {
                    assignments[entity] = pedestal;
                    return true;
                }

                if (!HasHigherPriority(entity, currentOwner, pedestal))
                    continue;

                pedestalOwners[pedestal] = entity;
                assignments[entity] = pedestal;
                assignments.Remove(currentOwner);

                var nextExcluded = new HashSet<CustomPedestal>(excluded)
                {
                    pedestal
                };
                TryAssignTarget(currentOwner, assignments, pedestalOwners, nextExcluded);
                return true;
            }

            assignments.Remove(entity);
            return false;
        }

        private IEnumerable<CustomPedestal> GetCandidatePedestals(Entity entity)
        {
            if (Scene == null)
                return Enumerable.Empty<CustomPedestal>();

            return Scene.Tracker
                .GetEntities<CustomPedestal>()
                .Cast<CustomPedestal>()
                .Where(p => CanTargetPedestal(entity, p))
                .OrderBy(p => Vector2.DistanceSquared(entity.Center, SnapPosition(p)))
                .ThenBy(GetStableId)
                .ToList();
        }

        private bool CanTargetPedestal(Entity entity, CustomPedestal pedestal)
        {
            if (pedestal.isBroken)
                return false;

            if (pedestal.ClaimedEntity != null && pedestal.ClaimedEntity != entity)
                return false;

            if (pedestal.maxDistance > 0f
                && Vector2.Distance(entity.Center, SnapPosition(pedestal)) > pedestal.maxDistance)
                return false;

            return true;
        }

        private bool HasHigherPriority(Entity contender, Entity incumbent, CustomPedestal pedestal)
        {
            float contenderDistance = Vector2.DistanceSquared(contender.Center, SnapPosition(pedestal));
            float incumbentDistance = Vector2.DistanceSquared(incumbent.Center, SnapPosition(pedestal));
            if (Math.Abs(contenderDistance - incumbentDistance) > 1f)
                return contenderDistance < incumbentDistance;

            float contenderTimer = GetPriorityTimer(contender, pedestal);
            float incumbentTimer = GetPriorityTimer(incumbent, pedestal);
            if (Math.Abs(contenderTimer - incumbentTimer) > 0.01f)
                return contenderTimer < incumbentTimer;

            return GetStableId(contender) < GetStableId(incumbent);
        }

        private float GetPriorityTimer(Entity entity, CustomPedestal pedestal)
        {
            if (returnTargets.TryGetValue(entity, out var currentTarget)
                && currentTarget == pedestal
                && returnTimers.TryGetValue(entity, out var remaining))
            {
                return remaining;
            }

            return GetTargetDelay(entity, pedestal, out _);
        }

        private float GetTargetDelay(Entity entity, CustomPedestal pedestal, out bool instantInBounds)
        {
            float delay = pedestal.returnDelay;
            instantInBounds = false;

            if (delay > 0f && pedestal.instantReturnInBounds && pedestal.CollidePoint(entity.Center))
            {
                delay = 0f;
                instantInBounds = true;
            }

            return delay;
        }

        private static int GetStableId(Entity entity)
            => entity.SourceData?.ID ?? RuntimeHelpers.GetHashCode(entity);

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

                var type = e.GetType();
                bool byType = managedTypeNames.Contains(e.SourceData?.Name ?? "") || managedTypeNames.Contains(type.Name);
                bool byId = false;

                if (!byType && managedEntityIds.Count > 0)
                {
                    if (e.SourceData?.ID is int eid)
                        byId = managedEntityIds.Contains(eid);
                }

                if (byType || byId)
                {
                    if (!speedFieldInfos.TryGetValue(type, out var fi))
                    {
                        fi = type.GetField("Speed",
                            BindingFlags.Instance | BindingFlags.Public);
                        if (fi?.FieldType != typeof(Vector2))
                        {
                            fi = null;
                        }
                        speedFieldInfos[type] = fi;
                    }
                    if (fi != null)
                    {
                        managedEntities.Add(e);
                    }
                }
            }
        }

        // ── Visual helpers ────────────────────────────────────────────────────────

        private static Vector2 SnapPosition(CustomPedestal pedestal)
            => pedestal.Position + new Vector2(0f, -32f);

        private ParticleType CreateReturnParticleType() => new ParticleType
        {
            Source = GFX.Game["particles/blob"],
            Color = returnParticleColorA,
            Color2 = returnParticleColorB,
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

        private ParticleType CreateExplodeParticleType() => new ParticleType
        {
            Source = GFX.Game[ExplodeParticleAtlases[0]],
            Color = explodeParticleColorA,
            Color2 = explodeParticleColorB,
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

        private ParticleType CreateBreakParticleType() => new ParticleType
        {
            Source = GFX.Game["particles/shard"],
            Color = breakParticleColorA,
            Color2 = breakParticleColorB,
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

        private ParticleType CreateRepairParticleType() => new ParticleType
        {
            Source = GFX.Game[RepairParticleAtlases[0]],
            Color = repairParticleColorA,
            Color2 = repairParticleColorB,
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

        private static Color ReadColor(EntityData data, string key, string fallbackHex)
            => Calc.HexToColor(data.Attr(key, fallbackHex));

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

        private void EmitParticleFamilyBurst(ParticleType type, IReadOnlyList<string> atlases, int count)
        {
            var particles = (Scene as Level)?.Particles;
            if (particles == null || atlases.Count == 0)
                return;

            var sprite = spriteBrokenImg.Visible ? spriteBrokenImg : spriteNormalImg;

            const float areaScale = 0.66f;

            var width = sprite.Width * areaScale;
            var height = sprite.Height * areaScale;

            var source = new Rectangle(
                (int)(X - sprite.Origin.X + (sprite.Width - width) * 0.5f),
                (int)(Y - sprite.Origin.Y + (sprite.Height - height) * 0.5f),
                Math.Max(1, (int)width),
                Math.Max(1, (int)height));

            for (int i = 0; i < count; i++)
            {
                type.Source = GFX.Game[atlases[Calc.Random.Next(atlases.Count)]];
                var position = new Vector2(
                    Calc.Random.Range(source.Left, source.Right),
                    Calc.Random.Range(source.Top, source.Bottom));
                particles.Emit(type, position);
            }
        }
    }
}