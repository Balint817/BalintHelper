using Celeste.Mod.Entities;
using DynamicInstructions;
using Microsoft.Xna.Framework;
using System;

namespace Celeste.Mod.BalintHelper.Triggers.Dynamic
{
    [CustomEntity(
        "BalintHelper/GetDynamicMethodInfoTrigger/LoadConstantInstruction"
        )]
    public class GetDynamicMethodInfoTrigger : GetThenActInstructionTrigger
    {
        public override object? ParseConstantValue(EntityData data)
        {
            var methodName = data.String("name") ?? throw new ArgumentException("no method name was provided", nameof(data));
            return new Interpreter.DynamicMethodInfo(methodName);
        }
        public GetDynamicMethodInfoTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
        }
    }
}
