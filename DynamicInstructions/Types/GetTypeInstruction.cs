using DynamicInstructions.Abstract;

namespace DynamicInstructions.Types
{
    public class GetTypeInstruction : BaseInstruction
    {
        public override void Execute(Interpreter.MethodState state, List<BaseInstruction> instructions)
        {
            if (!state.Stack.TryPop(out var value))
            {
                throw new InvalidProgramException("Stack imbalance, failed to get type of value");
            }
            if (value is null)
            {
                throw new InvalidProgramException("Cannot get type of 'null'");
            }
            state.Stack.Push(value.GetType());
        }
    }
}
