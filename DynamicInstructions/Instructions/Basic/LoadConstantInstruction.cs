using DynamicInstructions.Instructions.Abstract;

namespace DynamicInstructions.Instructions.Basic
{
    // LoadPrimitiveTrigger
    // SizeofTrigger
    // DefaultTrigger
    // TypeofTrigger
    // GetFieldInfo
    // GetPropertyInfo
    // GetMethodInfo
    // GetConstructorInfo
    // GetVariableInfo
    // GetDynamicMethodInfo

    // TODO:
    // GetEventInfo
    // GetFlagInfo
    // GetCounterInfo
    // GetSliderInfo
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
