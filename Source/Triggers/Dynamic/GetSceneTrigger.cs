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
    [CustomEntity("BalintHelper/GetSceneTrigger/NopInstruction")]
    public class GetSceneTrigger : CompoundInstructionTrigger
    {
        private static readonly PropertyInfo sceneField = typeof(Engine).GetProperty(nameof(Engine.Scene), BindingFlags.Public | BindingFlags.Static)!;
        public GetSceneTrigger(EntityData data, Vector2 offset) : base(data, offset) { }
        override public IEnumerable<BaseInstruction> GetCompoundInstructions()
        {
            return [new LoadConstantInstruction(sceneField), new ReadInstruction()];
        }
    }
}
