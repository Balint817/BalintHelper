using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.BalintHelper.Utils
{
    public sealed class EntityTypeFilter
    {
        private readonly HashSet<string> typeNames = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<int> entityIds = new HashSet<int>();

        public IReadOnlyCollection<string> TypeNames => typeNames;
        public IReadOnlyCollection<int> EntityIds => entityIds;

        private static readonly char[] separator = new[] { ',' };

        public EntityTypeFilter(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return;
            }

            foreach (string piece in raw.Split(separator, StringSplitOptions.RemoveEmptyEntries))
            {
                string token = piece.Trim();
                if (token.Length == 0)
                {
                    continue;
                }

                if (int.TryParse(token, out int entityId))
                {
                    entityIds.Add(entityId);
                }
                else
                {
                    typeNames.Add(token);
                }
            }
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

                foreach (var typeFromSid in BalintHelperModule.Instance.GetKnownTypesFromSid(sourceName))
                {
                    if (typeNames.Contains(typeFromSid.Name))
                    {
                        matchedTypeOrSid = typeFromSid.Name;
                        return true;
                    }
                }
            }

            foreach (string sidFromType in BalintHelperModule.Instance.GetKnownSidsFromType(type))
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
