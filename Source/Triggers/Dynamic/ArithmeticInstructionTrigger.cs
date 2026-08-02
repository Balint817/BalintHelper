using Celeste.Mod.Entities;
using DynamicInstructions.Instructions.Operators;
using Microsoft.Xna.Framework;
using System;

namespace Celeste.Mod.BalintHelper.Triggers.Dynamic
{
    [CustomEntity(
        "BalintHelper/ArithmeticInstructionTrigger"
        )]
    public class ArithmeticInstructionTrigger : BaseInstructionTrigger
    {
        public enum ArithmeticType
        {
            None,
            Add,
            Subtract,
            Multiply,
            Divide,
            Modulo,
            Increment,
            Decrement,
            Negate,
            BitwiseAnd,
            BitwiseOr,
            BitwiseXor,
            LeftShift,
            RightShift,
            Complement,
            Plus,
            Not,
            IndexFromEnd,
            IndexRange,
        }
        public ArithmeticInstructionTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            var type = data.Enum("type", ArithmeticType.None);
            InstructionType = type switch
            {
                ArithmeticType.Add => typeof(AddInstruction),
                ArithmeticType.Subtract => typeof(SubtractInstruction),
                ArithmeticType.Multiply => typeof(MultiplyInstruction),
                ArithmeticType.Divide => typeof(DivideInstruction),
                ArithmeticType.Modulo => typeof(ModuloInstruction),
                ArithmeticType.Increment => typeof(IncrementInstruction),
                ArithmeticType.Decrement => typeof(DecrementInstruction),
                ArithmeticType.Negate => typeof(NegateInstruction),
                ArithmeticType.BitwiseAnd => typeof(BitwiseAndInstruction),
                ArithmeticType.BitwiseOr => typeof(BitwiseOrInstruction),
                ArithmeticType.BitwiseXor => typeof(BitwiseXorInstruction),
                ArithmeticType.LeftShift => typeof(LeftShiftInstruction),
                ArithmeticType.RightShift => typeof(RightShiftInstruction),
                ArithmeticType.Complement => typeof(ComplementInstruction),
                ArithmeticType.Plus => typeof(PlusInstruction),
                ArithmeticType.Not => typeof(NotInstruction),
                ArithmeticType.IndexFromEnd => typeof(IndexFromEndInstruction),
                ArithmeticType.IndexRange => typeof(IndexRangeInstruction),
                _ => throw new ArgumentException("invalid arithmetic type", nameof(data)),
            };
        }
    }
}
