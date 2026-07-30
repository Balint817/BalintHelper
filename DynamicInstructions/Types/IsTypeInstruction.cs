using DynamicInstructions.Abstract;

namespace DynamicInstructions.Types
{
    public class IsTypeInstruction : BaseInstruction
    {
        public readonly Type Type;
        public IsTypeInstruction(Type type)
        {
            Type = type;
        }
        public override void Execute(Interpreter.MethodState state, List<BaseInstruction> instructions)
        {
            if (!state.Stack.TryPop(out var value))
            {
                throw new InvalidProgramException("stack imbalance, failed to type check value");
            }
            var flag = true;
            if (value is null)
            {
                if (Type.IsValueType)
                {
                    flag = false;
                }
            }
            else
            {
                flag = value.GetType().IsAssignableTo(Type);
            }
            state.Stack.Push(flag);
        }
    }
}
