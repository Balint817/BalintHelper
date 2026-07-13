using Celeste.Mod.BalintHelper.Entities;
using Celeste.Mod.Registry;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.BalintHelper
{
    public class BalintHelperModule : EverestModule
    {
        public static BalintHelperModule Instance { get; private set; } = null!;
        public BalintHelperModule()
        {
            Instance = this;
        }

        public override void Load()
        {
            On.Celeste.FloatingDebris.OnExplode += OnFloatingDebrisExplode;

            EntityRegistry_SidToTypes = new((Dictionary<string, HashSet<Type>>)typeof(EntityRegistry).GetField("SidToTypes", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!);

            EntityRegistry_TypeToSids = new((Dictionary<Type, HashSet<string>>)typeof(EntityRegistry).GetField("TypeToSids", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!);

            On.Celeste.OuiOptions.Update += OuiOptions_Update;
        }
        public override void Unload()
        {
            On.Celeste.FloatingDebris.OnExplode -= OnFloatingDebrisExplode;
            On.Celeste.OuiOptions.Update -= OuiOptions_Update;
        }
        private void OuiOptions_Update(On.Celeste.OuiOptions.orig_Update orig, OuiOptions self)
        {
            orig(self);

            //TODO: add patch to EeveeHelper to block demo binds!
            //var keyboardKeys = Settings.Instance.Dash.Keyboard.Intersect(Settings.Instance.Down.Keyboard.Concat(Settings.Instance.DownMoveOnly.Keyboard)).ToHashSet();
            //var controllerKeys = Settings.Instance.Dash.Controller.Intersect(Settings.Instance.Down.Controller.Concat(Settings.Instance.DownMoveOnly.Controller)).ToHashSet();
            //var mouseKeys = Settings.Instance.Dash.Mouse.Intersect(Settings.Instance.Down.Mouse.Concat(Settings.Instance.DownMoveOnly.Mouse)).ToHashSet();
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
