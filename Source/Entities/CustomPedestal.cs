using Celeste.Mod.BalintHelper.Utils;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Celeste.Mod.BalintHelper.Entities
{
    [CustomEntity("BalintHelper/CustomPedestal")]
    [Tracked]
    public class CustomPedestal : Solid
    {
        private const string DefaultReturnParticleColorA = "7fffff";
        private const string DefaultReturnParticleColorB = "ffffff";
        private const string DefaultExplodeParticleColorA = "7fffff";
        private const string DefaultExplodeParticleColorB = "ffffff";
        private const string DefaultBreakParticleColorA = "ffffff";
        private const string DefaultBreakParticleColorB = "aaaaaa";
        private const string DefaultRepairParticleColorA = "7fffff";
        private const string DefaultRepairParticleColorB = "ffffff";
        private const string DefaultGlowColor = "7fffff";

        private static readonly string[] RepairParticleAtlases = ["particles/smoke0", "particles/smoke1", "particles/smoke2", "particles/smoke3"];
        private static readonly string[] ExplodeParticleAtlases = ["particles/zappysmoke00", "particles/zappysmoke01", "particles/zappysmoke02", "particles/zappysmoke03"];

        private readonly string spriteNormalPath;
        private readonly string spriteBrokenPath;
        private readonly float returnDelay;
        private readonly bool instantReturnInBounds;
        private readonly float maxDistance;
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
        private readonly Color glowColor;

        private readonly string soundTeleport;
        private readonly string soundBreak;
        private readonly string soundRepair;

        private ParticleType? _ptReturnLine;
        private ParticleType? _ptExplode;
        private ParticleType? _ptBreak;
        private ParticleType? _ptRepair;

        private ParticleType PtReturnLine => _ptReturnLine ??= CreateReturnParticleType();
        private ParticleType PtExplode => _ptExplode ??= CreateExplodeParticleType();
        private ParticleType PtBreak => _ptBreak ??= CreateBreakParticleType();
        private ParticleType PtRepair => _ptRepair ??= CreateRepairParticleType();
        private sealed class ReturnTimerComponent : Component
        {
            public CustomPedestal Target;
            public float Remaining;

            public ReturnTimerComponent(CustomPedestal target, float remaining)
                : base(active: false, visible: false)
            {
                Target = target;
                Remaining = remaining;
            }
        }

        private static ReturnTimerComponent GetTimerComponent(Entity entity) => entity.Get<ReturnTimerComponent>();

        private static bool HasReturnTimer(Entity entity) => GetTimerComponent(entity) != null;

        private static void RemoveReturnTimer(Entity entity)
        {
            var c = GetTimerComponent(entity);
            if (c != null)
            {
                entity.Remove(c);
            }
        }

        private static void SetReturnTimer(Entity entity, CustomPedestal target, float remaining)
        {
            var c = GetTimerComponent(entity);
            if (c != null)
            {
                c.Target = target;
                c.Remaining = remaining;
            }
            else
            {
                entity.Add(new ReturnTimerComponent(target, remaining));
            }
        }

        private static bool TryGetReturnTarget(Entity entity, [MaybeNullWhen(false)] out CustomPedestal target)
        {
            var c = GetTimerComponent(entity);
            target = c?.Target;
            return c != null;
        }

        private static bool TryGetReturnRemaining(Entity entity, out float remaining)
        {
            var c = GetTimerComponent(entity);
            remaining = c?.Remaining ?? 0f;
            return c != null;
        }

        private readonly Image spriteNormalImg;
        private readonly Image spriteBrokenImg;

        private SilentFloatingDebris? explosionTrackerDebris;
        private bool hasPendingExplosionBreak;
        private Vector2 pendingExplosionFrom;

        private readonly bool startBroken;
        private bool isBroken = false;
        private bool isEnabled = true;
        private float brokenTimer = 0f;

        private readonly bool attachToSolid;
        private readonly bool applyLiftSpeed;
        private readonly StaticMover? _staticMover;
        private const float LiftSpeedGraceDuration = 10f / 60f; // ~0.1667s ~= 10 frames @ 60fps

        // "Repair,Entity,Grab,Explosion,Dash"
        private readonly bool triggerMoverOnRepair;
        private readonly bool triggerMoverOnEntityClaimed;
        private readonly bool triggerMoverOnGrab;
        private readonly bool triggerMoverOnExplosion;
        private readonly bool triggerMoverOnDash;


        private readonly List<KeyValuePair<float, Vector2>> _storedLiftSpeeds = [];
        private Vector2 AggregatedLiftSpeed
        {
            get
            {
                if (_storedLiftSpeeds.Count == 0)
                {
                    return Vector2.Zero;
                }
                var d = _storedLiftSpeeds.GroupBy(x => x.Key, x => x.Value).Select(x => x.Aggregate((acc, y) => acc + y)).ToArray();

                return d.Aggregate((acc, y) => acc + y) / d.Length;
            }
        }

        public Entity? ClaimedEntity { get; private set; } = null;

        private readonly bool canDash;
        private readonly bool canExplode;
        private readonly int dashRefillCount;
        private readonly bool refillStamina;
        private readonly float dashHitboxExtension;
        private readonly Hitbox dashCollider;
        private readonly Hitbox attachCheckCollider;
        private readonly bool cancelDash;

        private readonly bool canGrab;

        public CustomPedestal(EntityData data, Vector2 offset)
            : base(data.Position + offset, 32f, 32f, safe: false)
        {
            spriteNormalPath = data.Attr("spriteNormal", "characters/theoCrystal/pedestal");
            spriteBrokenPath = data.Attr("spriteBroken", "objects/pedestal/damaged");
            returnDelay = Math.Max(0f, data.Float("returnDelay", 2.0f));
            instantReturnInBounds = data.Bool("instantReturnInBounds", true);
            maxDistance = data.Float("maxDistance", 0f);
            Add(new EntityTypeFilterComponent(data.Attr("entityTypes", "TheoCrystal,ExtendedVariantMode/TheoCrystal")));
            breakable = data.Bool("breakable", false);
            brokenDisableDuration = data.Float("brokenDisableDuration", 5.0f);
            showReturnLine = data.Bool("showReturnLine", true);
            canDash = data.Bool("canDash", false);
            canExplode = data.Bool("canExplode", false);
            canGrab = data.Bool("canGrab", true);
            dashRefillCount = data.Int("dashRefillCount", 0);
            refillStamina = data.Bool("refillStamina", false);
            dashHitboxExtension = Math.Min(Math.Max(0f, data.Float("dashHitboxExtension", 0.5f)), 1f);
            cancelDash = data.Bool("cancelDash", false);

            returnParticleColorA = ReadColor(data, "returnParticleColorA", DefaultReturnParticleColorA);
            returnParticleColorB = ReadColor(data, "returnParticleColorB", DefaultReturnParticleColorB);
            explodeParticleColorA = ReadColor(data, "explodeParticleColorA", DefaultExplodeParticleColorA);
            explodeParticleColorB = ReadColor(data, "explodeParticleColorB", DefaultExplodeParticleColorB);
            breakParticleColorA = ReadColor(data, "breakParticleColorA", DefaultBreakParticleColorA);
            breakParticleColorB = ReadColor(data, "breakParticleColorB", DefaultBreakParticleColorB);
            repairParticleColorA = ReadColor(data, "repairParticleColorA", DefaultRepairParticleColorA);
            repairParticleColorB = ReadColor(data, "repairParticleColorB", DefaultRepairParticleColorB);
            glowColor = ReadColor(data, "glowColor", DefaultGlowColor);

            soundTeleport = data.Attr("soundTeleport", "event:/game/01_forsaken_city/birdbros_thrust");
            soundBreak = data.Attr("soundBreak", "event:/game/05_mirror_temple/crystaltheo_break_free");
            soundRepair = data.Attr("soundRepair", "event:/game/09_core/iceblock_reappear");

            startBroken = data.Bool("startBroken", false);
            attachToSolid = data.Bool("attachToSolid", false);
            applyLiftSpeed = data.Bool("applyLiftSpeed", true);

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

            attachCheckCollider = new Hitbox(
                32f,
                32 + 2,
                -16f,
                -32
            );

            void MoveAll(Vector2 amount)
            {
                Position += amount;
                ClaimedEntity?.Position += amount;
                explosionTrackerDebris?.Position += amount;
            }

            if (attachToSolid)
            {
                _staticMover = new StaticMover
                {
                    SolidChecker = solid =>
                    {
                        var orig = Collider;
                        Collider = attachCheckCollider;
                        bool result = CollideCheck(solid);
                        Collider = orig;
                        return result;
                    },
                    OnMove = amount =>
                    {
                        MoveAll(amount);

                        if (Engine.DeltaTime > 0f)
                        {
                            _storedLiftSpeeds.Add(new KeyValuePair<float, Vector2>(Engine.Scene.TimeActive, amount / Engine.DeltaTime));
                        }
                    },
                    OnShake = MoveAll,
                    OnEnable = () =>
                    {
                        Visible = true;
                        isEnabled = true;
                        explosionTrackerDebris?.Collidable = !isBroken;
                    },
                    OnDisable = () =>
                    {
                        EjectClaimedWithLiftSpeed();
                        hasPendingExplosionBreak = false;
                        Visible = false;
                        isEnabled = false;
                        explosionTrackerDebris?.Collidable = false;
                    },
                    OnDestroy = () =>
                    {
                        EjectClaimedWithLiftSpeed();
                        hasPendingExplosionBreak = false;
                        isEnabled = false;
                        Visible = false;
                        RemoveSelf();
                    }
                };
                Add(_staticMover);
            }

            dashCollider = new Hitbox(32f, 32f * (1f + dashHitboxExtension), -16f, -64f);

            var triggerMoverOn = data.Attr("triggerMoverOn", "").ToLowerInvariant().Split(",");
            foreach (var item in triggerMoverOn)
            {
                switch (item)
                {
                    case "repair":
                    case "repaired":
                        triggerMoverOnRepair = true;
                        break;
                    case "entity":
                    case "claim":
                    case "claimed":
                        triggerMoverOnEntityClaimed = true;
                        break;
                    case "grab":
                    case "grabbed":
                    case "release":
                    case "released":
                        triggerMoverOnGrab = true;
                        break;
                    case "explosion":
                    case "explode":
                    case "exploded":
                        triggerMoverOnExplosion = true;
                        break;
                    case "dash":
                    case "dashed":
                        triggerMoverOnDash = true;
                        break;
                    default:
                        break;
                }
            }


            Tag = Tags.TransitionUpdate;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            explosionTrackerDebris = new SilentFloatingDebris(Position + Collider.Position, (int)Width, (int)Height);
            explosionTrackerDebris.OnExploded += OnDebrisExploded;
            scene.Add(explosionTrackerDebris);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);

            if (breakable && startBroken)
            {
                ApplyBrokenState();
            }

            explosionTrackerDebris?.Collidable = !isBroken;

            foreach (var entity in GetHoldableMatches())
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
            hasPendingExplosionBreak = false;
            base.Removed(scene);
        }

        private bool IsAuthority() => GetAuthorityPedestal() == this;

        private CustomPedestal? GetAuthorityPedestal()
        {
            if (Scene == null)
            {
                return null;
            }

            CustomPedestal? fallback = null;
            foreach (var entity in Scene.Tracker.GetEntities<CustomPedestal>())
            {
                if (entity is not CustomPedestal pedestal)
                {
                    continue;
                }

                fallback ??= pedestal;
                if (!pedestal.isBroken && pedestal.isEnabled)
                {
                    return pedestal;
                }
            }

            return fallback;
        }
        public IEnumerable<Entity> GetHoldableMatches()
        {
            return Get<EntityTypeFilterComponent>().GetMatches().Where(x => x.Get<Holdable>() != null);
        }
        public override void Update()
        {
            base.Update();

            while (_storedLiftSpeeds.Count != 0 && _storedLiftSpeeds[0].Key < Engine.Scene.TimeActive - LiftSpeedGraceDuration)
            {
                _storedLiftSpeeds.RemoveAt(0);
            }

            explosionTrackerDebris?.Position = Position + Collider.Position;

            if (!isEnabled)
            {
                return;
            }

            if (hasPendingExplosionBreak && !isBroken && breakable && canExplode)
            {
                hasPendingExplosionBreak = false;
                Vector2 pushDirection = (SnapPosition(this) - pendingExplosionFrom).SafeNormalize(Vector2.UnitY);
                Break(pushDirection, 2.0f);
                if (triggerMoverOnExplosion)
                {
                    _staticMover?.TriggerWithRiders();
                }
            }

            if (isBroken)
            {
                if (brokenDisableDuration > 0f)
                {
                    brokenTimer -= Engine.DeltaTime;
                    if (brokenTimer <= 0f)
                    {
                        Repair();
                    }
                }
                return;
            }
            else
            {
                TryBreakFromDash();
            }

            if (!IsAuthority())
            {
                SnapClaimed();
                return;
            }

            var allPedestals = Scene.Tracker.GetEntities<CustomPedestal>().Cast<CustomPedestal>().ToList();

            // Scrub stale references
            foreach (var ped in allPedestals)
            {
                if (ped.ClaimedEntity.IsGone(Scene))
                {
                    ped.ClaimedEntity = null;
                }
            }

            var candidates = allPedestals.SelectMany(x => x.GetHoldableMatches()).Distinct().ToArray();

            var eligibleSet = new HashSet<Entity>();

            foreach (var entity in candidates)
            {
                var holdable = entity.Get<Holdable>();
                if (holdable == null)
                {
                    continue;
                }

                if (holdable.Holder != null)
                {
                    RemoveReturnTimer(entity);
                    ReleaseClaim(entity, true);
                    continue;
                }

                var claimingPedestal = FindClaimingPedestal(entity);
                if (claimingPedestal != null)
                {
                    if (claimingPedestal.isBroken || !claimingPedestal.isEnabled)
                    {
                        if (claimingPedestal.ClaimedEntity != null)
                        {
                            var claimedHoldable = claimingPedestal.ClaimedEntity.Get<Holdable>();
                            claimedHoldable.cannotHoldTimer = 0f;
                        }
                        claimingPedestal.ClaimedEntity = null;
                    }
                    else
                    {
                        RemoveReturnTimer(entity);
                        continue;
                    }
                }

                eligibleSet.Add(entity);
            }

            foreach (var entity in candidates)
            {
                if (HasReturnTimer(entity) && (!eligibleSet.Contains(entity) || entity.IsGone(Scene)))
                {
                    RemoveReturnTimer(entity);
                }
            }

            var assignments = AssignTargets(eligibleSet);

            foreach (var entity in eligibleSet)
            {
                if (!assignments.TryGetValue(entity, out var target))
                {
                    RemoveReturnTimer(entity);
                    continue;
                }

                var delay = GetTargetDelay(entity, target, out var instantInBounds);
                var targetChanged = !TryGetReturnTarget(entity, out var previousTarget) || previousTarget != target;

                if (delay <= 0f)
                {
                    TeleportEntityTo(entity, target, !instantInBounds);
                    continue;
                }

                if (targetChanged || !HasReturnTimer(entity))
                {
                    SetReturnTimer(entity, target, delay);
                }
            }

            var expired = new List<Entity>();
            foreach (var entity in candidates.Where(HasReturnTimer).ToList())
            {
                if (!assignments.TryGetValue(entity, out var timedTarget))
                {
                    expired.Add(entity);
                    continue;
                }

                GetTargetDelay(entity, timedTarget, out var instantInBounds);
                if (instantInBounds)
                {
                    TeleportEntityTo(entity, timedTarget, false);
                    expired.Add(entity);
                    continue;
                }

                if (timedTarget.showReturnLine)
                {
                    EmitReturnLine(entity.Center, timedTarget);
                }

                TryGetReturnRemaining(entity, out var currentRemaining);
                float remaining = currentRemaining - Engine.DeltaTime;
                if (remaining <= 0f)
                {
                    TeleportEntityTo(entity, timedTarget);
                    expired.Add(entity);
                }
                else
                {
                    SetReturnTimer(entity, timedTarget, remaining);
                }
            }

            foreach (var entity in expired)
            {
                RemoveReturnTimer(entity);
            }

            SnapClaimed();
        }

        private void SnapClaimed()
        {
            if (ClaimedEntity == null || isBroken)
            {
                return;
            }

            if (ClaimedEntity.IsGone(Scene))
            {
                ClaimedEntity = null;
                return;
            }

            var holdable = ClaimedEntity.Get<Holdable>();
            var isHeld = holdable?.Holder != null;

            if (!isHeld && !HasReturnTimer(ClaimedEntity))
            {
                ClaimedEntity.Position = SnapPosition(this);
                (ClaimedEntity as Actor)?.ZeroRemainderX();
                (ClaimedEntity as Actor)?.ZeroRemainderY();
            }

            if (!canGrab && holdable != null)
            {
                holdable.cannotHoldTimer = 0f;
            }
        }
        private void OnDebrisExploded(Vector2 from)
        {
            if (!isBroken && isEnabled && breakable && canExplode)
            {
                hasPendingExplosionBreak = true;
                pendingExplosionFrom = from;
            }
        }

        private void TryBreakFromDash()
        {
            if (!breakable || !canDash || isBroken || !isEnabled || Scene == null)
            {
                return;
            }

            var player = Scene.Tracker.GetEntity<Player>();
            if (player == null)
            {
                return;
            }

            if (player.DashAttacking && player.DashDir != Vector2.Zero)
            {
                // Temporarily swap to the extended dash collider for the check
                var origCollider = Collider;
                Collider = dashCollider;
                var hasCollided = CollideCheck(player);
                Collider = origCollider;

                if (hasCollided)
                {
                    Break(player.DashDir);
                    if (refillStamina)
                    {
                        player.RefillStamina();
                    }
                    player.Dashes = Math.Max(dashRefillCount, player.Dashes);
                    if (cancelDash)
                    {
                        player.CancelDash();
                    }
                    if (triggerMoverOnDash)
                    {
                        _staticMover?.TriggerWithRiders();
                    }
                }
            }
        }

        private void ResetReturnTimersForThisPedestal()
        {
            if (Scene == null)
            {
                return;
            }

            foreach (var entity in Scene.Entities.ToArray())
            {
                if (TryGetReturnTarget(entity, out var target) && target == this)
                {
                    RemoveReturnTimer(entity);
                }
            }
        }

        private void EjectClaimedWithLiftSpeed()
        {
            ResetReturnTimersForThisPedestal();

            if (ClaimedEntity == null)
            {
                return;
            }

            var entity = ClaimedEntity;
            ClaimedEntity = null;

            var holdable = entity.Get<Holdable>();
            holdable.cannotHoldTimer = 0f;

            var speed = Vector2.Zero;

            if (hasPendingExplosionBreak)
            {
                const float multiplier = 2;
                var baseDirectionMultiplier = 150f * multiplier;
                var verticalSpeedOffset = 0.1f;
                var verticalSpeedMultiplier = 150f * multiplier;

                var direction = pendingExplosionFrom;

                speed += direction * baseDirectionMultiplier;
                speed.Y = (direction.Y - verticalSpeedOffset) * verticalSpeedMultiplier;
            }

            if (applyLiftSpeed)
            {
                speed += AggregatedLiftSpeed;
            }

            if (speed != Vector2.Zero)
            {
                holdable.SetSpeed(speed);
            }
        }

        private void Break(Vector2 direction, float multiplier = 1f)
        {
            ApplyBrokenState();

            explosionTrackerDebris?.Collidable = false;

            if (ClaimedEntity != null)
            {
                var holdable = ClaimedEntity.Get<Holdable>();
                var baseDirectionMultiplier = 150f * multiplier;
                var verticalSpeedOffset = 0.1f;
                var verticalSpeedMultiplier = 150f * multiplier;

                Vector2 speed = Vector2.Zero;
                speed += direction * baseDirectionMultiplier;
                speed.Y = (direction.Y - verticalSpeedOffset) * verticalSpeedMultiplier;
                if (applyLiftSpeed)
                {
                    speed += AggregatedLiftSpeed;
                }

                holdable.SetSpeed(speed);
                holdable.cannotHoldTimer = 0f;

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

            explosionTrackerDebris?.Collidable = true;

            spriteNormalImg.Visible = true;
            spriteBrokenImg.Visible = false;

            EmitParticleFamilyBurst(PtRepair, RepairParticleAtlases, 20);
            Audio.Play(soundRepair, SnapPosition(this));
            if (triggerMoverOnRepair)
            {
                _staticMover?.TriggerWithRiders();
            }
        }

        private void TeleportEntityTo(Entity entity, CustomPedestal target, bool playEffects = true)
        {
            RemoveReturnTimer(entity);
            ReleaseClaim(entity);

            target.ClaimedEntity = entity;
            entity.Position = SnapPosition(target);
            var holdable = entity.Get<Holdable>();
            holdable.cannotHoldTimer = 0f;

            (entity as Actor)?.ZeroRemainderX();
            (entity as Actor)?.ZeroRemainderY();

            holdable.SetSpeed(Vector2.Zero);

            if (playEffects)
            {
                target.EmitParticleFamilyBurst(PtExplode, ExplodeParticleAtlases, 15);
                Audio.Play(soundTeleport, SnapPosition(target));
            }
            if (triggerMoverOnEntityClaimed)
            {
                target._staticMover?.TriggerWithRiders();
            }
        }

        private CustomPedestal? FindClaimingPedestal(Entity entity)
        {
            foreach (var ped in Scene.Tracker
                .GetEntities<CustomPedestal>()
                .Cast<CustomPedestal>())
            {
                if (ped.ClaimedEntity == entity)
                {
                    return ped;
                }
            }

            return null;
        }

        private void ReleaseClaim(Entity entity, bool staticMovers = false)
        {
            var ped = FindClaimingPedestal(entity);
            if (ped != null && ped.ClaimedEntity != null)
            {
                var holdable = ped.ClaimedEntity.Get<Holdable>();
                holdable.cannotHoldTimer = 0f;
                ped.ClaimedEntity = null;
                if (staticMovers && triggerMoverOnGrab)
                {
                    ped._staticMover?.TriggerWithRiders();
                }
            }
        }

        private Dictionary<Entity, CustomPedestal> AssignTargets(IEnumerable<Entity> entities)
        {
            var assignments = new Dictionary<Entity, CustomPedestal>();
            var pedestalOwners = new Dictionary<CustomPedestal, Entity>();

            foreach (var entity in entities.OrderBy(GetStableId))
            {
                TryAssignTarget(entity, assignments, pedestalOwners, []);
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
                {
                    continue;
                }

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
                {
                    continue;
                }

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

        private List<CustomPedestal> GetCandidatePedestals(Entity entity)
        {
            if (Scene == null)
            {
                return [];
            }

            return [.. Scene.Tracker
                .GetEntities<CustomPedestal>()
                .Cast<CustomPedestal>()
                .Where(p => CanTargetPedestal(entity, p))
                .OrderBy(p => Vector2.DistanceSquared(entity.Center, SnapPosition(p)))
                .ThenBy(GetStableId)];
        }

        private static bool CanTargetPedestal(Entity entity, CustomPedestal pedestal)
        {
            if (pedestal.isBroken || !pedestal.isEnabled)
            {
                return false;
            }

            // Ensure this specific pedestal actually wants this specific entity type
            if (!pedestal.WantsEntity(entity))
            {
                return false;
            }

            if (pedestal.ClaimedEntity != null && pedestal.ClaimedEntity != entity)
            {
                return false;
            }

            if (pedestal.maxDistance > 0f
                && Vector2.Distance(entity.Center, SnapPosition(pedestal)) > pedestal.maxDistance)
            {
                return false;
            }

            return true;
        }

        public bool WantsEntity(Entity entity)
        {
            var filter = Get<EntityTypeFilterComponent>();
            return filter.Matches(entity);
        }
        private static bool HasHigherPriority(Entity contender, Entity incumbent, CustomPedestal pedestal)
        {
            var contenderDistance = Vector2.DistanceSquared(contender.Center, SnapPosition(pedestal));
            var incumbentDistance = Vector2.DistanceSquared(incumbent.Center, SnapPosition(pedestal));
            if (Math.Abs(contenderDistance - incumbentDistance) > 1f)
            {
                return contenderDistance < incumbentDistance;
            }

            var contenderTimer = GetPriorityTimer(contender, pedestal);
            var incumbentTimer = GetPriorityTimer(incumbent, pedestal);
            if (Math.Abs(contenderTimer - incumbentTimer) > 0.01f)
            {
                return contenderTimer < incumbentTimer;
            }

            return GetStableId(contender) < GetStableId(incumbent);
        }

        private static float GetPriorityTimer(Entity entity, CustomPedestal pedestal)
        {
            if (TryGetReturnTarget(entity, out var currentTarget)
                && currentTarget == pedestal
                && TryGetReturnRemaining(entity, out var remaining))
            {
                return remaining;
            }

            return GetTargetDelay(entity, pedestal, out _);
        }

        private static float GetTargetDelay(Entity entity, CustomPedestal pedestal, out bool instantInBounds)
        {
            var delay = pedestal.returnDelay;
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

        private static Vector2 SnapPosition(CustomPedestal pedestal)
            => pedestal.Position + new Vector2(0f, -32f);

        private ParticleType CreateReturnParticleType() => new()
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

        private ParticleType CreateExplodeParticleType() => new()
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

        private ParticleType CreateBreakParticleType() => new()
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

        private ParticleType CreateRepairParticleType() => new()
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
            if (particles == null)
            {
                return;
            }

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

        public override void Render()
        {
            RenderNormalGlow();
            base.Render();
        }

        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);

            if (breakable && canDash)
            {
                var origCollider = Collider;
                Collider = dashCollider;

                dashCollider.Render(camera, Color.Goldenrod);

                Collider = origCollider;
            }
            if (attachToSolid)
            {
                var origCollider = Collider;
                Collider = attachCheckCollider;

                attachCheckCollider.Render(camera, Color.Aqua);

                Collider = origCollider;
            }
        }

        private static readonly Vector2[] GlowOffsets;
        static CustomPedestal()
        {
            const int glowSteps = 8;
            const float radius = 3f;
            GlowOffsets = new Vector2[glowSteps];
            for (int i = 0; i < glowSteps; i++)
            {
                var angle = (float)i / glowSteps * MathHelper.TwoPi;
                GlowOffsets[i] = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
            }
        }

        private void RenderNormalGlow()
        {
            if (!spriteNormalImg.Visible)
            {
                return;
            }

            // if not breakable with dash, or if breaking does nothing, don't glow
            if (!breakable || !canDash || (!refillStamina && dashRefillCount <= 0))
            {
                return;
            }

            var texture = GFX.Game[spriteNormalPath];
            var renderPos = Position - spriteNormalImg.Origin;

            var strength = (spriteNormalImg.Color.A / 255f) * 0.14f;
            var glowColorDimmed = glowColor * strength;

            foreach (Vector2 offset in GlowOffsets)
            {
                texture.Draw(renderPos + offset, Vector2.Zero, glowColorDimmed, 1f);
            }
        }

        private void EmitParticleFamilyBurst(ParticleType type, string[] atlases, int count)
        {
            var particles = (Scene as Level)?.Particles;
            if (particles == null || atlases.Length == 0)
            {
                return;
            }

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
                type.Source = GFX.Game[atlases[Calc.Random.Next(atlases.Length)]];
                var position = new Vector2(
                    Calc.Random.Range(source.Left, source.Right),
                    Calc.Random.Range(source.Top, source.Bottom));
                particles.Emit(type, position);
            }
        }
    }
}
