using Celeste.Mod.BalintHelper.Record;
using DynamicInstructions;
using DynamicInstructions.Instructions.Abstract;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.BalintHelper.Utils.Dynamic
{
    public class EntityFilterHandler : IReadHandler, IInvokeHandler
    {
        public void Invoke(Interpreter.MethodState state, List<BaseInstruction> instructions, object infoBoxed) => Read(state, instructions, infoBoxed);
        public object? Read(Interpreter.MethodState state, List<BaseInstruction> instructions, object infoBoxed)
        {
            if (infoBoxed is not EntityInfo info)
            {
                throw new InvalidProgramException("type mismatch, failed to get entity info to read");
            }
            if (!info.Any)
            {
                return Engine.Scene.Entities.ToArray();
            }
            return Engine.Scene.Entities.Where(entity =>
            (entity.SourceData?.ID is { } id && info.IDs.Contains(id))
            || info.Types.Any(t => entity.GetType() == t)
            ).ToArray();
        }
    }
}
