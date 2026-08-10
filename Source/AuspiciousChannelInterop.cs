using ModInteropImportGenerator;

namespace Celeste.Mod.BalintHelper
{
    [GenerateImports("auspicioushelper.templates", RequiredDependency = false)]
    public static partial class AuspiciousChannelInterop
    {
        public static partial double readChannel(string channelName);
        public static partial void setChannel(string channelName, double value);
    }
}
