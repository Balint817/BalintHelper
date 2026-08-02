using Celeste.Mod.BalintHelper.Record;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System;

namespace Celeste.Mod.BalintHelper.Triggers.Dynamic
{
    [CustomEntity(
        "BalintHelper/GetEntityInfoTrigger/LoadConstantInstruction"
        )]
    public class GetEntityInfoTrigger : LoadConstantInstructionTrigger
    {
        public override object? ParseConstantValue(EntityData data)
        {
            var types = data.String("types", "");
            return new EntityInfo(types, AppDomain.CurrentDomain.GetAssemblies());
        }
        public GetEntityInfoTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
        }
    }
}
