using Celeste.Mod.BalintHelper.Utils;
using Celeste.Mod.BalintHelper.Utils.Dynamic;
using Celeste.Mod.Entities;
using DynamicInstructions;
using DynamicInstructions.Instructions;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.BalintHelper.Triggers.Dynamic
{
    [CustomEntity(
        "BalintHelper/GetConstructorInfoTrigger/LoadConstantInstruction"
    )]
    public class GetConstructorInfoTrigger : GetThenActInstructionTrigger
    {
        public override object? ParseConstantValue(EntityData data)
        {
            var className = data.String("className") ?? throw new ArgumentException("no class name was provided", nameof(data));
            var argumentTypeNames = data.String("argumentTypes", "");

            var type = TypeNameCodec.ParseType(className, AppDomain.CurrentDomain.GetAssemblies())
                ?? throw new ArgumentException($"type {className} was not found", nameof(data));

            var argumentTypes = string.IsNullOrWhiteSpace(argumentTypeNames)
                ? Type.EmptyTypes
                : TypeNameCodec.ParseTypeList(argumentTypeNames, AppDomain.CurrentDomain.GetAssemblies())
                    .Select(t => t ?? throw new ArgumentException($"argument type {argumentTypeNames} was not found", nameof(data)))
                    .ToArray();

            const BindingFlags flags =
                BindingFlags.Public
                | BindingFlags.NonPublic
                | BindingFlags.Instance;

            var ctor = type.GetConstructor(flags, binder: null, types: argumentTypes, modifiers: null)
                ?? throw new ArgumentException(
                    $"constructor was not found in class {type} with the provided argument types",
                    nameof(data)
                );
            return ctor;
        }

        public GetConstructorInfoTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
        }
    }
}