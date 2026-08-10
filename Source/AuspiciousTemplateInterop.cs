using Microsoft.Xna.Framework;
using ModInteropImportGenerator;
using Monocle;
using System;

namespace Celeste.Mod.BalintHelper
{
#pragma warning disable IDE1006 // this is an interop, casing is not my choice
    [GenerateImports("auspicioushelper.templates", RequiredDependency = false)]
    public static partial class AuspiciousTemplateInterop
    {
        public static partial void registerEntity(Entity template, Entity ent);
        public static partial void triggerTemplate(Entity template, Entity ent);
        public static partial DashCollisionResults registerDashhit(Entity template, Player p, Vector2 dir);
        public static partial Vector2 getTemplateLiftspeed(Entity template);
        public static partial void customClarify(string name, Func<Level, LevelData, Vector2, EntityData, Component> loader);
    }
#pragma warning restore IDE1006
}
