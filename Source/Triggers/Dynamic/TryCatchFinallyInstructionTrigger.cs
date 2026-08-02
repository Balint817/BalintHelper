using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System;

namespace Celeste.Mod.BalintHelper.Triggers.Dynamic
{
    [CustomEntity(
        "BalintHelper/TryCatchFinallyInstructionTrigger/TryCatchFinallyInstruction"
        )]
    public class TryCatchFinallyInstructionTrigger : BaseInstructionTrigger
    {
        static readonly Type[] _params = [typeof(string), typeof(string), typeof(string)];
        public override Type[] ConstructorParameterTypes => _params;
        public string? TryMethodName { get; internal set; }
        public string? CatchMethodName { get; internal set; }
        public string? FinallyMethodName { get; internal set; }
        public override object?[] GetConstructorParameters()
        {
            return [TryMethodName ?? throw new InvalidOperationException("TryMethodName is not set"), CatchMethodName, FinallyMethodName];
        }
        public TryCatchFinallyInstructionTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            TryMethodName = data.Attr("tryMethodName");
            CatchMethodName = data.Attr("catchMethodName");
            FinallyMethodName = data.Attr("finallyMethodName");
        }
    }

}
