using Celeste.Mod.BalintHelper.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.BalintHelper
{
    public class BalintHelperModule : EverestModule
    {
        public static BalintHelperModule Instance { get; private set; } = null!;
        private Func<string, IReadOnlySet<Type>> knownTypesFromSid = null!;
        private Func<Type, IReadOnlySet<string>> knownSidsFromType = null!;
        public BalintHelperModule()
        {
            Instance = this;
        }

        public override void Load()
        {
            On.Celeste.FloatingDebris.OnExplode += OnFloatingDebrisExplode;

            var entityRegistryType = AppDomain.CurrentDomain.GetAssemblies().Select(x => x.GetTypes().FirstOrDefault(x => x.FullName == "Celeste.Mod.Registry.EntityRegistry")).Where(x => x != null).First()!;

            var methodInfo = entityRegistryType.GetMethod("GetKnownTypesFromSid", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!;

            knownTypesFromSid = methodInfo.CreateDelegate<Func<string, IReadOnlySet<Type>>>();

            methodInfo = entityRegistryType.GetMethod("GetKnownSidsFromType", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!;

            knownSidsFromType = methodInfo.CreateDelegate<Func<Type, IReadOnlySet<string>>>();
        }
        public IReadOnlySet<Type> GetKnownTypesFromSid(string sid) => knownTypesFromSid(sid);
        public IReadOnlySet<string> GetKnownSidsFromType(Type type) => knownSidsFromType(type);
        public override void Unload()
        {
            On.Celeste.FloatingDebris.OnExplode -= OnFloatingDebrisExplode;
        }
        private void OnFloatingDebrisExplode(On.Celeste.FloatingDebris.orig_OnExplode orig, FloatingDebris self, Vector2 from)
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
