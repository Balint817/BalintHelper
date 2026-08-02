using Celeste.Mod.Entities;
using DynamicInstructions.Instructions.Operators;
using Microsoft.Xna.Framework;
using System;

namespace Celeste.Mod.BalintHelper.Triggers.Dynamic
{
    [CustomEntity(
        "BalintHelper/CompareInstructionTrigger"
        )]
    public class CompareInstructionTrigger : BaseInstructionTrigger
    {
        public enum CompareType
        {
            None,
            Equals,
            NotEquals,
            GreaterThan,
            GreaterThanOrEquals,
            LessThan,
            LessThanOrEquals,
        }
        public CompareInstructionTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            var type = data.Enum("type", CompareType.None);
            InstructionType = type switch
            {
                CompareType.Equals => typeof(EqualsInstruction),
                CompareType.NotEquals => typeof(NotEqualsInstruction),
                CompareType.GreaterThan => typeof(GreaterThanInstruction),
                CompareType.GreaterThanOrEquals => typeof(GreaterThanOrEqualsInstruction),
                CompareType.LessThan => typeof(LessThanInstruction),
                CompareType.LessThanOrEquals => typeof(LessThanOrEqualsInstruction),
                _ => throw new ArgumentException("invalid compare type", nameof(data)),
            };
        }
    }
}
