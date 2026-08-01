using DynamicInstructions.Instructions.Abstract;
using System.Reflection;

namespace DynamicInstructions.Instructions.Read
{
    public delegate object? ReadHandler(
        Interpreter.MethodState state,
        List<BaseInstruction> instructions,
        object info);

    public class ReadInstruction : BaseInstruction
    {
        public static readonly List<KeyValuePair<Type, ReadHandler>> ReadHandlers = new()
        {
            new(typeof(Interpreter.VariableInfo), static (state, instructions, info) =>
            {
                var variableInfo = (Interpreter.VariableInfo)info;
                return variableInfo.GetValue(state);
            }),

            new(typeof(FieldInfo), static (state, instructions, info) =>
            {
                var fieldInfo = (FieldInfo)info;
                object? instance = null;

                if (!fieldInfo.IsStatic)
                {
                    if (!state.Stack.TryPop(out instance))
                    {
                        throw new InvalidProgramException(
                            "stack imbalance, failed to obtain instance for field read");
                    }
                }

                return fieldInfo.GetValue(instance);
            }),

            new(typeof(PropertyInfo), static (state, instructions, info) =>
            {
                var propertyInfo = (PropertyInfo)info;
                var getMethod = propertyInfo.GetMethod
                    ?? throw new InvalidProgramException(
                        $"property {propertyInfo.Name} is not readable");

                if (getMethod.GetParameters().Length != 0)
                {
                    throw new InvalidProgramException(
                        $"property {propertyInfo.Name} is an indexer");
                }

                object? instance = null;
                if (!getMethod.IsStatic)
                {
                    if (!state.Stack.TryPop(out instance))
                    {
                        throw new InvalidProgramException(
                            "stack imbalance, failed to obtain instance for property read");
                    }
                }

                return getMethod.Invoke(instance, null);
            })
        };

        private static bool TryReadRegistered(
            Interpreter.MethodState state,
            List<BaseInstruction> instructions,
            object info)
        {
            var runtimeType = info.GetType();

            foreach (var kv in ReadHandlers)
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
            if (!state.Stack.TryPop(out var infoBoxed))
            {
                throw new InvalidProgramException(
                    "stack imbalance, failed to obtain variable info to read");
            }

            if (infoBoxed is null)
            {
                throw new InvalidProgramException("type mismatch, variable info was null");
            }

            if (!TryReadRegistered(state, instructions, infoBoxed))
            {
                throw new InvalidProgramException(
                    $"type mismatch, object of type {infoBoxed.GetType().FullName} is not readable");
            }
        }
    }
}