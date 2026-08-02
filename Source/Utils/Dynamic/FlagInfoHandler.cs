using Celeste.Mod.BalintHelper.Record;
using DynamicInstructions;
using DynamicInstructions.Instructions;
using DynamicInstructions.Instructions.Abstract;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.BalintHelper.Utils.Dynamic
{
    public class FlagInfoHandler: CustomInstructionValueHandler, IReadHandler, IWriteHandler, IRefHandler
    {
        public override Type TargetType => typeof(FlagInfo);

        public object? Read(Interpreter.MethodState state, List<BaseInstruction> instructions, object infoBoxed)
        {
            if (infoBoxed is not FlagInfo info)
            {
                throw new InvalidProgramException("type mismatch, failed to get flag info to read");
            }
            return ((Level)Engine.Scene).Session.GetFlag(info.Name);
        }

        public void Write(Interpreter.MethodState state, List<BaseInstruction> instructions, object infoBoxed, object? valueBoxed)
        {
            if (infoBoxed is not FlagInfo info)
            {
                throw new InvalidProgramException("type mismatch, failed to get flag info to write");
            }
            if (valueBoxed.IsTrue() is not { } b)
            {
                throw new InvalidProgramException("type mismatch, failed to write flag value");
            }
            ((Level)Engine.Scene).Session.SetFlag(info.Name, b);
        }

        public object? RefRead(Interpreter.MethodState state, List<BaseInstruction> instructions, object original)
            => Read(state, instructions, original);

        public void RefWrite(Interpreter.MethodState state, List<BaseInstruction> instructions, object original, object? valueBoxed)
            => Write(state, instructions, original, valueBoxed);
    }
}
