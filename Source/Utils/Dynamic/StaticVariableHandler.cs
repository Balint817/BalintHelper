using Celeste.Mod.BalintHelper.Entities.Dynamic;
using Celeste.Mod.BalintHelper.Record;
using DynamicInstructions;
using DynamicInstructions.Instructions.Abstract;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.BalintHelper.Utils.Dynamic
{
    public class StaticVariableHandler : CustomInstructionValueHandler, IVariableHandler
    {
        public override Type TargetType => typeof(StaticVariableController.Info);

        public object? Read(Interpreter.MethodState state, List<BaseInstruction> instructions, object infoBoxed)
        {
            if (infoBoxed is not StaticVariableController.Info info)
            {
                throw new InvalidProgramException("type mismatch, failed to get static variable info to read");
            }
            return info.GetValue();
        }
        public void Write(Interpreter.MethodState state, List<BaseInstruction> instructions, object infoBoxed, object? valueBoxed)
        {
            if (infoBoxed is not StaticVariableController.Info info)
            {
                throw new InvalidProgramException("type mismatch, failed to get static variable info to write");
            }
            info.SetValue(valueBoxed);
        }

        public object? RefRead(Interpreter.MethodState state, List<BaseInstruction> instructions, object original)
            => Read(state, instructions, original);

        public void RefWrite(Interpreter.MethodState state, List<BaseInstruction> instructions, object original, object? valueBoxed)
            => Write(state, instructions, original, valueBoxed);

        public bool IsDefined(Interpreter.MethodState state, List<BaseInstruction> instructions, object infoBoxed)
        {
            if (infoBoxed is not StaticVariableController.Info info)
            {
                throw new InvalidProgramException("type mismatch, failed to get static variable info to read");
            }
            return info.IsDefined();
        }
        public void InitVariable(Interpreter.MethodState state, List<BaseInstruction> instructions, object infoBoxed, object? valueBoxed)
        {
            if (infoBoxed is not StaticVariableController.Info info)
            {
                throw new InvalidProgramException("type mismatch, failed to get static variable info to init");
            }
            if (!info.IsDefined())
            {
                info.SetValue(valueBoxed);
            }
        }
    }
}
