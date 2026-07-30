using DynamicInstructions.Abstract;

namespace DynamicInstructions.Arrays
{
    public class LoadArrayElementInstruction : BaseInstruction
    {
        public override void Execute(Interpreter.MethodState state, List<BaseInstruction> instructions)
        {
            var indices = InstructionUtils.GetArrayIntsFromStack(state, 0, out var array);
            var value = array!.GetValue(indices);
            state.Stack.Push(value);
        }
    }
}
