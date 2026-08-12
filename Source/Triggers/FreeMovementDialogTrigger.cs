using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.BalintHelper.Triggers
{
    // TODO
    [Tracked]
    [CustomEntity("BalintHelper/FreeMovementDialogTrigger")]
    public class FreeMovementDialogTrigger : Trigger
    {

        private static readonly HashSet<string> activeDialogIds = new();

        private readonly string dialogId;
        private readonly bool onlyOnce;
        private readonly bool endLevel;
        private readonly int deathCount;

        private bool triggered;

        public FreeMovementDialogTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            dialogId = data.Attr("dialogId", "");
            onlyOnce = data.Bool("onlyOnce", true);
            endLevel = data.Bool("endLevel", false);
            deathCount = data.Int("deathCount", -1);
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);

            if (onlyOnce && triggered)
            {
                return;
            }

            if (string.IsNullOrEmpty(dialogId))
            {
                return;
            }

            Level level = SceneAs<Level>();

            if (deathCount >= 0 && level.Session.DeathsInCurrentLevel != deathCount)
            {
                return;
            }

            if (activeDialogIds.Contains(dialogId))
            {
                return;
            }

            triggered = true;
            activeDialogIds.Add(dialogId);

            Scene.Add(new FreeDialogRunner(dialogId, endLevel, activeDialogIds));
        }

        private class FreeDialogRunner : Entity
        {

            private readonly string dialogId;
            private readonly bool endLevel;
            private readonly HashSet<string> activeDialogIds;

            public FreeDialogRunner(string dialogId, bool endLevel, HashSet<string> activeDialogIds)
            {
                this.dialogId = dialogId;
                this.endLevel = endLevel;
                this.activeDialogIds = activeDialogIds;
                Tag = Tags.HUD;
                Add(new Coroutine(Run()));
            }

            private IEnumerator Run()
            {
                yield return Textbox.Say(dialogId);

                activeDialogIds.Remove(dialogId);

                if (endLevel && Scene is Level level)
                {
                    level.CompleteArea(true, false, false);
                }

                RemoveSelf();
            }
        }
    }
}