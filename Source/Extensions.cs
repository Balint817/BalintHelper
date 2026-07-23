using Monocle;
using MonoMod.Utils;
using System.Diagnostics.CodeAnalysis;

namespace Celeste.Mod.BalintHelper
{
    internal static class Extensions
    {
        public static bool IsInLookout(this Scene scene)
        {
            return scene?.Entities.FindFirst<Lookout.Hud>() != null;
        }
        public static bool IsGone(this Entity? entity, Scene scene)
        {
            return entity == null || entity.Scene != scene;
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
