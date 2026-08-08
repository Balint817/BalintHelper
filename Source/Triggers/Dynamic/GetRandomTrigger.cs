using Celeste.Mod.Entities;
using DynamicInstructions.Instructions.Abstract;
using DynamicInstructions.Instructions.Basic;
using DynamicInstructions.Instructions.Read;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.BalintHelper.Triggers.Dynamic
{
    [CustomEntity("BalintHelper/GetRandomTrigger/NopInstruction")]
    public class GetRandomTrigger : CompoundInstructionTrigger
    {
        private static readonly FieldInfo randomField = typeof(Calc).GetField(nameof(Calc.Random), BindingFlags.Public | BindingFlags.Static)!;
        public GetRandomTrigger(EntityData data, Vector2 offset) : base(data, offset) { }
        override public IEnumerable<BaseInstruction> GetCompoundInstructions()
        {
            return [new LoadConstantInstruction(randomField), new ReadInstruction()];
        }
    }
}
