namespace DynamicInstructions.Abstract
{
    public abstract class BaseDynamicBinaryOperationInstruction : BaseInstruction
    {
        public abstract string OperationName { get; }
        public override void Execute(Interpreter.MethodState state, List<BaseInstruction> instructions)
        {
            if (!state.Stack.TryPop(out var right)
                || !state.Stack.TryPop(out var left))
            {
                throw new InvalidProgramException($"Stack imbalance, failed to execute binary operation '{OperationName}'");
            }
            state.Stack.Push(ExecuteOperation(left, right));
        }
        public abstract object? ExecuteOperation(object? left, object? right);
    }
}
