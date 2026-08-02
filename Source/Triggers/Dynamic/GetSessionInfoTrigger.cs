using Celeste.Mod.BalintHelper.Record;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System;

namespace Celeste.Mod.BalintHelper.Triggers.Dynamic
{
    [CustomEntity(
        "BalintHelper/GetSessionInfoTrigger/LoadConstantInstruction"
        )]
    public class GetSessionInfoTrigger : LoadConstantInstructionTrigger
    {
        public enum SessionInfoType
        {
            None,
            Flag,
            Counter,
            Slider,
        }
        public override object? ParseConstantValue(EntityData data)
        {
            var name = data.String("name") ?? throw new ArgumentException("no session variable name was provided", nameof(data));
            var type = data.Enum("type", SessionInfoType.None);
            return type switch
            {
                SessionInfoType.Flag => new FlagInfo(name),
                SessionInfoType.Counter => new CounterInfo(name),
                SessionInfoType.Slider => new SliderInfo(name),
                _ => throw new ArgumentException($"Unknown session variable type {type}", nameof(data)),
            };
        }
        public GetSessionInfoTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
        }
    }
}
