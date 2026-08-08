using Celeste.Mod.BalintHelper.Utils.Dynamic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

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
