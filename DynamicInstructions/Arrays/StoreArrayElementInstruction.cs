using DynamicInstructions.Abstract;

namespace DynamicInstructions.Arrays
{
    public class StoreArrayElementInstruction : BaseInstruction
    {
        public override void Execute(Interpreter.MethodState state, List<BaseInstruction> instructions)
        {
            if (!state.Stack.TryPop(out var value))
            {
                throw new InvalidProgramException("stack imbalance, failed to get value to store as array element");
            }
            var indices = InstructionUtils.GetArrayIntsFromStack(state, 0, out var array);
            array!.SetValue(value, indices);
        }
    }
}
