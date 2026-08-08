using DynamicInstructions.Instructions.Abstract;

namespace DynamicInstructions.Instructions.Variables
{
    public delegate object IsDefinedHandler(
        Interpreter.MethodState state,
        List<BaseInstruction> instructions,
        object info);

    public class IsDefinedInstruction : BaseInstruction
    {
        public static readonly List<KeyValuePair<Type, IsDefinedHandler>> IsDefinedHandlers =
        [
            new(typeof(Interpreter.VariableInfo), static (state, instructions, info) =>
            {
                var variableInfo = (Interpreter.VariableInfo)info;
                return variableInfo.Type switch
                {
                    Interpreter.VariableType.Local => state.Interpreter._globalVariables.ContainsKey(variableInfo.Name),
                    Interpreter.VariableType.Global => state.LocalVariables.ContainsKey(variableInfo.Name),
                    Interpreter.VariableType.Argument => 0 <= variableInfo.Index && variableInfo.Index < state.Args.Length,
                    _ => false
                };
            })
        ];

        private static bool TryCheckRegistered(
            Interpreter.MethodState state,
            List<BaseInstruction> instructions,
            object info,
            out object? result)
        {
            var runtimeType = info.GetType();

            foreach (var kv in IsDefinedHandlers)
            {
                if (runtimeType.IsAssignableTo(kv.Key))
                {
                    result = kv.Value(state, instructions, info);
                    return true;
                }
            }

            result = null;
            return false;
        }

        public override void Execute(Interpreter.MethodState state, List<BaseInstruction> instructions)
        {
            if (!state.Stack.TryPop(out var variableBoxed))
            {
                throw new InvalidProgramException(
                    "stack imbalance, failed to check variable initialization");
            }

            if (variableBoxed is null)
            {
                throw new InvalidProgramException("type mismatch, variable reference was null");
            }

            if (!TryCheckRegistered(state, instructions, variableBoxed, out var result))
            {
                throw new InvalidProgramException(
                    $"type mismatch, expected a variable reference, but got {variableBoxed.GetType().FullName}");
            }

            state.Stack.Push(result!);
        }
    }
}