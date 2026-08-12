using Celeste.Mod.BalintHelper.Utils;
using Celeste.Mod.Registry;
using Monocle;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Celeste.Mod.BalintHelper.Entities
{
    [Tracked(false)]
    public sealed class EntityTypeFilterComponent: Component
    {
        private bool howTheFuckFlag = false;
        public void BeforeUpdate(Level level)
        {
            if (this.IsGone(level) && !howTheFuckFlag)
            {
                howTheFuckFlag = true;
                Logger.Error("EntityTypeFilterEntity", "genuinely how tf?");
                RemoveSelf();
                return;
            }
            UpdateMatchedEntities(level);
        }

        private const char separator = ';';
        private static readonly char[] separatorArr = [separator];
        private void UpdateMatchedEntities(Scene scene)
        {
            ArgumentNullException.ThrowIfNull(scene);
            if (!AnyRegistered)
            {
                return;
            }
            entitySet.Clear();

            foreach (var type in cachedTypes!)
            {
                var value = scene.Tracker.GetEntitiesTrackIfNeeded(type);
                entityTypeDict[type] = [.. value];
                entitySet.UnionWith(value);
            }

            var findIds = entityIdDict.Where(x => x.Value == null).Select(x => x.Key).ToHashSet();

            if (findIds.Count != 0)
            {
                foreach (var e in Scene.Entities)
                {
                    if (e.SourceData?.ID is not { } id)
                    {
                        continue;
                    }
                    if (findIds.Remove(id))
                    {
                        entityIdDict[id] = e;
                    }
                }
            }

            entitySet.UnionWith(entityIdDict.Values.Where(x => x is not null)!);
        }

        private readonly HashSet<Entity> entitySet = [];
        private readonly Dictionary<int, Entity?> entityIdDict = [];
        private readonly Dictionary<Type, HashSet<Entity>> entityTypeDict = [];
        private readonly HashSet<Type> cachedTypes = [];

        public readonly ReadOnlyDictionary<int, Entity?> EntityIdDict;
        public readonly ReadOnlyDictionary<Type, HashSet<Entity>> EntityTypeDict;
        public bool AnyRegistered => cachedTypes.Count != 0 || entityIdDict.Count != 0;
        public bool AnyFound => entitySet.Count != 0;

        private readonly string rawInput;
        public EntityTypeFilterComponent(string raw): base(true, false)
        {
            rawInput = raw;

            EntityIdDict = new(entityIdDict);
            EntityTypeDict = new(entityTypeDict);
        }

        public override void EntityAwake()
        {
            base.EntityAwake();
            if (string.IsNullOrWhiteSpace(rawInput))
            {
                return;
            }

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (string piece in rawInput.Split(separatorArr, StringSplitOptions.RemoveEmptyEntries))
            {
                string token = piece.Trim();
                if (token.Length == 0)
                {
                    continue;
                }

                if (int.TryParse(token, CultureInfo.InvariantCulture, out int entityId))
                {
                    entityIdDict.Add(entityId, null);
                }
                else
                {
                    var types = token.ParseSIDAndTypeIgnoreNone(assemblies);
                    cachedTypes.UnionWith(types);
                }
            }
            UpdateMatchedEntities(Scene);
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            if (entityIdDict.Count != 0)
            {
                sb.Append(string.Join(separator, entityIdDict.Keys));
            }
            if (cachedTypes != null && cachedTypes.Count != 0)
            {
                if (sb.Length != 0)
                {
                    sb.Append(separator);
                }
                sb.Append(string.Join(separator, cachedTypes.Select(x => x.FullName)));
            }
            return sb.ToString();
        }

        public bool Matches(Entity entity)
        {
            return entitySet.Contains(entity);
        }
        public Entity[] GetMatches()
        {
            return [.. entitySet];
        }
        public bool Matches(Entity entity, out Type? matchedType, out int? matchedEntityId)
        {
            matchedType = null;
            matchedEntityId = null;

            if (entity.IsGone(Scene))
            {
                return false;
            }

            if (entity.SourceData?.ID is { } id)
            {
                if (entityIdDict.TryGetValue(id, out var storedEntityById))
                {
                    if (storedEntityById is null)
                    {
                        matchedEntityId = id;
                        entityIdDict[id] = entity;
                        return true;
                    }
                    if (storedEntityById != entity)
                    {
                        Logger.Warn("BalintHelper/EntityTypeFilter", "two entities with matching ID apparently???");
                    }
                    return true;
                }
            }

            foreach (var kv in entityTypeDict)
            {
                if (kv.Value.Contains(entity))
                {
                    matchedType = kv.Key;
                    return true;
                }
            }

            return false;
        }
    }
}
