using UnityEngine;

namespace LWalida
{
    public class LevelInfo
    {
        public string SceneId;
        public string DisplayName;
        public float WorldWidth = 30f;
        public float SpawnX = 3f;
        public float GroundY = -2f;
        public float CameraY = 2.5f;
        public Color Tint = new Color(1f, 1f, 1f, 0f);
        public string AmbienceKey = "";
        public string MusicKey = "exploration";
        public string ArtKey = "oudaya";
        public bool IsMenu;
        public bool IsCutscene;
        public bool IsCredits;
    }

    public static class LevelCatalog
    {
        public static readonly LevelInfo[] All =
        {
            new LevelInfo
            {
                SceneId = SceneIds.MainMenu, DisplayName = "Main Menu", WorldWidth = 0f,
                AmbienceKey = "", MusicKey = "menu", ArtKey = "oudaya", IsMenu = true
            },
            new LevelInfo
            {
                SceneId = SceneIds.Intro, DisplayName = "Intro", WorldWidth = 0f,
                AmbienceKey = "cafe", MusicKey = "ending", ArtKey = "cafe", IsCutscene = true
            },
            new LevelInfo
            {
                SceneId = SceneIds.CafeMaure, DisplayName = "Caf\u00e9 Maure", WorldWidth = 26f, SpawnX = 2.5f,
                AmbienceKey = "cafe", MusicKey = "exploration", ArtKey = "cafe",
                Tint = new Color(1f, 0.84f, 0.62f, 0.055f)
            },
            new LevelInfo
            {
                SceneId = SceneIds.Oudaya, DisplayName = "Kasbah des Oudayas", WorldWidth = 42f, SpawnX = 2.5f,
                AmbienceKey = "oudaya", MusicKey = "exploration", ArtKey = "oudaya",
                Tint = new Color(0.18f, 0.46f, 0.58f, 0.06f)
            },
            new LevelInfo
            {
                SceneId = SceneIds.Souk, DisplayName = "The Souk", WorldWidth = 38f, SpawnX = 2.5f,
                AmbienceKey = "souk", MusicKey = "exploration", ArtKey = "souk",
                Tint = new Color(0.85f, 0.56f, 0.22f, 0.075f)
            },
            new LevelInfo
            {
                SceneId = SceneIds.Hassan, DisplayName = "Hassan Tower at Sunset", WorldWidth = 30f, SpawnX = 3f,
                AmbienceKey = "hassan", MusicKey = "exploration", ArtKey = "hassan",
                Tint = new Color(0.95f, 0.55f, 0.22f, 0.11f)
            },
            new LevelInfo
            {
                SceneId = SceneIds.Home, DisplayName = "Home", WorldWidth = 22f, SpawnX = 4f,
                AmbienceKey = "home", MusicKey = "ending", ArtKey = "home",
                Tint = new Color(0.06f, 0.13f, 0.26f, 0.16f), IsCutscene = true
            },
            new LevelInfo
            {
                SceneId = SceneIds.Credits, DisplayName = "Credits", WorldWidth = 0f,
                AmbienceKey = "home", MusicKey = "ending", ArtKey = "home", IsCredits = true
            }
        };

        public static LevelInfo Get(string sceneId)
        {
            for (int i = 0; i < All.Length; i++)
            {
                if (All[i].SceneId == sceneId) return All[i];
            }
            return All[2];
        }
    }
}
