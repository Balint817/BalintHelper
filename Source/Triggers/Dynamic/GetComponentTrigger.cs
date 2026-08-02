using Celeste.Mod.Entities;
using DynamicInstructions.Instructions;
using DynamicInstructions.Instructions.Abstract;
using DynamicInstructions.Instructions.Basic;
using DynamicInstructions.Instructions.Invoke;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.BalintHelper.Triggers.Dynamic
{
    [CustomEntity(
        "BalintHelper/GetComponentTrigger/NopInstruction"
        )]
    public class GetComponentTrigger : CompoundInstructionTrigger
    {
        static readonly MethodInfo getMethod = typeof(Entity).GetMethod(nameof(Entity.Get), BindingFlags.Public | BindingFlags.Instance)!;
        public readonly MethodInfo GenericGetMethod;
        public GetComponentTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            var typeName = data.String("componentType") ?? throw new ArgumentException("no component type was provided", nameof(data));
            var type = TypeNameCodec.ParseType(typeName, AppDomain.CurrentDomain.GetAssemblies())
                ?? throw new ArgumentException($"type {typeName} was not found", nameof(data));
            GenericGetMethod = getMethod.MakeGenericMethod(type);
        }
        override public IEnumerable<BaseInstruction> GetCompoundInstructions()
        {
            return [new LoadConstantInstruction(GenericGetMethod), new InvokeInstruction()];
        }
    }
}

