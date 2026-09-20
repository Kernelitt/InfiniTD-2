namespace InfiniTD_2.Framework.Audio
{
    using System;
    using System.Collections.Generic;

    public static class MusicPlayer
    {
        public struct Note
        {
            public int Frequency;
            public float StartTime;
            public float Duration;
            public float Release;
            public float Velocity;
            public string Instrument;
            public string Channel;

            public Note(int freq, float start, float dur, string inst = "piano", float release = 0.1f, float velocity = 1.0f, string channel = "default")
            {
                Frequency = freq;
                StartTime = start;
                Duration = dur;
                Release = release;
                Velocity = velocity;
                Instrument = inst;
                Channel = channel;
            }
        }

        public class Track
        {
            public List<Note> Notes = new List<Note>();
            public string Channel = "default";

            public Track(string channel = "default")
            {
                Channel = channel;
            }

            public void AddNote(int frequency, float startTime, float duration, string instrument = "piano", float release = 0.1f, float velocity = 1.0f)
            {
                Notes.Add(new Note(frequency, startTime, duration, instrument, release, velocity, Channel));
            }

            public void AddKick(float startTime, float velocity = 1.0f)
            {
                Notes.Add(new Note(0, startTime, 0, "kick", 0, velocity, Channel));
            }

            public void AddKick808(float startTime, float velocity = 1.0f)
            {
                Notes.Add(new Note(0, startTime, 0, "kick808", 0, velocity, Channel));
            }

            public void AddSnare(float startTime, float velocity = 1.0f)
            {
                Notes.Add(new Note(0, startTime, 0, "snare", 0, velocity, Channel));
            }

            public void AddSnare808(float startTime, float velocity = 1.0f)
            {
                Notes.Add(new Note(0, startTime, 0, "snare808", 0, velocity, Channel));
            }

            public void AddHiHat(float startTime, float velocity = 1.0f)
            {
                Notes.Add(new Note(0, startTime, 0, "hihat", 0, velocity, Channel));
            }

            public void AddHiHatOpen(float startTime, float velocity = 1.0f)
            {
                Notes.Add(new Note(0, startTime, 0, "hihatOpen", 0, velocity, Channel));
            }

            public void AddClap(float startTime, float velocity = 1.0f)
            {
                Notes.Add(new Note(0, startTime, 0, "clap", 0, velocity, Channel));
            }

            public void AddCrash(float startTime, float velocity = 1.0f)
            {
                Notes.Add(new Note(0, startTime, 0, "crash", 0, velocity, Channel));
            }

            public void AddBass(int frequency, float startTime, float duration = 0.3f, float release = 0.1f, float velocity = 1.0f)
            {
                Notes.Add(new Note(frequency, startTime, duration, "bass", release, velocity, Channel));
            }

            public void AddLead(int frequency, float startTime, float duration = 0.4f, float release = 0.15f, float velocity = 1.0f)
            {
                Notes.Add(new Note(frequency, startTime, duration, "lead", release, velocity, Channel));
            }

            public void AddPad(int frequency, float startTime, float duration = 1.0f, float release = 0.3f, float velocity = 1.0f)
            {
                Notes.Add(new Note(frequency, startTime, duration, "pad", release, velocity, Channel));
            }

            public void AddLaser(float startTime, float velocity = 1.0f)
            {
                Notes.Add(new Note(0, startTime, 0, "laser", 0, velocity, Channel));
            }
        }

        private static readonly Dictionary<string, List<Track>> channels = new Dictionary<string, List<Track>>();
        private static readonly Dictionary<string, float> channelTempos = new Dictionary<string, float>();
        private static readonly Dictionary<string, float> channelTimes = new Dictionary<string, float>();
        private static readonly Dictionary<string, float> channelLastTimes = new Dictionary<string, float>();
        private static readonly Dictionary<string, bool> channelPlaying = new Dictionary<string, bool>();
        private static readonly Dictionary<string, bool> channelLooping = new Dictionary<string, bool>();
        private static readonly Dictionary<string, float> channelMaxTimes = new Dictionary<string, float>();
        private static readonly Dictionary<string, float> channelVolumes = new Dictionary<string, float>();

        private static readonly Random random = new Random();
        public static void Init()
        {
            channels.Clear();
            channelTempos.Clear();
            channelTimes.Clear();
            channelLastTimes.Clear();
            channelPlaying.Clear();
            channelLooping.Clear();
            channelMaxTimes.Clear();
            channelVolumes.Clear();

            // Создаём канал по умолчанию
            CreateChannel("default");
        }

        public static void CreateChannel(string channelName)
        {
            if (!channels.ContainsKey(channelName))
            {
                channels[channelName] = new List<Track>();
                channelTempos[channelName] = 120f;
                channelTimes[channelName] = 0f;
                channelLastTimes[channelName] = 0f;
                channelPlaying[channelName] = false;
                channelLooping[channelName] = false;
                channelMaxTimes[channelName] = 0f;
                channelVolumes[channelName] = 1.0f;
            }
        }

        public static void ClearChannel(string channelName)
        {
            if (channels.ContainsKey(channelName))
            {
                channels[channelName].Clear();
                channelTimes[channelName] = 0f;
                channelLastTimes[channelName] = 0f;
                channelMaxTimes[channelName] = 0f;
            }
        }

        public static void ClearAll()
        {
            channels.Clear();
            channelTempos.Clear();
            channelTimes.Clear();
            channelLastTimes.Clear();
            channelPlaying.Clear();
            channelLooping.Clear();
            channelMaxTimes.Clear();
            channelVolumes.Clear();
            CreateChannel("default");
        }

        public static Track CreateTrack(string channelName = "default")
        {
            if (!channels.ContainsKey(channelName))
            {
                CreateChannel(channelName);
            }

            Track track = new Track(channelName);
            channels[channelName].Add(track);
            return track;
        }

        public static void Play(string channelName, float tempo, bool loop = false)
        {
            if (!channels.ContainsKey(channelName))
            {
                CreateChannel(channelName);
            }

            channelTempos[channelName] = tempo;
            channelTimes[channelName] = 0f;
            channelLastTimes[channelName] = 0f;
            channelPlaying[channelName] = true;
            channelLooping[channelName] = loop;

            // Вычисляем максимальную длину трека
            float maxTime = 0f;
            foreach (Track track in channels[channelName])
            {
                foreach (Note note in track.Notes)
                {
                    float endTime = note.StartTime + note.Duration;
                    if (endTime > maxTime) maxTime = endTime;
                }
            }
            channelMaxTimes[channelName] = maxTime;

            Console.WriteLine($"Channel '{channelName}': Playing at {tempo} BPM (loop={loop}, length={maxTime:F2}s)");
        }

        public static void Stop(string channelName)
        {
            if (channelPlaying.ContainsKey(channelName))
            {
                channelPlaying[channelName] = false;
            }
        }

        public static void StopAll()
        {
            foreach (var kvp in channelPlaying)
            {
                channelPlaying[kvp.Key] = false;
            }
        }

        public static void SetChannelVolume(string channelName, float volume)
        {
            if (channelVolumes.ContainsKey(channelName))
            {
                channelVolumes[channelName] = Math.Max(0f, Math.Min(1f, volume));
            }
        }

        public static void SetGlobalVolume(float volume)
        {
            AudioSynth.SetVolume(volume);
        }

        public static void SetLooping(string channelName, bool loop)
        {
            if (channelLooping.ContainsKey(channelName))
            {
                channelLooping[channelName] = loop;
            }
        }

        public static void Update(float deltaTime)
        {
            foreach (string channelName in new List<string>(channels.Keys))
            {
                if (!channelPlaying[channelName] || channels[channelName].Count == 0) continue;

                float tempoMultiplier = channelTempos[channelName] / 120f;
                channelTimes[channelName] += deltaTime * tempoMultiplier;

                // Проверка на конец трека
                if (channelTimes[channelName] > channelMaxTimes[channelName] + 0.1f)
                {
                    if (channelLooping[channelName])
                    {
                        channelTimes[channelName] = 0f;
                        channelLastTimes[channelName] = 0f;
                    }
                    else
                    {
                        channelPlaying[channelName] = false;
                        continue;
                    }
                }

                float channelVolume = channelVolumes[channelName];

                foreach (Track track in channels[channelName])
                {
                    foreach (Note note in track.Notes)
                    {
                        float noteStart = note.StartTime;

                        if (noteStart <= channelTimes[channelName] && noteStart > channelLastTimes[channelName])
                        {
                            float noteVelocity = note.Velocity * channelVolume;

                            switch (note.Instrument)
                            {
                                case "kick":
                                    AudioSynth.PlayKick();
                                    break;
                                case "kick808":
                                    AudioSynth.PlayKick808();
                                    break;
                                case "snare":
                                    AudioSynth.PlaySnare();
                                    break;
                                case "snare808":
                                    AudioSynth.PlaySnare808();
                                    break;
                                case "hihat":
                                    AudioSynth.PlayHiHat();
                                    break;
                                case "hihatOpen":
                                    AudioSynth.PlayHiHatOpen();
                                    break;
                                case "clap":
                                    AudioSynth.PlayClap();
                                    break;
                                case "crash":
                                    AudioSynth.PlayCrash();
                                    break;
                                case "bass":
                                    AudioSynth.PlayBass(note.Frequency, note.Duration, note.Release, noteVelocity);
                                    break;
                                case "lead":
                                    AudioSynth.PlayLead(note.Frequency, note.Duration, note.Release, noteVelocity);
                                    break;
                                case "laser":
                                    AudioSynth.PlayLaser();
                                    break;
                                case "piano":
                                default:
                                    AudioSynth.PlayPiano(note.Frequency, note.Duration, note.Release, noteVelocity);
                                    break;
                            }
                        }
                    }
                }

                channelLastTimes[channelName] = channelTimes[channelName];
            }
        }

        public static bool IsPlaying(string channelName)
        {
            return channelPlaying.ContainsKey(channelName) && channelPlaying[channelName];
        }

        public static float GetChannelTime(string channelName)
        {
            return channelTimes.ContainsKey(channelName) ? channelTimes[channelName] : 0f;
        }

        public static void PlayMainMusic()
        {

            Track piano = CreateTrack("bgm");
            Track lead  = CreateTrack("bgm");
            Track drums = CreateTrack("bgm");

            for (int i = 0; i < 32; i++)
            {
                drums.AddKick(0.25f + i);

                drums.AddSnare808(0.75f + i);

                
            }
            for (int i = 0; i < 36; i++)
            {
                piano.AddNote(121, 0.01f + i * 2, 0.4f, "piano", 0.3f, 0.3f);
                piano.AddNote(111, 0.25f + i * 2, 0.4f, "piano", 0.3f, 0.3f);
                piano.AddNote(221, 0.50f + i * 2, 0.4f, "piano", 0.3f, 0.3f);
                piano.AddNote(211, 0.75f + i * 2, 0.4f, "piano", 0.3f, 0.3f);
                piano.AddNote(321, 1.00f + i * 2, 0.4f, "piano", 0.3f, 0.3f);
                piano.AddNote(311, 1.25f + i * 2, 0.4f, "piano", 0.3f, 0.3f);
                piano.AddNote(421, 1.50f + i * 2, 0.4f, "piano", 0.3f, 0.3f);
                piano.AddNote(391, 1.75f + i * 2, 0.4f, "piano", 0.3f, 0.3f);
            }
            for (int i = 0; i < 2; i++)
            {
                drums.AddKick(0.25f + i);

                drums.AddSnare808(0.75f + i);


                piano.AddNote(120, 0.001f  + i * 32, 8f, "lead", 4.2f, 0.2f);
                piano.AddNote(110, 8.000f  + i * 32, 8f, "lead", 4.2f, 0.2f);
                piano.AddNote(100, 16.000f + i * 32, 8f, "lead", 4.2f, 0.2f);
                piano.AddNote(140, 24.00f  + i * 32, 8f, "lead", 4.2f, 0.2f);
            }
            Play("bgm", 90f, true);
        }

        public static void PlayShootSound()
        {
            int sfx_num = random.Next();
            Track sound = CreateTrack("sfx" + sfx_num);
            sound.AddLaser(0.001f, 0.3f);
            Play("sfx"+sfx_num, 90f, false);
            SetChannelVolume("sfx" + sfx_num, 0.5f);
        }
        public static void PlayExplodeSound()
        {
            int sfx_num = random.Next();
            Track sound = CreateTrack("sfx" + sfx_num);
            sound.AddSnare(0.001f, 0.3f);
            sound.AddCrash(0.001f, 0.3f);
            Play("sfx" + sfx_num, 90f, false);
            SetChannelVolume("sfx" + sfx_num, 0.5f);
        }
    }

    public static class Time
    {
        public static float DeltaTime { get; private set; }
        private static float lastTime = 0f;

        public static void Update()
        {
            float currentTime = (float)MainApp.DeltaTime;
            DeltaTime = currentTime - lastTime;
            lastTime = currentTime;
        }
    }
}