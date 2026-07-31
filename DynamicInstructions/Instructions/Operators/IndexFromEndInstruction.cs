using DynamicInstructions.Instructions.Abstract;

namespace DynamicInstructions.Instructions.Operators
{
    public class IndexFromEndInstruction : BaseDynamicUnaryOperationInstruction
    {
        override public string OperationName => "^x";
        public override object? ExecuteOperation(object? operand)
        {
            var t1 = (dynamic?)operand;
            return ^t1;
        }
    }
}
