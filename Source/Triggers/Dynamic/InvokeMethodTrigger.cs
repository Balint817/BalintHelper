using Celeste.Mod.BalintHelper.Entities.Dynamic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System;

namespace Celeste.Mod.BalintHelper.Triggers.Dynamic
{
    [CustomEntity("BalintHelper/InvokeMethodTrigger")]
    public class InvokeMethodTrigger : Trigger
    {
        public enum ArgumentMode
        {
            None,
            Position,
            Bounds
        }

        private readonly string methodName;
        private readonly bool onlyOnce;
        private readonly ArgumentMode argumentMode;
        private bool invoked;

        public InvokeMethodTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            methodName = data.String("methodName") ?? throw new ArgumentException("no method name was provided", nameof(data));
            onlyOnce = data.Bool("onlyOnce", true);
            argumentMode = data.Enum("argumentMode", ArgumentMode.None);
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);

            if (onlyOnce && invoked)
            {
                return;
            }
            invoked = true;

            object?[]? args = argumentMode switch
            {
                ArgumentMode.Position => [Position],
                ArgumentMode.Bounds => [new Rectangle((int)X, (int)Y, (int)Width, (int)Height)],
                _ => null
            };

            var controller = DynamicMethodController.GetOrCreate(Scene);
            controller.Interpreter.InvokeDynamicMethod(methodName, out _, args);
        }
    }
}
