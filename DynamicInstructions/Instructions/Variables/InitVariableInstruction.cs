using DynamicInstructions.Instructions.Abstract;

namespace DynamicInstructions.Instructions.Variables
{
    public class InitVariableInstruction : BaseInstruction
    {
        public override void Execute(Interpreter.MethodState state, List<BaseInstruction> instructions)
        {
            if (!state.Stack.TryPop(out var variableBoxed) || !state.Stack.TryPop(out var initValue))
            {
                throw new InvalidProgramException("stack imbalance, failed to initialize variable");
            }
            if (variableBoxed is not Interpreter.VariableInfo variableInfo)
            {
                throw new InvalidProgramException("type mismatch, expected a variable reference to initialize, but got " + variableBoxed?.GetType().Name);
            }
            if (variableInfo.Type == Interpreter.VariableType.Argument)
            {
                return;
            }
            var targetDict = state.LocalVariables;
            if (variableInfo.Type == Interpreter.VariableType.Global)
            {
                targetDict = state.Interpreter._globalVariables;
            }
            if (targetDict.ContainsKey(variableInfo.Name))
            {
                return;
            }
            targetDict[variableInfo.Name] = initValue;
        }
    }
}
