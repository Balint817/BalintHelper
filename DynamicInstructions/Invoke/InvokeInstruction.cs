using DynamicInstructions.Abstract;
using System.Reflection;

namespace DynamicInstructions.Invoke
{
    public delegate void RefWrite(Interpreter.MethodState state, object original, object? value);
    public delegate object? RefRead(Interpreter.MethodState state, object original);

    public class RefHandler
    {
        public RefRead Read { get; }
        public RefWrite Write { get; }
        public RefHandler(RefRead read, RefWrite write)
        {
            Read = read;
            Write = write;
        }
    }
    public class InvokeInstruction : BaseInstruction
    {
        // extract these into delegate types
        public static readonly Dictionary<Type, Func<Interpreter.MethodState, List<BaseInstruction>, object, object>> CustomHandlers = [];
        public static readonly Dictionary<Type, RefHandler> RefHandlers = new()
        {
            [typeof(Interpreter.VariableInfo)] = new RefHandler(
                (state, original) =>
                {
                    var variable = (Interpreter.VariableInfo)original;
                    return variable.GetValue(state);
                },
                (state, original, newValue) =>
                {
                    var variable = (Interpreter.VariableInfo)original;
                    variable.SetValue(state, newValue);
                }
            )
        };

        internal class ParameterInfoBox
        {
            internal object?[] Args;
            internal object?[] Orig;
            internal bool[] ByRef;
            public ParameterInfoBox(ParameterInfo[] parameters, Interpreter.MethodState state)
            {
                Args = new object?[parameters.Length];
                Orig = new object?[parameters.Length];
                ByRef = new bool[parameters.Length];
                for (int i = parameters.Length - 1; i >= 0; i--)
                {
                    if (!state.Stack.TryPop(out var arg))
                    {
                        throw new InvalidProgramException("stack imbalance, failed to obtain method args");
                    }
                    var parameter = parameters[i];
                    ByRef[i] = parameter.ParameterType.IsByRef || parameter.IsOut || parameter.IsIn;
                    if (arg?.GetType() is { } type)
                    {
                        foreach (var kv in RefHandlers)
                        {
                            if (type.IsAssignableTo(kv.Key))
                            {
                                var refRead = kv.Value.Read(state, arg);
                                arg = refRead;
                                return;
                            }
                        }
                    }
                    Orig[i] = Args[i] = arg;
                }
            }

            public void WriteBack(Interpreter.MethodState state)
            {
                for (int i = 0; i < Args.Length; i++)
                {
                    if (ByRef[i])
                    {
                        var arg = Args[i];
                        var orig = Orig[i];
                        if (orig?.GetType() is { } type)
                        {
                            foreach (var kv in RefHandlers)
                            {
                                if (type.IsAssignableTo(kv.Key))
                                {
                                    kv.Value.Write(state, orig, arg);
                                    break;
                                }
                            }
                        }
                    }
                }
                Args = null!;
                Orig = null!;
                ByRef = null!;
            }
        }
        public override void Execute(Interpreter.MethodState state, List<BaseInstruction> instructions)
        {
            if (!state.Stack.TryPop(out var infoBoxed))
            {
                throw new InvalidProgramException("stack imbalance, failed to obtain method info to invoke");
            }
            if (infoBoxed is null)
            {
                throw new InvalidProgramException("type mismatch, method info was null");
            }
            switch (infoBoxed)
            {
                case MethodInfo methodInfo:
                    {
                        var parameters = methodInfo.GetParameters();
                        var paramInfoBox = new ParameterInfoBox(parameters, state);
                        object? instance = null;
                        if (!methodInfo.IsStatic)
                        {
                            if (!state.Stack.TryPop(out instance))
                            {
                                throw new InvalidProgramException("stack imbalance, failed to obtain method instance");
                            }
                        }
                        var value = methodInfo.Invoke(instance, paramInfoBox.Args);
                        if (methodInfo.ReturnType != typeof(void))
                        {
                            state.Stack.Push(value);
                        }
                        paramInfoBox.WriteBack(state);
                    }
                    break;
                case Interpreter.DynamicMethodInfo dynamicMethodInfo:
                    {
                        var hasReturn = state.Interpreter.InvokeDynamicMethod(dynamicMethodInfo.Name, out var returnValue, null, state);
                        if (hasReturn)
                        {
                            state.Stack.Push(returnValue);
                        }
                    }
                    break;
                case ConstructorInfo constructorInfo:
                    {
                        if (constructorInfo.IsStatic)
                        {
                            throw new InvalidProgramException("cannot call a static constructor");
                        }
                        var parameters = constructorInfo.GetParameters();
                        var paramInfoBox = new ParameterInfoBox(parameters, state);
                        var value = constructorInfo.Invoke(paramInfoBox.Args);
                        state.Stack.Push(value);
                        paramInfoBox.WriteBack(state);
                    }
                    break;
                default:
                    {
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
                        throw new InvalidProgramException($"type mismatch, object of type {type.FullName} is not readable");
                    }
            }
        }
    }
}
