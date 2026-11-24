using System;
using UnityEngine;

namespace LWalida
{
    public static class SettingsService
    {
        private const string Prefix = "lwalida.settings.";
        private static bool _loaded;

        public static float Master { get; private set; } = 0.9f;
        public static float Music { get; private set; } = 0.7f;
        public static float Ambience { get; private set; } = 0.8f;
        public static float Dialogue { get; private set; } = 1f;
        public static float Sfx { get; private set; } = 0.8f;
        public static bool Subtitles { get; private set; } = true;
        public static bool Fullscreen { get; private set; } = true;
        public static bool ReducedFlashes { get; private set; }
        public static bool ReducedMotion { get; private set; }

        public static event Action Changed;

        public static void Load()
        {
            if (_loaded) return;
            _loaded = true;
            Master = PlayerPrefs.GetFloat(Prefix + "master", Master);
            Music = PlayerPrefs.GetFloat(Prefix + "music", Music);
            Ambience = PlayerPrefs.GetFloat(Prefix + "ambience", Ambience);
            Dialogue = PlayerPrefs.GetFloat(Prefix + "dialogue", Dialogue);
            Sfx = PlayerPrefs.GetFloat(Prefix + "sfx", Sfx);
            Subtitles = PlayerPrefs.GetInt(Prefix + "subtitles", Subtitles ? 1 : 0) == 1;
            Fullscreen = PlayerPrefs.GetInt(Prefix + "fullscreen", Fullscreen ? 1 : 0) == 1;
            ReducedFlashes = PlayerPrefs.GetInt(Prefix + "reducedFlashes", 0) == 1;
            ReducedMotion = PlayerPrefs.GetInt(Prefix + "reducedMotion", 0) == 1;
        }

        public static void SetMaster(float value) { Master = Mathf.Clamp01(value); Save(); }
        public static void SetMusic(float value) { Music = Mathf.Clamp01(value); Save(); }
        public static void SetAmbience(float value) { Ambience = Mathf.Clamp01(value); Save(); }
        public static void SetDialogue(float value) { Dialogue = Mathf.Clamp01(value); Save(); }
        public static void SetSfx(float value) { Sfx = Mathf.Clamp01(value); Save(); }
        public static void SetSubtitles(bool value) { Subtitles = value; Save(); }
        public static void SetFullscreen(bool value) { Fullscreen = value; Save(); }
        public static void SetReducedFlashes(bool value) { ReducedFlashes = value; Save(); }
        public static void SetReducedMotion(bool value) { ReducedMotion = value; Save(); }

        public static void ApplyScreenSettings()
        {
            if (Application.isEditor) return;
            try
            {
                if (Screen.fullScreen != Fullscreen) Screen.fullScreen = Fullscreen;
            }
            catch (Exception)
            {
                // Some platforms do not allow fullscreen changes at runtime; ignore safely.
            }
        }

        public static void Save()
        {
            PlayerPrefs.SetFloat(Prefix + "master", Master);
            PlayerPrefs.SetFloat(Prefix + "music", Music);
            PlayerPrefs.SetFloat(Prefix + "ambience", Ambience);
            PlayerPrefs.SetFloat(Prefix + "dialogue", Dialogue);
            PlayerPrefs.SetFloat(Prefix + "sfx", Sfx);
            PlayerPrefs.SetInt(Prefix + "subtitles", Subtitles ? 1 : 0);
            PlayerPrefs.SetInt(Prefix + "fullscreen", Fullscreen ? 1 : 0);
            PlayerPrefs.SetInt(Prefix + "reducedFlashes", ReducedFlashes ? 1 : 0);
            PlayerPrefs.SetInt(Prefix + "reducedMotion", ReducedMotion ? 1 : 0);
            PlayerPrefs.Save();
            ApplyScreenSettings();
            Changed?.Invoke();
        }

        public static SaveSettingsData Snapshot()
        {
            return new SaveSettingsData
            {
                master = Master,
                music = Music,
                ambience = Ambience,
                dialogue = Dialogue,
                sfx = Sfx,
                subtitles = Subtitles,
                fullscreen = Fullscreen,
                reducedFlashes = ReducedFlashes,
                reducedMotion = ReducedMotion
            };
        }

        public static void Apply(SaveSettingsData data)
        {
            if (data == null) return;
            Master = Mathf.Clamp01(data.master);
            Music = Mathf.Clamp01(data.music);
            Ambience = Mathf.Clamp01(data.ambience);
            Dialogue = Mathf.Clamp01(data.dialogue);
            Sfx = Mathf.Clamp01(data.sfx);
            Subtitles = data.subtitles;
            Fullscreen = data.fullscreen;
            ReducedFlashes = data.reducedFlashes;
            ReducedMotion = data.reducedMotion;
            Save();
        }

        public static void ResetToDefaults()
        {
            Master = 0.9f;
            Music = 0.7f;
            Ambience = 0.8f;
            Dialogue = 1f;
            Sfx = 0.8f;
            Subtitles = true;
            Fullscreen = true;
            ReducedFlashes = false;
            ReducedMotion = false;
            Save();
        }
    }
}
