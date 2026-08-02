using Microsoft.Xna.Framework;
using System;

namespace Celeste.Mod.BalintHelper.Triggers.Dynamic
{
    public abstract class LoadConstantInstructionTrigger : BaseInstructionTrigger
    {
        static readonly Type[] _params = [typeof(object)];
        public override Type[] ConstructorParameterTypes => _params;
        public readonly object? ConstantValue;
        public override object?[] GetConstructorParameters()
        {
            return [ConstantValue];
        }
        public LoadConstantInstructionTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            ConstantValue = ParseConstantValue(data);
        }
        public abstract object? ParseConstantValue(EntityData data);
    }

}
