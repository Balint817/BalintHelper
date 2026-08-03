using System.Reflection;
using System.Text;

namespace DynamicInstructions.Instructions
{

    public static class TypeNameCodec
    {
        private static readonly Dictionary<string, string> Aliases = new(StringComparer.Ordinal)
        {
            ["bool"] = "System.Boolean",
            ["byte"] = "System.Byte",
            ["sbyte"] = "System.SByte",
            ["short"] = "System.Int16",
            ["ushort"] = "System.UInt16",
            ["int"] = "System.Int32",
            ["uint"] = "System.UInt32",
            ["long"] = "System.Int64",
            ["ulong"] = "System.UInt64",
            ["nint"] = "System.IntPtr",
            ["nuint"] = "System.UIntPtr",
            ["char"] = "System.Char",
            ["float"] = "System.Single",
            ["double"] = "System.Double",
            ["decimal"] = "System.Decimal",
            ["string"] = "System.String",
            ["object"] = "System.Object",
            ["void"] = "System.Void"
        };

        private static readonly HashSet<char> SpecialChars =
    [
        ',', '[', ']', '&', '*', '+', '\\', '`'
    ];

        public static Type? ParseType(string input, IEnumerable<Assembly> searchAssemblies)
        {
            ArgumentNullException.ThrowIfNull(input);
            ArgumentNullException.ThrowIfNull(searchAssemblies);

            var assemblies = searchAssemblies.Distinct().ToArray();
            var normalized = NormalizeFriendlySyntax(input.Trim());

            Console.WriteLine(normalized);

            var type = Type.GetType(
                normalized,
                assemblyResolver: name => ResolveAssembly(name, assemblies),
                typeResolver: (asm, name, ignoreCase) => ResolveType(asm, name, assemblies, ignoreCase),
                throwOnError: false,
                ignoreCase: false);
            return type;
        }

        public static IReadOnlyList<Type?> ParseTypeList(string input, IEnumerable<Assembly> searchAssemblies)
        {
            if (string.IsNullOrWhiteSpace(input))
                return [];

            return [.. SplitTopLevel(input, ';')
                .Select(x => x.Trim())
                .Where(x => x.Length > 0)
                .Select(x => ParseType(x, searchAssemblies))];
        }

        public static string ToCanonicalString(Type type, bool includeAssembly = false)
        {
            ArgumentNullException.ThrowIfNull(type);
            return includeAssembly
                ? (type.AssemblyQualifiedName ?? type.ToString())
                : (type.FullName ?? type.ToString());
        }

        private static Assembly? ResolveAssembly(AssemblyName name, Assembly[] assemblies)
        {
            var full = assemblies.FirstOrDefault(a => string.Equals(a.FullName, name.FullName, StringComparison.Ordinal));
            if (full != null) return full;

            var simpleMatches = assemblies
                .Where(a => string.Equals(a.GetName().Name, name.Name, StringComparison.Ordinal))
                .ToArray();

            return simpleMatches.Length switch
            {
                0 => null,
                1 => simpleMatches[0],
                _ => throw new InvalidOperationException($"Ambiguous assembly '{name.Name}'.")
            };
        }

        private static Type? ResolveType(Assembly? asm, string simpleOrFullName, Assembly[] assemblies, bool ignoreCase)
        {
            var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            var name = NormalizeAliasOnly(simpleOrFullName);

            if (asm != null)
                return asm.GetType(name, throwOnError: false, ignoreCase: ignoreCase);

            var direct = Type.GetType(name, throwOnError: false, ignoreCase: ignoreCase);
            if (direct != null) return direct;

            var types = assemblies
                .SelectMany(GetLoadableTypes)
                .ToArray();

            var fullNameMatches = types
                .Where(t => string.Equals(t.FullName, name, comparison))
                .Distinct()
                .ToArray();

            if (fullNameMatches.Length == 1)
                return fullNameMatches[0];

            if (fullNameMatches.Length > 1)
                throw new InvalidOperationException(
                    $"Ambiguous type '{simpleOrFullName}'. Matches: {string.Join(", ", fullNameMatches.Select(t => t.FullName))}");

            var nameMatches = types
                .Where(t => string.Equals(t.Name, name, comparison))
                .Distinct()
                .ToArray();

            return nameMatches.Length switch
            {
                0 => null,
                1 => nameMatches[0],
                _ => throw new InvalidOperationException(
                    $"Ambiguous type '{simpleOrFullName}'. Matches: {string.Join(", ", nameMatches.Select(t => t.FullName))}")
            };
        }

