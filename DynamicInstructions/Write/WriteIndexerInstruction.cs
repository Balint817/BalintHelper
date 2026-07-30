using DynamicInstructions.Abstract;
using System.Reflection;

namespace DynamicInstructions.Write
{
    public class WriteIndexerInstruction : BaseInstruction
    {
        public static readonly Dictionary<Type, Action<Interpreter.MethodState, List<BaseInstruction>, object>> CustomHandlers = [];
        public override void Execute(Interpreter.MethodState state, List<BaseInstruction> instructions)
        {
            if (!state.Stack.TryPop(out var propertyInfoBoxed))
            {
                throw new InvalidProgramException("stack imbalance, failed to obtain property info to read");
            }
            if (propertyInfoBoxed is null)
            {
                throw new InvalidProgramException("type mismatch, property info was null");
            }
            if (propertyInfoBoxed is not PropertyInfo propInfo)
            {
                var type = propertyInfoBoxed.GetType();
                foreach (var kv in CustomHandlers)
                {
                    if (type.IsAssignableTo(kv.Key))
                    {
                        kv.Value(state, instructions, propertyInfoBoxed);
                        return;
                    }
                }
                throw new InvalidProgramException("type mismatch, expected property info in indexer");
            }
            var paramInfos = propInfo.GetIndexParameters();
            var setMethod = propInfo.SetMethod;
            if (paramInfos.Length == 0 || setMethod is null)
            {
                throw new InvalidProgramException($"property {propInfo.Name} is not a writeable indexer");
            }
            object?[] args = new object?[paramInfos.Length];
            for (int i = paramInfos.Length - 1; i >= 0; i--)
            {
                if (!state.Stack.TryPop(out var arg))
                {
                    throw new InvalidProgramException("stack imbalance, failed to obtain indexer args");
                }
                args[i] = arg;
            }
            object? instance = null;
            if (!setMethod!.IsStatic)
            {
                if (!state.Stack.TryPop(out instance))
                {
                    throw new InvalidProgramException("stack imbalance, failed to obtain indexer instance");
                }
            }
            setMethod.Invoke(instance, args);
        }
    }
}
