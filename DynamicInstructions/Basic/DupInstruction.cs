using DynamicInstructions.Abstract;

namespace DynamicInstructions.Basic
{
    public class DupInstruction : BaseInstruction
    {
        public override void Execute(Interpreter.MethodState state, List<BaseInstruction> instructions)
        {
            if (!state.Stack.TryPeek(out var value))
            {
                throw new InvalidProgramException("Stack imbalance, failed to duplicate value");
            }
            state.Stack.Push(value);
        }
    }
}
