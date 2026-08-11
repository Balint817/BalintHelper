using DynamicInstructions.Instructions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.BalintHelper.Record
{
    public class EntityInfo
    {
        public enum FilterMode
        {
            None,
            First,
            All
        }
        public readonly HashSet<int> IDs = [];
        public readonly HashSet<Type> Types = [];
        public readonly FilterMode Mode = FilterMode.None;
        public bool Any => IDs.Count > 0 || Types.Count > 0;
        public EntityInfo(string types, FilterMode mode)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            switch (mode)
            {
                case FilterMode.First:
                case FilterMode.All:
                    break;
                default:
                    throw new ArgumentException("invalid entity filter mode", nameof(mode));
            }
            ArgumentNullException.ThrowIfNull(types, nameof(types));
            var split = types.Split(';').ToList();
            while (split.Count > 0)
            {
                var s = split[^1].Trim();
                if (s.Length == 0)
                {
                    split.RemoveAt(split.Count - 1);
                    continue;
                }
                if (int.TryParse(s, CultureInfo.InvariantCulture, out var id))
                {
                    IDs.Add(id);
                    split.RemoveAt(split.Count - 1);
                    continue;
                }
                else if (TypeNameCodec.ParseType(s, assemblies) is { } type)
                {
                    Types.Add(type);
                }
                else
                {
                    throw new ArgumentException($"failed to parse type \"{s}\" in \"{nameof(types)}\" argument", nameof(types));
                }
                split.RemoveAt(split.Count - 1);
            }

            Mode = mode;
        }
    }
}
