using Celeste.Mod.BalintHelper.Entities.Dynamic;
using Celeste.Mod.Entities;
using DynamicInstructions;
using Microsoft.Xna.Framework;
using System;

namespace Celeste.Mod.BalintHelper.Triggers.Dynamic
{
    [CustomEntity(
        "BalintHelper/GetVariableInfoTrigger/LoadConstantInstruction"
        )]
    public class GetVariableInfoTrigger : GetThenActInstructionTrigger
    {
        public enum VariableType
        {
            Local,
            Global,
            Argument,
            Static
        }
        public override object? ParseConstantValue(EntityData data)
        {
            var variableName = data.String("name") ?? throw new ArgumentException("no variable name was provided", nameof(data));
            var variableType = data.Enum("type", (VariableType)(-1));
            switch (variableType)
            {
                case VariableType.Local:
                case VariableType.Global:
                case VariableType.Argument:
                    return new Interpreter.VariableInfo(variableName, (Interpreter.VariableType)variableType);
                case VariableType.Static:
                    return new StaticVariableController.Info(variableName);
                default:
                    throw new ArgumentException("invalid variable type", nameof(data));
            }
        }
        public GetVariableInfoTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
        }
    }
}
