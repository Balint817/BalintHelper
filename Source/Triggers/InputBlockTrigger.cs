using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Celeste.Mod.BalintHelper.Triggers
{
    [CustomEntity("BalintHelper/InputBlockTrigger")]
    public class InputBlockTrigger : Trigger
    {
        static Action CreateBlocker()
        {

            var inputType = typeof(Input);
            var buttonType = typeof(VirtualButton);
            var joystickType = typeof(VirtualJoystick);
            var axisType = typeof(VirtualIntegerAxis);

            var fields = inputType
                .GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(f => f.FieldType == buttonType || f.FieldType == joystickType || f.FieldType == axisType)
                .ToArray();

            var buttonFields = fields.Where(f => f.FieldType == buttonType).ToArray();
            var joystickFields = fields.Where(f => f.FieldType == joystickType).ToArray();
            var axisFields = fields.Where(f => f.FieldType == axisType).ToArray();

            var consumePress = buttonType.GetMethod(
                "ConsumePress",
                BindingFlags.Instance | BindingFlags.Public)!;

            var joystickSetter = joystickType.GetProperty(
                "Value",
                BindingFlags.Instance | BindingFlags.Public)!.SetMethod!;

            var axisSetter = axisType.GetProperty(
                "Value",
                BindingFlags.Instance | BindingFlags.Public)!.SetMethod!;

            var vector2Zero = typeof(Vector2).GetField(
                nameof(Vector2.Zero),
                BindingFlags.Public | BindingFlags.Static)!;

            var dm = new DynamicMethod(
                "InputBlockTrigger_BlockerMethod",
                typeof(void),
                Type.EmptyTypes,
                inputType,
                true);

            var il = dm.GetILGenerator();

            foreach (var field in buttonFields)
            {
                il.Emit(OpCodes.Ldsfld, field);
                il.Emit(OpCodes.Dup);
                var skip = il.DefineLabel();
                il.Emit(OpCodes.Brfalse_S, skip);
                il.Emit(OpCodes.Callvirt, consumePress);
                il.MarkLabel(skip);
                il.Emit(OpCodes.Pop);
            }

            foreach (var field in joystickFields)
            {
                il.Emit(OpCodes.Ldsfld, field);
                il.Emit(OpCodes.Dup);
                var skip = il.DefineLabel();
                il.Emit(OpCodes.Brfalse_S, skip);
                il.Emit(OpCodes.Ldsfld, vector2Zero);
                il.Emit(OpCodes.Callvirt, joystickSetter);
                il.MarkLabel(skip);
                il.Emit(OpCodes.Pop);
            }

            foreach (var field in axisFields)
            {
                il.Emit(OpCodes.Ldsfld, field);
                il.Emit(OpCodes.Dup);
                var skip = il.DefineLabel();
                il.Emit(OpCodes.Brfalse_S, skip);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Callvirt, axisSetter);
                il.MarkLabel(skip);
                il.Emit(OpCodes.Pop);
            }

            il.Emit(OpCodes.Ret);

            return (Action)dm.CreateDelegate(typeof(Action));
        }
        static readonly Action Blocker = CreateBlocker();

        public InputBlockTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);

            BlockInputs();
        }

        public override void OnStay(Player player)
        {
            base.OnStay(player);

            BlockInputs();
        }

        public override void OnLeave(Player player)
        {
            base.OnLeave(player);

            BlockInputs();
        }

        public static void BlockInputs()
        {
            Blocker();
        }
    }
}