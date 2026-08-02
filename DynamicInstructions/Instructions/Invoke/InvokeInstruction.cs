using DynamicInstructions.Instructions.Abstract;
using System.Reflection;

namespace DynamicInstructions.Instructions.Invoke
{
    public delegate void RefWrite(
        Interpreter.MethodState state,
        List<BaseInstruction> instructions,
        object original,
        object? value);

    public delegate object? RefRead(
        Interpreter.MethodState state,
        List<BaseInstruction> instructions,
        object original);

    public delegate void InvokeHandler(
        Interpreter.MethodState state,
        List<BaseInstruction> instructions,
        object info);

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
        public static readonly List<KeyValuePair<Type, InvokeHandler>> InvokeHandlers =
        [
            new(typeof(MethodInfo), static (state, instructions, info) =>
            {
                var methodInfo = (MethodInfo)info;
                var parameters = methodInfo.GetParameters();
                var paramInfoBox = new ParameterInfoBox(parameters, state, instructions);

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

                paramInfoBox.WriteBack(state, instructions);
            }),

            new(typeof(Interpreter.DynamicMethodInfo), static (state, instructions, info) =>
            {
                var dynamicMethodInfo = (Interpreter.DynamicMethodInfo)info;
                var hasReturn = state.Interpreter.InvokeDynamicMethod(dynamicMethodInfo.Name, out var returnValue, null, state);
                if (hasReturn)
                {
                    state.Stack.Push(returnValue);
                }
            }),

            new(typeof(ConstructorInfo), static (state, instructions, info) =>
            {
                var constructorInfo = (ConstructorInfo)info;
                if (constructorInfo.IsStatic)
                {
                    throw new InvalidProgramException("cannot call a static constructor");
                }

                var parameters = constructorInfo.GetParameters();
                var paramInfoBox = new ParameterInfoBox(parameters, state, instructions);
                var value = constructorInfo.Invoke(paramInfoBox.Args);
                state.Stack.Push(value);
                paramInfoBox.WriteBack(state, instructions);
            }),

            new(typeof(Delegate), static (state, instructions, info) =>
            {
                var del = (Delegate)info;
                var invokeMethod = del.GetType().GetMethod("Invoke")
                    ?? throw new InvalidProgramException("type mismatch, delegate has no Invoke method");

                var parameters = invokeMethod.GetParameters();
                var paramInfoBox = new ParameterInfoBox(parameters, state, instructions);

                var value = del.DynamicInvoke(paramInfoBox.Args);
                if (invokeMethod.ReturnType != typeof(void))
                {
                    state.Stack.Push(value);
                }

                paramInfoBox.WriteBack(state, instructions);
            })
        ];

        public static readonly List<KeyValuePair<Type, RefHandler>> RefHandlers =
        [
            new(typeof(Interpreter.VariableInfo), new RefHandler(
                static (state, instructions, original) =>
                {
                    var variable = (Interpreter.VariableInfo)original;
                    return variable.GetValue(state);
                },
                static (state, instructions, original, newValue) =>
                {
                    var variable = (Interpreter.VariableInfo)original;
                    variable.SetValue(state, newValue);
                }))
        ];

        internal class ParameterInfoBox
        {
            internal object?[] Args;
            internal object?[] Orig;
            internal bool[] ByRef;

            public ParameterInfoBox(
                ParameterInfo[] parameters,
                Interpreter.MethodState state,
                List<BaseInstruction> instructions)
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

                    var original = arg;
                    var parameter = parameters[i];
                    ByRef[i] = parameter.ParameterType.IsByRef || parameter.IsOut || parameter.IsIn;

                    if (arg is not null && TryReadRef(state, instructions, arg, out var readValue))
                    {
                        arg = readValue;
                    }

                    Orig[i] = original;
                    Args[i] = arg;
                }
            }

            public void WriteBack(Interpreter.MethodState state, List<BaseInstruction> instructions)
            {
                for (int i = 0; i < Args.Length; i++)
                {
                    if (!ByRef[i])
                    {
                        continue;
                    }

                    var arg = Args[i];
                    var orig = Orig[i];

                    if (orig is not null && TryWriteRef(state, instructions, orig, arg))
                    {
                        continue;
                    }
                }

                Args = null!;
                Orig = null!;
                ByRef = null!;
            }
        }

        private static bool TryInvokeRegistered(
            Interpreter.MethodState state,
            List<BaseInstruction> instructions,
            object info)
        {
            var runtimeType = info.GetType();

            foreach (var kv in InvokeHandlers)
            {
                if (runtimeType.IsAssignableTo(kv.Key))
                {
                    kv.Value(state, instructions, info);
                    return true;
                }
            }

            return false;
        }

        private static bool TryReadRef(
            Interpreter.MethodState state,
            List<BaseInstruction> instructions,
            object original,
            out object? value)
        {
            var runtimeType = original.GetType();

            foreach (var kv in RefHandlers)
            {
                if (runtimeType.IsAssignableTo(kv.Key))
                {
                    value = kv.Value.Read(state, instructions, original);
                    return true;
                }
            }

            value = null;
            return false;
        }

        private static bool TryWriteRef(
            Interpreter.MethodState state,
            List<BaseInstruction> instructions,
            object original,
            object? value)
        {
            var runtimeType = original.GetType();

            foreach (var kv in RefHandlers)
            {
                if (runtimeType.IsAssignableTo(kv.Key))
                {
                    kv.Value.Write(state, instructions, original, value);
                    return true;
                }
            }

            return false;
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

            if (!TryInvokeRegistered(state, instructions, infoBoxed))
            {
                throw new InvalidProgramException(
                    $"type mismatch, object of type {infoBoxed.GetType().FullName} is not invokable");
            }
        }
    }
}