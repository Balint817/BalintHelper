using Celeste.Mod.BalintHelper.Utils;
using Celeste.Mod.BalintHelper.Utils.Dynamic;
using Celeste.Mod.Entities;
using DynamicInstructions;
using DynamicInstructions.Instructions;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.BalintHelper.Triggers.Dynamic
{
    [CustomEntity(
        "BalintHelper/GetPropertyInfoTrigger/LoadConstantInstruction"
    )]
    public class GetPropertyInfoTrigger : GetThenActInstructionTrigger
    {
        public override object? ParseConstantValue(EntityData data)
        {
            var className = data.String("className") ?? throw new ArgumentException("no class name was provided", nameof(data));
            var propertyName = data.String("propertyName") ?? throw new ArgumentException("no property name was provided", nameof(data));
            var returnTypeName = data.String("returnType", "");
            var indexerTypeNames = data.String("indexerTypes", "");

            var type = TypeNameCodec.ParseType(className, AppDomain.CurrentDomain.GetAssemblies())
                ?? throw new ArgumentException($"type {className} was not found", nameof(data));

            Type? returnType = null;
            if (!string.IsNullOrWhiteSpace(returnTypeName))
            {
                returnType = TypeNameCodec.ParseType(returnTypeName, AppDomain.CurrentDomain.GetAssemblies())
                    ?? throw new ArgumentException($"return type {returnTypeName} was not found", nameof(data));
            }

            var indexerTypes = Array.Empty<Type>();
            if (!string.IsNullOrWhiteSpace(indexerTypeNames))
            {
                indexerTypes = TypeNameCodec.ParseTypeList(indexerTypeNames, AppDomain.CurrentDomain.GetAssemblies())
                    .Select(t => t ?? throw new ArgumentException($"indexer type {indexerTypeNames} was not found", nameof(data)))
                    .ToArray();
            }

            const BindingFlags allFlags =
                BindingFlags.Public
                | BindingFlags.NonPublic
                | BindingFlags.Static
                | BindingFlags.Instance;

            var foundProperties = new List<PropertyInfo>();

            foreach (var p in type.GetProperties(allFlags))
            {
                if (p.Name != propertyName)
                {
                    continue;
                }

                if (returnType is not null && p.PropertyType != returnType)
                {
                    continue;
                }

                var indexParameters = p.GetIndexParameters();
                if (indexParameters.Length != indexerTypes.Length)
                {
                    continue;
                }

                bool matches = true;
                for (int i = 0; i < indexParameters.Length; i++)
                {
                    if (indexParameters[i].ParameterType != indexerTypes[i])
                    {
                        matches = false;
                        break;
                    }
                }

                if (!matches)
                {
                    continue;
                }

                foundProperties.Add(p);
            }

            if (foundProperties.Count == 0)
            {
                throw new ArgumentException(
                    $"property {propertyName} was not found in class {type} with the provided return and indexer types",
                    nameof(data)
                );
            }

            var property = foundProperties.MinBy(p => p.DeclaringType?.TypeDepth() ?? int.MaxValue);
            return property;
        }

        public GetPropertyInfoTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
        }
    }
}