using Celeste.Mod.Entities;
using DynamicInstructions.Instructions;
using DynamicInstructions.Instructions.Abstract;
using DynamicInstructions.Instructions.Basic;
using DynamicInstructions.Instructions.Invoke;
using DynamicInstructions.Instructions.Read;
using DynamicInstructions.Instructions.Write;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.BalintHelper.Triggers.Dynamic
{
    public abstract class GetThenActInstructionTrigger : CompoundInstructionTrigger
    {
        public enum GetAction
        {
            Raw,
            Read,
            ReadIndexer,
            Write,
            WriteIndexer,
            Invoke
        }
        public readonly GetAction ActionType;
        protected GetThenActInstructionTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            ActionType = data.Enum("action", GetAction.Raw);
            ConstantValue = ParseConstantValue(data);
        }
        public sealed override IEnumerable<BaseInstruction> GetCompoundInstructions()
        {
            return [ActionToInstruction()];
        }
        private BaseInstruction ActionToInstruction()
        {
            return ActionType switch
            {
                GetAction.Raw => new NopInstruction(),
                GetAction.Read => new ReadInstruction(),
                GetAction.ReadIndexer => new ReadIndexerInstruction(),
                GetAction.Write => new WriteInstruction(),
                GetAction.WriteIndexer => new WriteIndexerInstruction(),
                GetAction.Invoke => new InvokeInstruction(),
                _ => throw new InvalidProgramException($"Unknown action type: {ActionType}"),
            };
        }
        static readonly Type[] _params = [typeof(object)];
        public override Type[] ConstructorParameterTypes => _params;
        public readonly object? ConstantValue;
        public override object?[] GetConstructorParameters()
        {
            return [ConstantValue];
        }
        public abstract object? ParseConstantValue(EntityData data);
    }
}
