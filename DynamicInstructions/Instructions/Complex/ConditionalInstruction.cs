using DynamicInstructions.Instructions.Abstract;
using DynamicInstructions.Instructions.Basic;

namespace DynamicInstructions.Instructions.Complex
{
    public class ConditionalInstruction : NopInstruction
    {
        public readonly BaseInstruction TruePath;
        public override void Execute(Interpreter.MethodState state, List<BaseInstruction> instructions)
        {
            var nextInstruction = EvaluateCondition(state);
            if (nextInstruction is null)
            {
                return;
            }
            var instructionIdx = instructions.IndexOf(nextInstruction);
            if (instructionIdx == -1)
            {
                throw new InvalidProgramException("critical error: conditional branch nextInstruction pointed outside the current method");
            }
            state.Cursor = instructionIdx - 1;
        }

        protected virtual BaseInstruction? EvaluateCondition(Interpreter.MethodState state)
        {
            if (!state.Stack.TryPop(out var value))
            {
                throw new InvalidProgramException("Stack imbalance, failed to evaluate conditional branch");
            }
            if (value is bool flag)
            {
                return flag ? TruePath : null;
            }
            try
            {
                if (value is null || (value.GetType().IsValueType && (dynamic)value == 0))
                {
                    return null;
                }
                return TruePath;
            }
            catch (Exception)
            {
                throw new InvalidProgramException("Unsupported type in conditional branch");
            }
        }

        public ConditionalInstruction(BaseInstruction truePath)
        {
            TruePath = truePath;
        }
    }
}
