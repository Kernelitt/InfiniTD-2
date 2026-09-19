namespace InfiniTD_2.Framework.Audio
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
        const int BITS_PER_SAMPLE = 32; // Теперь 32 бита
        const int BUFFER_SAMPLES = 1024;
        const int NUM_BUFFERS = 8;

        private static readonly List<Voice> voices = new List<Voice>();

        struct Voice
        {
            public float[] Samples; // Сэмплы теперь типа float
            public int Position;
            public bool IsActive;
        }

        private static readonly IntPtr[] bufferDataPtrs = new IntPtr[NUM_BUFFERS];
        private static readonly IntPtr[] bufferWhPtrs = new IntPtr[NUM_BUFFERS];
        private static readonly bool[] bufferInUse = new bool[NUM_BUFFERS];

        private static readonly float[] mixBuffer = new float[BUFFER_SAMPLES * CHANNELS];
        private static readonly float[] outputBuffer = new float[BUFFER_SAMPLES * CHANNELS]; // Выходной буфер тоже float

        public static void Init()
        {
            if (isInitialized) return;

            WAVEFORMATEX wfx = new WAVEFORMATEX
            {
                wFormatTag = 3, // WAVE_FORMAT_IEEE_FLOAT (для 32-bit float звука)
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

            for (int i = 0; i < NUM_BUFFERS; i++)
            {
                bufferInUse[i] = false;
            }

            isInitialized = true;
            Console.WriteLine($"AudioSynth initialized with {NUM_BUFFERS} buffers (32-bit Float)");
        }

        public static void PlaySample(float[] sample) // Принимает float[] вместо short[]
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

            int freeBuffer = -1;
            for (int i = 0; i < NUM_BUFFERS; i++)
            {
                if (!bufferInUse[i])
                {
                    freeBuffer = i;
                    break;
                }
            }

            if (freeBuffer == -1) return;

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
                        mixBuffer[j] += voice.Samples[voice.Position + j];
                    }
                }

                voice.Position += samplesToMix * CHANNELS;
                if (voice.Position >= voice.Samples.Length)
                {
                    voice.IsActive = false;
                }
                voices[i] = voice;
            }

            voices.RemoveAll(v => !v.IsActive);

            // Мягкое ограничение (Soft Clipping) с помощью кубической функции tanh-аппроксимации
            // Это убирает неприятный цифровой треск при наложении множества звуков
            for (int i = 0; i < BUFFER_SAMPLES * CHANNELS; i++)
            {
                float x = mixBuffer[i];
                // Кубический софт-клиппер
                if (x > 1.0f) x = 1.0f;
                else if (x < -1.0f) x = -1.0f;
                else x = x - (x * x * x) / 3.0f;

                outputBuffer[i] = x * 1.2f; // Немного компенсируем громкость после мягкого сжатия
            }

            SendBuffer(freeBuffer);
        }

        private static void SendBuffer(int bufferIndex)
        {
            IntPtr dataPtr = Marshal.AllocHGlobal(outputBuffer.Length * sizeof(float)); // Размер памяти теперь под float
            Marshal.Copy(outputBuffer, 0, dataPtr, outputBuffer.Length);

            WAVEHDR wh = new WAVEHDR
            {
                lpData = dataPtr,
                dwBufferLength = (uint)(outputBuffer.Length * sizeof(float)), // Теперь sizeof(float)
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

        // --- Инструменты генерируют значения напрямую в диапазоне от -1.0 до 1.0 ---

        public static void PlayPiano(int frequency, float duration = 0.3f, float release = 0.1f, float velocity = 1.0f)
        {
            if (!isInitialized) return;

            int samples = (int)(SAMPLE_RATE * duration);
            if (samples <= 0) return;

            float[] buffer = new float[samples * CHANNELS];

            float attackTime = 0.01f;
            float decayTime = 0.05f;
            float sustainLevel = 0.7f;
            float releaseTime = Math.Min(release, duration * 0.3f);

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;

                float envelope;
                if (t < attackTime) envelope = t / attackTime;
                else if (t < attackTime + decayTime) envelope = 1.0f - (1.0f - sustainLevel) * ((t - attackTime) / decayTime);
                else if (t < duration - releaseTime) envelope = sustainLevel;
                else envelope = sustainLevel * (1.0f - ((t - (duration - releaseTime)) / releaseTime));

                float sample = (float)Math.Sin(2 * Math.PI * frequency * t) * envelope * 0.5f * velocity;
                buffer[i * CHANNELS] = sample;
                buffer[i * CHANNELS + 1] = sample;
            }

            PlaySample(buffer);
        }

        public static void PlayBass(int frequency, float duration = 0.3f, float release = 0.1f, float velocity = 1.0f)
        {
            if (!isInitialized) return;
            int samples = (int)(SAMPLE_RATE * duration);
            float[] buffer = new float[samples * CHANNELS];

            float attackTime = 0.005f;
            float decayTime = 0.02f;
            float sustainLevel = 0.8f;
            float releaseTime = release;

            // Накопитель фазы для чистой пилы без разрывов
            float phase = 0f;
            float phaseIncrement = (float)frequency / SAMPLE_RATE;

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;

                // Генерация идеальной пилы [-1.0, 1.0]
                float saw = 2.0f * phase - 1.0f;
                phase += phaseIncrement;
                if (phase >= 1.0f) phase -= 1.0f;

                float envelope;
                if (t < attackTime) envelope = t / attackTime;
                else if (t < attackTime + decayTime) envelope = 1.0f - (1.0f - sustainLevel) * ((t - attackTime) / decayTime);
                else if (t < duration - releaseTime) envelope = sustainLevel;
                else envelope = sustainLevel * (1.0f - ((t - (duration - releaseTime)) / releaseTime));

                // Снизили общую амплитуду до 0.25f, чтобы бас не забивал микс
                float sample = saw * envelope * 0.4f * velocity;
                buffer[i * CHANNELS] = sample;
                buffer[i * CHANNELS + 1] = sample;
            }

            PlaySample(buffer);
        }

        public static void PlayLead(int frequency, float duration = 0.4f, float release = 0.15f, float velocity = 1.0f)
        {
            if (!isInitialized) return;
            int samples = (int)(SAMPLE_RATE * duration);
            float[] buffer = new float[samples * CHANNELS];

            float attackTime = 0.01f;
            float decayTime = 0.03f;
            float sustainLevel = 0.6f;
            float releaseTime = release;

            float phase1 = 0f;
            float phase2 = 0f;
            float phaseInc1 = (float)frequency / SAMPLE_RATE;
            float phaseInc2 = (float)(frequency * 1.005f) / SAMPLE_RATE; // Чуть уменьшили детюн для более благородного звучания

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;

                float saw1 = 2.0f * phase1 - 1.0f;
                phase1 += phaseInc1;
                if (phase1 >= 1.0f) phase1 -= 1.0f;

                float saw2 = 2.0f * phase2 - 1.0f;
                phase2 += phaseInc2;
                if (phase2 >= 1.0f) phase2 -= 1.0f;

                float envelope;
                if (t < attackTime) envelope = t / attackTime;
                else if (t < attackTime + decayTime) envelope = 1.0f - (1.0f - sustainLevel) * ((t - attackTime) / decayTime);
                else if (t < duration - releaseTime) envelope = sustainLevel;
                else envelope = sustainLevel * (1.0f - ((t - (duration - releaseTime)) / releaseTime));

                // Снизили амплитуду до 0.2f
                float sample = (saw1 + saw2) * 0.7f * envelope * 0.4f * velocity;
                buffer[i * CHANNELS] = sample;
                buffer[i * CHANNELS + 1] = sample;
            }

            PlaySample(buffer);
        }

        public static void PlayKick()
        {
            if (!isInitialized) return;
            int samples = (int)(SAMPLE_RATE * 0.2f);
            float[] buffer = new float[samples * CHANNELS];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float freq = 150 * (float)Math.Exp(-t * 20);
                float envelope = (float)Math.Exp(-t * 15);
                // Снизили базовый пиковый уровень с 0.8 до 0.45, чтобы не клипповало при микшировании
                float sample = (float)Math.Sin(2 * Math.PI * freq * t) * envelope * 0.45f;
                buffer[i * CHANNELS] = sample;
                buffer[i * CHANNELS + 1] = sample;
            }

            PlaySample(buffer);
        }

        public static void PlayKick808()
        {
            if (!isInitialized) return;
            int samples = (int)(SAMPLE_RATE * 0.3f);
            float[] buffer = new float[samples * CHANNELS];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float freq = 100 * (float)Math.Exp(-t * 15);
                float envelope = (float)Math.Exp(-t * 10);
                // Оптимизировали громкость до 0.5f
                float sample = (float)Math.Sin(2 * Math.PI * freq * t) * envelope * 0.5f;
                buffer[i * CHANNELS] = sample;
                buffer[i * CHANNELS + 1] = sample;
            }

            PlaySample(buffer);
        }

        public static void PlaySnare()
        {
            if (!isInitialized) return;
            int samples = (int)(SAMPLE_RATE * 0.15f);
            float[] buffer = new float[samples * CHANNELS];
            Random rand = new Random();

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float envelope = (float)Math.Exp(-t * 20);
                // Сбалансировали шум и тон, снизив общую громкость снейра до 0.3f
                float noise = ((float)rand.NextDouble() * 2 - 1) * envelope * 0.3f;
                float tone = (float)Math.Sin(2 * Math.PI * 200 * t) * envelope * 0.15f;
                float sample = noise + tone;
                buffer[i * CHANNELS] = sample;
                buffer[i * CHANNELS + 1] = sample;
            }

            PlaySample(buffer);
        }

        public static void PlaySnare808()
        {
            if (!isInitialized) return;
            int samples = (int)(SAMPLE_RATE * 0.12f);
            float[] buffer = new float[samples * CHANNELS];
            Random rand = new Random();

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float envelope = (float)Math.Exp(-t * 30);
                float noise = ((float)rand.NextDouble() * 2 - 1) * envelope * 0.35f;
                float tone = (float)Math.Sin(2 * Math.PI * 250 * t) * envelope * 0.2f;
                float sample = noise + tone;
                buffer[i * CHANNELS] = sample;
                buffer[i * CHANNELS + 1] = sample;
            }

            PlaySample(buffer);
        }

        public static void PlayHiHat()
        {
            if (!isInitialized) return;
            int samples = (int)(SAMPLE_RATE * 0.05f);
            float[] buffer = new float[samples * CHANNELS];
            Random rand = new Random();

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float envelope = (float)Math.Exp(-t * 50);
                // Хэты были слишком громкими, снизили до 0.15f
                float sample = ((float)rand.NextDouble() * 2 - 1) * envelope * 0.15f;
                buffer[i * CHANNELS] = sample;
                buffer[i * CHANNELS + 1] = sample;
            }

            PlaySample(buffer);
        }
        public static void PlayHiHatOpen() { if (!isInitialized) return; int samples = (int)(SAMPLE_RATE * 0.15f); float[] buffer = new float[samples * CHANNELS]; Random rand = new Random(); for (int i = 0; i < samples; i++) { float t = (float)i / SAMPLE_RATE; float envelope = (float)Math.Exp(-t * 25); float sample = ((float)rand.NextDouble() * 2 - 1) * envelope * 0.5f; buffer[i * CHANNELS] = sample; buffer[i * CHANNELS + 1] = sample; } PlaySample(buffer); }
        public static void PlayClap() { if (!isInitialized) return; int samples = (int)(SAMPLE_RATE * 0.2f); float[] buffer = new float[samples * CHANNELS]; Random rand = new Random(); for (int i = 0; i < samples; i++) { float t = (float)i / SAMPLE_RATE; float envelope = (float)Math.Exp(-t * 15); if (t < 0.01f) envelope = 0; float sample = ((float)rand.NextDouble() * 2 - 1) * envelope * 0.6f; buffer[i * CHANNELS] = sample; buffer[i * CHANNELS + 1] = sample; } PlaySample(buffer); }
        public static void PlayCrash() { if (!isInitialized) return; int samples = (int)(SAMPLE_RATE * 0.5f); float[] buffer = new float[samples * CHANNELS]; Random rand = new Random(); for (int i = 0; i < samples; i++) { float t = (float)i / SAMPLE_RATE; float envelope = (float)Math.Exp(-t * 8); float sample = ((float)rand.NextDouble() * 2 - 1) * envelope * 0.5f; buffer[i * CHANNELS] = sample; buffer[i * CHANNELS + 1] = sample; } PlaySample(buffer); }
        public static void PlayLaser() { if (!isInitialized) return; int samples = (int)(SAMPLE_RATE * 0.3f); float[] buffer = new float[samples * CHANNELS]; for (int i = 0; i < samples; i++) { float t = (float)i / SAMPLE_RATE; float freq = 800 * (float)Math.Exp(-t * 10); float envelope = (float)Math.Exp(-t * 15); float sample = (float)Math.Sin(2 * Math.PI * freq * t) * envelope * 0.5f; buffer[i * CHANNELS] = sample; buffer[i * CHANNELS + 1] = sample; } PlaySample(buffer); }
        public static void PlayTone(int frequency, float duration = 0.1f) { if (!isInitialized) return; int samples = (int)(SAMPLE_RATE * duration); float[] buffer = new float[samples * CHANNELS]; for (int i = 0; i < samples; i++) { float t = (float)i / SAMPLE_RATE; float sample = (float)Math.Sin(2 * Math.PI * frequency * t) * 0.3f; buffer[i * CHANNELS] = sample; buffer[i * CHANNELS + 1] = sample; } PlaySample(buffer); }
        public static void PlayChord(int[] frequencies, float duration = 0.2f) { if (!isInitialized || frequencies.Length == 0) return; int samples = (int)(SAMPLE_RATE * duration); float[] buffer = new float[samples * CHANNELS]; foreach (int freq in frequencies) { for (int i = 0; i < samples; i++) { float t = (float)i / SAMPLE_RATE; float sample = (float)Math.Sin(2 * Math.PI * freq * t) * 0.3f / frequencies.Length; buffer[i * CHANNELS] += sample; buffer[i * CHANNELS + 1] += sample; } } PlaySample(buffer); }
        public static void SetVolume(float volume) { if (!isInitialized || hWaveOut == IntPtr.Zero) return; volume = Math.Max(0, Math.Min(1, volume)); uint vol = (uint)(volume * 65535); uint stereoVol = (vol << 16) | vol; waveOutSetVolume(hWaveOut, stereoVol); }
        public static void Cleanup() { if (hWaveOut != IntPtr.Zero) { waveOutReset(hWaveOut); for (int i = 0; i < NUM_BUFFERS; i++) { if (bufferInUse[i]) { try { WAVEHDR wh = Marshal.PtrToStructure<WAVEHDR>(bufferWhPtrs[i]); waveOutUnprepareHeader(hWaveOut, bufferWhPtrs[i], Marshal.SizeOf(wh)); Marshal.FreeHGlobal(wh.lpData); Marshal.FreeHGlobal(bufferWhPtrs[i]); } catch { } } } waveOutClose(hWaveOut); hWaveOut = IntPtr.Zero; } voices.Clear(); isInitialized = false; }
    }
}