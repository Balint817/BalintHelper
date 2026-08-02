using FMOD.Studio;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Celeste.Mod.BalintHelper.Record
{
    public class SfxEventInfo
    {
        public readonly string EventPath;
        public readonly IReadOnlyList<KeyValuePair<string, float>> Parameters;
        public readonly bool Loop;

        private EventDescription? _cachedDescription;
        private bool _cachedIs3D;

        public SfxEventInfo(string eventPath, string parameters, bool loop)
        {
            ArgumentException.ThrowIfNullOrEmpty(eventPath, nameof(eventPath));
            ArgumentNullException.ThrowIfNull(parameters, nameof(parameters));
            EventPath = eventPath;
            Loop = loop;

            var list = new List<KeyValuePair<string, float>>();
            foreach (var entry in parameters.Split(';'))
            {
                var trimmed = entry.Trim();
                if (trimmed.Length == 0)
                {
                    continue;
                }
                var parts = trimmed.Split('=', 2);
                if (parts.Length != 2)
                {
                    throw new ArgumentException($"invalid parameter entry \"{trimmed}\", expected format \"name=value\"", nameof(parameters));
                }
                var name = parts[0].Trim();
                if (name.Length == 0)
                {
                    throw new ArgumentException($"invalid parameter entry \"{trimmed}\", parameter name cannot be empty", nameof(parameters));
                }
                if (!float.TryParse(parts[1].Trim(), CultureInfo.InvariantCulture, out var value))
                {
                    throw new ArgumentException($"invalid parameter entry \"{trimmed}\", failed to parse value as a float", nameof(parameters));
                }
                list.Add(new(name, value));
            }
            Parameters = list;
        }

        public EventInstance? Play(Microsoft.Xna.Framework.Vector2? position)
        {
            if (_cachedDescription is null)
            {
                _cachedDescription = Audio.GetEventDescription(EventPath);
                if (_cachedDescription is not null)
                {
                    _cachedDescription.is3D(out _cachedIs3D);
                }
            }

            if (_cachedDescription is null)
            {
                return null;
            }

            _cachedDescription.createInstance(out var instance);
            if (instance is null)
            {
                return null;
            }

            if (_cachedIs3D && position.HasValue)
            {
                Audio.Position(instance, position.Value);
            }

            for (int i = 0; i < Parameters.Count; i++)
            {
                instance.setParameterValue(Parameters[i].Key, Parameters[i].Value);
            }

            instance.start();
            if (!Loop)
            {
                instance.release();
            }

            return instance;
        }
    }
}
