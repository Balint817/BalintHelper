using Celeste.Mod.BalintHelper.Record;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System;

namespace Celeste.Mod.BalintHelper.Triggers.Dynamic
{
    [CustomEntity(
        "BalintHelper/GetEntityInfoTrigger/LoadConstantInstruction"
        )]
    public class GetEntityInfoTrigger : GetThenActInstructionTrigger
    {
        public override object? ParseConstantValue(EntityData data)
        {
            var types = data.String("types", "");
            var mode = data.Enum("mode", EntityInfo.FilterMode.None);
            return new EntityInfo(types, AppDomain.CurrentDomain.GetAssemblies(), mode);
        }
        public GetEntityInfoTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
        }
    }
}
