using Celeste.Mod;

namespace Celeste.Mod.BalintHelper
{
    public class BalintHelperModule : EverestModule
    {
        public static BalintHelperModule Instance { get; private set; }

        public BalintHelperModule()
        {
            Instance = this;
        }

        public override void Load() { }
        public override void Unload() { }
    }
}
