using DynamicInstructions;
using DynamicInstructions.Instructions.Abstract;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.BalintHelper.Utils.Dynamic
{
    public class LogInstruction : BaseInstruction
    {
        public override void Execute(Interpreter.MethodState state, List<BaseInstruction> instructions)
        {
            if (!state.Stack.TryPop(out var value))
            {
                throw new InvalidProgramException("stack imbalance, failed to get value to log");
            }
            Logger.Log(LogLevel.Info, "BalintHelper/LogInstruction", value?.ToString());
        }
    }
}
