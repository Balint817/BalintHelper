using Celeste.Mod.Entities;
using DynamicInstructions.Instructions;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Globalization;

namespace Celeste.Mod.BalintHelper.Triggers.Dynamic
{
    [CustomEntity(
        "BalintHelper/LoadPrimitiveInstructionTrigger/LoadConstantInstruction"
        )]
    public class LoadPrimitiveInstructionTrigger : LoadConstantInstructionTrigger
    {
        public LoadPrimitiveInstructionTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
        }
        public enum ConstantType
        {
            None,
            Bool,
            Byte,
            SByte,
            Int16,
            Int32,
            Int64,
            UInt16,
            UInt32,
            UInt64,
            Decimal,
            Double,
            Float,
            NativeInt,
            NativeUInt,
            Char,
            String,
            Vector2,
            Color,
            Null
        }
        public override object? ParseConstantValue(EntityData data)
        {
            var constantType = data.Enum("type", ConstantType.None);
            var value = data.String("value")
                ?? throw new ArgumentException("no value was provided", nameof(data));
            switch (constantType)
            {
                case ConstantType.Bool:
                    {

                        value = value.Trim().ToLowerInvariant();
                        if (value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                        if (value == "0" || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        {
                            return false;
                        }
                        throw new ArgumentException("invalid boolean value", nameof(data));
                    }
                case ConstantType.Byte:
                    {
                        if (byte.TryParse(value, CultureInfo.InvariantCulture, out var parsedValue))
                        {
                            return parsedValue;
                        }
                        throw new ArgumentException("invalid byte value", nameof(data));
                    }
                case ConstantType.SByte:
                    {
                        if (sbyte.TryParse(value, CultureInfo.InvariantCulture, out var parsedValue))
                        {
                            return parsedValue;
                        }
                        throw new ArgumentException("invalid sbyte value", nameof(data));
                    }
                case ConstantType.Int16:
                    {
                        if (short.TryParse(value, CultureInfo.InvariantCulture, out var parsedValue))
                        {
                            return parsedValue;
                        }
                        throw new ArgumentException("invalid short value", nameof(data));
                    }
                case ConstantType.Int32:
                    {
                        if (int.TryParse(value, CultureInfo.InvariantCulture, out var parsedValue))
                        {
                            return parsedValue;
                        }
                        throw new ArgumentException("invalid int value", nameof(data));
                    }
                case ConstantType.Int64:
                    {
                        if (long.TryParse(value, CultureInfo.InvariantCulture, out var parsedValue))
                        {
                            return parsedValue;
                        }
                        throw new ArgumentException("invalid long value", nameof(data));
                    }
                case ConstantType.UInt16:
                    {
                        if (ushort.TryParse(value, CultureInfo.InvariantCulture, out var parsedValue))
                        {
                            return parsedValue;
                        }
                        throw new ArgumentException("invalid ushort value", nameof(data));
                    }
                case ConstantType.UInt32:
                    {
                        if (uint.TryParse(value, CultureInfo.InvariantCulture, out var parsedValue))
                        {
                            return parsedValue;
                        }
                        throw new ArgumentException("invalid uint value", nameof(data));
                    }
                case ConstantType.UInt64:
                    {
                        if (ulong.TryParse(value, CultureInfo.InvariantCulture, out var parsedValue))
                        {
                            return parsedValue;
                        }
                        throw new ArgumentException("invalid ulong value", nameof(data));
                    }
                case ConstantType.Decimal:
                    {
                        if (decimal.TryParse(value, CultureInfo.InvariantCulture, out var parsedValue))
                        {
                            return parsedValue;
                        }
                        throw new ArgumentException("invalid decimal value", nameof(data));
                    }
                case ConstantType.Double:
                    {
                        if (double.TryParse(value, CultureInfo.InvariantCulture, out var parsedValue))
                        {
                            return parsedValue;
                        }
                        throw new ArgumentException("invalid double value", nameof(data));
                    }
                case ConstantType.Float:
                    {
                        if (float.TryParse(value, CultureInfo.InvariantCulture, out var parsedValue))
                        {
                            return parsedValue;
                        }
                        throw new ArgumentException("invalid float value", nameof(data));
                    }
                case ConstantType.NativeInt:
                    {
                        if (nint.TryParse(value, CultureInfo.InvariantCulture, out var parsedValue))
                        {
                            return parsedValue;
                        }
                        throw new ArgumentException("invalid nint value", nameof(data));
                    }
                case ConstantType.NativeUInt:
                    {
                        if (nuint.TryParse(value, CultureInfo.InvariantCulture, out var parsedValue))
                        {
                            return parsedValue;
                        }
                        throw new ArgumentException("invalid nuint value", nameof(data));
                    }
                case ConstantType.Char:
                    {
                        var parsedValue = value.Unescape();
                        if (parsedValue.Length == 1)
                        {
                            return parsedValue;
                        }
                        throw new ArgumentException("invalid char value", nameof(data));
                    }
                case ConstantType.String:
                    {
                        return value.Unescape();
                    }
                case ConstantType.Vector2:
                    {
                        var parts = value.Split(';');
                        if (parts.Length != 2
                            || !float.TryParse(parts[0].Trim(), CultureInfo.InvariantCulture, out var x)
                            || !float.TryParse(parts[1].Trim(), CultureInfo.InvariantCulture, out var y))
                        {
                            throw new ArgumentException("invalid vector2 value, expected format \"x;y\"", nameof(data));
                        }
                        return new Vector2(x, y);
                    }
                case ConstantType.Color:
                    {
                        try
                        {
                            return Calc.HexToColorWithAlpha(value.Trim());
                        }
                        catch (Exception)
                        {
                            throw new ArgumentException("invalid color value, expected a hex color string", nameof(data));
                        }
                    }
                case ConstantType.Null:
                    return null;
                default:
                    throw new ArgumentException("unknown constant type", nameof(data));
            }
        }
    }

}
