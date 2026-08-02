using System;

namespace Celeste.Mod.BalintHelper.Record
{
    public class SliderInfo
    {
        public readonly string Name;
        public SliderInfo(string name)
        {
            Name = name;
            ArgumentNullException.ThrowIfNullOrEmpty(name, nameof(name));

        }
    }
}
