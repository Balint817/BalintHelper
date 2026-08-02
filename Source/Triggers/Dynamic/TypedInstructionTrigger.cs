using Celeste.Mod.BalintHelper.Entities.Dynamic;
using Celeste.Mod.Entities;
using DynamicInstructions.Instructions;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.BalintHelper.Triggers.Dynamic
{
    [CustomEntity(
        "BalintHelper/TypedInstructionTrigger/ReadPointerInstruction",
        "BalintHelper/TypedInstructionTrigger/WritePointerInstruction",
        "BalintHelper/TypedInstructionTrigger/IsTypeInstruction"
        )]
    public class TypedInstructionTrigger : BaseInstructionTrigger
    {
        static readonly Type[] _params = [typeof(Type)];
        public override object?[] GetConstructorParameters()
        {
            return [TypeParameter ?? throw new InvalidOperationException("TypeParameter is not set")];
        }
        public override Type[] ConstructorParameterTypes => _params;
        public Type? TypeParameter { get; private set; }
        private readonly string _typeName;
        public TypedInstructionTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            _typeName = data.String("type");
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            var controller = DynamicMethodController.GetOrCreate(Scene);

            TypeParameter = TypeNameCodec.ParseType(_typeName, controller.Assemblies)
                ?? throw new ArgumentException("could not find matching type", "data");
        }
    }
}
