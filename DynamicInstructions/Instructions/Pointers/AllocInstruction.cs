using DynamicInstructions.Instructions.Abstract;

namespace DynamicInstructions.Instructions.Pointers
{
    public class AllocInstruction : BaseInstruction
    {
        public unsafe override void Execute(Interpreter.MethodState state, List<BaseInstruction> instructions)
        {
            if (!state.Stack.TryPop(out var sizeBoxed))
            {
                throw new InvalidProgramException("stack imbalance, failed to obtain size to allocate");
            }
            var ptrSize = sizeof(nuint);
            nuint size;
            try
            {
                if (ptrSize > 4)
                {
                    size = (nuint)Convert.ToUInt64(sizeBoxed);
                }
                else
                {
                    size = (nuint)Convert.ToUInt32(sizeBoxed);
                }
            }
            catch (Exception)
            {
                throw new InvalidProgramException("expected integer for alloc size");
            }

            state.Stack.Push(size);
        }
    }
}
