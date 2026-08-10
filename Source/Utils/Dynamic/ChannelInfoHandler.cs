using Celeste.Mod.BalintHelper.Record;
using DynamicInstructions;
using DynamicInstructions.Instructions.Abstract;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.BalintHelper.Utils.Dynamic
{
    public class ChannelInfoHandler : CustomInstructionValueHandler, IReadHandler, IWriteHandler, IRefHandler
    {
        public override Type TargetType => typeof(ChannelInfo);
        public object? Read(Interpreter.MethodState state, List<BaseInstruction> instructions, object infoBoxed)
        {
            if (infoBoxed is not ChannelInfo info)
            {
                throw new InvalidProgramException("type mismatch, failed to get channel info to read");
            }
            return AuspiciousChannelInterop.readChannel(info.Name);
        }
        public void Write(Interpreter.MethodState state, List<BaseInstruction> instructions, object infoBoxed, object? valueBoxed)
        {
            if (infoBoxed is not ChannelInfo info)
            {
                throw new InvalidProgramException("type mismatch, failed to get channel info to write");
            }
            double value;
            try
            {
                value = (double)(dynamic?)valueBoxed;
            }
            catch (Exception)
            {

                throw new InvalidProgramException("type mismatch, failed to write channel value");
            }
            AuspiciousChannelInterop.setChannel(info.Name, value);
        }

        public object? RefRead(Interpreter.MethodState state, List<BaseInstruction> instructions, object original)
            => Read(state, instructions, original);

        public void RefWrite(Interpreter.MethodState state, List<BaseInstruction> instructions, object original, object? valueBoxed)
            => Write(state, instructions, original, valueBoxed);
    }
}
