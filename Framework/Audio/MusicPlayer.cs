using System;
using System.Collections.Generic;

namespace InfiniTD_2.Framewok.Audio
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

            public Note(int freq, float start, float dur, string inst = "piano", float release = 0.1f, float velocity = 1.0f)
            {
                Frequency = freq;
                StartTime = start;
                Duration = dur;
                Release = release;
                Velocity = velocity;
                Instrument = inst;
            }
        }

        public class Track
        {
            public List<Note> Notes = new List<Note>();

            public void AddNote(int frequency, float startTime, float duration, string instrument = "piano", float release = 0.1f, float velocity = 1.0f)
            {
                Notes.Add(new Note(frequency, startTime, duration, instrument, release, velocity));
            }

            public void AddKick(float startTime)
            {
                Notes.Add(new Note(0, startTime, 0, "kick"));
            }

            public void AddKick808(float startTime)
            {
                Notes.Add(new Note(0, startTime, 0, "kick808"));
            }

            public void AddSnare(float startTime)
            {
                Notes.Add(new Note(0, startTime, 0, "snare"));
            }

            public void AddSnare808(float startTime)
            {
                Notes.Add(new Note(0, startTime, 0, "snare808"));
            }

            public void AddHiHat(float startTime)
            {
                Notes.Add(new Note(0, startTime, 0, "hihat"));
            }

            public void AddHiHatOpen(float startTime)
            {
                Notes.Add(new Note(0, startTime, 0, "hihatOpen"));
            }

            public void AddClap(float startTime)
            {
                Notes.Add(new Note(0, startTime, 0, "clap"));
            }

            public void AddCrash(float startTime)
            {
                Notes.Add(new Note(0, startTime, 0, "crash"));
            }

            public void AddBass(int frequency, float startTime, float duration = 0.3f, float release = 0.1f, float velocity = 1.0f)
            {
                Notes.Add(new Note(frequency, startTime, duration, "bass", release, velocity));
            }

            public void AddLead(int frequency, float startTime, float duration = 0.4f, float release = 0.15f, float velocity = 1.0f)
            {
                Notes.Add(new Note(frequency, startTime, duration, "lead", release, velocity));
            }

            public void AddPad(int frequency, float startTime, float duration = 1.0f, float release = 0.3f, float velocity = 1.0f)
            {
                Notes.Add(new Note(frequency, startTime, duration, "pad", release, velocity));
            }

            public void AddLaser(float startTime)
            {
                Notes.Add(new Note(0, startTime, 0, "laser"));
            }
        }

        private static List<Track> tracks = new List<Track>();
        private static float currentTempo = 120f;
        private static float trackTime = 0f;
        private static float lastTrackTime = 0f;
        private static bool isPlaying = false;
        private static bool isLooping = false;
        private static float globalVolume = 1.0f;
        private static float maxTrackTime = 0f;

        public static void Clear()
        {
            tracks.Clear();
            trackTime = 0f;
            lastTrackTime = 0f;
            maxTrackTime = 0f;
        }

        public static Track CreateTrack()
        {
            Track track = new Track();
            tracks.Add(track);
            return track;
        }

        public static void Play(float tempo, bool loop = false)
        {
            currentTempo = tempo;
            trackTime = 0f;
            lastTrackTime = 0f;
            isPlaying = true;
            isLooping = loop;

            // Вычисляем максимальную длину трека
            maxTrackTime = 0f;
            foreach (Track track in tracks)
            {
                foreach (Note note in track.Notes)
                {
                    float endTime = note.StartTime + note.Duration;
                    if (endTime > maxTrackTime) maxTrackTime = endTime;
                }
            }

            Console.WriteLine($"Playing {tracks.Count} tracks at {tempo} BPM (loop={loop}, length={maxTrackTime:F2}s)");
        }

        public static void Stop()
        {
            isPlaying = false;
        }

        public static void SetGlobalVolume(float volume)
        {
            globalVolume = Math.Max(0f, Math.Min(1f, volume));
            AudioSynth.SetVolume(globalVolume);
        }

        public static void SetLooping(bool loop)
        {
            isLooping = loop;
        }

        public static void Update(float deltaTime)
        {
            if (!isPlaying || tracks.Count == 0) return;

            float tempoMultiplier = currentTempo / 120f;
            trackTime += deltaTime * tempoMultiplier;

            // Проверка на конец трека
            if (trackTime > maxTrackTime + 0.1f)
            {
                if (isLooping)
                {
                    trackTime = 0f;
                    lastTrackTime = 0f;
                    Console.WriteLine("Looping track...");
                }
                else
                {
                    isPlaying = false;
                    Console.WriteLine("Track finished");
                    return;
                }
            }

            foreach (Track track in tracks)
            {
                foreach (Note note in track.Notes)
                {
                    // Проверяем ноту с учётом зацикливания
                    float noteStart = note.StartTime;
                    float trackLength = maxTrackTime;

                    // Если нота должна сыграть в этом кадре
                    if (noteStart <= trackTime && noteStart > lastTrackTime)
                    {
                        float noteVelocity = note.Velocity * globalVolume;

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
                            case "pad":
                                AudioSynth.PlayPad(note.Frequency, note.Duration, note.Release, noteVelocity);
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

            lastTrackTime = trackTime;
        }

        public static void LoadMainMusic()
        {
            Clear();
            Track piano = CreateTrack();
            Track lead  = CreateTrack();
            Track drums = CreateTrack();

            for (int i = 0; i < 32; i++)
            {
                drums.AddKick(0.25f + i);
                drums.AddHiHat(0.5f + i);
                drums.AddSnare(0.75f + i);
                drums.AddHiHat(1.0f + i);

                piano.AddNote(161, 0.01f + i, 1f, "pad", 1f, 0.2f);
                piano.AddNote(211, 0.25f + i, 1f, "pad", 1f, 0.2f);
                piano.AddNote(181, 0.5f + i, 1f, "pad", 1f, 0.2f);
                piano.AddNote(221, 0.75f + i, 1f, "pad", 1f, 0.2f);
            }
            for (int i = 0; i < 20; i++)
            {
                piano.AddNote(121, 0.01f + i * 2, 0.4f, "piano", 0.3f, 0.3f);
                piano.AddNote(111, 0.25f + i * 2, 0.4f, "piano", 0.3f, 0.3f);
                piano.AddNote(221, 0.50f + i * 2, 0.4f, "piano", 0.3f, 0.3f);
                piano.AddNote(211, 0.75f + i * 2, 0.4f, "piano", 0.3f, 0.3f);
                piano.AddNote(321, 1.00f + i * 2, 0.4f, "piano", 0.3f, 0.3f);
                piano.AddNote(311, 1.25f + i * 2, 0.4f, "piano", 0.3f, 0.3f);
                piano.AddNote(421, 1.50f + i * 2, 0.4f, "piano", 0.3f, 0.3f);
                piano.AddNote(411, 1.75f + i * 2, 0.4f, "piano", 0.3f, 0.3f);
            }

            lead.AddNote(61, 0.01f, 8f, "lead", 8f);  
            lead.AddNote(51, 8f, 8f, "lead", 8f);
            lead.AddNote(81, 16f, 8f, "lead", 8f); 
            lead.AddNote(121, 24f, 8f, "lead", 8f);  
        }

        public static void LoadShootSound()
        {
            Track laser = CreateTrack();
            laser.AddLaser(0f);
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