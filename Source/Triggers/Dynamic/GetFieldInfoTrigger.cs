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
        "BalintHelper/GetFieldInfoTrigger/LoadConstantInstruction"
        )]
    public class GetFieldInfoTrigger : LoadConstantInstructionTrigger
    {
        public override object? ParseConstantValue(EntityData data)
        {
            var className = data.String("className") ?? throw new ArgumentException("no class name was provided", nameof(data));
            var fieldName = data.String("fieldName") ?? throw new ArgumentException("no field name was provided", nameof(data));
            var fieldTypeName = data.String("fieldType");
            Type? fieldType = null;
            if (!string.IsNullOrWhiteSpace(fieldTypeName))
            {
                fieldType = TypeNameCodec.ParseType(fieldTypeName, AppDomain.CurrentDomain.GetAssemblies()) ?? throw new ArgumentException($"type {fieldTypeName} was not found", nameof(data));
            }
            var type = TypeNameCodec.ParseType(className, AppDomain.CurrentDomain.GetAssemblies()) ?? throw new ArgumentException($"type {className} was not found", nameof(data));
            const BindingFlags allFlags =
                BindingFlags.Public
                | BindingFlags.NonPublic
                | BindingFlags.Static
                | BindingFlags.Instance;

            var possibleMatches = type.GetFields(allFlags).Where(x => x.Name == fieldName && (fieldType is null || x.FieldType == fieldType)).ToArray();

            if (possibleMatches.Length == 0)
            {
                throw new ArgumentException($"field {fieldName} was not found in class {type}", nameof(data));
            }

            return possibleMatches.MinBy(x => x.DeclaringType?.TypeDepth() ?? int.MaxValue);
        }
        public GetFieldInfoTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
        }
    }
}
