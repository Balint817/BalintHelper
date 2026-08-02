using Celeste.Mod.BalintHelper.Record;
using DynamicInstructions;
using DynamicInstructions.Instructions.Abstract;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.BalintHelper.Utils.Dynamic
{
    public class CounterInfoHandler : CustomInstructionValueHandler, IReadHandler, IWriteHandler, IRefHandler
    {
        public override Type TargetType => typeof(CounterInfo);

        public object? Read(Interpreter.MethodState state, List<BaseInstruction> instructions, object infoBoxed)
        {
            if (infoBoxed is not CounterInfo info)
            {
                throw new InvalidProgramException("type mismatch, failed to get counter info to read");
            }
            return ((Level)Engine.Scene).Session.GetCounter(info.Name);
        }
        public void Write(Interpreter.MethodState state, List<BaseInstruction> instructions, object infoBoxed, object? valueBoxed)
        {
            if (infoBoxed is not CounterInfo info)
            {
                throw new InvalidProgramException("type mismatch, failed to get counter info to write");
            }
            int value;
            try
            {
                value = (int)(dynamic?)valueBoxed;
            }
            catch (Exception)
            {

                throw new InvalidProgramException("type mismatch, failed to write counter value");
            }
            ((Level)Engine.Scene).Session.SetCounter(info.Name, value);
        }
        public object? RefRead(Interpreter.MethodState state, List<BaseInstruction> instructions, object original)
            => Read(state, instructions, original);

        public void RefWrite(Interpreter.MethodState state, List<BaseInstruction> instructions, object original, object? valueBoxed)
            => Write(state, instructions, original, valueBoxed);
    }
}
