using DynamicInstructions.Abstract;

namespace DynamicInstructions.Operators
{
    public class PlusInstruction : BaseDynamicUnaryOperationInstruction
    {
        override public string OperationName => "+x";
        public override object? ExecuteOperation(object? operand)
        {
            return +(dynamic?)operand;
        }
    }
}
