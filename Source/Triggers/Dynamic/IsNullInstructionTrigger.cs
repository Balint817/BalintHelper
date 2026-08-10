using Celeste.Mod.Entities;
using DynamicInstructions.Instructions.Abstract;
using DynamicInstructions.Instructions.Basic;
using DynamicInstructions.Instructions.Operators;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.BalintHelper.Triggers.Dynamic
{
    [CustomEntity(
        "BalintHelper/IsNullInstructionTrigger/NopInstruction"
        )]
    public class IsNullInstructionTrigger : CompoundInstructionTrigger
    {
        public override IEnumerable<BaseInstruction> GetCompoundInstructions()
        {
            return [new DupInstruction(), new LoadConstantInstruction(null), new EqualsInstruction()];
        }
        public IsNullInstructionTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {

        }
    }
}
