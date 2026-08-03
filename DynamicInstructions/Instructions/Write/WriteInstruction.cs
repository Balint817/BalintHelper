using DynamicInstructions.Instructions.Abstract;
using System.Reflection;

namespace DynamicInstructions.Instructions.Write
{
    public delegate void WriteHandler(
        Interpreter.MethodState state,
        List<BaseInstruction> instructions,
        object info,
        object? value);

    public class WriteInstruction : BaseInstruction
    {
        public static readonly List<KeyValuePair<Type, WriteHandler>> WriteHandlers = new()
        {
            new(typeof(Interpreter.VariableInfo), static (state, instructions, info, value) =>
            {
                var variableInfo = (Interpreter.VariableInfo)info;
                variableInfo.SetValue(state, value);
            }),

            new(typeof(FieldInfo), static (state, instructions, info, value) =>
            {
                var fieldInfo = (FieldInfo)info;
                object? instance = null;

                if (!fieldInfo.IsStatic)
                {
                    if (!state.Stack.TryPop(out instance))
                    {
                        throw new InvalidProgramException(
                            "stack imbalance, failed to obtain instance for field write");
                    }
                }

                fieldInfo.SetValue(instance, value);
            }),

            new(typeof(PropertyInfo), static (state, instructions, info, value) =>
            {
                var propertyInfo = (PropertyInfo)info;
                var setMethod = propertyInfo.SetMethod
                    ?? throw new InvalidProgramException(
                        $"property {propertyInfo.Name} is not writable");

                if (setMethod.GetParameters().Length != 1)
                {
                    throw new InvalidProgramException(
                        $"property {propertyInfo.Name} is an indexer");
                }

                object? instance = null;
                if (!setMethod.IsStatic)
                {
                    if (!state.Stack.TryPop(out instance))
                    {
                        throw new InvalidProgramException(
                            "stack imbalance, failed to obtain instance for property write");
                    }
                }

                setMethod.Invoke(instance, new object?[] { value });
            })
        };

        private static bool TryWriteRegistered(
            Interpreter.MethodState state,
            List<BaseInstruction> instructions,
            object info,
            object? value)
        {
            var runtimeType = info.GetType();

            foreach (var kv in WriteHandlers)
            {
                if (runtimeType.IsAssignableTo(kv.Key))
                {
                    kv.Value(state, instructions, info, value);
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
                    "stack imbalance, failed to obtain variable info to write");
            }

            if (!state.Stack.TryPop(out var value))
            {
                throw new InvalidProgramException(
                    "stack imbalance, failed to obtain value to write");
            }

            if (infoBoxed is null)
            {
                throw new InvalidProgramException("type mismatch, variable info was null");
            }

            if (!TryWriteRegistered(state, instructions, infoBoxed, value))
            {
                throw new InvalidProgramException(
                    $"type mismatch, object of type {infoBoxed.GetType().FullName} is not writable");
            }
        }
    }
}