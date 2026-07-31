using DynamicInstructions.Instructions.Abstract;

namespace DynamicInstructions.Instructions.Operators
{
    public class ComplementInstruction : BaseDynamicUnaryOperationInstruction
    {
        override public string OperationName => "~x";
        public override object? ExecuteOperation(object? operand)
        {
            return ~(dynamic?)operand;
        }
    }
}
