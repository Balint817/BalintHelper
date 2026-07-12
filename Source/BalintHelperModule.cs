using Celeste.Mod.BalintHelper.Entities;
using Celeste.Mod.Registry;
using Microsoft.Xna.Framework;
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
        }
        public ReadOnlyDictionary<string, HashSet<Type>> EntityRegistry_SidToTypes { get; private set; } = null!;
        public ReadOnlyDictionary<Type, HashSet<string>> EntityRegistry_TypeToSids { get; private set; } = null!;
        public override void Unload()
        {
            On.Celeste.FloatingDebris.OnExplode -= OnFloatingDebrisExplode;
        }
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
