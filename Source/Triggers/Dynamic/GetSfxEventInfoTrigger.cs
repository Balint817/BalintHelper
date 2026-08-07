using Celeste.Mod.BalintHelper.Record;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.BalintHelper.Triggers.Dynamic
{
    [CustomEntity(
        "BalintHelper/GetSfxEventInfoTrigger/LoadConstantInstruction"
        )]
    public class GetSfxEventInfoTrigger : GetThenActInstructionTrigger
    {
        public override object? ParseConstantValue(EntityData data)
        {
            var eventPath = data.String("eventPath", "");
            var parameters = data.String("parameters", "");
            var loop = data.Bool("loop", false);
            return new SfxEventInfo(eventPath, parameters, loop);
        }
        public GetSfxEventInfoTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
        }
    }
}
