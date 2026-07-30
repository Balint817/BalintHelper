using DynamicInstructions.Abstract;

namespace DynamicInstructions.Arrays
{
    public class ArrayVectorLengthInstruction : BaseInstruction
    {
        public override void Execute(Interpreter.MethodState state, List<BaseInstruction> instructions)
        {
            if (!state.Stack.TryPop(out var arrayBoxed))
            {
                throw new InvalidProgramException("stack imbalance, failed to get array for length");
            }
            if (arrayBoxed is not Array array)
            {
                throw new InvalidProgramException("type mismatch, expected array for length");
            }
            state.Stack.Push(array.Length);
        }
    }
}
