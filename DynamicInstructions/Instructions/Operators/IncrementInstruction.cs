using DynamicInstructions.Instructions.Abstract;

namespace DynamicInstructions.Instructions.Operators
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
