using Celeste.Mod.BalintHelper.Triggers.Dynamic;
using DynamicInstructions;
using DynamicInstructions.Instructions.Abstract;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.BalintHelper.Utils.Dynamic
{
    public class SessionStringHandler : CustomInstructionValueHandler, IReadHandler, IWriteHandler, IRefHandler
    {
        public override Type TargetType => typeof(SessionStringInfo);
        public object? Read(Interpreter.MethodState state, List<BaseInstruction> instructions, object infoBoxed)
        {
            if (infoBoxed is not SessionStringInfo info)
            {
                throw new InvalidProgramException("type mismatch, failed to get static variable info to read");
            }
            BalintHelperModule.Session.SessionsStrings.TryGetValue(info.Name, out var value);
            return value;
        }
        public void Write(Interpreter.MethodState state, List<BaseInstruction> instructions, object infoBoxed, object? valueBoxed)
        {
            if (infoBoxed is not SessionStringInfo info)
            {
                throw new InvalidProgramException("type mismatch, failed to get static variable info to write");
            }
            BalintHelperModule.Session.SessionsStrings[info.Name] = valueBoxed?.ToString();
        }
        public object? RefRead(Interpreter.MethodState state, List<BaseInstruction> instructions, object original)
            => Read(state, instructions, original);

        public void RefWrite(Interpreter.MethodState state, List<BaseInstruction> instructions, object original, object? valueBoxed)
            => Write(state, instructions, original, valueBoxed);
    }
}
