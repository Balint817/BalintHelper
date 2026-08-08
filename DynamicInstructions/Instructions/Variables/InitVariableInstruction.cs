using DynamicInstructions.Instructions.Abstract;

namespace DynamicInstructions.Instructions.Variables
{
    public delegate void InitVariableHandler(
        Interpreter.MethodState state,
        List<BaseInstruction> instructions,
        object info,
        object? initValue);

    public class InitVariableInstruction : BaseInstruction
    {
        public static readonly List<KeyValuePair<Type, InitVariableHandler>> InitHandlers =
        [
            new(typeof(Interpreter.VariableInfo), static (state, instructions, info, initValue) =>
            {
                var variableInfo = (Interpreter.VariableInfo)info;

                if (variableInfo.Type == Interpreter.VariableType.Argument)
                {
                    // nothing to initialize.
                    return;
                }

                var targetDict = variableInfo.Type == Interpreter.VariableType.Global
                    ? state.Interpreter._globalVariables
                    : state.LocalVariables;

                if (!targetDict.ContainsKey(variableInfo.Name))
                {
                    targetDict[variableInfo.Name] = initValue;
                }
            })
        ];

        private static bool TryInitRegistered(
            Interpreter.MethodState state,
            List<BaseInstruction> instructions,
            object info,
            object? initValue)
        {
            var runtimeType = info.GetType();

            foreach (var kv in InitHandlers)
            {
                if (runtimeType.IsAssignableTo(kv.Key))
                {
                    kv.Value(state, instructions, info, initValue);
                    return true;
                }
            }

            return false;
        }

        public override void Execute(Interpreter.MethodState state, List<BaseInstruction> instructions)
        {
            if (!state.Stack.TryPop(out var variableBoxed))
            {
                throw new InvalidProgramException(
                    "stack imbalance, failed to obtain variable reference to initialize");
            }

            if (!state.Stack.TryPop(out var initValue))
            {
                throw new InvalidProgramException(
                    "stack imbalance, failed to obtain initial value");
            }

            if (variableBoxed is null)
            {
                throw new InvalidProgramException("type mismatch, variable reference was null");
            }

            if (!TryInitRegistered(state, instructions, variableBoxed, initValue))
            {
                throw new InvalidProgramException(
                    $"type mismatch, object of type {variableBoxed.GetType().FullName} is not initializable");
            }
        }
    }
}