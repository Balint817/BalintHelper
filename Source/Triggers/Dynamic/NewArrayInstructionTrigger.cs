using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System;

namespace Celeste.Mod.BalintHelper.Triggers.Dynamic
{
    [CustomEntity(
        "BalintHelper/NewArrayInstructionTrigger/NewArrayInstruction"
        )]
    public class NewArrayInstructionTrigger : TypedInstructionTrigger
    {
        static readonly Type[] _params = [typeof(Type), typeof(int)];
        public override object?[] GetConstructorParameters()
        {
            return [TypeParameter ?? throw new InvalidOperationException("TypeParameter is not set"), Dimensions == 0 ? throw new InvalidOperationException("Invalid dimension") : Dimensions];
        }
        public override Type[] ConstructorParameterTypes => _params;
        public readonly int Dimensions;
        public NewArrayInstructionTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            Dimensions = data.Int("dimensions", 0);
            if (Dimensions < 1)
            {
                throw new ArgumentException("invalid array dimension", nameof(data));
            }
        }
    }

}
