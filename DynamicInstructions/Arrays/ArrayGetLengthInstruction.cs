using DynamicInstructions.Abstract;

namespace DynamicInstructions.Arrays
{
    public class ArrayGetLengthInstruction : BaseInstruction
    {
        public override void Execute(Interpreter.MethodState state, List<BaseInstruction> instructions)
        {
            if (!state.Stack.TryPop(out var rankBoxed) || !state.Stack.TryPop(out var arrayBoxed))
            {
                throw new InvalidProgramException("stack imbalance, Array.GetLength failed");
            }
            if (arrayBoxed is not Array array)
            {
                throw new InvalidProgramException("type mismatch, expected array instance for Array.GetLength");
            }
            int rank;
            try
            {
                rank = Convert.ToInt32(rankBoxed);
            }
            catch (Exception)
            {
                throw new InvalidProgramException("type mismatch, expected integer for Array.GetLength");
            }
            state.Stack.Push(array.GetLength(rank));
        }
    }
}
