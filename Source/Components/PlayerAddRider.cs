using Monocle;

namespace Celeste.Mod.BalintHelper.Components
{
    public class PlayerAddRider : Component
    {
        public readonly StaticMover TargetMover;
        public PlayerAddRider(StaticMover targetMover) : base(true, false)
        {
            TargetMover = targetMover;
        }

        public override void Update()
        {
            if (Used)
            {
                RemoveSelf();
            }
        }
        public bool Used { get; internal set; }
    }
}
