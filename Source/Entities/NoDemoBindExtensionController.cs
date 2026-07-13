using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.BalintHelper.Source.Entities
{

    [CustomEntity("BalintHelper/SilentFloatingDebris")]
    public class NoDemoBindExtensionController: Entity
    {
        public NoDemoBindExtensionController(EntityData data, Vector2 offset)
            : base(offset)
        {


        }
    }
}
