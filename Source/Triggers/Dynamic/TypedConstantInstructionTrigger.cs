using Microsoft.Xna.Framework;
using System;

namespace Celeste.Mod.BalintHelper.Triggers.Dynamic
{
    public abstract class TypedConstantInstructionTrigger : TypedInstructionTrigger
    {
        override public Type[] ConstructorParameterTypes => [typeof(object)];
        public override object?[] GetConstructorParameters()
        {
            if (TypeParameter is null)
            {
                throw new InvalidOperationException("TypeParameter is not set");
            }
            return [GetValueFromType(TypeParameter)];
        }
        protected abstract object? GetValueFromType(Type type);
        public TypedConstantInstructionTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
        }
    }
}
