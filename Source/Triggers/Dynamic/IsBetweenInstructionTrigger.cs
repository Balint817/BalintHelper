using Celeste.Mod.BalintHelper.Utils.Dynamic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System;

namespace Celeste.Mod.BalintHelper.Triggers.Dynamic
{
    [CustomEntity("BalintHelper/IsBetweenInstructionTrigger")]
    public class IsBetweenInstructionTrigger : BaseInstructionTrigger
    {
        private static readonly Type[] types = [typeof(bool), typeof(bool)];
        public override Type[] ConstructorParameterTypes => types;

        public override object?[] GetConstructorParameters()
        {
            return [BottomInclusive, TopInclusive];
        }
        public readonly bool TopInclusive;
        public readonly bool BottomInclusive;
        public IsBetweenInstructionTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            InstructionType = typeof(IsBetweenInstruction);
            TopInclusive = data.Bool("topInclusive", true);
            BottomInclusive = data.Bool("bottomInclusive", true);
        }
    }
}
