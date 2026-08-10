using System;

namespace Celeste.Mod.BalintHelper.Record
{
    public class ChannelInfo
    {
        public readonly string Name;
        public ChannelInfo(string name)
        {
            if (!AuspiciousChannelInterop.IsImported)
            {
                throw new InvalidOperationException("cannot use channels because auspicioushelper is not loaded!");
            }
            Name = name;
            ArgumentNullException.ThrowIfNullOrEmpty(name, nameof(name));

        }
    }
}