        public static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t != null)!;
            }
        }

        private static string NormalizeFriendlySyntax(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;
            var parser = new FriendlyTypeParser(input);
            return NormalizeAliasTokens(parser.ParseType());
        }

        private static string NormalizeAliasOnly(string input) => NormalizeAliasTokens(input.Trim());

        private static string NormalizeAliasTokens(string input)
        {
            foreach (var kvp in Aliases.OrderByDescending(x => x.Key.Length))
                input = ReplaceStandaloneToken(input, kvp.Key, kvp.Value);
            return input;
        }

        private static string ReplaceStandaloneToken(string text, string token, string replacement)
        {
            var sb = new StringBuilder(text.Length);
            int i = 0;

            while (i < text.Length)
            {
                if (IsTokenAt(text, i, token) &&
                    (i == 0 || !IsIdentChar(text[i - 1])) &&
                    (i + token.Length == text.Length || !IsIdentChar(text[i + token.Length])))
                {
                    sb.Append(replacement);
                    i += token.Length;
                }
                else
                {
                    sb.Append(text[i]);
                    i++;
                }
            }

            return sb.ToString();
        }

        private static bool IsTokenAt(string text, int index, string token) =>
            index + token.Length <= text.Length &&
            string.CompareOrdinal(text, index, token, 0, token.Length) == 0;

        private static bool IsIdentChar(char c) =>
            char.IsLetterOrDigit(c) || c == '_' || c == '.';

        private static List<string> SplitTopLevel(string input, char separator)
        {
            var result = new List<string>();
            var sb = new StringBuilder();
            int angleDepth = 0;
            int bracketDepth = 0;
            bool escape = false;

            foreach (var ch in input)
            {
                if (escape)
                {
                    sb.Append(ch);
                    escape = false;
                    continue;
                }

                if (ch == '\\')
                {
                    sb.Append(ch);
                    escape = true;
                    continue;
                }

                if (ch == '<') angleDepth++;
                else if (ch == '>') angleDepth--;
                else if (ch == '[') bracketDepth++;
                else if (ch == ']') bracketDepth--;

                if (ch == separator && angleDepth == 0 && bracketDepth == 0)
                {
                    result.Add(sb.ToString());
                    sb.Clear();
                }
                else
                {
                    sb.Append(ch);
                }
            }

            result.Add(sb.ToString());
            return result;
        }

        private sealed class FriendlyTypeParser
        {
            private readonly string _text;
            private int _i;

            public FriendlyTypeParser(string text) => _text = text;

            public string ParseType()
            {
                var s = ParsePrimaryType();
                SkipWs();

                while (_i < _text.Length)
                {
                    if (Peek("[]"))
                    {
                        s += "[]";
                        _i += 2;
                    }
                    else if (Peek('*'))
                    {
                        s += "*";
                        _i++;
                    }
                    else if (Peek('&'))
                    {
                        s += "&";
                        _i++;
                    }
                    else
                    {
                        break;
                    }
                }

                SkipWs();
                if (_i != _text.Length)
                    throw new FormatException($"Unexpected trailing characters at position {_i}.");

                return s;
            }

            private string ParsePrimaryType()
            {
                SkipWs();
                var name = ParseIdentifierOrEscapedName();
                SkipWs();

                if (Peek('<'))
                {
                    _i++;
                    var args = new List<string>();
                    while (true)
                    {
                        args.Add(ParseTypeArgument());
                        SkipWs();

                        if (Peek(','))
                        {
                            _i++;
                            SkipWs();
                            continue;
                        }

                        if (Peek('>'))
                        {
                            _i++;
                            break;
                        }

                        throw new FormatException($"Expected ',' or '>' at position {_i}.");
                    }

                    name += "`" + args.Count + "[" + string.Join(",", args) + "]";
                }

                return name;
            }

            private string ParseTypeArgument()
            {
                SkipWs();
                var arg = ParsePrimaryType();
                SkipWs();

                while (_i < _text.Length)
                {
                    if (Peek("[]"))
                    {
                        arg += "[]";
                        _i += 2;
                        SkipWs();
                        continue;
                    }
                    if (Peek('*'))
                    {
                        arg += "*";
                        _i++;
                        SkipWs();
                        continue;
                    }
                    if (Peek('&'))
                    {
                        arg += "&";
                        _i++;
                        SkipWs();
                        continue;
                    }
                    break;
                }

                return arg;
            }

            private string ParseIdentifierOrEscapedName()
            {
                var sb = new StringBuilder();
                bool any = false;

                while (_i < _text.Length)
                {
                    var c = _text[_i];
                    if (c == '\\')
                    {
                        if (_i + 1 >= _text.Length)
                            throw new FormatException("Dangling escape at end of input.");

                        sb.Append(_text[_i + 1]);
                        _i += 2;
                        any = true;
                        continue;
                    }

                    if (IsTerminator(c))
                        break;

                    sb.Append(c);
                    _i++;
                    any = true;
                }

                var name = sb.ToString().Trim();
                if (!any || name.Length == 0)
                    throw new FormatException($"Expected type name at position {_i}.");

                return name;
            }

            private static bool IsTerminator(char c) =>
                c == '<' || c == '>' || c == ',' || c == ';' || c == '[' || c == ']' || c == '*' || c == '&';

            private void SkipWs()
            {
                while (_i < _text.Length && char.IsWhiteSpace(_text[_i])) _i++;
            }

            private bool Peek(char c) => _i < _text.Length && _text[_i] == c;
            private bool Peek(string s) => _i + s.Length <= _text.Length && string.CompareOrdinal(_text, _i, s, 0, s.Length) == 0;
        }
    }

}
