using DynamicInstructions.Instructions.Abstract;

namespace DynamicInstructions.Instructions.Basic
{
    // TODO:
    // GetEventInfo
    // GetChannel => channelutils.cs => Dictionary<string, ChannelVal> state = new (); or _getVal(string ch) might be better
    public class LoadConstantInstruction : BaseInstruction
    {
        public readonly object? Value;
        public LoadConstantInstruction(object? value)
        {
            Value = value;
        }
        public override void Execute(Interpreter.MethodState state, List<BaseInstruction> instructions)
        {
            state.Stack.Push(Value);
        }
    }

    public class MainClass
    {
        public abstract class NestedClassBase
        {
            protected NestedClassBase()
            {

            }
        }
        protected class NestedClass : NestedClassBase
        {
            public NestedClass() : base()
            {

            }
        }
    }
}
