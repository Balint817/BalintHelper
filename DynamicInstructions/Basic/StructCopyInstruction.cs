using DynamicInstructions.Abstract;
using System.Reflection;

namespace DynamicInstructions.Basic
{
    public class StructCopyInstruction : BaseInstruction
    {
        private static readonly Dictionary<Type, Func<object, object>> _structCopyFuncs = [];
        private static readonly MethodInfo _copyStructMethod = typeof(StructCopyInstruction).GetMethod(nameof(CopyStruct), BindingFlags.NonPublic | BindingFlags.Static)!;
        public override void Execute(Interpreter.MethodState state, List<BaseInstruction> instructions)
        {
            if (!state.Stack.TryPop(out var value))
            {
                throw new InvalidProgramException("Stack imbalance, failed to copy struct");
            }
            if (value is null)
            {
                state.Stack.Push(value);
                return;
            }
            var type = value.GetType();
            if (!type.IsValueType)
            {
                state.Stack.Push(value);
                return;
            }
            if (!_structCopyFuncs.TryGetValue(type, out var method))
            {
                _structCopyFuncs[type] = method = (Func<object, object>)_copyStructMethod.MakeGenericMethod(type).CreateDelegate(typeof(Func<object, object>));
            }
            state.Stack.Push(method(value));
        }

        private static object CopyStruct<T>(object value) where T : struct
        {
            return (T)value;
        }
    }
}
