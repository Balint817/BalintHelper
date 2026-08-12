using Celeste.Mod.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.BalintHelper.Entities
{
    [CustomEntity("BalintHelper/MusicSliderController")]
    public class MusicSliderController : Entity
    {
        public enum Mode
        {
            Time,
            Percentage,
        }

        public readonly string SliderName;
        public readonly string? MusicName;
        public readonly Mode SliderMode;

        public MusicSliderController(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            SliderName = data.Attr("sliderName");
            MusicName = data.Attr("musicName", null);
            SliderMode = data.Enum("mode", Mode.Time);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);

            if (string.IsNullOrEmpty(SliderName))
            {
                RemoveSelf();
            }
        }

        public override void Update()
        {
            base.Update();

            if (Scene is not Level level)
            {
                RemoveSelf();
                return;
            }

            if (!string.IsNullOrWhiteSpace(MusicName) && Audio.CurrentMusic != MusicName)
            {
                SetSliderValue(level, -1f);
                return;
            }

            EventInstance instance = Audio.currentMusicEvent;
            if (instance == null || !instance.isValid())
            {
                SetSliderValue(level, -1f);
                return;
            }

            instance.getPlaybackState(out PLAYBACK_STATE state);
            if (state != PLAYBACK_STATE.PLAYING && state != PLAYBACK_STATE.SUSTAINING)
            {
                SetSliderValue(level, -1f);
                return;
            }

            instance.getTimelinePosition(out int positionMs);

            switch (SliderMode)
            {
                case Mode.Time:
                    SetSliderValue(level, positionMs / 1000f);
                    break;

                case Mode.Percentage:
                default:
                    instance.getDescription(out EventDescription description);
                    description.getLength(out int lengthMs);
                    float progress = lengthMs > 0 ? (float)positionMs / lengthMs : 0f;
                    SetSliderValue(level, progress);
                    break;
            }
        }

        private void SetSliderValue(Level level, float value)
        {
            level.Session.SetSlider(SliderName, value);
        }
    }
}