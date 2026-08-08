using Celeste.Mod.BalintHelper.Entities.Dynamic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Linq;

namespace Celeste.Mod.BalintHelper.Triggers.Dynamic
{

    [Tracked(true)]
    [CustomEntity(
        "BalintHelper/BaseInstructionTrigger/ArrayGetLengthInstruction",
        "BalintHelper/BaseInstructionTrigger/ArrayRankInstruction",
        "BalintHelper/BaseInstructionTrigger/ArrayVectorLengthInstruction",
        "BalintHelper/BaseInstructionTrigger/LoadArrayElementInstruction",
        "BalintHelper/BaseInstructionTrigger/StoreArrayElementInstruction",

        "BalintHelper/BaseInstructionTrigger/DupInstruction",
        "BalintHelper/BaseInstructionTrigger/NopInstruction",
        "BalintHelper/BaseInstructionTrigger/PopInstruction",
        "BalintHelper/BaseInstructionTrigger/ReturnInstruction",
        "BalintHelper/BaseInstructionTrigger/StructCopyInstruction",

        "BalintHelper/BaseInstructionTrigger/InvokeInstruction",

        "BalintHelper/BaseInstructionTrigger/AllocInstruction",

        "BalintHelper/BaseInstructionTrigger/ReadIndexerInstruction",
        "BalintHelper/BaseInstructionTrigger/ReadInstruction",

        "BalintHelper/BaseInstructionTrigger/GetTypeInstruction",

        "BalintHelper/BaseInstructionTrigger/InitVariableInstruction",
        "BalintHelper/BaseInstructionTrigger/IsDefinedInstruction",

        "BalintHelper/BaseInstructionTrigger/WriteIndexerInstruction",
        "BalintHelper/BaseInstructionTrigger/WriteInstruction"
        )]
    public class BaseInstructionTrigger : Trigger
    {
        public virtual Type[] ConstructorParameterTypes => [];
        public virtual object?[] GetConstructorParameters()
        {
            return [];
        }
        public Type? InstructionType { get; protected set; }
        private readonly string _instructionTypeName;
        public BaseInstructionTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            var name = data.Name;
            var split = name.Split('/');
            if (split.Length < 2)
            {
                throw new ArgumentException("invalid CustomEntity name", nameof(data));
            }
            _instructionTypeName = split[^1];

        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            var controller = DynamicMethodController.GetOrCreate(Scene);
            InstructionType ??= controller.InstructionTypes
                .FirstOrDefault(type => type.Name == _instructionTypeName)
                ?? throw new ArgumentException("invalid CustomEntity name");
        }
    }
}
