using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Runtime.InteropServices;

namespace Celeste.Mod.BalintHelper.Triggers.Dynamic
{
    [CustomEntity(
        "BalintHelper/SizeOfTrigger/LoadConstantInstruction"
        )]
    public class SizeOfTrigger : TypedConstantInstructionTrigger
    {
        protected override object? GetValueFromType(Type type)
        {
            return Marshal.SizeOf(type);
        }
        public SizeOfTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
        }
    }
}
