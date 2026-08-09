using Celeste.Mod.BalintHelper.Components;
using Celeste.Mod.BalintHelper.Entities;
using Celeste.Mod.BalintHelper.Triggers;
using Celeste.Mod.BalintHelper.Utils.Dynamic;
using Celeste.Mod.Registry;
using DynamicInstructions.Instructions;
using Microsoft.Xna.Framework;
using ModInteropImportGenerator;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.ModInterop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.BalintHelper
{
    //[ModImportName("auspicioushelper.templates")]
    //public static class TemplateIop
    //{
    //    public static class EntityParseTypes
    //    {
    //        public const int unable = 0; //will not include this entity in templates
    //        public const int platformbasic = 1; //basic platform; use moveV/moveH when moving
    //        public const int unwrapped = 2; //use this entity directly; do not put into tree
    //        public const int basic = 3; //basic entity; movement done via position change
    //    }

    //    //Here is a template class with callbacks when stuff changes in the template. If you leave values
    //    //null, they will use the default implementation
    //    public class TemplateChildComponent : Component
    //    {
    //        public TemplateChildComponent(Entity ent) : base(false, false)
    //        {
    //            Entity = ent;
    //        }
    //        //This is a reference to the template's parent and should not be changed
    //        Entity parent = null;
    //        //Called when this is added to a template. Parent will be non-null before this function.
    //        //<IMPORTANT> This is called before Entity.Added is called. For maximum compatibility,
    //        //make sure that AddSelf returns all associated entities to this one before you return from it.
    //        public Action<Scene> AddTo = null;
    //        //This function should add your entity and any entities it makes to the provided list.
    //        public Action<List<Entity>> AddSelf = null;


    //        //Called when your entity repositions; First parameter is the new location, second parameter is the liftspeed.
    //        //If you have definied SetOffset, the location will be the location of the template; if you have not, I try to
    //        //guess a location based on your original entity's location!
    //        public Action<Vector2, Vector2> RepositionCB = null;
    //        public Action<Vector2> SetOffsetCB = null;

    //        //Called when the template changes visibility, collidability and active status (in order).
    //        //0 means no change, 1 means set to true, -1 means set to false. 
    //        //Note that this is the parent collidability; your component should only be actually collidable
    //        //if it's normal logic would have it be collidable AND the last value from these was 1.
    //        public Action<int, int, int> ChangeStatusCB = null;
    //        //You can also read from these parameters to get the current status
    //        public bool ParentVisible = true;
    //        public bool ParentCollidable = true;
    //        public bool ParentActive = true;

    //        //Called when the template this entity is a part of is destroyed. Parameter is true if particles/debris
    //        //should be used. Should remove the current entity and any children
    //        public Action<bool> DestroyCB = null;

    //        public void TriggerParent() => triggerTemplate(parent, Entity);
    //        //call this when your solids are hit please <3
    //        public DashCollisionResults RegisterDashhit(Player p, Vector2 dir) => registerDashhit(parent, p, dir);
    //        public void RegisterEntity() => registerEntity(parent, Entity);
    //        public Vector2 getParentLiftspeed() => getTemplateLiftspeed(parent);
    //    }
    //    public static Action<string, int, Level.EntityLoader> clarify;
    //    public static Action<string, Func<Level, LevelData, Vector2, EntityData, Component>> customClarify;
    //    public static Action<Entity, Entity> triggerTemplate;
    //    public static Func<Entity, Player, Vector2, DashCollisionResults> registerDashhit;

    //    //Call this function on any entities your entity adds that are 'part of it' after addTo has been called. This sets up visuals and necessary components if applicable.
    //    public static Action<Entity, Entity> registerEntity;

    //    //Get the liftspeed of the template containing this entity.
    //    public static Func<Entity, Vector2> getTemplateLiftspeed;

    //    //Get the template parent of the given entity
    //    public static Func<Entity, Entity> getParentTemplate;
    //}


    [GenerateImports("auspicioushelper.templates", RequiredDependency = false)]
    public static partial class AuspiciousTemplateInterop
    {
        public static partial void registerEntity(Entity template, Entity ent);
    }

    [GenerateImports("auspicioushelper.templates", RequiredDependency = false)]
    public static partial class AuspiciousChannelInterop
    {
        public static partial double readChannel(string channelName);
        public static partial void setChannel(string channelName, double value);
    }
    public class BalintHelperModuleSession : EverestModuleSession
    {
        public Dictionary<string, string?> SessionsStrings = [];
    }
    public class BalintHelperModule : EverestModule
    {
        public static BalintHelperModule Instance { get; private set; } = null!;
        public override Type SessionType => typeof(BalintHelperModuleSession);
        public static BalintHelperModuleSession Session => (BalintHelperModuleSession)Instance._Session!;
        public override void Load()
        {
            Instance = this;

            EntityRegistry_SidToTypes = new((Dictionary<string, HashSet<Type>>)typeof(EntityRegistry).GetField("SidToTypes", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!);
            EntityRegistry_TypeToSids = new((Dictionary<Type, HashSet<string>>)typeof(EntityRegistry).GetField("TypeToSids", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!);

            On.Celeste.FloatingDebris.OnExplode += OnFloatingDebrisExplode;
            On.Celeste.OuiOptions.Update += OuiOptions_Update;

            IL.Celeste.Player.OnCollideV += PatchOnCollideV;
            IL.Celeste.Player.OnCollideH += PatchOnCollideH;

            Everest.Events.CustomBirdTutorial.OnParseCommand += CustomBirdTutorial_OnParseCommand;

            On.Celeste.Player.Pickup += Player_Pickup;

            On.Celeste.Solid.GetPlayerRider += Solid_GetPlayerRider;

            foreach (var item in CustomInstructionValueHandler.AllInstances)
            {
                item.Load();
            }

            TypeNameCodec.ResolveAmbiguousType += AmbiguousTypeResolver;
        }
        public override void Initialize()
        {
            base.Initialize();

            AuspiciousTemplateInterop.Load();
            AuspiciousChannelInterop.Load();
        }
        public override void Unload()
        {
            On.Celeste.FloatingDebris.OnExplode -= OnFloatingDebrisExplode;
            On.Celeste.OuiOptions.Update -= OuiOptions_Update;

            IL.Celeste.Player.OnCollideV -= PatchOnCollideV;
            IL.Celeste.Player.OnCollideH -= PatchOnCollideH;

            Everest.Events.CustomBirdTutorial.OnParseCommand -= CustomBirdTutorial_OnParseCommand;

            On.Celeste.Player.Pickup -= Player_Pickup;

            On.Celeste.Solid.GetPlayerRider -= Solid_GetPlayerRider;

            foreach (var item in CustomInstructionValueHandler.AllInstances)
            {
                item.Unload();
            }

            TypeNameCodec.ResolveAmbiguousType -= AmbiguousTypeResolver;

            Instance = null!;
        }
        private bool AmbiguousTypeResolver(string typeName, IEnumerable<Type> matches, [MaybeNullWhen(false)] out Type resolvedType)
        {
            var sorted = matches.OrderBy(t => t.FullName!.Length).ToArray();
            resolvedType = sorted[0];
            var excludeNames = new HashSet<string>()
            {
                "On." + resolvedType.FullName,
                "IL." + resolvedType.FullName,
            };
            foreach (var item in sorted[1..])
            {
                if (!excludeNames.Contains(item.FullName!))
                {
                    return false;
                }
            }
            return true;
        }
        private static Player Solid_GetPlayerRider(On.Celeste.Solid.orig_GetPlayerRider orig, Solid self)
        {
            if (self.Scene?.Tracker?.GetEntity<Player>() is not { } player
                || player.Get<PlayerAddRider>() is not { } component
                || component.TargetMover.Platform != self)
            {
                return orig(self);
            }
            component.Used = true;
            return player;
        }

        private static bool Player_Pickup(On.Celeste.Player.orig_Pickup orig, Player self, Holdable pickup)
        {
            if (self.Scene is not { } scene || HoldablePriorityController.GetOrCreate(scene) is not { } controller)
            {
                return orig(self, pickup);
            }
            var target = controller.GetTarget(self);
            if (target == null)
            {
                // this branch might need some performance checks?
                return orig(self, pickup);
            }
            return orig(self, target);
        }
        private static void OuiOptions_Update(On.Celeste.OuiOptions.orig_Update orig, OuiOptions self)
        {
            orig(self);

            //TODO: add patch to EeveeHelper to block demo binds!
            //var keyboardKeys = Settings.Instance.Dash.Keyboard.Intersect(Settings.Instance.Down.Keyboard.Concat(Settings.Instance.DownMoveOnly.Keyboard)).ToHashSet();
            //var controllerKeys = Settings.Instance.Dash.Controller.Intersect(Settings.Instance.Down.Controller.Concat(Settings.Instance.DownMoveOnly.Controller)).ToHashSet();
            //var mouseKeys = Settings.Instance.Dash.Mouse.Intersect(Settings.Instance.Down.Mouse.Concat(Settings.Instance.DownMoveOnly.Mouse)).ToHashSet();
        }
        private MTexture CustomBirdTutorial_OnParseCommand(string command)
        {
            if (command == "GroundedUltra")
            {
                return GFX.Gui["tech/BalintHelper/grounded_ultra"];
            }
            return null!;
        }

        private static void PatchOnCollideV(ILContext il) => PatchCanCurveDashAssignment(il);
        private static void PatchOnCollideH(ILContext il) => PatchCanCurveDashAssignment(il);
        private static void PatchCanCurveDashAssignment(ILContext il)
        {
            // Starting point:
            // ```
            // this.canCurveDash = false;
            // ```
            var cursor = new ILCursor(il);

            if (!cursor.TryGotoNext(
                MoveType.Before,
                i => i.MatchLdarg(0),
                i => i.MatchLdcI4(0),
                i => i.MatchStfld<Player>("canCurveDash")
            ))
            {
                throw new Exception("Couldn't find canCurveDash assignment in " + il.Method.FullName);
            }

            // We're now positioned before:
            // ldarg.0
            // ldc.i4.0
            // stfld Player::canCurveDash

            ILLabel skipAssign = cursor.DefineLabel();

            // Mark the instruction immediately after the stfld as the branch target.
            cursor.Index += 3;
            cursor.MarkLabel(skipAssign);

            // Go back before the assignment and inject:
            // ldarg.0
            // call AllowCurveCheck
            // brtrue skipAssign
            cursor.Index -= 3;
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate(AllowCurveCheck);
            cursor.Emit(OpCodes.Brtrue, skipAssign);

            // End result:
            // ```
            // if (!AllowCurveCheck(this)) {
            //   this.canCurveDash = false;
            // }
            // ```
        }
        private static bool AllowCurveCheck(Player self)
        {
            var entities = self.Scene?.Tracker?.Entities;
            if (entities is null)
            {
                return false;
            }
            return entities[typeof(AllowCurveOnCollideTrigger)].Cast<AllowCurveOnCollideTrigger>().Any(x => x.IsTriggered);

        }
        public ReadOnlyDictionary<string, HashSet<Type>> EntityRegistry_SidToTypes { get; private set; } = null!;
        public ReadOnlyDictionary<Type, HashSet<string>> EntityRegistry_TypeToSids { get; private set; } = null!;
        private static void OnFloatingDebrisExplode(On.Celeste.FloatingDebris.orig_OnExplode orig, FloatingDebris self, Vector2 from)
        {
            if (self is SilentFloatingDebris silentDebris)
            {
                silentDebris.TriggerExplodeEvent(from);
            }
            else
            {
                orig(self, from);
            }
        }
    }
}
