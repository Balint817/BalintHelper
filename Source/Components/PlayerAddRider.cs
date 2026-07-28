using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
