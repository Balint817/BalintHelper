using DynamicInstructions.Instructions.Abstract;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Celeste.Mod.BalintHelper.Triggers.Dynamic
{
    public abstract class CompoundInstructionTrigger : BaseInstructionTrigger
    {
        public abstract IEnumerable<BaseInstruction> GetCompoundInstructions();
        public CompoundInstructionTrigger(EntityData data, Vector2 offset) : base(data, offset) { }
    }
}
