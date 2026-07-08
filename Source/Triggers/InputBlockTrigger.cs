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
            var types = new[] { buttonType, joystickType, axisType };

            var fields = inputType
                .GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(f => types.Contains(f.FieldType))
                .GroupBy(f => f.FieldType)
                .ToDictionary(x => x.Key, x => x.ToArray());

            var consumePressMethod = buttonType.GetMethod(
                nameof(VirtualButton.ConsumePress),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;

            var joystickField = joystickType.GetField(
                nameof(VirtualJoystick.value),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;
            var joystickPropSetter = joystickType.GetProperty(
                nameof(VirtualJoystick.Value),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!.SetMethod!;

            var axisField = axisType.GetField(
                nameof(VirtualIntegerAxis.Value),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;

            var vector2Zero = typeof(Vector2).GetProperty(
                nameof(Vector2.Zero),
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)!.GetMethod!;

            var dm = new DynamicMethod(
                "InputBlockTrigger_BlockerMethod",
                typeof(void),
                Type.EmptyTypes,
                inputType,
                true);

            var il = dm.GetILGenerator();

            foreach (var field in fields[buttonType])
            {
                il.Emit(OpCodes.Ldsfld, field);
                il.Emit(OpCodes.Dup);
                var skip = il.DefineLabel();
                il.Emit(OpCodes.Brfalse_S, skip);

                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Callvirt, consumePressMethod);

                il.MarkLabel(skip);
                il.Emit(OpCodes.Pop);
            }

            foreach (var field in fields[joystickType])
            {
                il.Emit(OpCodes.Ldsfld, field);
                il.Emit(OpCodes.Dup);
                var skip = il.DefineLabel();
                il.Emit(OpCodes.Brfalse_S, skip); // removes one instance of the field

                il.Emit(OpCodes.Dup); // dup again to have the field on the stack twice
                il.Emit(OpCodes.Call, vector2Zero); // get Vector2.Zero
                il.Emit(OpCodes.Callvirt, joystickPropSetter); // set the field, pops one field reference of the two and the Vector2.Zero

                il.Emit(OpCodes.Dup); // dup again to have the field on the stack twice
                il.Emit(OpCodes.Call, vector2Zero); // get Vector2.Zero again
                il.Emit(OpCodes.Stfld, joystickField); // set the value field, pops the other field reference and the Vector2.Zero

                il.MarkLabel(skip);
                il.Emit(OpCodes.Pop);
            }

            foreach (var field in fields[axisType])
            {
                il.Emit(OpCodes.Ldsfld, field);
                il.Emit(OpCodes.Dup);
                var skip = il.DefineLabel();
                il.Emit(OpCodes.Brfalse_S, skip);

                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Stfld, axisField);

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