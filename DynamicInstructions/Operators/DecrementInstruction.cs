using DynamicInstructions.Abstract;

namespace DynamicInstructions.Operators
{
    public class DecrementInstruction : BaseDynamicUnaryOperationInstruction
    {
        override public string OperationName => "--x";
        public override object? ExecuteOperation(object? operand)
        {
            var t = (dynamic?)operand;
            return --t;
        }
    }
}
