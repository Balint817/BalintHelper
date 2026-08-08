using DynamicInstructions;
using DynamicInstructions.Instructions.Abstract;
using DynamicInstructions.Instructions.Invoke;
using DynamicInstructions.Instructions.Read;
using DynamicInstructions.Instructions.Write;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;

namespace Celeste.Mod.BalintHelper.Utils.Dynamic
{
    public abstract class CustomInstructionValueHandler
    {
        private static CustomInstructionValueHandler[]? _allInstances;
        private static ReadOnlyCollection<CustomInstructionValueHandler>? _allInstancesReadonly;
        public static ReadOnlyCollection<CustomInstructionValueHandler> AllInstances
        {
            get
            {
                if (_allInstancesReadonly is null)
                {
                    _allInstances = [.. typeof(CustomInstructionValueHandler).Assembly.DefinedTypes.Where(x => x.IsAssignableTo(typeof(CustomInstructionValueHandler)) && !x.IsAbstract).Select(x => (CustomInstructionValueHandler)Activator.CreateInstance(x)!)];
                    _allInstancesReadonly = Array.AsReadOnly(_allInstances);
                }
                return _allInstancesReadonly;
            }
        }
        public readonly RefHandler? RefHandler;
        public abstract Type TargetType { get; }
        public CustomInstructionValueHandler()
        {
            if (this is IRefHandler refHandler)
            {
                RefHandler = new RefHandler(refHandler.RefRead, refHandler.RefWrite);
            }
        }

        public void Load()
        {
            if (this is IInvokeHandler invokeHandler)
            {
                InvokeInstruction.InvokeHandlers.Add(new(TargetType, invokeHandler.Invoke));
            }
            if (this is IRefHandler refHandler)
            {
                InvokeInstruction.RefHandlers.Add(new(TargetType, RefHandler!));
            }
            if (this is IReadHandler readHandler)
            {
                ReadInstruction.ReadHandlers.Add(new(TargetType, readHandler.Read));
            }
            if (this is IWriteHandler writeHandler)
            {
                WriteInstruction.WriteHandlers.Add(new(TargetType, writeHandler.Write));
            }
            if (this is IReadIndexerHandler readIndexerHandler)
            {
                ReadIndexerInstruction.ReadIndexerHandlers.Add(new(TargetType, readIndexerHandler.ReadIndexer));
            }
            if (this is IWriteIndexerHandler writeIndexerHandler)
            {
                WriteIndexerInstruction.WriteIndexerHandlers.Add(new(TargetType, writeIndexerHandler.WriteIndexer));
            }
        }

        public void Unload()
        {
            if (this is IInvokeHandler invokeHandler)
            {
                InvokeInstruction.InvokeHandlers.RemoveAll(x => x.Key == TargetType && x.Value == invokeHandler.Invoke);
            }
            if (this is IRefHandler)
            {
                InvokeInstruction.RefHandlers.RemoveAll(x => x.Key == TargetType && x.Value == RefHandler);
            }
            if (this is IReadHandler readHandler)
            {
                ReadInstruction.ReadHandlers.RemoveAll(x => x.Key == TargetType && x.Value == readHandler.Read);
            }
            if (this is IWriteHandler writeHandler)
            {
                WriteInstruction.WriteHandlers.RemoveAll(x => x.Key == TargetType && x.Value == writeHandler.Write);
            }
            if (this is IReadIndexerHandler readIndexerHandler)
            {
                ReadIndexerInstruction.ReadIndexerHandlers.RemoveAll(x => x.Key == TargetType && x.Value == readIndexerHandler.ReadIndexer);
            }
            if (this is IWriteIndexerHandler writeIndexerHandler)
            {
                WriteIndexerInstruction.WriteIndexerHandlers.RemoveAll(x => x.Key == TargetType && x.Value == writeIndexerHandler.WriteIndexer);
            }
        }

    }
    public interface IInvokeHandler
    {
        void Invoke(Interpreter.MethodState state, List<BaseInstruction> instructions, object infoBoxed);
    }
    public interface IRefHandler
    {
        object? RefRead(Interpreter.MethodState state, List<BaseInstruction> instructions, object originalBoxed);
        void RefWrite(Interpreter.MethodState state, List<BaseInstruction> instructions, object originalBoxed, object? valueBoxed);
    }
    public interface IReadHandler
    {
        object? Read(Interpreter.MethodState state, List<BaseInstruction> instructions, object infoBoxed);
    }
    public interface IWriteHandler
    {
        void Write(Interpreter.MethodState state, List<BaseInstruction> instructions, object infoBoxed, object? valueBoxed);
    }
    public interface IReadIndexerHandler
    {
        object? ReadIndexer(Interpreter.MethodState state, List<BaseInstruction> instructions, object infoBoxed);
    }
    public interface IWriteIndexerHandler
    {
        void WriteIndexer(Interpreter.MethodState state, List<BaseInstruction> instructions, object infoBoxed);
    }
    public interface IVariableHandler : IReadHandler, IWriteHandler, IRefHandler
    {
        bool IsDefined(Interpreter.MethodState state, List<BaseInstruction> instructions, object infoBoxed);
        void InitVariable(Interpreter.MethodState state, List<BaseInstruction> instructions, object infoBoxed, object? valueBoxed);
    }
}
