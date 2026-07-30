using DynamicInstructions.Abstract;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DynamicInstructions.Pointers
{
    public class WritePointerInstruction : BaseInstruction
    {
        public Type PointerType;
        private static readonly Dictionary<Type, Action<IntPtr, object>> _cache = [];
        public WritePointerInstruction(Type type)
        {
            PointerType = type ?? throw new ArgumentNullException(nameof(type));

            if (!_cache.TryGetValue(type, out var action))
            {
                try
                {
                    _cache[type] = typeof(WritePointerInstruction)
                        .GetMethod(nameof(Write), BindingFlags.NonPublic | BindingFlags.Static)!
                        .MakeGenericMethod(type)
                        .CreateDelegate<Action<IntPtr, object>>();
                }
                catch
                {
                    _cache[type] = null!;
                }
            }
        }
        private static unsafe void Write<T>(IntPtr ptr, object value) where T : unmanaged
        {
            *(T*)ptr = (T)value;
        }
        public override void Execute(Interpreter.MethodState state, List<BaseInstruction> instructions)
        {
            if (!state.Stack.TryPop(out var value))
            {
                throw new InvalidProgramException("stack imbalance, failed to obtain value to write");
            }

            if (!state.Stack.TryPop(out var intPtrBoxed))
            {
                throw new InvalidProgramException("stack imbalance, failed to obtain pointer to write");
            }

            if (intPtrBoxed is not IntPtr intPtr)
            {
                if (intPtrBoxed is not UIntPtr uintPtr)
                {
                    throw new InvalidProgramException("type mismatch, expected a pointer to write");
                }

                intPtr = (IntPtr)uintPtr;
            }

            if (value is null)
            {
                throw new InvalidProgramException($"pointer {intPtr} cannot be written with a null object reference");
            }

            try
            {
                if (PointerType.IsValueType &&
                    _cache.TryGetValue(PointerType, out var action) &&
                    action is not null)
                {
                    action(intPtr, value);
                    return;
                }
            }
            catch { }

            Marshal.StructureToPtr(value, intPtr, false);
        }
    }
}