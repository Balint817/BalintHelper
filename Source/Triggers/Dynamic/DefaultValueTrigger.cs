using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Runtime.CompilerServices;

namespace Celeste.Mod.BalintHelper.Triggers.Dynamic
{
    [CustomEntity(
        "BalintHelper/DefaultValueTrigger/LoadConstantInstruction"
        )]
    public class DefaultValueTrigger : TypedConstantInstructionTrigger
    {
        protected override object? GetValueFromType(Type type)
        {
            return type.IsValueType ? RuntimeHelpers.GetUninitializedObject(type) : null;
        }
        public DefaultValueTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
        }
    }
}
