using UnityEngine;

namespace LWalida
{
    /// <summary>
    /// Central serialized reference table. Authored assets can be dropped into these
    /// slots to replace the generated placeholders. Loaded from Resources/AssetCatalog
    /// so every scene shares the same composition root.
    /// </summary>
    [CreateAssetMenu(fileName = "AssetCatalog", menuName = "LWalida/Asset Catalog", order = 0)]
    public class AssetCatalog : ScriptableObject
    {
        [Header("Memories")]
        public MemoryData[] memories = new MemoryData[0];

        [Header("Dialogue")]
        public DialogueData aminaDialogue;
        public DialogueData aminaAfterDialogue;
        public DialogueData allalDialogue;
        public DialogueData allalAfterDialogue;

        [Header("Environments")]
        public Sprite envCafe;
        public Sprite envOudaya;
        public Sprite envSouk;
        public Sprite envHassan;
        public Sprite envHome;

        [Header("Parallax")]
        public Sprite skyCafe;
        public Sprite skyOudaya;
        public Sprite skySouk;
        public Sprite skyHassan;
        public Sprite skyHome;
        public Sprite farCafe;
        public Sprite farOudaya;
        public Sprite farSouk;
        public Sprite farHassan;
        public Sprite farHome;
        public Sprite groundStrip;

        [Header("Characters")]
        public Sprite playerIdle;
        public Sprite aminaIdle;
        public Sprite allalIdle;
        public Sprite neighborIdle;

        [Header("Portraits")]
        public Sprite portraitPlayer;
        public Sprite portraitAmina;
        public Sprite portraitAllal;
        public Sprite portraitNeighbor;

        [Header("Props")]
        public Sprite doorCorrect;
        public Sprite doorWrong01;
        public Sprite doorWrong02;
        public Sprite photoProp;
        public Sprite teaSet;
        public Sprite flowerpot;
        public Sprite bougainvillea;
        public Sprite stall;
        public Sprite exitMarker;
        public Sprite soundMarker;
        public Sprite referencePhoto;

        [Header("Memories / UI art")]
        public Sprite memoryCafe;
        public Sprite memoryDoor;
        public Sprite memoryAmina;
        public Sprite memorySouk;
        public Sprite logoEmblem;
        public Sprite titleArabic;
        public Sprite menuBackground;
        public Sprite panelDark;
        public Sprite panelLight;
        public Sprite paperTexture;
        public Sprite zelligePattern;
        public Sprite vignette;
        public Sprite grain;
        public Sprite iconCamera;
        public Sprite iconJournal;
        public Sprite iconRecord;
        public Sprite iconLock;
        public Sprite iconAudio;
        public Sprite whitePixel;

        [Header("Audio - Music")]
        public AudioClip menuMusic;
        public AudioClip explorationMusic;
        public AudioClip endingMusic;

        [Header("Audio - Ambience")]
        public AudioClip ambienceCafe;
        public AudioClip ambienceOudaya;
        public AudioClip ambienceSouk;
        public AudioClip ambienceHassan;
        public AudioClip ambienceHome;

        [Header("Audio - SFX")]
        public AudioClip sfxShutter;
        public AudioClip sfxKnock;
        public AudioClip sfxFootstep;
        public AudioClip sfxUiClick;
        public AudioClip sfxMemoryUnlock;
        public AudioClip sfxRecordComplete;

        [Header("Prefabs")]
        public GameObject playerPrefab;
        public GameObject npcPrefab;
        public GameObject doorPrefab;
        public GameObject propPrefab;
        public GameObject exitPrefab;
        public GameObject zonePrefab;

        public MemoryData GetMemory(string id)
        {
            if (memories == null) return null;
            for (int i = 0; i < memories.Length; i++)
            {
                if (memories[i] != null && memories[i].id == id) return memories[i];
            }
            return null;
        }

        public AudioClip GetAmbience(string key)
        {
            switch (key)
            {
                case "cafe": return ambienceCafe;
                case "oudaya": return ambienceOudaya;
                case "souk": return ambienceSouk;
                case "hassan": return ambienceHassan;
                case "home": return ambienceHome;
                default: return null;
            }
        }

        public AudioClip GetMusic(string key)
        {
            switch (key)
            {
                case "menu": return menuMusic;
                case "exploration": return explorationMusic;
                case "ending": return endingMusic;
                default: return null;
            }
        }

        public Sprite GetEnvironment(string key)
        {
            switch (key)
            {
                case "cafe": return envCafe;
                case "oudaya": return envOudaya;
                case "souk": return envSouk;
                case "hassan": return envHassan;
                case "home": return envHome;
                default: return null;
            }
        }

        public Sprite GetSky(string key)
        {
            switch (key)
            {
                case "cafe": return skyCafe;
                case "oudaya": return skyOudaya;
                case "souk": return skySouk;
                case "hassan": return skyHassan;
                case "home": return skyHome;
                default: return null;
            }
        }

        public Sprite GetFar(string key)
        {
            switch (key)
            {
                case "cafe": return farCafe;
                case "oudaya": return farOudaya;
                case "souk": return farSouk;
                case "hassan": return farHassan;
                case "home": return farHome;
                default: return null;
            }
        }
    }
}
