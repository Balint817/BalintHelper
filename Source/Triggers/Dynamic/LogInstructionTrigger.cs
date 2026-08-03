using Celeste.Mod.BalintHelper.Utils.Dynamic;
using Celeste.Mod.Entities;
using DynamicInstructions.Instructions.Abstract;
using DynamicInstructions.Instructions.Basic;
using DynamicInstructions.Instructions.Read;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.BalintHelper.Triggers.Dynamic
{
    [CustomEntity("BalintHelper/LogInstructionTrigger")]
    public class LogInstructionTrigger : BaseInstructionTrigger
    {
        public LogInstructionTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            InstructionType = typeof(LogInstruction);
        }
    }
}
