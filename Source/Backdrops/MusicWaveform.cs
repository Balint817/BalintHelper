using Celeste.Mod.Backdrops;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.BalintHelper.Backdrops
{
    [CustomBackdrop("BalintHelper/MusicWaveform")]
    public class MusicWaveform : Backdrop
    {
        private const string LogTag = "BalintHelper/MusicWaveform";

        private const float ScreenWidth = 320f;
        private const float ScreenHeight = 180f;

        public enum WaveformPlacement
        {
            Top,
            Bottom,
            Both
        }

        public enum IdleBehavior
        {
            Ripple,
            Flat
        }

        private readonly WaveformPlacement placement;
        private readonly Color color;
        private readonly float height;
        private readonly int barCount;
        private readonly float barFillPercent;
        private readonly float smoothing;
        private readonly float gain;
        private readonly float edgeOffset;
        private readonly IdleBehavior idleBehavior;

        private readonly float[] targetAmplitudes;
        private readonly float[] currentAmplitudes;

        private float idleTimer;
        private bool loggedFirstUpdate;
        private bool loggedFirstRender;

        public MusicWaveform(BinaryPacker.Element data) : base()
        {
            placement = (WaveformPlacement)Enum.Parse(typeof(WaveformPlacement), data.Attr("placement", "Bottom"));
            color = Calc.HexToColor(data.Attr("color", "ffffff"));
            height = data.AttrFloat("height", 32f);
            barCount = Math.Max(1, data.AttrInt("barCount", 64));
            barFillPercent = Calc.Clamp(data.AttrFloat("barFillPercent", 0.8f), 0.05f, 1f);
            smoothing = Calc.Clamp(data.AttrFloat("smoothing", 0.35f), 0f, 1f);
            gain = data.AttrFloat("gain", 1f);
            edgeOffset = data.AttrFloat("edgeOffset", 0f);
            idleBehavior = (IdleBehavior)Enum.Parse(typeof(IdleBehavior), data.Attr("idleBehavior", "Ripple"));

            targetAmplitudes = new float[barCount];
            currentAmplitudes = new float[barCount];

            Logger.Log(LogLevel.Info, LogTag,
                $"Constructed. placement={placement} height={height} barCount={barCount} barFillPercent={barFillPercent} idleBehavior={idleBehavior}");
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);

            if (!loggedFirstUpdate)
            {
                loggedFirstUpdate = true;
                Logger.Log(LogLevel.Info, LogTag, "First Update() call.");
            }

            if (!Visible)
            {
                return;
            }

            if (!MusicWaveformCapture.TryGetAmplitudes(targetAmplitudes, barCount, gain))
            {
                ApplyIdleBehavior();
            }

            float smoothFactor = 1f - smoothing;
            for (int i = 0; i < barCount; i++)
            {
                currentAmplitudes[i] = Calc.Approach(currentAmplitudes[i], targetAmplitudes[i], smoothFactor * 4f * Engine.DeltaTime);
            }
        }

        private void ApplyIdleBehavior()
        {
            switch (idleBehavior)
            {
                case IdleBehavior.Flat:
                    Array.Clear(targetAmplitudes, 0, targetAmplitudes.Length);
                    idleTimer = 0f;
                    break;

                case IdleBehavior.Ripple:
                default:
                    idleTimer += Engine.DeltaTime;
                    for (int i = 0; i < barCount; i++)
                    {
                        float phase = idleTimer * 1.5f + i * 0.35f;
                        targetAmplitudes[i] = (float)(0.08 + 0.05 * Math.Sin(phase));
                    }
                    break;
            }
        }

        public override void Render(Scene scene)
        {
            if (!loggedFirstRender)
            {
                loggedFirstRender = true;
                Logger.Log(LogLevel.Info, LogTag, "First Render() call.");
            }

            if (!Visible)
            {
                return;
            }

            Color drawColor = color * FadeAlphaMultiplier;
            if (drawColor.A <= 0)
            {
                return;
            }

            float bottomBaseline = (float)Math.Floor(ScreenHeight - edgeOffset);
            float topBaseline = (float)Math.Floor(edgeOffset);

            if (placement == WaveformPlacement.Bottom || placement == WaveformPlacement.Both)
            {
                DrawBars(bottomBaseline, -1f, drawColor, flipHorizontal: false);
            }

            if (placement == WaveformPlacement.Top || placement == WaveformPlacement.Both)
            {
                DrawBars(topBaseline, 1f, drawColor, flipHorizontal: true);
            }
        }

        private void DrawBars(float baseline, float direction, Color drawColor, bool flipHorizontal)
        {
            float slotWidth = ScreenWidth / barCount;

            for (int i = 0; i < barCount; i++)
            {
                int index = flipHorizontal ? barCount - 1 - i : i;
                float amplitude = currentAmplitudes[index];
                float barHeight = Math.Max(1f, (float)Math.Round(amplitude * height));

                float slotLeft = (float)Math.Round(i * slotWidth);
                float slotRight = (float)Math.Round((i + 1) * slotWidth);
                float slotPixelWidth = slotRight - slotLeft;

                float barWidth = Math.Max(1f, (float)Math.Round(slotPixelWidth * barFillPercent));
                float gap = slotPixelWidth - barWidth;
                float x = slotLeft + (float)Math.Floor(gap / 2f);

                float y = direction < 0f ? baseline - barHeight : baseline;

                Draw.Rect(x, y, barWidth, barHeight, drawColor);
            }
        }
    }
}