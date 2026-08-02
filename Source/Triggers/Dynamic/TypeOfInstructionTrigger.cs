using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System;

namespace Celeste.Mod.BalintHelper.Triggers.Dynamic
{
    [CustomEntity(
        "BalintHelper/TypeOfInstructionTrigger/LoadConstantInstruction"
        )]
    public class TypeOfInstructionTrigger : TypedConstantInstructionTrigger
    {
        protected override object? GetValueFromType(Type type)
        {
            return type;
        }
        public TypeOfInstructionTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
        }
    }
}
