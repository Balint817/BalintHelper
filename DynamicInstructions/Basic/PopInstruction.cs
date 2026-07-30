using DynamicInstructions.Abstract;

namespace DynamicInstructions.Basic
{
    public class PopInstruction : BaseInstruction
    {
        public override void Execute(Interpreter.MethodState state, List<BaseInstruction> instructions)
        {
            if (!state.Stack.TryPop(out var _))
            {
                throw new InvalidProgramException("Stack imbalance, failed to pop value");
            }
        }
    }
}
