using Celeste.Mod;
using Celeste.Mod.BalintHelper.Source.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.BalintHelper
{
    public class BalintHelperModule : EverestModule
    {
#pragma warning disable CS8618
        public static BalintHelperModule Instance { get; private set; }
#pragma warning restore CS8618

        public BalintHelperModule()
        {
            Instance = this;
        }

        public override void Load()
        {
            On.Celeste.FloatingDebris.OnExplode += OnFloatingDebrisExplode;
        }
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
