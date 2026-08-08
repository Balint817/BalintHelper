using Celeste.Mod.BalintHelper.Components;
using Celeste.Mod.BalintHelper.Entities;
using Celeste.Mod.BalintHelper.Triggers;
using Celeste.Mod.BalintHelper.Utils.Dynamic;
using Celeste.Mod.Registry;
using DynamicInstructions.Instructions;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.BalintHelper
{
    public class BalintHelperModule : EverestModule
    {
        public static BalintHelperModule Instance { get; private set; } = null!;
        public override void Load()
        {
            Instance = this;

            EntityRegistry_SidToTypes = new((Dictionary<string, HashSet<Type>>)typeof(EntityRegistry).GetField("SidToTypes", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!);
            EntityRegistry_TypeToSids = new((Dictionary<Type, HashSet<string>>)typeof(EntityRegistry).GetField("TypeToSids", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!);

            On.Celeste.FloatingDebris.OnExplode += OnFloatingDebrisExplode;
            On.Celeste.OuiOptions.Update += OuiOptions_Update;

            IL.Celeste.Player.OnCollideV += PatchOnCollideV;
            IL.Celeste.Player.OnCollideH += PatchOnCollideH;

            Everest.Events.CustomBirdTutorial.OnParseCommand += CustomBirdTutorial_OnParseCommand;

            On.Celeste.Player.Pickup += Player_Pickup;

            On.Celeste.Solid.GetPlayerRider += Solid_GetPlayerRider;

            foreach (var item in CustomInstructionValueHandler.AllInstances)
            {
                item.Load();
            }

            TypeNameCodec.ResolveAmbiguousType += AmbiguousTypeResolver;
        }

        public override void Unload()
        {
            On.Celeste.FloatingDebris.OnExplode -= OnFloatingDebrisExplode;
            On.Celeste.OuiOptions.Update -= OuiOptions_Update;

            IL.Celeste.Player.OnCollideV -= PatchOnCollideV;
            IL.Celeste.Player.OnCollideH -= PatchOnCollideH;

            Everest.Events.CustomBirdTutorial.OnParseCommand -= CustomBirdTutorial_OnParseCommand;

            On.Celeste.Player.Pickup -= Player_Pickup;

            On.Celeste.Solid.GetPlayerRider -= Solid_GetPlayerRider;

            foreach (var item in CustomInstructionValueHandler.AllInstances)
            {
                item.Unload();
            }

            TypeNameCodec.ResolveAmbiguousType -= AmbiguousTypeResolver;

            Instance = null!;
        }
        private bool AmbiguousTypeResolver(string typeName, IEnumerable<Type> matches, [MaybeNullWhen(false)] out Type resolvedType)
        {
            var sorted = matches.OrderBy(t => t.FullName!.Length).ToArray();
            resolvedType = sorted[0];
            var excludeNames = new HashSet<string>()
            {
                "On." + resolvedType.FullName,
                "IL." + resolvedType.FullName,
            };
            foreach (var item in sorted[1..])
            {
                if (!excludeNames.Contains(item.FullName!))
                {
                    return false;
                }
            }
            return true;
        }
        private static Player Solid_GetPlayerRider(On.Celeste.Solid.orig_GetPlayerRider orig, Solid self)
        {
            if (self.Scene?.Tracker?.GetEntity<Player>() is not { } player
                || player.Get<PlayerAddRider>() is not { } component
                || component.TargetMover.Platform != self)
            {
                return orig(self);
            }
            component.Used = true;
            return player;
        }

        private static bool Player_Pickup(On.Celeste.Player.orig_Pickup orig, Player self, Holdable pickup)
        {
            if (self.Scene is not { } scene || HoldablePriorityController.GetOrCreate(scene) is not { } controller)
            {
                return orig(self, pickup);
            }
            var target = controller.GetTarget(self);
            if (target == null)
            {
                // this branch might need some performance checks?
                return orig(self, pickup);
            }
            return orig(self, target);
        }
        private static void OuiOptions_Update(On.Celeste.OuiOptions.orig_Update orig, OuiOptions self)
        {
            orig(self);

            //TODO: add patch to EeveeHelper to block demo binds!
            //var keyboardKeys = Settings.Instance.Dash.Keyboard.Intersect(Settings.Instance.Down.Keyboard.Concat(Settings.Instance.DownMoveOnly.Keyboard)).ToHashSet();
            //var controllerKeys = Settings.Instance.Dash.Controller.Intersect(Settings.Instance.Down.Controller.Concat(Settings.Instance.DownMoveOnly.Controller)).ToHashSet();
            //var mouseKeys = Settings.Instance.Dash.Mouse.Intersect(Settings.Instance.Down.Mouse.Concat(Settings.Instance.DownMoveOnly.Mouse)).ToHashSet();
        }
        private MTexture CustomBirdTutorial_OnParseCommand(string command)
        {
            if (command == "GroundedUltra")
            {
                return GFX.Gui["tech/BalintHelper/grounded_ultra"];
            }
            return null!;
        }

        private static void PatchOnCollideV(ILContext il) => PatchCanCurveDashAssignment(il);
        private static void PatchOnCollideH(ILContext il) => PatchCanCurveDashAssignment(il);
        private static void PatchCanCurveDashAssignment(ILContext il)
        {
            // Starting point:
            // ```
            // this.canCurveDash = false;
            // ```
            var cursor = new ILCursor(il);

            if (!cursor.TryGotoNext(
                MoveType.Before,
                i => i.MatchLdarg(0),
                i => i.MatchLdcI4(0),
                i => i.MatchStfld<Player>("canCurveDash")
            ))
            {
                throw new Exception("Couldn't find canCurveDash assignment in " + il.Method.FullName);
            }

            // We're now positioned before:
            // ldarg.0
            // ldc.i4.0
            // stfld Player::canCurveDash

            ILLabel skipAssign = cursor.DefineLabel();

            // Mark the instruction immediately after the stfld as the branch target.
            cursor.Index += 3;
            cursor.MarkLabel(skipAssign);

            // Go back before the assignment and inject:
            // ldarg.0
            // call AllowCurveCheck
            // brtrue skipAssign
            cursor.Index -= 3;
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate(AllowCurveCheck);
            cursor.Emit(OpCodes.Brtrue, skipAssign);

            // End result:
            // ```
            // if (!AllowCurveCheck(this)) {
            //   this.canCurveDash = false;
            // }
            // ```
        }
        private static bool AllowCurveCheck(Player self)
        {
            var entities = self.Scene?.Tracker?.Entities;
            if (entities is null)
            {
                return false;
            }
            return entities[typeof(AllowCurveOnCollideTrigger)].Cast<AllowCurveOnCollideTrigger>().Any(x => x.IsTriggered);

        }
        public ReadOnlyDictionary<string, HashSet<Type>> EntityRegistry_SidToTypes { get; private set; } = null!;
        public ReadOnlyDictionary<Type, HashSet<string>> EntityRegistry_TypeToSids { get; private set; } = null!;
        private static void OnFloatingDebrisExplode(On.Celeste.FloatingDebris.orig_OnExplode orig, FloatingDebris self, Vector2 from)
        {
            if (self is SilentFloatingDebris silentDebris)
            {
                silentDebris.TriggerExplodeEvent(from);
            }
            else
            {
                orig(self, from);
            }
        }
    }
}
