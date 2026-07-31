using DynamicInstructions.Instructions.Abstract;

namespace DynamicInstructions.Instructions.Arrays
{
    public class ArrayRankInstruction : BaseInstruction
    {
        public override void Execute(Interpreter.MethodState state, List<BaseInstruction> instructions)
        {
            if (!state.Stack.TryPop(out var arrayBoxed))
            {
                throw new InvalidProgramException("stack imbalance, failed to get array for rank");
            }
            if (arrayBoxed is not Array array)
            {
                throw new InvalidProgramException("type mismatch, expected array for rank");
            }
            state.Stack.Push(array.Rank);
        }
    }
}
