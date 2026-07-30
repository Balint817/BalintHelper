using DynamicInstructions.Abstract;

namespace DynamicInstructions.Operators
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
