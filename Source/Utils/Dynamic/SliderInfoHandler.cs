using Celeste.Mod.BalintHelper.Record;
using DynamicInstructions;
using DynamicInstructions.Instructions.Abstract;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.BalintHelper.Utils.Dynamic
{
    public class SliderInfoHandler : CustomInstructionValueHandler, IReadHandler, IWriteHandler, IRefHandler
    {
        public override Type TargetType => typeof(SliderInfo);

        public object? Read(Interpreter.MethodState state, List<BaseInstruction> instructions, object infoBoxed)
        {
            if (infoBoxed is not SliderInfo info)
            {
                throw new InvalidProgramException("type mismatch, failed to get slider info to read");
            }
            return ((Level)Engine.Scene).Session.GetSlider(info.Name);
        }
        public void Write(Interpreter.MethodState state, List<BaseInstruction> instructions, object infoBoxed, object? valueBoxed)
        {
            if (infoBoxed is not SliderInfo info)
            {
                throw new InvalidProgramException("type mismatch, failed to get slider info to write");
            }
            float value;
            try
            {
                value = (float)(dynamic?)valueBoxed;
            }
            catch (Exception)
            {

                throw new InvalidProgramException("type mismatch, failed to write slider value");
            }
            ((Level)Engine.Scene).Session.SetSlider(info.Name, value);
        }

        public object? RefRead(Interpreter.MethodState state, List<BaseInstruction> instructions, object original)
            => Read(state, instructions, original);

        public void RefWrite(Interpreter.MethodState state, List<BaseInstruction> instructions, object original, object? valueBoxed)
            => Write(state, instructions, original, valueBoxed);
    }
}
