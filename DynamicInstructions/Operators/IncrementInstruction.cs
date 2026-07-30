using DynamicInstructions.Abstract;

namespace DynamicInstructions.Operators
{
    public class IncrementInstruction : BaseDynamicUnaryOperationInstruction
    {
        override public string OperationName => "++x";
        public override object? ExecuteOperation(object? operand)
        {
            var t = (dynamic?)operand;
            return ++t;
        }
    }
}
