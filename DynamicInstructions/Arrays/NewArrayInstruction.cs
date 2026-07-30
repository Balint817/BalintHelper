using DynamicInstructions.Abstract;

namespace DynamicInstructions.Arrays
{
    public class NewArrayInstruction : BaseInstruction
    {
        public readonly Type ArrayType;
        public readonly int Dimensions;
        public NewArrayInstruction(Type arrayType, int dimensions)
        {
            ArrayType = arrayType ?? throw new ArgumentNullException(nameof(arrayType));
            Dimensions = dimensions;
            if (Dimensions < 1)
            {
                throw new ArgumentException("invalid dimension", nameof(dimensions));
            }
        }
        public override void Execute(Interpreter.MethodState state, List<BaseInstruction> instructions)
        {
            var lengths = InstructionUtils.GetArrayIntsFromStack(state, Dimensions);

            var newArray = Array.CreateInstance(ArrayType, lengths);
            state.Stack.Push(newArray);
        }
    }
}
