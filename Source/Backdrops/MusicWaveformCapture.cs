using FMOD;
using FMOD.Studio;
using Monocle;
using System;
using System.Runtime.InteropServices;

namespace Celeste.Mod.BalintHelper.Backdrops
{
    // The class shared by all MusicWaveform instances.
    // This is to solve the issue that respawns discard and recreate Backdrops without calling Ended,
    // so a DSP tied to an instance would get orphaned evry single reload.
    // After enough reloads this would crash the runtime.
    internal static class MusicWaveformCapture
    {
        private const string LogTag = "BalintHelper/MusicWaveformCapture";

        private const int SampleCount = 512;
        private const int MaxCallbackFrames = 4096;

        private static readonly object bufferLock = new();
        private static readonly float[] ringBuffer = new float[SampleCount];
        private static readonly float[] waveBuffer = new float[SampleCount];
        private static readonly float[] scratchInterleaved = new float[MaxCallbackFrames * 8];
        private static int ringWritePos;
        private static bool hasAudioData;

        private static DSP? musicDsp;
        private static bool dspCreated;
        private static bool dspCreateFailed;
        private static ChannelGroup? attachedGroup;
        private static bool hasAttachedGroup;

        private static readonly DSP_READCALLBACK ReadCallback = OnDspRead;

        private static int dspReadCallCount;

        // Attempts to populate the targetAmplitudes[barCount] buffer with
        // the current song's per-bar peak amplitude, scaled by the gain and clamped to [0, 1].
        // Returns false if no music is currently audible or no audio data has been captured yet.
        public static bool TryGetAmplitudes(float[] targetAmplitudes, int barCount, float gain, float earlyPower, float latePower)
        {
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

            if (!EnsureAttached(group))
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
                var result = MathF.Pow(Calc.Clamp(MathF.Pow(peak * gain, earlyPower), 0f, 1f), latePower);
                if (!float.IsFinite(result))
                {
                    result = 0;
                }
                targetAmplitudes[bar] = result;
            }

            return true;
        }

        private static bool EnsureAttached(ChannelGroup group)
        {
            if (hasAttachedGroup && group.Equals(attachedGroup))
            {
                return true;
            }

            // detach from the previous group before attaching elsewhere
            if (hasAttachedGroup && musicDsp != null && attachedGroup != null)
            {
                RESULT detachResult = attachedGroup.removeDSP(musicDsp);
                Logger.Log(LogLevel.Info, LogTag, $"Detached DSP from previous group, result={detachResult}");
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
                    read = ReadCallback
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
                Logger.Log(LogLevel.Info, LogTag, "DSP created");
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

        private static RESULT OnDspRead(ref DSP_STATE dsp_state, IntPtr inbuffer, IntPtr outbuffer, uint length, int inchannels, ref int outchannels)
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
    }
}