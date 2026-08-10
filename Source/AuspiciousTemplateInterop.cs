using ModInteropImportGenerator;
using Monocle;

namespace Celeste.Mod.BalintHelper
{
    [GenerateImports("auspicioushelper.templates", RequiredDependency = false)]
    public static partial class AuspiciousTemplateInterop
    {
        public static partial void registerEntity(Entity template, Entity ent);
    }
}
