namespace DynamicInstructions.Instructions.Abstract
{
    // max per frame: 35ms
    // max per awake: 2500ms
    public abstract class BaseInstruction
    {
        public abstract void Execute(Interpreter.MethodState state, List<BaseInstruction> instructions);
    }
}
