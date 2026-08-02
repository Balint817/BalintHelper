using System;

namespace Celeste.Mod.BalintHelper.Record
{
    public class CounterInfo
    {
        public readonly string Name;
        public CounterInfo(string name)
        {
            Name = name;
            ArgumentNullException.ThrowIfNullOrEmpty(name, nameof(name));

        }
    }
}
