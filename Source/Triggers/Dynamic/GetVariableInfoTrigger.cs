using Celeste.Mod.Entities;
using DynamicInstructions;
using Microsoft.Xna.Framework;
using System;

namespace Celeste.Mod.BalintHelper.Triggers.Dynamic
{
    [CustomEntity(
        "BalintHelper/GetVariableInfoTrigger/LoadConstantInstruction"
        )]
    public class GetVariableInfoTrigger: GetThenActInstructionTrigger
    {
        public override object? ParseConstantValue(EntityData data)
        {
            var variableName = data.String("name") ?? throw new ArgumentException("no variable name was provided", nameof(data));
            var variableType = data.Enum("type", (Interpreter.VariableType)(-1));
            switch (variableType)
            {
                case Interpreter.VariableType.Local:
                case Interpreter.VariableType.Global:
                case Interpreter.VariableType.Argument:
                    return new Interpreter.VariableInfo(variableName, variableType);
                default:
                    throw new ArgumentException("invalid variable type", nameof(data));
            }
        }
        public GetVariableInfoTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
        }
    }
}
