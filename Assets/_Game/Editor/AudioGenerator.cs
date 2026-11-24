using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LWalida.EditorTools
{
    /// <summary>
    /// Generates original, procedural placeholder audio (music, ambience and SFX)
    /// as 16-bit mono WAV files. No external samples or voice recordings are used.
    /// </summary>
    public static class AudioGenerator
    {
        private const int SampleRate = 22050;

        public static readonly string[] MusicFiles = { "menuMusic", "explorationMusic", "endingMusic" };
        public static readonly string[] AmbienceFiles =
        {
            "ambienceCafe", "ambienceOudaya", "ambienceSouk", "ambienceHassan", "ambienceHome"
        };
        public static readonly string[] SfxFiles =
        {
            "sfxShutter", "sfxKnock", "sfxFootstep", "sfxUiClick", "sfxMemoryUnlock", "sfxRecordComplete"
        };

        public static void GenerateMissing()
        {
            PlaceholderArtGenerator.EnsureFolder(LWalidaPaths.PlaceholderAudio);
            GenerateMenuMusic();
            GenerateExplorationMusic();
            GenerateEndingMusic();
            GenerateAmbience("ambienceCafe", AmbienceKind.Cafe);
            GenerateAmbience("ambienceOudaya", AmbienceKind.Oudaya);
            GenerateAmbience("ambienceSouk", AmbienceKind.Souk);
            GenerateAmbience("ambienceHassan", AmbienceKind.Hassan);
            GenerateAmbience("ambienceHome", AmbienceKind.Home);
            GenerateShutter();
            GenerateKnock();
            GenerateFootstep();
            GenerateUiClick();
            GenerateMemoryUnlock();
            GenerateRecordComplete();
            AssetDatabase.Refresh();
        }

        public static string PathFor(string name)
        {
            return LWalidaPaths.PlaceholderAudio + "/" + name + ".wav";
        }

        private static void GenerateMenuMusic()
        {
            WriteIfMissing("menuMusic", 24f, (buffer, rng) =>
            {
                float root = 146.83f;
                var notes = new[] { 0, 3, 5, 7, 10, 12, 15, 12, 10, 7, 5, 3 };
                for (int i = 0; i < 48; i++)
                {
                    float start = i * 0.5f;
                    float freq = root * Mathf.Pow(2f, notes[i % notes.Length] / 12f);
                    float amp = 0.12f + (i % 4 == 0 ? 0.05f : 0f);
                    Pluck(buffer, start, 1.4f, freq, amp, 2);
                    if (i % 6 == 0) Pluck(buffer, start, 3.2f, freq * 0.5f, 0.05f, 1);
                }
                Drone(buffer, root * 0.5f, 0.035f, 24f, 0.11f);
                Drone(buffer, root * 0.75f, 0.018f, 24f, 0.07f);
            });
        }

        private static void GenerateExplorationMusic()
        {
            WriteIfMissing("explorationMusic", 32f, (buffer, rng) =>
            {
                float root = 130.81f;
                var notes = new[] { 0, 2, 5, 7, 10, 12, 14, 12, 10, 7, 5, 2 };
                for (int i = 0; i < 64; i++)
                {
                    float start = i * 0.5f;
                    float freq = root * Mathf.Pow(2f, notes[i % notes.Length] / 12f);
                    Pluck(buffer, start, 1.8f, freq, 0.085f, 3);
                    if (i % 8 == 0)
                    {
                        Pluck(buffer, start, 3.5f, freq * 0.5f, 0.04f, 1);
                    }
                }
                Drone(buffer, root * 0.5f, 0.03f, 32f, 0.09f);
                for (int i = 0; i < 20; i++)
                {
                    float start = 1f + (float)rng.NextDouble() * 30f;
                    Pluck(buffer, start, 2.2f, root * 2f * Mathf.Pow(2f, (float)rng.NextDouble() * 0.5f), 0.02f, 1);
                }
            });
        }

        private static void GenerateEndingMusic()
        {
            WriteIfMissing("endingMusic", 24f, (buffer, rng) =>
            {
                float root = 110f;
                var notes = new[] { 0, 4, 7, 12, 11, 7, 4, 2 };
                for (int i = 0; i < 36; i++)
                {
                    float start = i * 0.66f;
                    float freq = root * Mathf.Pow(2f, notes[i % notes.Length] / 12f);
                    Pluck(buffer, start, 2.6f, freq, 0.075f, 1);
                }
                Drone(buffer, root * 0.5f, 0.035f, 24f, 0.06f);
                Drone(buffer, root * 0.75f, 0.02f, 24f, 0.04f);
            });
        }

        private static void GenerateAmbience(string name, AmbienceKind kind)
        {
            WriteIfMissing(name, 12f, (buffer, rng) =>
            {
                float lowPass = 0f;
                for (int i = 0; i < buffer.Length; i++)
                {
                    float noise = (float)rng.NextDouble() * 2f - 1f;
                    lowPass += (noise - lowPass) * 0.035f;
                    buffer[i] += lowPass * 0.12f;
                }

                switch (kind)
                {
                    case AmbienceKind.Cafe:
                        AddRandomTaps(buffer, rng, 22, 0.12f, 0.05f);
                        AddMurmur(buffer, rng, 0.035f);
                        break;
                    case AmbienceKind.Oudaya:
                        AddWind(buffer, 0.05f);
                        AddBirds(buffer, rng, 7);
                        break;
                    case AmbienceKind.Souk:
                        AddRandomTaps(buffer, rng, 48, 0.18f, 0.04f);
                        AddMurmur(buffer, rng, 0.055f);
                        for (int i = 0; i < 10; i++) AddBell(buffer, (float)rng.NextDouble() * 11f, 0.025f);
                        break;
                    case AmbienceKind.Hassan:
                        AddWind(buffer, 0.065f);
                        AddDistantCall(buffer, 2.2f, 0.03f);
                        AddDistantCall(buffer, 6.8f, 0.025f);
                        break;
                    case AmbienceKind.Home:
                        AddWind(buffer, 0.018f);
                        AddClock(buffer, 0.5f, 0.045f);
                        break;
                }
                EdgeFade(buffer, 0.25f);
            });
        }

        private static void GenerateShutter()
        {
            WriteIfMissing("sfxShutter", 0.22f, (buffer, rng) =>
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    float t = i / (float)SampleRate;
                    float click = Mathf.Exp(-t * 90f) * Mathf.Sin(t * 2f * Mathf.PI * 2200f);
                    float noise = ((float)rng.NextDouble() * 2f - 1f) * Mathf.Exp(-t * 40f);
                    buffer[i] += click * 0.5f + noise * 0.35f;
                }
                Thump(buffer, 0.06f, 0.04f, 120f, 0.35f);
            });
        }

        private static void GenerateKnock()
        {
            WriteIfMissing("sfxKnock", 0.9f, (buffer, rng) =>
            {
                Thump(buffer, 0.03f, 0.16f, 95f, 0.8f);
                Thump(buffer, 0.28f, 0.16f, 88f, 0.7f);
                Thump(buffer, 0.52f, 0.16f, 92f, 0.65f);
            });
        }

        private static void GenerateFootstep()
        {
            WriteIfMissing("sfxFootstep", 0.18f, (buffer, rng) =>
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    float t = i / (float)SampleRate;
                    float env = Mathf.Exp(-t * 34f);
                    float noise = ((float)rng.NextDouble() * 2f - 1f) * env * 0.35f;
                    buffer[i] += noise + Mathf.Sin(t * 2f * Mathf.PI * 70f) * env * 0.2f;
                }
            });
        }

        private static void GenerateUiClick()
        {
            WriteIfMissing("sfxUiClick", 0.09f, (buffer, rng) =>
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    float t = i / (float)SampleRate;
                    float env = Mathf.Exp(-t * 45f);
                    buffer[i] += Mathf.Sin(t * 2f * Mathf.PI * 660f) * env * 0.24f;
                }
            });
        }

        private static void GenerateMemoryUnlock()
        {
            WriteIfMissing("sfxMemoryUnlock", 1.6f, (buffer, rng) =>
            {
                float[] notes = { 523.25f, 659.25f, 783.99f, 1046.5f };
                for (int n = 0; n < notes.Length; n++)
                {
                    float start = n * 0.18f;
                    int from = (int)(start * SampleRate);
                    for (int i = 0; i < SampleRate; i++)
                    {
                        int index = from + i;
                        if (index >= buffer.Length) break;
                        float t = i / (float)SampleRate;
                        float env = Mathf.Exp(-t * 2.4f);
                        buffer[index] += Mathf.Sin(t * 2f * Mathf.PI * notes[n]) * env * 0.09f;
                        buffer[index] += Mathf.Sin(t * 2f * Mathf.PI * notes[n] * 2f) * env * 0.025f;
                    }
                }
            });
        }

        private static void GenerateRecordComplete()
        {
            WriteIfMissing("sfxRecordComplete", 0.8f, (buffer, rng) =>
            {
                float[] notes = { 392f, 523.25f, 659.25f };
                for (int n = 0; n < notes.Length; n++)
                {
                    float start = n * 0.13f;
                    int from = (int)(start * SampleRate);
                    for (int i = 0; i < SampleRate / 2; i++)
                    {
                        int index = from + i;
                        if (index >= buffer.Length) break;
                        float t = i / (float)SampleRate;
                        float env = Mathf.Exp(-t * 4.5f);
                        buffer[index] += Mathf.Sin(t * 2f * Mathf.PI * notes[n]) * env * 0.16f;
                    }
                }
            });
        }

        private static void Pluck(float[] buffer, float start, float duration, float frequency, float amplitude,
            int harmonics)
        {
            int from = (int)(start * SampleRate);
            int count = (int)(duration * SampleRate);
            for (int i = 0; i < count; i++)
            {
                int index = from + i;
                if (index >= buffer.Length) break;
                float t = i / (float)SampleRate;
                float env = Mathf.Exp(-t * (2.2f / Mathf.Max(0.2f, duration)));
                float value = 0f;
                for (int h = 1; h <= harmonics; h++)
                {
                    value += Mathf.Sin(t * 2f * Mathf.PI * frequency * h) / (h * h);
                }
                buffer[index] += value * env * amplitude;
            }
        }

        private static void Drone(float[] buffer, float frequency, float amplitude, float duration, float wobble)
        {
            int count = Mathf.Min(buffer.Length, (int)(duration * SampleRate));
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)SampleRate;
                float modulation = 1f + Mathf.Sin(t * 2f * Mathf.PI * 0.07f) * wobble;
                buffer[i] += Mathf.Sin(t * 2f * Mathf.PI * frequency * modulation) * amplitude;
            }
        }

        private static void Thump(float[] buffer, float start, float duration, float frequency, float amplitude)
        {
            int from = (int)(start * SampleRate);
            int count = (int)(duration * SampleRate);
            for (int i = 0; i < count; i++)
            {
                int index = from + i;
                if (index >= buffer.Length) break;
                float t = i / (float)SampleRate;
                float env = Mathf.Exp(-t * 18f);
                float freq = frequency * (1f - 0.35f * t / Mathf.Max(0.01f, duration));
                buffer[index] += Mathf.Sin(t * 2f * Mathf.PI * freq) * env * amplitude;
            }
        }

        private static void AddRandomTaps(float[] buffer, System.Random rng, int count, float volume, float decay)
        {
            for (int i = 0; i < count; i++)
            {
                float start = (float)rng.NextDouble() * (buffer.Length / (float)SampleRate - 0.2f);
                float freq = 380f + (float)rng.NextDouble() * 1400f;
                int from = (int)(start * SampleRate);
                int length = (int)(0.16f * SampleRate);
                for (int s = 0; s < length; s++)
                {
                    int index = from + s;
                    if (index >= buffer.Length) break;
                    float t = s / (float)SampleRate;
                    buffer[index] += Mathf.Sin(t * 2f * Mathf.PI * freq) * Mathf.Exp(-t / decay) * volume;
                }
            }
        }

        private static void AddMurmur(float[] buffer, System.Random rng, float volume)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                float t = i / (float)SampleRate;
                float voice = Mathf.Sin(t * 2f * Mathf.PI * 120f + Mathf.Sin(t * 0.7f) * 3f) *
                              Mathf.Sin(t * 2f * Mathf.PI * 0.23f);
                buffer[i] += voice * volume;
            }
        }

        private static void AddWind(float[] buffer, float volume)
        {
            float low = 0f;
            for (int i = 0; i < buffer.Length; i++)
            {
                float t = i / (float)SampleRate;
                float noise = Mathf.Sin(t * 2f * Mathf.PI * 0.13f + Mathf.Sin(t * 0.31f) * 2f);
                low += (noise - low) * 0.004f;
                buffer[i] += low * volume;
            }
        }

        private static void AddBirds(float[] buffer, System.Random rng, int count)
        {
            for (int i = 0; i < count; i++)
            {
                float start = (float)rng.NextDouble() * 10f;
                int from = (int)(start * SampleRate);
                int length = SampleRate / 4;
                float baseFreq = 1600f + (float)rng.NextDouble() * 900f;
                for (int s = 0; s < length; s++)
                {
                    int index = from + s;
                    if (index >= buffer.Length) break;
                    float t = s / (float)SampleRate;
                    float env = Mathf.Sin(Mathf.PI * s / length);
                    float freq = baseFreq + Mathf.Sin(t * 60f) * 240f;
                    buffer[index] += Mathf.Sin(t * 2f * Mathf.PI * freq) * env * 0.035f;
                }
            }
        }

        private static void AddBell(float[] buffer, float start, float volume)
        {
            int from = (int)(start * SampleRate);
            for (int s = 0; s < SampleRate; s++)
            {
                int index = from + s;
                if (index >= buffer.Length) break;
                float t = s / (float)SampleRate;
                float env = Mathf.Exp(-t * 4f);
                buffer[index] += Mathf.Sin(t * 2f * Mathf.PI * 880f) * env * volume;
            }
        }

        private static void AddDistantCall(float[] buffer, float start, float volume)
        {
            int from = (int)(start * SampleRate);
            int length = SampleRate * 2;
            for (int s = 0; s < length; s++)
            {
                int index = from + s;
                if (index >= buffer.Length) break;
                float t = s / (float)SampleRate;
                float env = Mathf.Sin(Mathf.PI * s / length);
                buffer[index] += Mathf.Sin(t * 2f * Mathf.PI * 220f + Mathf.Sin(t * 4f) * 2f) * env * volume;
            }
        }

        private static void AddClock(float[] buffer, float interval, float volume)
        {
            for (float start = 0.2f; start < buffer.Length / (float)SampleRate - 0.1f; start += interval)
            {
                int from = (int)(start * SampleRate);
                for (int s = 0; s < SampleRate / 30; s++)
                {
                    int index = from + s;
                    if (index >= buffer.Length) break;
                    float t = s / (float)SampleRate;
                    buffer[index] += Mathf.Sin(t * 2f * Mathf.PI * 1800f) * Mathf.Exp(-t * 120f) * volume;
                }
            }
        }

        private static void EdgeFade(float[] buffer, float seconds)
        {
            int fade = Mathf.Min(buffer.Length / 2, (int)(seconds * SampleRate));
            for (int i = 0; i < fade; i++)
            {
                float gain = i / (float)fade;
                buffer[i] *= gain;
                buffer[buffer.Length - 1 - i] *= gain;
            }
        }

        private static void WriteIfMissing(string name, float seconds, Action<float[], System.Random> generate)
        {
            string path = PathFor(name);
            if (File.Exists(path)) return;
            int count = Mathf.CeilToInt(seconds * SampleRate);
            var buffer = new float[count];
            generate(buffer, new System.Random(name.GetHashCode()));
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = Mathf.Clamp(buffer[i], -0.95f, 0.95f);
            }
            WriteWav(path, buffer, SampleRate);
        }

        private static void WriteWav(string path, float[] samples, int sampleRate)
        {
            using (var stream = new FileStream(path, FileMode.Create))
            using (var writer = new BinaryWriter(stream))
            {
                int dataSize = samples.Length * 2;
                writer.Write(new[] { 'R', 'I', 'F', 'F' });
                writer.Write(36 + dataSize);
                writer.Write(new[] { 'W', 'A', 'V', 'E' });
                writer.Write(new[] { 'f', 'm', 't', ' ' });
                writer.Write(16);
                writer.Write((short)1);
                writer.Write((short)1);
                writer.Write(sampleRate);
                writer.Write(sampleRate * 2);
                writer.Write((short)2);
                writer.Write((short)16);
                writer.Write(new[] { 'd', 'a', 't', 'a' });
                writer.Write(dataSize);
                for (int i = 0; i < samples.Length; i++)
                {
                    writer.Write((short)(samples[i] * 32767f));
                }
            }
        }

        private enum AmbienceKind
        {
            Cafe,
            Oudaya,
            Souk,
            Hassan,
            Home
        }
    }
}
