namespace InfiniTD_2.Framewok.Audio
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public static class AudioSynth
    {
        [StructLayout(LayoutKind.Sequential)]
        struct WAVEFORMATEX
        {
            public ushort wFormatTag;
            public ushort nChannels;
            public uint nSamplesPerSec;
            public uint nAvgBytesPerSec;
            public ushort nBlockAlign;
            public ushort wBitsPerSample;
            public ushort cbSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct WAVEHDR
        {
            public IntPtr lpData;
            public uint dwBufferLength;
            public uint dwBytesRecorded;
            public IntPtr dwUser;
            public uint dwFlags;
            public uint dwLoops;
            public IntPtr lpNext;
            public IntPtr reserved;
        }

        [DllImport("winmm.dll")]
        static extern int waveOutOpen(out IntPtr hWaveOut, int uDeviceID, IntPtr pwfx, IntPtr callback, IntPtr dwInstance, int fdwOpen);

        [DllImport("winmm.dll")]
        static extern int waveOutClose(IntPtr hWaveOut);

        [DllImport("winmm.dll")]
        static extern int waveOutPrepareHeader(IntPtr hWaveOut, IntPtr pwh, int cbwh);

        [DllImport("winmm.dll")]
        static extern int waveOutUnprepareHeader(IntPtr hWaveOut, IntPtr pwh, int cbwh);

        [DllImport("winmm.dll")]
        static extern int waveOutWrite(IntPtr hWaveOut, IntPtr pwh, int cbwh);

        [DllImport("winmm.dll")]
        static extern int waveOutReset(IntPtr hWaveOut);

        [DllImport("winmm.dll")]
        static extern int waveOutSetVolume(IntPtr hWaveOut, uint dwVolume);

        private static IntPtr hWaveOut = IntPtr.Zero;
        private static bool isInitialized = false;

        const int SAMPLE_RATE = 44100;
        const int CHANNELS = 2;
        const int BITS_PER_SAMPLE = 16;
        const int BUFFER_SAMPLES = 1024; // Маленький буфер для streaming
        const int NUM_BUFFERS = 8; // 4 буфера в очереди

        // Активные голоса
        private static List<Voice> voices = new List<Voice>();

        struct Voice
        {
            public short[] Samples;
            public int Position;
            public bool IsActive;
        }

        // Буферы для waveOut
        private static IntPtr[] bufferDataPtrs = new IntPtr[NUM_BUFFERS];
        private static IntPtr[] bufferWhPtrs = new IntPtr[NUM_BUFFERS];
        private static bool[] bufferInUse = new bool[NUM_BUFFERS];

        // Микшер буфер
        private static float[] mixBuffer = new float[BUFFER_SAMPLES * CHANNELS];
        private static short[] outputBuffer = new short[BUFFER_SAMPLES * CHANNELS];

        public static void Init()
        {
            if (isInitialized) return;

            WAVEFORMATEX wfx = new WAVEFORMATEX
            {
                wFormatTag = 1,
                nChannels = CHANNELS,
                nSamplesPerSec = SAMPLE_RATE,
                nAvgBytesPerSec = (uint)(SAMPLE_RATE * CHANNELS * BITS_PER_SAMPLE / 8),
                nBlockAlign = (ushort)(CHANNELS * BITS_PER_SAMPLE / 8),
                wBitsPerSample = BITS_PER_SAMPLE,
                cbSize = 0
            };

            IntPtr pwfx = Marshal.AllocHGlobal(Marshal.SizeOf(wfx));
            Marshal.StructureToPtr(wfx, pwfx, false);

            int result = waveOutOpen(out hWaveOut, 0, pwfx, IntPtr.Zero, IntPtr.Zero, 0);

            Marshal.FreeHGlobal(pwfx);

            if (result != 0)
            {
                Console.WriteLine($"waveOutOpen failed: {result}");
                return;
            }

            // Инициализируем буферы
            for (int i = 0; i < NUM_BUFFERS; i++)
            {
                bufferInUse[i] = false;
            }

            isInitialized = true;
            Console.WriteLine($"AudioSynth initialized with {NUM_BUFFERS} buffers");
        }

        public static void PlaySample(short[] sample)
        {
            if (!isInitialized || sample == null) return;

            voices.Add(new Voice
            {
                Samples = sample,
                Position = 0,
                IsActive = true
            });
        }

        public static void Update()
        {
            if (!isInitialized) return;

            // Проверяем какие буферы освободились
            for (int i = 0; i < NUM_BUFFERS; i++)
            {
                if (bufferInUse[i])
                {
                    WAVEHDR wh = Marshal.PtrToStructure<WAVEHDR>(bufferWhPtrs[i]);
                    if ((wh.dwFlags & 1) != 0) // WHDR_DONE
                    {
                        waveOutUnprepareHeader(hWaveOut, bufferWhPtrs[i], Marshal.SizeOf(wh));
                        Marshal.FreeHGlobal(wh.lpData);
                        Marshal.FreeHGlobal(bufferWhPtrs[i]);
                        bufferInUse[i] = false;
                    }
                }
            }

            // Находим свободный буфер
            int freeBuffer = -1;
            for (int i = 0; i < NUM_BUFFERS; i++)
            {
                if (!bufferInUse[i])
                {
                    freeBuffer = i;
                    break;
                }
            }

            if (freeBuffer == -1) return; // Нет свободных буферов

            // Микшируем все активные голоса
            Array.Clear(mixBuffer, 0, mixBuffer.Length);

            for (int i = 0; i < voices.Count; i++)
            {
                if (!voices[i].IsActive) continue;

                Voice voice = voices[i];
                int samplesToMix = Math.Min(BUFFER_SAMPLES, (voice.Samples.Length - voice.Position) / CHANNELS);

                if (samplesToMix <= 0)
                {
                    voice.IsActive = false;
                    voices[i] = voice;
                    continue;
                }

                for (int j = 0; j < samplesToMix * CHANNELS; j++)
                {
                    if (voice.Position + j < voice.Samples.Length)
                    {
                        mixBuffer[j] += voice.Samples[voice.Position + j] / 32767f;
                    }
                }

                voice.Position += samplesToMix * CHANNELS;
                if (voice.Position >= voice.Samples.Length)
                {
                    voice.IsActive = false;
                }
                voices[i] = voice;
            }

            // Удаляем неактивные голоса
            voices.RemoveAll(v => !v.IsActive);

            // Конвертируем в short
            for (int i = 0; i < BUFFER_SAMPLES * CHANNELS; i++)
            {
                float sample = mixBuffer[i];
                sample = Math.Max(-1f, Math.Min(1f, sample));
                outputBuffer[i] = (short)(sample * 32767);
            }

            // Отправляем буфер
            SendBuffer(freeBuffer);
        }

        private static void SendBuffer(int bufferIndex)
        {
            IntPtr dataPtr = Marshal.AllocHGlobal(outputBuffer.Length * sizeof(short));
            Marshal.Copy(outputBuffer, 0, dataPtr, outputBuffer.Length);

            WAVEHDR wh = new WAVEHDR
            {
                lpData = dataPtr,
                dwBufferLength = (uint)(outputBuffer.Length * sizeof(short)),
                dwBytesRecorded = 0,
                dwUser = IntPtr.Zero,
                dwFlags = 0,
                dwLoops = 0,
                lpNext = IntPtr.Zero,
                reserved = IntPtr.Zero
            };

            IntPtr whPtr = Marshal.AllocHGlobal(Marshal.SizeOf(wh));
            Marshal.StructureToPtr(wh, whPtr, false);

            int result = waveOutPrepareHeader(hWaveOut, whPtr, Marshal.SizeOf(wh));
            if (result != 0)
            {
                Marshal.FreeHGlobal(dataPtr);
                Marshal.FreeHGlobal(whPtr);
                return;
            }

            result = waveOutWrite(hWaveOut, whPtr, Marshal.SizeOf(wh));
            if (result == 0)
            {
                bufferDataPtrs[bufferIndex] = dataPtr;
                bufferWhPtrs[bufferIndex] = whPtr;
                bufferInUse[bufferIndex] = true;
            }
            else
            {
                waveOutUnprepareHeader(hWaveOut, whPtr, Marshal.SizeOf(wh));
                Marshal.FreeHGlobal(dataPtr);
                Marshal.FreeHGlobal(whPtr);
            }
        }

        public static void PlayPiano(int frequency, float duration = 0.3f, float release = 0.1f, float velocity = 1.0f)
        {
            if (!isInitialized) return;

            int samples = (int)(SAMPLE_RATE * duration);
            if (samples <= 0) return;

            short[] buffer = new short[samples * CHANNELS];

            float attackTime = 0.01f;
            float decayTime = 0.05f;
            float sustainLevel = 0.7f;
            float releaseTime = Math.Min(release, duration * 0.3f); // Release не больше 30% от duration

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;

                float envelope;
                if (t < attackTime)
                {
                    envelope = t / attackTime;
                }
                else if (t < attackTime + decayTime)
                {
                    float decayT = (t - attackTime) / decayTime;
                    envelope = 1.0f - (1.0f - sustainLevel) * decayT;
                }
                else if (t < duration - releaseTime)
                {
                    envelope = sustainLevel;
                }
                else
                {
                    float releaseT = (t - (duration - releaseTime)) / releaseTime;
                    envelope = sustainLevel * (1.0f - releaseT);
                }

                float sample = (float)Math.Sin(2 * Math.PI * frequency * t) * envelope * short.MaxValue * 0.5f * velocity;
                int clamped = (int)sample;
                if (clamped > short.MaxValue) clamped = short.MaxValue;
                if (clamped < short.MinValue) clamped = short.MinValue;
                buffer[i * CHANNELS] = (short)clamped;
                buffer[i * CHANNELS + 1] = (short)clamped;
            }

            PlaySample(buffer);
        }

        public static void PlayBass(int frequency, float duration = 0.3f, float release = 0.1f, float velocity = 1.0f)
        {
            if (!isInitialized) return;
            int samples = (int)(SAMPLE_RATE * duration);
            short[] buffer = new short[samples * CHANNELS];

            float attackTime = 0.005f;
            float decayTime = 0.02f;
            float sustainLevel = 0.8f;
            float releaseTime = release;

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float saw = 2 * ((float)(t * frequency) - (float)Math.Floor(t * frequency + 0.5f));

                float envelope;
                if (t < attackTime)
                {
                    envelope = t / attackTime;
                }
                else if (t < attackTime + decayTime)
                {
                    float decayT = (t - attackTime) / decayTime;
                    envelope = 1.0f - (1.0f - sustainLevel) * decayT;
                }
                else if (t < duration - releaseTime)
                {
                    envelope = sustainLevel;
                }
                else
                {
                    float releaseT = (t - (duration - releaseTime)) / releaseTime;
                    envelope = sustainLevel * (1.0f - releaseT);
                }

                float sample = saw * envelope * short.MaxValue * 0.6f * velocity;
                int clamped = (int)sample;
                if (clamped > short.MaxValue) clamped = short.MaxValue;
                if (clamped < short.MinValue) clamped = short.MinValue;
                buffer[i * CHANNELS] = (short)clamped;
                buffer[i * CHANNELS + 1] = (short)clamped;
            }

            PlaySample(buffer);
        }

        public static void PlayLead(int frequency, float duration = 0.4f, float release = 0.15f, float velocity = 1.0f)
        {
            if (!isInitialized) return;
            int samples = (int)(SAMPLE_RATE * duration);
            short[] buffer = new short[samples * CHANNELS];

            float attackTime = 0.01f;
            float decayTime = 0.03f;
            float sustainLevel = 0.6f;
            float releaseTime = release;

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float saw1 = 2 * ((float)(t * frequency) - (float)Math.Floor(t * frequency + 0.5f));
                float saw2 = 2 * ((float)(t * frequency * 1.01f) - (float)Math.Floor(t * frequency * 1.01f + 0.5f));

                float envelope;
                if (t < attackTime)
                {
                    envelope = t / attackTime;
                }
                else if (t < attackTime + decayTime)
                {
                    float decayT = (t - attackTime) / decayTime;
                    envelope = 1.0f - (1.0f - sustainLevel) * decayT;
                }
                else if (t < duration - releaseTime)
                {
                    envelope = sustainLevel;
                }
                else
                {
                    float releaseT = (t - (duration - releaseTime)) / releaseTime;
                    envelope = sustainLevel * (1.0f - releaseT);
                }

                float sample = (saw1 + saw2) / 2 * envelope * short.MaxValue * 0.4f * velocity;
                int clamped = (int)sample;
                if (clamped > short.MaxValue) clamped = short.MaxValue;
                if (clamped < short.MinValue) clamped = short.MinValue;
                buffer[i * CHANNELS] = (short)clamped;
                buffer[i * CHANNELS + 1] = (short)clamped;
            }

            PlaySample(buffer);
        }

        public static void PlayPad(int frequency, float duration = 1.0f, float release = 0.3f, float velocity = 1.0f)
        {
            if (!isInitialized) return;
            int samples = (int)(SAMPLE_RATE * duration);
            short[] buffer = new short[samples * CHANNELS];

            float attackTime = 0.1f;   // Медленная атака
            float decayTime = 0.2f;
            float sustainLevel = 0.8f;
            float releaseTime = release;

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;

                float envelope;
                if (t < attackTime)
                {
                    envelope = t / attackTime;
                }
                else if (t < attackTime + decayTime)
                {
                    float decayT = (t - attackTime) / decayTime;
                    envelope = 1.0f - (1.0f - sustainLevel) * decayT;
                }
                else if (t < duration - releaseTime)
                {
                    envelope = sustainLevel;
                }
                else
                {
                    float releaseT = (t - (duration - releaseTime)) / releaseTime;
                    envelope = sustainLevel * (1.0f - releaseT);
                }

                float sample = (float)Math.Sin(2 * Math.PI * frequency * t) * envelope * short.MaxValue * 0.3f * velocity;
                int clamped = (int)sample;
                if (clamped > short.MaxValue) clamped = short.MaxValue;
                if (clamped < short.MinValue) clamped = short.MinValue;
                buffer[i * CHANNELS] = (short)clamped;
                buffer[i * CHANNELS + 1] = (short)clamped;
            }

            PlaySample(buffer);
        }

        public static void PlayKick()
        {
            if (!isInitialized) return;
            int samples = (int)(SAMPLE_RATE * 0.2f);
            short[] buffer = new short[samples * CHANNELS];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float freq = 150 * (float)Math.Exp(-t * 20);
                float envelope = (float)Math.Exp(-t * 15);
                float sample = (float)Math.Sin(2 * Math.PI * freq * t) * envelope * short.MaxValue * 0.8f;
                int clamped = (int)sample;
                if (clamped > short.MaxValue) clamped = short.MaxValue;
                if (clamped < short.MinValue) clamped = short.MinValue;
                buffer[i * CHANNELS] = (short)clamped;
                buffer[i * CHANNELS + 1] = (short)clamped;
            }

            PlaySample(buffer);
        }

        public static void PlayKick808()
        {
            if (!isInitialized) return;
            int samples = (int)(SAMPLE_RATE * 0.3f);
            short[] buffer = new short[samples * CHANNELS];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float freq = 100 * (float)Math.Exp(-t * 15);
                float envelope = (float)Math.Exp(-t * 10);
                float sample = (float)Math.Sin(2 * Math.PI * freq * t) * envelope * short.MaxValue * 0.9f;
                int clamped = (int)sample;
                if (clamped > short.MaxValue) clamped = short.MaxValue;
                if (clamped < short.MinValue) clamped = short.MinValue;
                buffer[i * CHANNELS] = (short)clamped;
                buffer[i * CHANNELS + 1] = (short)clamped;
            }

            PlaySample(buffer);
        }

        public static void PlaySnare()
        {
            if (!isInitialized) return;
            int samples = (int)(SAMPLE_RATE * 0.15f);
            short[] buffer = new short[samples * CHANNELS];
            Random rand = new Random();

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float envelope = (float)Math.Exp(-t * 20);
                float noise = ((float)rand.NextDouble() * 2 - 1) * envelope * short.MaxValue * 0.5f;
                float tone = (float)Math.Sin(2 * Math.PI * 200 * t) * envelope * short.MaxValue * 0.3f;
                int clamped = (int)(noise + tone);
                if (clamped > short.MaxValue) clamped = short.MaxValue;
                if (clamped < short.MinValue) clamped = short.MinValue;
                buffer[i * CHANNELS] = (short)clamped;
                buffer[i * CHANNELS + 1] = (short)clamped;
            }

            PlaySample(buffer);
        }

        public static void PlaySnare808()
        {
            if (!isInitialized) return;
            int samples = (int)(SAMPLE_RATE * 0.12f);
            short[] buffer = new short[samples * CHANNELS];
            Random rand = new Random();

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float envelope = (float)Math.Exp(-t * 30);
                float noise = ((float)rand.NextDouble() * 2 - 1) * envelope * short.MaxValue * 0.7f;
                float tone = (float)Math.Sin(2 * Math.PI * 250 * t) * envelope * short.MaxValue * 0.4f;
                int clamped = (int)(noise + tone);
                if (clamped > short.MaxValue) clamped = short.MaxValue;
                if (clamped < short.MinValue) clamped = short.MinValue;
                buffer[i * CHANNELS] = (short)clamped;
                buffer[i * CHANNELS + 1] = (short)clamped;
            }

            PlaySample(buffer);
        }

        public static void PlayHiHat()
        {
            if (!isInitialized) return;
            int samples = (int)(SAMPLE_RATE * 0.05f);
            short[] buffer = new short[samples * CHANNELS];
            Random rand = new Random();

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float envelope = (float)Math.Exp(-t * 50);
                float noise = ((float)rand.NextDouble() * 2 - 1) * envelope * short.MaxValue * 0.4f;
                int clamped = (int)noise;
                if (clamped > short.MaxValue) clamped = short.MaxValue;
                if (clamped < short.MinValue) clamped = short.MinValue;
                buffer[i * CHANNELS] = (short)clamped;
                buffer[i * CHANNELS + 1] = (short)clamped;
            }

            PlaySample(buffer);
        }

        public static void PlayHiHatOpen()
        {
            if (!isInitialized) return;
            int samples = (int)(SAMPLE_RATE * 0.15f);
            short[] buffer = new short[samples * CHANNELS];
            Random rand = new Random();

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float envelope = (float)Math.Exp(-t * 25);
                float noise = ((float)rand.NextDouble() * 2 - 1) * envelope * short.MaxValue * 0.5f;
                int clamped = (int)noise;
                if (clamped > short.MaxValue) clamped = short.MaxValue;
                if (clamped < short.MinValue) clamped = short.MinValue;
                buffer[i * CHANNELS] = (short)clamped;
                buffer[i * CHANNELS + 1] = (short)clamped;
            }

            PlaySample(buffer);
        }

        public static void PlayClap()
        {
            if (!isInitialized) return;
            int samples = (int)(SAMPLE_RATE * 0.2f);
            short[] buffer = new short[samples * CHANNELS];
            Random rand = new Random();

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float envelope = (float)Math.Exp(-t * 15);
                if (t < 0.01f) envelope = 0;
                float noise = ((float)rand.NextDouble() * 2 - 1) * envelope * short.MaxValue * 0.6f;
                int clamped = (int)noise;
                if (clamped > short.MaxValue) clamped = short.MaxValue;
                if (clamped < short.MinValue) clamped = short.MinValue;
                buffer[i * CHANNELS] = (short)clamped;
                buffer[i * CHANNELS + 1] = (short)clamped;
            }

            PlaySample(buffer);
        }

        public static void PlayCrash()
        {
            if (!isInitialized) return;
            int samples = (int)(SAMPLE_RATE * 0.5f);
            short[] buffer = new short[samples * CHANNELS];
            Random rand = new Random();

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float envelope = (float)Math.Exp(-t * 8);
                float noise = ((float)rand.NextDouble() * 2 - 1) * envelope * short.MaxValue * 0.5f;
                int clamped = (int)noise;
                if (clamped > short.MaxValue) clamped = short.MaxValue;
                if (clamped < short.MinValue) clamped = short.MinValue;
                buffer[i * CHANNELS] = (short)clamped;
                buffer[i * CHANNELS + 1] = (short)clamped;
            }

            PlaySample(buffer);
        }

        public static void PlayLaser()
        {
            if (!isInitialized) return;
            int samples = (int)(SAMPLE_RATE * 0.3f);
            short[] buffer = new short[samples * CHANNELS];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float freq = 800 * (float)Math.Exp(-t * 10);
                float envelope = (float)Math.Exp(-t * 15);
                float sample = (float)Math.Sin(2 * Math.PI * freq * t) * envelope * short.MaxValue * 0.5f;
                int clamped = (int)sample;
                if (clamped > short.MaxValue) clamped = short.MaxValue;
                if (clamped < short.MinValue) clamped = short.MinValue;
                buffer[i * CHANNELS] = (short)clamped;
                buffer[i * CHANNELS + 1] = (short)clamped;
            }

            PlaySample(buffer);
        }
        public static void PlayTone(int frequency, float duration = 0.1f)
        {
            if (!isInitialized) return;
            int samples = (int)(SAMPLE_RATE * duration);
            short[] buffer = new short[samples * CHANNELS];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float sample = (float)Math.Sin(2 * Math.PI * frequency * t) *short.MaxValue* 0.3f;
                short s = (short)Math.Min(32767, Math.Max(short.MinValue, sample));
                buffer[i * CHANNELS] = s;
                buffer[i * CHANNELS + 1] = s;
            }

            PlaySample(buffer);
        }

        public static void PlayChord(int[] frequencies, float duration = 0.2f)
        {
            if (!isInitialized || frequencies.Length == 0) return;

            int samples = (int)(SAMPLE_RATE * duration);
            short[] buffer = new short[samples * CHANNELS];

            foreach (int freq in frequencies)
            {
                for (int i = 0; i < samples; i++)
                {
                    float t = (float)i / SAMPLE_RATE;
                    float sample = (float)Math.Sin(2 * Math.PI * freq * t) *short.MaxValue* 0.3f / frequencies.Length;
                    buffer[i * CHANNELS] = (short)Math.Min(32767, buffer[i * CHANNELS] + sample);
                    buffer[i * CHANNELS + 1] = (short)Math.Min(32767, buffer[i * CHANNELS + 1] + sample);
                }
            }

            PlaySample(buffer);
        }

        public static void SetVolume(float volume)
        {
            if (!isInitialized || hWaveOut == IntPtr.Zero) return;
            volume = Math.Max(0, Math.Min(1, volume));
            uint vol = (uint)(volume * 65535);
            uint stereoVol = (vol << 16) | vol;
            waveOutSetVolume(hWaveOut, stereoVol);
        }

        public static void Cleanup()
        {
            if (hWaveOut != IntPtr.Zero)
            {
                waveOutReset(hWaveOut);

                for (int i = 0; i < NUM_BUFFERS; i++)
                {
                    if (bufferInUse[i])
                    {
                        try
                        {
                            WAVEHDR wh = Marshal.PtrToStructure<WAVEHDR>(bufferWhPtrs[i]);
                            waveOutUnprepareHeader(hWaveOut, bufferWhPtrs[i], Marshal.SizeOf(wh));
                            Marshal.FreeHGlobal(wh.lpData);
                            Marshal.FreeHGlobal(bufferWhPtrs[i]);
                        }
                        catch { }
                    }
                }

                waveOutClose(hWaveOut);
                hWaveOut = IntPtr.Zero;
            }
            voices.Clear();
            isInitialized = false;
        }
    }
}