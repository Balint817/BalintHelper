using DynamicInstructions.Instructions.Abstract;

namespace DynamicInstructions.Instructions.Operators
{
    public class IndexRangeInstruction : BaseDynamicBinaryOperationInstruction
    {
        override public string OperationName => "x..y";
        public override object? ExecuteOperation(object? left, object? right)
        {
            var t1 = (dynamic?)left;
            var t2 = (dynamic?)right;
            return t1..t2;
        }
    }
}
