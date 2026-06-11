using Celeste.Mod;

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

        public override void Load() { }
        public override void Unload() { }
    }
}
