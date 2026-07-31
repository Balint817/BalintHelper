using DynamicInstructions.Instructions.Abstract;
using System.Reflection;

namespace DynamicInstructions.Instructions.Write
{
    public class WriteInstruction : BaseInstruction
    {
        public static readonly Dictionary<Type, Action<Interpreter.MethodState, List<BaseInstruction>, object, object?>> CustomHandlers = [];
        public override void Execute(Interpreter.MethodState state, List<BaseInstruction> instructions)
        {
            if (!state.Stack.TryPop(out var value))
            {
                throw new InvalidProgramException("stack imbalance, failed to obtain value to write");
            }
            if (!state.Stack.TryPop(out var infoBoxed))
            {
                throw new InvalidProgramException("stack imbalance, failed to obtain variable info to write");
            }
            if (infoBoxed is null)
            {
                throw new InvalidProgramException("type mismatch, variable info was null");
            }
            switch (infoBoxed)
            {
                case Interpreter.VariableInfo variableInfo:
                    {
                        variableInfo.SetValue(state, value);
                    }
                    break;
                case FieldInfo fieldInfo:
                    {
                        object? instance = null;
                        if (!fieldInfo.IsStatic)
                        {
                            if (!state.Stack.TryPop(out instance))
                            {
                                throw new InvalidProgramException("stack imbalance, failed to obtain instance for field write");
                            }
                        }
                        fieldInfo.SetValue(instance, value);
                    }
                    break;
                case PropertyInfo propertyInfo:
                    {
                        var setMethod = propertyInfo.SetMethod
                            ?? throw new InvalidProgramException($"property {propertyInfo.Name} is not writable");
                        object? instance = null;
                        if (!setMethod.IsStatic)
                        {
                            state.Stack.TryPop(out instance);
                        }
                        if (setMethod.GetParameters().Length != 1)
                        {
                            throw new InvalidProgramException($"property {propertyInfo.Name} is an indexer");
                        }
                        setMethod.Invoke(instance, [value]);
                        break;
                    }
                default:
                    var type = infoBoxed.GetType();
                    foreach (var kv in CustomHandlers)
                    {
                        if (type.IsAssignableTo(kv.Key))
                        {
                            kv.Value(state, instructions, infoBoxed, value);
                            return;
                        }
                    }
                    throw new InvalidProgramException($"type mismatch, object of type {infoBoxed?.GetType().FullName} is not writable");
            }
        }
    }
}
