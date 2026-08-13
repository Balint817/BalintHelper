using Celeste.Mod.BalintHelper.Entities;
using System.Linq;

namespace Celeste.Mod.BalintHelper
{
    public static class SmokeAuraHooks
    {
        private const string LogTag = "BalintHelper/SmokeAuraHooks";

        public static void Load()
        {
            On.Celeste.Level.Render += OnLevelRender;
            Everest.Events.Level.OnLoadLevel += OnLoadLevel;
        }

        public static void Unload()
        {
            On.Celeste.Level.Render -= OnLevelRender;
            Everest.Events.Level.OnLoadLevel -= OnLoadLevel;
        }

        private static void OnLevelRender(On.Celeste.Level.orig_Render orig, Level self)
        {
            orig(self);
            SmokeAuraRenderer.CompositeForScene(self);
        }
        private static void OnLoadLevel(Level level, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            var existing = level.RendererList.Renderers.OfType<SmokeAuraRenderer>().FirstOrDefault();
            existing?.HardReset();
        }
    }
}
