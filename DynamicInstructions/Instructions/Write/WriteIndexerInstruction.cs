using DynamicInstructions.Instructions.Abstract;
using System.Reflection;

namespace DynamicInstructions.Instructions.Write
{
    public delegate void WriteIndexerHandler(
        Interpreter.MethodState state,
        List<BaseInstruction> instructions,
        object info);

    public class WriteIndexerInstruction : BaseInstruction
    {
        public static readonly List<KeyValuePair<Type, WriteIndexerHandler>> WriteIndexerHandlers = new()
        {
            new(typeof(PropertyInfo), static (state, instructions, info) =>
            {
                var propInfo = (PropertyInfo)info;
                var paramInfos = propInfo.GetIndexParameters();
                var setMethod = propInfo.SetMethod;

                if (paramInfos.Length == 0 || setMethod is null)
                {
                    throw new InvalidProgramException(
                        $"property {propInfo.Name} is not a writeable indexer");
                }

                if (!state.Stack.TryPop(out var value))
                {
                    throw new InvalidProgramException(
                        "stack imbalance, failed to obtain indexer value");
                }

                object?[] args = new object?[paramInfos.Length + 1];
                args[^1] = value;

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
                if (!setMethod.IsStatic)
                {
                    if (!state.Stack.TryPop(out instance))
                    {
                        throw new InvalidProgramException(
                            "stack imbalance, failed to obtain indexer instance");
                    }
                }

                setMethod.Invoke(instance, args);
            })
        };

        private static bool TryWriteIndexerRegistered(
            Interpreter.MethodState state,
            List<BaseInstruction> instructions,
            object info)
        {
            var runtimeType = info.GetType();

            foreach (var kv in WriteIndexerHandlers)
            {
                if (runtimeType.IsAssignableTo(kv.Key))
                {
                    kv.Value(state, instructions, info);
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
                    "stack imbalance, failed to obtain property info to write");
            }

            if (propertyInfoBoxed is null)
            {
                throw new InvalidProgramException("type mismatch, property info was null");
            }

            if (!TryWriteIndexerRegistered(state, instructions, propertyInfoBoxed))
            {
                throw new InvalidProgramException(
                    $"type mismatch, object of type {propertyInfoBoxed.GetType().FullName} is not a writeable indexer");
            }
        }
    }
}