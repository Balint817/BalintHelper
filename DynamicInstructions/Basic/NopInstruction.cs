using DynamicInstructions.Abstract;

namespace DynamicInstructions.Basic
{
    public class NopInstruction : BaseInstruction
    {
        public override void Execute(Interpreter.MethodState state, List<BaseInstruction> instructions)
        {
            // Do nothing
        }
    }
}
