using Celeste.Mod.Entities;
using DynamicInstructions.Instructions.Abstract;
using Microsoft.Xna.Framework;
using System;

namespace Celeste.Mod.BalintHelper.Triggers.Dynamic
{
    [CustomEntity(
        "BalintHelper/ConditionalInstructionTrigger/ConditionalInstruction"
        )]
    public sealed class ConditionalInstructionTrigger : BaseInstructionTrigger
    {
        static readonly Type[] _params = [typeof(BaseInstruction)];
        public override object?[] GetConstructorParameters()
        {
            return [TruePathCompiled ?? throw new InvalidOperationException("TruePath is not set")];
        }
        public override Type[] ConstructorParameterTypes => _params;
        public BaseInstructionTrigger? TruePath { get; internal set; }
        public BaseInstruction? TruePathCompiled { get; internal set; }
        public ConditionalInstructionTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
        }
    }

}
