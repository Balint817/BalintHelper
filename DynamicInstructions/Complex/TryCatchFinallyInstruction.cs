using DynamicInstructions.Abstract;
using DynamicInstructions.Basic;

namespace DynamicInstructions.Complex
{
    public class TryCatchFinallyInstruction : NopInstruction
    {
        public readonly string TryMethod;
        public readonly string CatchMethod;
        public readonly string FinallyMethod;

        public TryCatchFinallyInstruction(string tryMethod, string catchMethod, string finallyMethod)
        {
            TryMethod = tryMethod ?? throw new ArgumentNullException(nameof(tryMethod));
            CatchMethod = catchMethod;
            FinallyMethod = finallyMethod;
        }
        public override void Execute(Interpreter.MethodState state, List<BaseInstruction> instructions)
        {
            try
            {
                state.Interpreter.InvokeDynamicMethod(TryMethod, out _, null, state);
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(CatchMethod))
                {
                    if (!state.Interpreter._dynamicMethods.TryGetValue(CatchMethod, out var dynamicMethod))
                    {
                        throw new InvalidProgramException($"attempted to invoke undefined dynamic method {CatchMethod}");
                    }
                    if (dynamicMethod.ArgCount != 0)
                    {
                        if (dynamicMethod.ArgCount > 1)
                        {
                            throw new InvalidProgramException($"dynamic method {CatchMethod} used in catch statement has more than one argument");
                        }
                        state.Stack.Push(ex);
                    }
                    state.Interpreter.InvokeDynamicMethod(CatchMethod, out _, null, state);
                }
            }
            finally
            {
                if (!string.IsNullOrEmpty(FinallyMethod))
                {
#pragma warning disable CA2219 // don't really have a choice.
                    if (!state.Interpreter._dynamicMethods.TryGetValue(FinallyMethod, out var dynamicMethod))
                    {
                        throw new InvalidProgramException($"attempted to invoke undefined dynamic method {FinallyMethod}");
                    }
                    if (dynamicMethod.ArgCount != 0)
                    {
                        throw new InvalidProgramException($"dynamic method {FinallyMethod} used in finally statement should not have an argument");
                    }
#pragma warning restore CA2219
                    state.Interpreter.InvokeDynamicMethod(FinallyMethod, out _, null, state);
                }
            }
        }
    }
}
