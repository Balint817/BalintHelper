using DynamicInstructions.Instructions.Abstract;
using DynamicInstructions.Instructions.Pointers;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace DynamicInstructions
{
    public sealed class Interpreter : IDisposable
    {
        public readonly Dictionary<string, object?> GlobalVariables = [];
        public enum VariableType
        {
            Local,
            Global,
            Argument
        }
        public class VariableInfo
        {
            private string _name;
            public string Name
            {
                get
                {
                    return _name;
                }
                [MemberNotNull(nameof(_name))]
                private init
                {
                    _name = value;
                    if (int.TryParse(_name, out var n) && n >= 0)
                    {
                        Index = n;
                    }
                    else
                    {
                        Index = -1;
                    }
                }
            }
            public int Index { get; private init; } = -1;
            public readonly VariableType Type;
            public VariableInfo(string name, VariableType variableType)
            {
                Name = name;
                Type = variableType;
            }
            public object? GetValue(MethodState state)
            {
                switch (Type)
                {
                    case VariableType.Local:
                        if (state.LocalVariables.TryGetValue(Name, out var localValue))
                        {
                            return localValue;
                        }
                        throw new InvalidProgramException($"attempted to access undefined local variable {Name}");
                    case VariableType.Global:
                        if (state.Interpreter.GlobalVariables.TryGetValue(Name, out var globalValue))
                        {
                            return globalValue;
                        }
                        throw new InvalidProgramException($"attempted to access undefined global variable {Name}");
                    case VariableType.Argument:
                        if (Index >= 0 && Index < state.Args.Length)
                        {
                            return state.Args[Index];
                        }
                        throw new InvalidProgramException($"attempted to access undefined argument at index {Index}");
                    default:
                        throw new InvalidProgramException($"critical error: unknown variable type: {Type}");
                }
            }
            public void SetValue(MethodState state, object? value)
            {
                switch (Type)
                {
                    case VariableType.Local:
                        state.LocalVariables[Name] = value;
                        break;
                    case VariableType.Global:
                        state.Interpreter.GlobalVariables[Name] = value;
                        break;
                    case VariableType.Argument:
                        if (Index >= 0 && Index < state.Args.Length)
                        {
                            state.Args[Index] = value;
                            break;
                        }
                        throw new InvalidProgramException($"attempted to access undefined argument at index {Index}");
                    default:
                        throw new InvalidProgramException($"critical error: unknown variable type: {Type}");
                }
            }
        }
        public sealed class MethodState : IDisposable
        {
            public Dictionary<string, object?> LocalVariables { get; private set; } = [];
            public Stack<object?> Stack { get; private set; } = [];
            public Interpreter Interpreter { get; private set; }
            public object?[] Args { get; private set; }
            public int Cursor { get; set; } = 0;
            public MethodState(Interpreter interpreter, object?[]? args = null)
            {
                Interpreter = interpreter;
                Args = args ?? [];
            }
            public MethodState Copy()
            {
                var copy = new MethodState(Interpreter, Args);
                foreach (var kvp in LocalVariables)
                {
                    copy.LocalVariables[kvp.Key] = kvp.Value;
                }
                foreach (var item in Stack)
                {
                    copy.Stack.Push(item);
                }
                copy.Cursor = Cursor;
                return copy;
            }

            public void Dispose()
            {
                LocalVariables.Clear();
                LocalVariables = null!;
                Stack.Clear();
                Stack = null!;
                Interpreter = null!;
                Args = null!;
            }
        }
        public class DynamicMethodDefinition
        {
            public readonly ReadOnlyCollection<BaseInstruction> Body;
            public readonly int ArgCount;
            internal readonly List<BaseInstruction> _body;
            public DynamicMethodDefinition(List<BaseInstruction> body, int argCount)
            {
                _body = [.. body];
                Body = new ReadOnlyCollection<BaseInstruction>(body);
                ArgCount = argCount;
                if (argCount < 0)
                {
                    throw new ArgumentException("argument count cannot be negative", nameof(argCount));
                }
            }
        }
        public class DynamicMethodInfo
        {
            public readonly string Name;
            public DynamicMethodInfo(string name)
            {
                Name = name;
            }
        }

        internal readonly Dictionary<string, DynamicMethodDefinition> _dynamicMethods = [];
        public ReadOnlyDictionary<string, DynamicMethodDefinition> DynamicMethods => new(_dynamicMethods);

        public bool RegisterDynamicMethod(string methodName, List<BaseInstruction> instructions, int argCount = 0)
        {
            return _dynamicMethods.TryAdd(methodName, new DynamicMethodDefinition(instructions, argCount));
        }
        public bool InvokeDynamicMethod(string methodName, out object? returnValue, object?[]? args = null)
        {
            return InvokeDynamicMethod(methodName, out returnValue, args, null);
        }
        internal bool InvokeDynamicMethod(string methodName, out object? returnValue, object?[]? args, MethodState? state)
        {
            if (!_dynamicMethods.TryGetValue(methodName, out var dynamicMethodDefinition))
            {
                throw new InvalidProgramException($"attempted to invoke undefined dynamic method {methodName}");
            }
            var argsList = args?.ToList() ?? [];
            if (dynamicMethodDefinition.ArgCount < argsList.Count)
            {
                throw new InvalidProgramException($"attempted to invoke dynamic method {methodName} with {argsList.Count} argument(s) but {dynamicMethodDefinition.ArgCount} were expected");
            }
            if (dynamicMethodDefinition.ArgCount > argsList.Count)
            {
                if (state is null)
                {
                    throw new InvalidProgramException($"attempted to invoke dynamic method {methodName} with {argsList.Count} argument(s) but {dynamicMethodDefinition.ArgCount} were expected");
                }
                while (argsList.Count < dynamicMethodDefinition.ArgCount)
                {
                    if (!state.Stack.TryPop(out var value))
                    {
                        throw new InvalidProgramException($"stack imbalance, failed to obtain {dynamicMethodDefinition.ArgCount} parameters for dynamic method {methodName} (only got {argsList.Count})");
                    }
                    argsList.Add(value);
                }
            }
            return InterpretMethod(dynamicMethodDefinition, out returnValue, [.. argsList]);
        }
        internal unsafe bool InterpretMethod(DynamicMethodDefinition dynamicMethodDefinition, out object? returnValue, object?[]? args = null)
        {
            using var state = new MethodState(this, args);
            var localPointers = new List<UIntPtr>();
            try
            {
                var instructions = dynamicMethodDefinition._body;
                for (; state.Cursor < instructions.Count; state.Cursor++)
                {
                    var instruction = instructions[state.Cursor];
                    switch (instruction)
                    {
                        case AllocInstruction _:
                            {
                                instruction.Execute(state, instructions);
                                var size = (nuint)state.Stack.Pop()!;
                                var ptr = new UIntPtr(NativeMemory.AllocZeroed(size));
                                localPointers.Add(ptr);
                                state.Stack.Push(ptr);
                                break;
                            }
                        default:
                            {
                                instruction.Execute(state, instructions);
                                break;
                            }
                    }
                }
                if (state.Stack.TryPop(out var finalValue))
                {
                    if (state.Stack.Count != 0)
                    {
                        throw new InvalidProgramException("stack imbalance on method return");
                    }
                    returnValue = finalValue;
                    return true;
                }
                returnValue = null;
                return false;
            }
            finally
            {
                foreach (var ptr in localPointers)
                {
                    NativeMemory.Free(ptr.ToPointer());
                }
            }
        }

        public void Dispose()
        {
            GlobalVariables.Clear();
            // clear events here once implemented
        }
    }
}
