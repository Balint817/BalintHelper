using Celeste.Mod.BalintHelper.Components;
using Monocle;
using MonoMod.Utils;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Celeste.Mod.BalintHelper.Utils
{
    internal static class Extensions
    {
        public static T GetOrCreateTrackedSingleton<T>(this Scene scene) where T : Entity, new()
        {
            var instance = scene.Tracker.GetEntity<T>();
            if (instance == null)
            {
                instance = [];
                scene.Tracker.EntityAdded(instance);
                scene.Add(instance);
            }
            return instance;
        }
        public static void DuplicateCheck<T>(this T e, Scene scene) where T : Entity
        {
            var entities = scene.Tracker.GetEntities<T>();
            if (entities.Count > 2 || (entities.Count != 0 && entities.First() is { } existing && existing != e))
            {
                throw new InvalidOperationException($"attempted to add a new {typeof(T).Name} when one was already present!");
            }
        }
        public static int TypeDepth(this Type t)
        {
            int depth = 0;
            while (t.BaseType != null)
            {
                depth++;
                t = t.BaseType;
            }
            return depth;
        }
        public static bool IsInLookout(this Scene scene)
        {
            return scene?.Entities.FindFirst<Lookout.Hud>() != null;
        }

        public static void TrySetFlag(this Scene? scene, string? flag, bool value)
        {
            if (!string.IsNullOrEmpty(flag)
                && scene is Level level
                && level.Session is { } session)
            {
                session.SetFlag(flag, value);
            }
        }
        public static void TriggerWithRiders(this StaticMover staticMover)
        {
            if (staticMover is null)
            {
                return;
            }
            staticMover.TriggerPlatform();
            if (staticMover.Scene?.Tracker?.GetEntity<Player>() is not { } player)
            {
                return;
            }
            player.Add(new PlayerAddRider(staticMover));
        }
        public static bool IsGone(this Entity? entity, Scene scene)
        {
            return entity == null || entity.Scene == null || entity.Scene != scene;
        }
        public static bool TryGetSafe<T>(this DynamicData data, string fieldName, [MaybeNullWhen(false)] out T? value)
        {
            return data.TryGetSafeExtended(fieldName, out value) ?? false;
        }
        public static bool? TryGetSafeExtended<T>(this DynamicData data, string fieldName, [MaybeNullWhen(false)] out T? value)
        {
            value = default;
            if (data.TryGet(fieldName, out object? boxedValue))
            {
                if (boxedValue is T tValue)
                {
                    value = tValue;
                    return true;
                }
                // caller can handle type mismatch however they please.
                // if they want more control than this they should just use TryGet and handle it themselves.
                return null;
            }
            return false;
        }

        public static void CancelDash(this Player player)
        {
            if (player is null)
            {
                return;
            }
            player.StateMachine.State = Player.StNormal;
        }
    }
}
