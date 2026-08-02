using System;

namespace Celeste.Mod.BalintHelper.Record
{
    public class FlagInfo
    {
        public readonly string Name;
        public FlagInfo(string name)
        {
            Name = name;
            ArgumentNullException.ThrowIfNullOrEmpty(name, nameof(name));

        }
    }
}
