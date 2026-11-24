using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LWalida
{
    public enum GameMode
    {
        Boot,
        Menu,
        Exploring,
        Dialogue,
        Camera,
        Recording,
        Journal,
        Paused,
        Cutscene,
        Transition,
        Credits
    }

    public enum MemoryType
    {
        Photograph,
        Sound
    }

    public static class MemoryIds
    {
        public const string Cafe = "cafe_01";
        public const string Door = "door_01";
        public const string Amina = "amina_01";
        public const string Souk = "souk_01";

        public static readonly string[] All = { Cafe, Door, Amina, Souk };
        public const int RequiredCount = 4;
    }

    public static class SceneIds
    {
        public const string MainMenu = "01_MainMenu";
        public const string Intro = "02_Intro";
        public const string CafeMaure = "03_CafeMaure";
        public const string Oudaya = "04_Oudaya";
        public const string Souk = "05_Souk";
        public const string Hassan = "06_Hassan";
        public const string Home = "07_Home";
        public const string Credits = "08_Credits";

        public static readonly string[] BuildOrder =
        {
            MainMenu, Intro, CafeMaure, Oudaya, Souk, Hassan, Home, Credits
        };

        public static readonly string[] GameplayScenes =
        {
            CafeMaure, Oudaya, Souk, Hassan
        };

        public static string AssetPath(string sceneId)
        {
            return "Assets/_Game/Scenes/" + sceneId + ".unity";
        }

        public static bool IsKnown(string sceneId)
        {
            if (string.IsNullOrEmpty(sceneId)) return false;
            for (int i = 0; i < BuildOrder.Length; i++)
            {
                if (BuildOrder[i] == sceneId) return true;
            }
            return false;
        }

        public static bool IsGameplay(string sceneId)
        {
            if (string.IsNullOrEmpty(sceneId)) return false;
            for (int i = 0; i < GameplayScenes.Length; i++)
            {
                if (GameplayScenes[i] == sceneId) return true;
            }
            return false;
        }
    }

    [Serializable]
    public class SaveSettingsData
    {
        public float master = 0.9f;
        public float music = 0.7f;
        public float ambience = 0.8f;
        public float dialogue = 1f;
        public float sfx = 0.8f;
        public bool subtitles = true;
        public bool fullscreen = true;
        public bool reducedFlashes = false;
        public bool reducedMotion = false;

        public SaveSettingsData Clone()
        {
            return new SaveSettingsData
            {
                master = master,
                music = music,
                ambience = ambience,
                dialogue = dialogue,
                sfx = sfx,
                subtitles = subtitles,
                fullscreen = fullscreen,
                reducedFlashes = reducedFlashes,
                reducedMotion = reducedMotion
            };
        }
    }

    [Serializable]
    public class SaveData
    {
        public int version = 1;
        public string sceneId = "";
        public List<string> memoryIds = new List<string>();
        public List<string> flags = new List<string>();
        public SaveSettingsData settings = new SaveSettingsData();
        public string savedAtUtc = "";
    }

    public static class ProgressionRules
    {
        public static bool HasAllMemories(IEnumerable<string> memories)
        {
            if (memories == null) return false;
            for (int i = 0; i < MemoryIds.All.Length; i++)
            {
                if (!memories.Contains(MemoryIds.All[i])) return false;
            }
            return true;
        }

        public static int CountMemories(IEnumerable<string> memories)
        {
            if (memories == null) return 0;
            int count = 0;
            for (int i = 0; i < MemoryIds.All.Length; i++)
            {
                if (memories.Contains(MemoryIds.All[i])) count++;
            }
            return count;
        }

        public static bool HasMemory(IEnumerable<string> memories, string id)
        {
            return memories != null && !string.IsNullOrEmpty(id) && memories.Contains(id);
        }

        public static bool RequirementsMet(string[] requiredMemories, string[] requiredFlags, IEnumerable<string> memories,
            IEnumerable<string> flags)
        {
            if (requiredMemories != null)
            {
                for (int i = 0; i < requiredMemories.Length; i++)
                {
                    if (!HasMemory(memories, requiredMemories[i])) return false;
                }
            }

            if (requiredFlags != null)
            {
                for (int i = 0; i < requiredFlags.Length; i++)
                {
                    string flag = requiredFlags[i];
                    if (string.IsNullOrEmpty(flag)) continue;
                    if (flags == null || !flags.Contains(flag)) return false;
                }
            }

            return true;
        }

        public static string ObjectiveFor(string sceneId, IEnumerable<string> memories, IEnumerable<string> flags)
        {
            bool cafe = HasMemory(memories, MemoryIds.Cafe);
            bool door = HasMemory(memories, MemoryIds.Door);
            bool amina = HasMemory(memories, MemoryIds.Amina);
            bool souk = HasMemory(memories, MemoryIds.Souk);
            bool recordingUnlocked = flags != null && flags.Contains(GameFlags.RecordingUnlocked);

            switch (sceneId)
            {
                case SceneIds.CafeMaure:
                    if (!cafe) return "Inspect the old photograph on the caf\u00e9 table.";
                    return "Follow the blue walls into the Kasbah.";
                case SceneIds.Oudaya:
                    if (!cafe) return "Find the old photograph first.";
                    if (!door) return "Find the door that matches the photograph. Stand near a door and press C.";
                    if (!amina) return "Ask Lalla Amina about the house.";
                    return "Continue toward the souk.";
                case SceneIds.Souk:
                    if (!recordingUnlocked) return "Ask Allal about your mother.";
                    if (!souk) return "Record the souk. Stand by the sound marker and press R.";
                    if (!door || !amina) return "You still need the house and Amina's memory.";
                    return "You carry all four memories. Return home.";
                case SceneIds.Hassan:
                    return "The city is quiet. Return home when you are ready.";
                case SceneIds.Home:
                    return "Home.";
                case SceneIds.Intro:
                case SceneIds.MainMenu:
                case SceneIds.Credits:
                    return string.Empty;
                default:
                    return "Every street holds a memory.";
            }
        }
    }

    public static class GameFlags
    {
        public const string RecordingUnlocked = "recording_unlocked";
        public const string IntroSeen = "intro_seen";
        public const string DoorCameraHint = "door_camera_hint";
        public const string HomeSeen = "home_seen";
        public const string CreditsSeen = "credits_seen";
    }
}

