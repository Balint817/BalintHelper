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
        "BalintHelper/GetMethodInfoTrigger/LoadConstantInstruction"
        )]
    public class GetMethodInfoTrigger : GetThenActInstructionTrigger
    {
        public override object? ParseConstantValue(EntityData data)
        {
            var className = data.String("className") ?? throw new ArgumentException("no class name was provided", nameof(data));
            var methodName = data.String("methodName") ?? throw new ArgumentException("no method name was provided", nameof(data));
            var genericTypeNames = data.String("genericTypes", "");
            var argumentTypeNames = data.String("argumentTypes", "");
            var returnTypeName = data.String("returnType", "");

            var type = TypeNameCodec.ParseType(className, AppDomain.CurrentDomain.GetAssemblies()) ?? throw new ArgumentException($"type {className} was not found", nameof(data));
            var genericTypes = Array.Empty<Type>();
            if (!string.IsNullOrWhiteSpace(genericTypeNames))
            {
                genericTypes = TypeNameCodec.ParseTypeList(genericTypeNames, AppDomain.CurrentDomain.GetAssemblies())
                    .Select(t => t ?? throw new ArgumentException($"generic type {genericTypeNames} was not found", nameof(data)))
                    .ToArray();
            }
            var argumentTypes = Array.Empty<Type>();
            if (!string.IsNullOrWhiteSpace(argumentTypeNames))
            {
                argumentTypes = TypeNameCodec.ParseTypeList(argumentTypeNames, AppDomain.CurrentDomain.GetAssemblies())
                    .Select(t => t ?? throw new ArgumentException($"argument type {argumentTypeNames} was not found", nameof(data)))
                    .ToArray();
            }
            Type? returnType = null;
            if (!string.IsNullOrWhiteSpace(returnTypeName))
            {
                returnType = TypeNameCodec.ParseType(returnTypeName, AppDomain.CurrentDomain.GetAssemblies())
                    ?? throw new ArgumentException($"return type {returnTypeName} was not found", nameof(data));
            }

            const BindingFlags allFlags =
                BindingFlags.Public
                | BindingFlags.NonPublic
                | BindingFlags.Static
                | BindingFlags.Instance;


            var foundMethods = new List<MethodInfo>();

            foreach (var m in type.GetMethods(allFlags))
            {
                if (m.Name != methodName)
                {
                    continue;
                }
                var genericArgs = m.GetGenericArguments();
                if (genericArgs.Length != genericTypes.Length)
                {
                    continue;
                }
                var genericMethod = m;
                try
                {
                    if (m.IsGenericMethod)
                    {
                        genericMethod = m.MakeGenericMethod(genericTypes);
                    }
                }
                catch (Exception)
                {
                    continue;
                }
                var parameters = genericMethod.GetParameters();
                if (parameters.Length != argumentTypes.Length)
                {
                    continue;
                }
                bool notMatching = false;
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i].ParameterType != argumentTypes[i])
                    {
                        notMatching = true;
                        break;
                    }
                }
                if (notMatching)
                {
                    continue;
                }
                if (returnType is not null && genericMethod.ReturnType != returnType)
                {
                    continue;
                }
                foundMethods.Add(genericMethod);
            }

            if (foundMethods.Count == 0)
            {
                throw new ArgumentException($"method {methodName} was not found in class {type} with the provided generic and argument types", nameof(data));
            }

            var method = foundMethods.MinBy(m => m.DeclaringType?.TypeDepth() ?? int.MaxValue);

            return method;
        }
        public GetMethodInfoTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
        }
    }
}
