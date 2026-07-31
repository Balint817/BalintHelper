using DynamicInstructions.Instructions.Abstract;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DynamicInstructions.Instructions.Pointers
{
    public class ReadPointerInstruction : BaseInstruction
    {
        public Type PointerType;
        private static readonly Dictionary<Type, Func<IntPtr, object>> _cache = [];
        public ReadPointerInstruction(Type type)
        {
            PointerType = type ?? throw new ArgumentNullException(nameof(type));
            if (!_cache.TryGetValue(type, out var func))
            {
                try
                {
                    _cache[type] = typeof(ReadPointerInstruction)
                        .GetMethod(nameof(Read), BindingFlags.NonPublic | BindingFlags.Static)!
                        .MakeGenericMethod(type)
                        .CreateDelegate<Func<IntPtr, object>>();
                }
                catch (Exception)
                {
                    // unmanaged type constraint not met
                    _cache[type] = null!;
                }
            }
        }
        private static unsafe object Read<T>(IntPtr ptr) where T : unmanaged
        {
            return *(T*)ptr;
        }
        public unsafe override void Execute(Interpreter.MethodState state, List<BaseInstruction> instructions)
        {
            if (!state.Stack.TryPop(out var intPtrBoxed))
            {
                throw new InvalidProgramException("stack imbalance, failed to obtain pointer to read");
            }
            if (intPtrBoxed is not IntPtr intPtr)
            {
                if (intPtrBoxed is not UIntPtr uintPtr)
                {
                    throw new InvalidProgramException("type mismatch, expected a pointer to read");
                }
                intPtr = (IntPtr)uintPtr;
            }
            var ptr = intPtr.ToPointer();
            try
            {
                if (PointerType.IsValueType)
                {
                    if (_cache.TryGetValue(PointerType, out var func) && func is not null)
                    {
                        var value = func(intPtr);
                        state.Stack.Push(value);
                        return;
                    }
                }
            }
            catch (Exception) { }
            var obj = Marshal.PtrToStructure(intPtr, PointerType);
            state.Stack.Push(obj);
        }
    }
}
