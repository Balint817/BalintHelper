using System.Text;

namespace DynamicInstructions.Instructions
{
    public static class InstructionUtils
    {
        public static bool? IsTrue(this object? value)
        {
            if (value is bool flag)
            {
                return flag;
            }
            try
            {
                if (value is null || (value.GetType().IsValueType && (dynamic)value == 0))
                {
                    return false;
                }
                return true;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static int[] GetArrayIntsFromStack(Interpreter.MethodState state, int dimensions)
        {
            return GetArrayIntsFromStack(state, dimensions, out _);
        }
        //private static readonly Dictionary<MethodInfo, IntPtr> _pointerCache = [];
        //public static IntPtr GetMethodPointer(MethodInfo method)
        //{
        //    method = method ?? throw new ArgumentNullException(nameof(method));

        //    if (!method.IsStatic)
        //    {
        //        throw new ArgumentException(
        //            $"Method '{method.DeclaringType?.FullName}.{method.Name}' must be static.",
        //            nameof(method));
        //    }

        //    if (method.IsGenericMethodDefinition)
        //    {
        //        throw new ArgumentException(
        //            $"Method '{method.DeclaringType?.FullName}.{method.Name}' is an open generic method definition; " +
        //            "close it (MakeGenericMethod) before taking its pointer.",
        //            nameof(method));
        //    }

        //    if (!_pointerCache.TryGetValue(method, out var pointer))
        //    {
        //        try
        //        {
        //            var types = method.GetParameters()
        //                .Select(p => p.ParameterType)
        //                .Concat(new[] { method.ReturnType })
        //                .ToArray();
        //            Type delegateType = Expression.GetDelegateType(types);
        //            var del = method.CreateDelegate(delegateType);
        //            // Ensure the method is JIT-compiled before we grab its entry point.
        //            RuntimeHelpers.PrepareMethod(method.MethodHandle);
        //            pointer = _pointerCache[method] = Marshal.GetFunctionPointerForDelegate(del);
        //        }
        //        catch (Exception)
        //        {
        //            // e.g. abstract method, or handle unavailable for this member type
        //            pointer = _pointerCache[method] = IntPtr.Zero;
        //        }
        //    }
        //    if (pointer == IntPtr.Zero)
        //    {
        //        throw new InvalidOperationException(
        //            $"Failed to get function pointer for method '{method.DeclaringType?.FullName}.{method.Name}'.");
        //    }
        //    return pointer;
        //}
        public static int[] GetArrayIntsFromStack(Interpreter.MethodState state, int dimensions, out Array? array)
        {
            array = null;

            if (dimensions > 0)
            {
                var lengths = new int[dimensions];

                for (int i = dimensions - 1; i >= 0; i--)
                {
                    if (!state.Stack.TryPop(out var lengthBoxed))
                    {
                        throw new InvalidProgramException(
                            "stack imbalance, failed to get length for new array");
                    }

                    if (lengthBoxed is null)
                    {
                        throw new InvalidProgramException(
                            "type mismatch, length for new array was null");
                    }

                    try
                    {
                        lengths[i] = Convert.ToInt32(lengthBoxed);
                    }
                    catch
                    {
                        throw new InvalidProgramException(
                            $"type mismatch, failed to get length for dimension {i}");
                    }
                }

                return lengths;
            }

            // dimensions == 0: read until we find an Array after at least one length.
            var collectedLengths = new List<int>();

            while (true)
            {
                if (!state.Stack.TryPop(out var value))
                {
                    throw new InvalidProgramException("stack imbalance, failed to get lengths from stack (or did you forget to push your array onto the stack?)");
                }

                if (value is Array foundArray)
                {
                    if (collectedLengths.Count == 0)
                    {
                        throw new InvalidProgramException(
                            "expected at least one array dimension before array reference");
                    }

                    array = foundArray;

                    collectedLengths.Reverse();
                    return [.. collectedLengths];
                }

                try
                {
                    collectedLengths.Add(Convert.ToInt32(value));
                }
                catch
                {
                    throw new InvalidProgramException("expected integer for array reference");
                }
            }
        }
        public static string Unescape(this string input)
        {
            var sb = new StringBuilder(input.Length);
            int i = 0;

            while (i < input.Length)
            {
                char c = input[i];

                if (c != '\\')
                {
                    sb.Append(c);
                    i++;
                    continue;
                }

                i++;
                if (i >= input.Length)
                {
                    // Trailing backslash
                    sb.Append('\\');
                    break;
                }

                char next = input[i];
                switch (next)
                {
                    case '\\': sb.Append('\\'); i++; break;
                    case '0': sb.Append('\0'); i++; break;
                    case 'a': sb.Append('\a'); i++; break;
                    case 'b': sb.Append('\b'); i++; break;
                    case 'f': sb.Append('\f'); i++; break;
                    case 'n': sb.Append('\n'); i++; break;
                    case 'r': sb.Append('\r'); i++; break;
                    case 't': sb.Append('\t'); i++; break;
                    case 'v': sb.Append('\v'); i++; break;

                    case 'u': // \uXXXX exactly 4 hex digits
                        {
                            i++;
                            string hex = ReadHex(input, ref i, 4, 4);
                            sb.Append((char)Convert.ToInt32(hex, 16));
                            break;
                        }

                    case 'U': // \UXXXXXXXX exactly 8 hex digits
                        {
                            i++;
                            string hex = ReadHex(input, ref i, 8, 8);
                            int codePoint = Convert.ToInt32(hex, 16);
                            sb.Append(char.ConvertFromUtf32(codePoint));
                            break;
                        }

                    case 'x': // \xH[H][H][H] greedy
                        {
                            i++;
                            string hex = ReadHex(input, ref i, 1, 4);
                            sb.Append((char)Convert.ToInt32(hex, 16));
                            break;
                        }

                    default:
                        // keep instead of erroring
                        sb.Append('\\');
                        sb.Append(next);
                        i++;
                        break;
                }
            }

            return sb.ToString();
        }

        private static string ReadHex(string input, ref int i, int minDigits, int maxDigits)
        {
            int start = i;
            int count = 0;
            while (count < maxDigits && i < input.Length && Uri.IsHexDigit(input[i]))
            {
                i++;
                count++;
            }

            if (count < minDigits)
            {
                throw new FormatException(
                    $"Expected at least {minDigits} hex digit(s) at position {start}, found {count}.");
            }

            return input.Substring(start, count);
        }
    }
}
