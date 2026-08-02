using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.BalintHelper.Triggers.Dynamic
{
    [Tracked(false)]
    [CustomEntity("BalintHelper/DefineMethodTrigger")]
    public class DefineMethodTrigger : Trigger
    {
        public readonly string MethodName;
        public readonly int ArgCount;
        public DefineMethodTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            MethodName = data.String("methodName");
            ArgumentException.ThrowIfNullOrEmpty(MethodName, nameof(data));
            ArgCount = data.Int("argCount");
            if (ArgCount < 0)
            {
                throw new ArgumentException("argCount was negative", nameof(data));
            }
        }
    }

}
