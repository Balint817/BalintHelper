using DynamicInstructions.Instructions.Abstract;
using System.Reflection;

namespace DynamicInstructions.Instructions.Read
{
    public class ReadInstruction : BaseInstruction
    {
        public static readonly Dictionary<Type, Func<Interpreter.MethodState, List<BaseInstruction>, object, object>> CustomHandlers = [];
        public override void Execute(Interpreter.MethodState state, List<BaseInstruction> instructions)
        {
            if (!state.Stack.TryPop(out var infoBoxed))
            {
                throw new InvalidProgramException("stack imbalance, failed to obtain variable info to read");
            }
            if (infoBoxed is null)
            {
                throw new InvalidProgramException("type mismatch, variable info was null");
            }
            switch (infoBoxed)
            {
                case Interpreter.VariableInfo variableInfo:
                    {
                        state.Stack.Push(variableInfo.GetValue(state));
                    }
                    break;
                case FieldInfo fieldInfo:
                    {
                        object? instance = null;
                        if (!fieldInfo.IsStatic)
                        {
                            if (!state.Stack.TryPop(out instance))
                            {
                                throw new InvalidProgramException("stack imbalance, failed to obtain instance for field read");
                            }
                        }
                        var value = fieldInfo.GetValue(instance);
                        state.Stack.Push(value);
                    }
                    break;
                case PropertyInfo propertyInfo:
                    {
                        var getMethod = propertyInfo.GetMethod
                            ?? throw new InvalidProgramException($"property {propertyInfo.Name} is not readable");
                        object? instance = null;
                        if (!getMethod.IsStatic)
                        {
                            state.Stack.TryPop(out instance);
                        }
                        if (getMethod.GetParameters().Length != 0)
                        {
                            throw new InvalidProgramException($"property {propertyInfo.Name} is an indexer");
                        }
                        var value = getMethod.Invoke(instance, null);
                        state.Stack.Push(value);
                        break;
                    }
                default:
                    var type = infoBoxed.GetType();
                    foreach (var kv in CustomHandlers)
                    {
                        if (type.IsAssignableTo(kv.Key))
                        {
                            var value = kv.Value(state, instructions, infoBoxed);
                            state.Stack.Push(value);
                            return;
                        }
                    }
                    throw new InvalidProgramException($"type mismatch, object of type {infoBoxed?.GetType().FullName} is not readable");
            }
        }
    }
}
