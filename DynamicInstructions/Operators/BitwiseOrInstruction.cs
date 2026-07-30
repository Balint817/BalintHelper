using DynamicInstructions.Abstract;

namespace DynamicInstructions.Operators
{
    public class BitwiseOrInstruction : BaseDynamicBinaryOperationInstruction
    {
        override public string OperationName => "x|y";
        public override object? ExecuteOperation(object? left, object? right)
        {
            return (dynamic?)left | (dynamic?)right;
        }
    }
}
