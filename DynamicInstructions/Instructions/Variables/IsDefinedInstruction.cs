using DynamicInstructions.Instructions.Abstract;

namespace DynamicInstructions.Instructions.Variables
{
    public class IsDefinedInstruction : BaseInstruction
    {
        public override void Execute(Interpreter.MethodState state, List<BaseInstruction> instructions)
        {
            if (!state.Stack.TryPop(out var variableBoxed))
            {
                throw new InvalidProgramException("stack imbalance, failed to check variable initialization");
            }
            switch (variableBoxed)
            {
                case Interpreter.VariableInfo variableInfo:
                    {
                        bool flag = false;
                        switch (variableInfo.Type)
                        {
                            case Interpreter.VariableType.Local:
                                flag = state.Interpreter._globalVariables.ContainsKey(variableInfo.Name);
                                break;
                            case Interpreter.VariableType.Global:
                                flag = state.LocalVariables.ContainsKey(variableInfo.Name);
                                break;
                            case Interpreter.VariableType.Argument:
                                flag = 0 <= variableInfo.Index && variableInfo.Index < state.Args.Length;
                                break;
                        }
                        state.Stack.Push(flag);
                    }
                    break;
                default:
                    throw new InvalidProgramException("type mismatch, expected a variable reference, but got " + variableBoxed?.GetType().Name);
            }
        }
    }
}
