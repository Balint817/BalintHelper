using DynamicInstructions.Instructions.Abstract;
using System.Reflection;

namespace DynamicInstructions.Instructions.Read
{
    public delegate object? ReadIndexerHandler(
        Interpreter.MethodState state,
        List<BaseInstruction> instructions,
        object info);

    public class ReadIndexerInstruction : BaseInstruction
    {
        public static readonly List<KeyValuePair<Type, ReadIndexerHandler>> ReadIndexerHandlers = new()
        {
            new(typeof(PropertyInfo), static (state, instructions, info) =>
            {
                var propInfo = (PropertyInfo)info;
                var paramInfos = propInfo.GetIndexParameters();
                var getMethod = propInfo.GetMethod;

                if (paramInfos.Length == 0 || getMethod is null)
                {
                    throw new InvalidProgramException(
                        $"property {propInfo.Name} is not a readable indexer");
                }

                object?[] args = new object?[paramInfos.Length];
                for (int i = paramInfos.Length - 1; i >= 0; i--)
                {
                    if (!state.Stack.TryPop(out var arg))
                    {
                        throw new InvalidProgramException(
                            "stack imbalance, failed to obtain indexer args");
                    }

                    args[i] = arg;
                }

                object? instance = null;
                if (!getMethod.IsStatic)
                {
                    if (!state.Stack.TryPop(out instance))
                    {
                        throw new InvalidProgramException(
                            "stack imbalance, failed to obtain indexer instance");
                    }
                }

                return getMethod.Invoke(instance, args);
            })
        };

        private static bool TryReadIndexerRegistered(
            Interpreter.MethodState state,
            List<BaseInstruction> instructions,
            object info)
        {
            var runtimeType = info.GetType();

            foreach (var kv in ReadIndexerHandlers)
            {
                if (runtimeType.IsAssignableTo(kv.Key))
                {
                    var value = kv.Value(state, instructions, info);
                    state.Stack.Push(value);
                    return true;
                }
            }

            return false;
        }

        public override void Execute(Interpreter.MethodState state, List<BaseInstruction> instructions)
        {
            if (!state.Stack.TryPop(out var propertyInfoBoxed))
            {
                throw new InvalidProgramException(
                    "stack imbalance, failed to obtain property info to read");
            }

            if (propertyInfoBoxed is null)
            {
                throw new InvalidProgramException("type mismatch, property info was null");
            }

            if (!TryReadIndexerRegistered(state, instructions, propertyInfoBoxed))
            {
                throw new InvalidProgramException(
                    $"type mismatch, object of type {propertyInfoBoxed.GetType().FullName} is not a readable indexer");
            }
        }
    }
}