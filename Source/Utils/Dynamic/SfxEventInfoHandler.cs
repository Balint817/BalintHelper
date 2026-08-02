using Celeste.Mod.BalintHelper.Record;
using DynamicInstructions;
using DynamicInstructions.Instructions.Abstract;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.BalintHelper.Utils.Dynamic
{
    public class SfxEventInfoHandler : CustomInstructionValueHandler, IInvokeHandler
    {
        public override Type TargetType => typeof(SfxEventInfo);

        public void Invoke(Interpreter.MethodState state, List<BaseInstruction> instructions, object infoBoxed)
        {
            if (infoBoxed is not SfxEventInfo info)
            {
                throw new InvalidProgramException("type mismatch, failed to get sfx event info to play");
            }

            if (!state.Stack.TryPop(out var positionBoxed))
            {
                throw new InvalidProgramException("stack imbalance, failed to obtain position argument to play sfx event");
            }

            Vector2? position = positionBoxed switch
            {
                null => null,
                Vector2 v => v,
                _ => throw new InvalidProgramException("type mismatch, expected a Vector2 or null position argument to play sfx event")
            };

            var instance = info.Play(position);
            state.Stack.Push(instance);
        }
    }
}
