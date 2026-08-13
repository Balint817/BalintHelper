using Celeste.Mod.Backdrops;
using FMOD;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Runtime.InteropServices;

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

        private const int SampleCount = 512;
        private const int MaxCallbackFrames = 4096;

        private readonly WaveformPlacement placement;
        private readonly Color color;
        private readonly float height;
        private readonly int barCount;
        private readonly float barSpacing;
        private readonly float smoothing;
        private readonly float gain;
        private readonly float edgeOffset;
        private readonly IdleBehavior idleBehavior;

        private readonly float[] targetAmplitudes;
        private readonly float[] currentAmplitudes;
        private readonly float[] waveBuffer = new float[SampleCount];

        private readonly object bufferLock = new();
        private readonly float[] ringBuffer = new float[SampleCount];
        private readonly float[] scratchInterleaved = new float[MaxCallbackFrames * 8];
        private int ringWritePos;
        private bool hasAudioData;

        private DSP? musicDsp;
        private bool dspCreated;
        private bool dspCreateFailed;
        private bool ended;
        private ChannelGroup? attachedGroup;
        private bool hasAttachedGroup;
        private readonly DSP_READCALLBACK readCallback;

        private float idleTimer;

        private bool loggedFirstUpdate;
        private bool loggedFirstRender;
        private int dspReadCallCount;
        private double lastDiagnosticLogTime;

        public MusicWaveform(BinaryPacker.Element data) : base()
        {
            placement = (WaveformPlacement)Enum.Parse(typeof(WaveformPlacement), data.Attr("placement", "Bottom"));
            color = Calc.HexToColor(data.Attr("color", "ffffff"));
            height = data.AttrFloat("height", 32f);
            barCount = Math.Max(4, data.AttrInt("barCount", 64));
            barSpacing = data.AttrFloat("barSpacing", 1f);
            smoothing = Calc.Clamp(data.AttrFloat("smoothing", 0.35f), 0f, 1f);
            gain = data.AttrFloat("gain", 1f);
            edgeOffset = data.AttrFloat("edgeOffset", 0f);
            idleBehavior = (IdleBehavior)Enum.Parse(typeof(IdleBehavior), data.Attr("idleBehavior", "Ripple"));

            targetAmplitudes = new float[barCount];
            currentAmplitudes = new float[barCount];

            readCallback = OnDspRead;

            Logger.Log(LogLevel.Info, LogTag,
                $"Constructed. placement={placement} height={height} barCount={barCount} idleBehavior={idleBehavior}");
        }

        public override void Ended(Scene scene)
        {
            base.Ended(scene);

            if (ended)
            {
                return;
            }
            ended = true;

            if (hasAttachedGroup && musicDsp != null)
            {
                RESULT removeResult = attachedGroup!.removeDSP(musicDsp);
                Logger.Log(LogLevel.Info, LogTag, $"Ended(): removeDSP result={removeResult}");
            }

            if (musicDsp != null)
            {
                RESULT releaseResult = musicDsp.release();
                Logger.Log(LogLevel.Info, LogTag, $"Ended(): DSP released, result={releaseResult}");
            }

            hasAttachedGroup = false;
            musicDsp = null;
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

            if (!TryReadWaveform())
            {
                ApplyIdleBehavior();
            }

            float smoothFactor = 1f - smoothing;
            for (int i = 0; i < barCount; i++)
            {
                currentAmplitudes[i] = Calc.Approach(currentAmplitudes[i], targetAmplitudes[i], smoothFactor * 4f * Engine.DeltaTime);
            }
        }

        private void LogPeriodic(string message)
        {
            double now = Engine.Scene?.RawTimeActive ?? 0.0;
            if (now - lastDiagnosticLogTime < 2.0)
            {
                return;
            }
            lastDiagnosticLogTime = now;
            Logger.Log(LogLevel.Info, LogTag, message);
        }

        private bool TryReadWaveform()
        {
            if (ended)
            {
                return false;
            }

            EventInstance instance = Audio.currentMusicEvent;
            if (instance == null || !instance.isValid())
            {
                return false;
            }

            instance.getPlaybackState(out PLAYBACK_STATE state);
            if (state != PLAYBACK_STATE.PLAYING && state != PLAYBACK_STATE.SUSTAINING)
            {
                return false;
            }

            if (instance.getChannelGroup(out ChannelGroup group) != RESULT.OK)
            {
                return false;
            }

            if (!EnsureDspAttached(group))
            {
                return false;
            }

            lock (bufferLock)
            {
                if (!hasAudioData)
                {
                    return false;
                }

                for (int i = 0; i < SampleCount; i++)
                {
                    int index = (ringWritePos + i) % SampleCount;
                    waveBuffer[i] = ringBuffer[index];
                }
            }

            int samplesPerBar = SampleCount / barCount;
            for (int bar = 0; bar < barCount; bar++)
            {
                float peak = 0f;
                int start = bar * samplesPerBar;
                int end = Math.Min(start + samplesPerBar, SampleCount);
                for (int s = start; s < end; s++)
                {
                    float abs = Math.Abs(waveBuffer[s]);
                    if (abs > peak)
                    {
                        peak = abs;
                    }
                }
                targetAmplitudes[bar] = Calc.Clamp(peak * gain, 0f, 1f);
            }

            return true;
        }

        private bool EnsureDspAttached(ChannelGroup group)
        {
            if (ended)
            {
                return false;
            }

            if (hasAttachedGroup && group.Equals(attachedGroup))
            {
                return true;
            }

            if (hasAttachedGroup && musicDsp != null)
            {
                attachedGroup!.removeDSP(musicDsp);
                hasAttachedGroup = false;
            }

            if (dspCreateFailed)
            {
                return false;
            }

            if (!dspCreated)
            {
                FMOD.Studio.System studioSystem = Audio.system;
                RESULT lowLevelResult = studioSystem.getLowLevelSystem(out FMOD.System coreSystem);
                if (lowLevelResult != RESULT.OK)
                {
                    dspCreateFailed = true;
                    Logger.Log(LogLevel.Error, LogTag, $"getLowLevelSystem failed: {lowLevelResult}");
                    return false;
                }

                char[] nameBuffer = new char[32];
                "BalintWaveform".CopyTo(0, nameBuffer, 0, "BalintWaveform".Length);

                DSP_DESCRIPTION description = new DSP_DESCRIPTION
                {
                    pluginsdkversion = 0x00010800,
                    name = nameBuffer,
                    version = 1,
                    numinputbuffers = 1,
                    numoutputbuffers = 1,
                    read = readCallback
                };

                RESULT createResult = coreSystem.createDSP(ref description, out DSP createdDsp);
                if (createResult != RESULT.OK)
                {
                    dspCreateFailed = true;
                    Logger.Log(LogLevel.Error, LogTag, $"createDSP failed: {createResult}");
                    return false;
                }

                musicDsp = createdDsp;
                dspCreated = true;
                Logger.Log(LogLevel.Info, LogTag, "DSP created successfully.");
            }

            if (musicDsp == null)
            {
                return false;
            }

            RESULT addResult = group.addDSP(CHANNELCONTROL_DSP_INDEX.HEAD, musicDsp);
            if (addResult != RESULT.OK)
            {
                Logger.Log(LogLevel.Warn, LogTag, $"addDSP failed: {addResult}");
                return false;
            }

            attachedGroup = group;
            hasAttachedGroup = true;
            Logger.Log(LogLevel.Info, LogTag, "DSP attached to music channel group.");

            lock (bufferLock)
            {
                hasAudioData = false;
                ringWritePos = 0;
                Array.Clear(ringBuffer, 0, ringBuffer.Length);
            }

            return true;
        }

        private RESULT OnDspRead(ref DSP_STATE dsp_state, IntPtr inbuffer, IntPtr outbuffer, uint length, int inchannels, ref int outchannels)
        {
            dspReadCallCount++;
            if (dspReadCallCount == 1)
            {
                Logger.Log(LogLevel.Info, LogTag, $"First DSP read callback fired. length={length} inchannels={inchannels}");
            }

            int frameCount = (int)length;
            int totalSamples = frameCount * inchannels;

            if (inbuffer == IntPtr.Zero || outbuffer == IntPtr.Zero || totalSamples <= 0 || totalSamples > scratchInterleaved.Length)
            {
                return RESULT.OK;
            }

            Marshal.Copy(inbuffer, scratchInterleaved, 0, totalSamples);

            lock (bufferLock)
            {
                for (int frame = 0; frame < frameCount; frame++)
                {
                    float sum = 0f;
                    int baseIndex = frame * inchannels;
                    for (int c = 0; c < inchannels; c++)
                    {
                        sum += scratchInterleaved[baseIndex + c];
                    }
                    float mono = inchannels > 0 ? sum / inchannels : 0f;

                    ringBuffer[ringWritePos] = mono;
                    ringWritePos = (ringWritePos + 1) % SampleCount;
                }
                hasAudioData = true;
            }

            Marshal.Copy(scratchInterleaved, 0, outbuffer, totalSamples);

            return RESULT.OK;
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

            float left = 0f;
            float top = 0f;
            float width = ScreenWidth;

            float totalSpacing = barSpacing * (barCount - 1);
            float barWidth = (width - totalSpacing) / barCount;
            if (barWidth <= 0f)
            {
                return;
            }

            if (placement == WaveformPlacement.Bottom || placement == WaveformPlacement.Both)
            {
                float baseline = ScreenHeight - edgeOffset;
                DrawBars(left, baseline, barWidth, -1f, drawColor, flipHorizontal: false);
            }

            if (placement == WaveformPlacement.Top || placement == WaveformPlacement.Both)
            {
                float baseline = top + edgeOffset;
                DrawBars(left, baseline, barWidth, 1f, drawColor, flipHorizontal: true);
            }
        }

        private void DrawBars(float left, float baseline, float barWidth, float direction, Color drawColor, bool flipHorizontal)
        {
            for (int i = 0; i < barCount; i++)
            {
                int index = flipHorizontal ? barCount - 1 - i : i;
                float amplitude = currentAmplitudes[index];
                float barHeight = Math.Max(1f, amplitude * height);

                float x = left + i * (barWidth + barSpacing);
                float y = direction < 0f ? baseline - barHeight : baseline;

                Draw.Rect(x, y, barWidth, barHeight, drawColor);
            }
        }
    }
}
