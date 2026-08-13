using DynamicInstructions;
using DynamicInstructions.Instructions.Abstract;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.BalintHelper.Utils.Dynamic
{
    public class IsBetweenInstruction : BaseInstruction
    {
        public readonly bool TopInclusive;
        public readonly bool BottomInclusive;
        public IsBetweenInstruction(bool bottomInclusive = true, bool topInclusive = false)
        {
            BottomInclusive = bottomInclusive;
            TopInclusive = topInclusive;
        }
        public override void Execute(Interpreter.MethodState state, List<BaseInstruction> instructions)
        {
            if (!state.Stack.TryPop(out var maxBoxed) || !state.Stack.TryPop(out var minBoxed) || !state.Stack.TryPop(out var valueBoxed))
            {
                throw new InvalidProgramException("stack imbalance, failed to get values to compare for (min <= value <= max)");
            }
            if (maxBoxed is null || minBoxed is null || valueBoxed is null)
            {
                throw new ArgumentException("Cannot compare null values for (min <= value <= max)");
            }
            dynamic max = maxBoxed;
            dynamic min = minBoxed;
            dynamic value = valueBoxed;

            bool result = (BottomInclusive ? value >= min : value > min) && (TopInclusive ? value <= max : value < max);
            state.Stack.Push(result);
        }
    }
}
