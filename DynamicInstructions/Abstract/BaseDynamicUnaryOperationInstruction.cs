namespace DynamicInstructions.Abstract
{
    public abstract class BaseDynamicUnaryOperationInstruction : BaseInstruction
    {
        public abstract string OperationName { get; }
        public override void Execute(Interpreter.MethodState state, List<BaseInstruction> instructions)
        {
            if (!state.Stack.TryPop(out var value))
            {
                throw new InvalidProgramException($"Stack imbalance, failed to execute unary operation '{OperationName}'");
            }
            state.Stack.Push(ExecuteOperation(value));
        }
        public abstract object? ExecuteOperation(object? operand);
    }
}
