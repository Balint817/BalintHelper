using DynamicInstructions.Instructions.Abstract;

namespace DynamicInstructions.Instructions.Basic
{
    public class ReturnInstruction : NopInstruction
    {
        public override void Execute(Interpreter.MethodState state, List<BaseInstruction> instructions)
        {
            state.Cursor = instructions.Count;
        }
    }
}
