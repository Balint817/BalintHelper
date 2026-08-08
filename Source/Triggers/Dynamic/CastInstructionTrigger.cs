using Celeste.Mod.BalintHelper.Entities.Dynamic;
using Celeste.Mod.Entities;
using DynamicInstructions.Instructions;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.BalintHelper.Triggers.Dynamic
{
    [CustomEntity(
        "BalintHelper/CastInstructionTrigger/CastInstruction"
        )]
    public class CastInstructionTrigger : TypedInstructionTrigger
    {
        private readonly string _sourceTypeName;
        public Type? SourceType { get; private set; }

        private static readonly Type[] _typeParams = new[] { typeof(Type), typeof(Type) };
        public override Type[] ConstructorParameterTypes => _typeParams;
        public override object?[] GetConstructorParameters()
        {
            return [SourceType, TypeParameter ?? throw new InvalidOperationException("TypeParameter is not set")];
        }
        public CastInstructionTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            _sourceTypeName = data.String("sourceType");
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            var controller = DynamicMethodController.GetOrCreate(Scene);

            if (!string.IsNullOrWhiteSpace(_sourceTypeName))
            {
                SourceType = TypeNameCodec.ParseType(_sourceTypeName, controller.Assemblies)
                    ?? throw new ArgumentException("could not find matching type", "data");
            }
        }
    }

}
