using Celeste.Mod.Registry;
using Monocle;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Celeste.Mod.BalintHelper.Utils
{
    public sealed class EntityTypeFilter
    {
        private readonly HashSet<string> typeNames = new(StringComparer.Ordinal);
        private readonly HashSet<int> entityIds = [];

        public IReadOnlyCollection<string> TypeNames => typeNames;
        public IReadOnlyCollection<int> EntityIds => entityIds;

        private const char separatorChar = ';';
        private static readonly char[] separatorArr = [separatorChar];
        public bool Any => typeNames.Count != 0 || entityIds.Count != 0;
        public EntityTypeFilter(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return;
            }

            foreach (string piece in raw.Split(separatorArr, StringSplitOptions.RemoveEmptyEntries))
            {
                string token = piece.Trim();
                if (token.Length == 0)
                {
                    continue;
                }

                if (int.TryParse(token, CultureInfo.InvariantCulture, out int entityId))
                {
                    entityIds.Add(entityId);
                }
                else
                {
                    typeNames.Add(token);
                }
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (typeNames.Count != 0)
            {
                sb.Append(string.Join(separatorChar, typeNames));
            }
            if (entityIds.Count != 0)
            {
                if (sb.Length != 0)
                {
                    sb.Append(separatorChar);
                }
                sb.Append(string.Join(separatorChar, entityIds));
            }
            return sb.ToString();
        }

        public bool Matches(Entity entity)
        {
            return Matches(entity, out _, out _);
        }

        public bool Matches(Entity entity, out string? matchedTypeOrSid, out int? matchedEntityId)
        {
            matchedTypeOrSid = null;
            matchedEntityId = null;

            if (entity == null)
            {
                return false;
            }

            if (entity.SourceData?.ID is int entityId && entityIds.Contains(entityId))
            {
                matchedEntityId = entityId;
                return true;
            }

            var type = entity.GetType();
            if (typeNames.Contains(type.Name))
            {
                matchedTypeOrSid = type.Name;
                return true;
            }

            var sourceName = entity.SourceData?.Name;
            if (!string.IsNullOrEmpty(sourceName))
            {
                if (typeNames.Contains(sourceName))
                {
                    matchedTypeOrSid = sourceName;
                    return true;
                }

                foreach (var typeFromSid in EntityRegistry.GetKnownTypesFromSid(sourceName))
                {
                    if (typeNames.Contains(typeFromSid.Name))
                    {
                        matchedTypeOrSid = typeFromSid.Name;
                        return true;
                    }
                }
            }

            foreach (string sidFromType in EntityRegistry.GetKnownSidsFromType(type))
            {
                if (typeNames.Contains(sidFromType))
                {
                    matchedTypeOrSid = sidFromType;
                    return true;
                }
            }

            return false;
        }
    }
}
