using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LWalida.EditorTools
{
    /// <summary>
    /// Creates the runtime ScriptableObject data: memories, dialogue assets and the
    /// central AssetCatalog. Existing user-authored values are preserved.
    /// </summary>
    public static class DataAssetFactory
    {
        private static readonly Dictionary<string, string> EnvironmentFiles = new Dictionary<string, string>
        {
            { "cafe", "cafe_maure.jpg" },
            { "oudaya", "oudaya_street.jpg" },
            { "souk", "souk.jpg" },
            { "hassan", "hassan_sunset.jpg" },
            { "home", "home_evening.jpg" }
        };

        public static AssetCatalog BuildAll()
        {
            PlaceholderArtGenerator.EnsureFolder(LWalidaPaths.Memories);
            PlaceholderArtGenerator.EnsureFolder(LWalidaPaths.Dialogues);
            PlaceholderArtGenerator.EnsureFolder(LWalidaPaths.Resources);

            ImportEnvironmentSprites();
            var memories = BuildMemories();
            var amina = BuildDialogueAsset("amina", NarrativeDefaults.BuildAminaDialogue());
            var aminaAfter = BuildDialogueAsset("amina_after", NarrativeDefaults.BuildAminaAfterDialogue());
            var allal = BuildDialogueAsset("allal", NarrativeDefaults.BuildAllalDialogue());
            var allalAfter = BuildDialogueAsset("allal_after", NarrativeDefaults.BuildAllalAfterDialogue());

            var catalog = AssetDatabase.LoadAssetAtPath<AssetCatalog>(LWalidaPaths.CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<AssetCatalog>();
                AssetDatabase.CreateAsset(catalog, LWalidaPaths.CatalogPath);
            }

            catalog.memories = memories.ToArray();
            if (catalog.aminaDialogue == null) catalog.aminaDialogue = amina;
            if (catalog.aminaAfterDialogue == null) catalog.aminaAfterDialogue = aminaAfter;
            if (catalog.allalDialogue == null) catalog.allalDialogue = allal;
            if (catalog.allalAfterDialogue == null) catalog.allalAfterDialogue = allalAfter;

            AssignEnvironment(catalog);
            AssignArt(catalog);
            AssignAudio(catalog);

            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            return catalog;
        }

        private static List<MemoryData> BuildMemories()
        {
            var result = new List<MemoryData>();
            for (int i = 0; i < NarrativeDefaults.MemoryTemplates.Length; i++)
            {
                var template = NarrativeDefaults.MemoryTemplates[i];
                string path = LWalidaPaths.Memories + "/" + template.id + ".asset";
                var memory = AssetDatabase.LoadAssetAtPath<MemoryData>(path);
                if (memory == null)
                {
                    memory = ScriptableObject.CreateInstance<MemoryData>();
                    AssetDatabase.CreateAsset(memory, path);
                }

                if (string.IsNullOrEmpty(memory.id)) memory.id = template.id;
                if (string.IsNullOrEmpty(memory.title)) memory.title = template.title;
                if (string.IsNullOrEmpty(memory.description)) memory.description = template.description;
                memory.type = template.type;
                memory.order = template.order;
                memory.name = template.id;

                if (memory.image == null) memory.image = LoadSprite(MemoryArtName(template.id));
                if (memory.audio == null && template.id == MemoryIds.Souk)
                {
                    memory.audio = LoadAudio("ambienceSouk");
                    memory.audioIsPlaceholder = memory.audio != null;
                }

                EditorUtility.SetDirty(memory);
                result.Add(memory);
            }
            return result;
        }

        private static string MemoryArtName(string id)
        {
            switch (id)
            {
                case MemoryIds.Cafe: return "memoryCafe";
                case MemoryIds.Door: return "memoryDoor";
                case MemoryIds.Amina: return "memoryAmina";
                case MemoryIds.Souk: return "memorySouk";
                default: return "memoryCafe";
            }
        }

        private static DialogueData BuildDialogueAsset(string id, DialogueData generated)
        {
            string path = LWalidaPaths.Dialogues + "/" + id + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<DialogueData>(path);
            if (existing != null) return existing;
            generated.name = id;
            AssetDatabase.CreateAsset(generated, path);
            EditorUtility.SetDirty(generated);
            return generated;
        }

        private static void ImportEnvironmentSprites()
        {
            foreach (var pair in EnvironmentFiles)
            {
                string path = LWalidaPaths.EnvironmentArt + "/" + pair.Value;
                if (!File.Exists(path)) continue;
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;
                if (importer.textureType != TextureImporterType.Sprite ||
                    importer.spriteImportMode != SpriteImportMode.Single ||
                    importer.maxTextureSize < 4096)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.spritePixelsPerUnit = 100f;
                    importer.maxTextureSize = 4096;
                    importer.textureCompression = TextureImporterCompression.CompressedHQ;
                    importer.mipmapEnabled = true;
                    importer.SaveAndReimport();
                }
            }
        }

        private static void AssignEnvironment(AssetCatalog catalog)
        {
            if (catalog.envCafe == null) catalog.envCafe = LoadEnvironmentSprite("cafe_maure.jpg");
            if (catalog.envOudaya == null) catalog.envOudaya = LoadEnvironmentSprite("oudaya_street.jpg");
            if (catalog.envSouk == null) catalog.envSouk = LoadEnvironmentSprite("souk.jpg");
            if (catalog.envHassan == null) catalog.envHassan = LoadEnvironmentSprite("hassan_sunset.jpg");
            if (catalog.envHome == null) catalog.envHome = LoadEnvironmentSprite("home_evening.jpg");

            Sprite fallback = catalog.menuBackground != null ? catalog.menuBackground : LoadSprite("menuBackground");
            if (catalog.envCafe == null) catalog.envCafe = fallback;
            if (catalog.envOudaya == null) catalog.envOudaya = fallback;
            if (catalog.envSouk == null) catalog.envSouk = fallback;
            if (catalog.envHassan == null) catalog.envHassan = fallback;
            if (catalog.envHome == null) catalog.envHome = fallback;
        }

        private static void AssignArt(AssetCatalog catalog)
        {
            AssignSprite(ref catalog.skyCafe, "skyCafe");
            AssignSprite(ref catalog.skyOudaya, "skyOudaya");
            AssignSprite(ref catalog.skySouk, "skySouk");
            AssignSprite(ref catalog.skyHassan, "skyHassan");
            AssignSprite(ref catalog.skyHome, "skyHome");
            AssignSprite(ref catalog.farCafe, "farCafe");
            AssignSprite(ref catalog.farOudaya, "farOudaya");
            AssignSprite(ref catalog.farSouk, "farSouk");
            AssignSprite(ref catalog.farHassan, "farHassan");
            AssignSprite(ref catalog.farHome, "farHome");
            AssignSprite(ref catalog.groundStrip, "groundStrip");

            AssignSprite(ref catalog.playerIdle, "playerIdle");
            AssignSprite(ref catalog.aminaIdle, "aminaIdle");
            AssignSprite(ref catalog.allalIdle, "allalIdle");
            AssignSprite(ref catalog.neighborIdle, "neighborIdle");

            AssignSprite(ref catalog.portraitPlayer, "portraitPlayer");
            AssignSprite(ref catalog.portraitAmina, "portraitAmina");
            AssignSprite(ref catalog.portraitAllal, "portraitAllal");
            AssignSprite(ref catalog.portraitNeighbor, "portraitNeighbor");

            AssignSprite(ref catalog.doorCorrect, "doorCorrect");
            AssignSprite(ref catalog.doorWrong01, "doorWrong01");
            AssignSprite(ref catalog.doorWrong02, "doorWrong02");
            AssignSprite(ref catalog.photoProp, "photoProp");
            AssignSprite(ref catalog.teaSet, "teaSet");
            AssignSprite(ref catalog.flowerpot, "flowerpot");
            AssignSprite(ref catalog.bougainvillea, "bougainvillea");
            AssignSprite(ref catalog.stall, "stall");
            AssignSprite(ref catalog.exitMarker, "exitMarker");
            AssignSprite(ref catalog.soundMarker, "soundMarker");
            AssignSprite(ref catalog.referencePhoto, "referencePhoto");

            AssignSprite(ref catalog.memoryCafe, "memoryCafe");
            AssignSprite(ref catalog.memoryDoor, "memoryDoor");
            AssignSprite(ref catalog.memoryAmina, "memoryAmina");
            AssignSprite(ref catalog.memorySouk, "memorySouk");

            AssignSprite(ref catalog.logoEmblem, "logoEmblem");
            AssignSprite(ref catalog.menuBackground, "menuBackground");
            AssignSprite(ref catalog.panelDark, "panelDark");
            AssignSprite(ref catalog.panelLight, "panelLight");
            AssignSprite(ref catalog.paperTexture, "paperTexture");
            AssignSprite(ref catalog.zelligePattern, "zelligePattern");
            AssignSprite(ref catalog.vignette, "vignette");
            AssignSprite(ref catalog.grain, "grain");
            AssignSprite(ref catalog.iconCamera, "iconCamera");
            AssignSprite(ref catalog.iconJournal, "iconJournal");
            AssignSprite(ref catalog.iconRecord, "iconRecord");
            AssignSprite(ref catalog.iconLock, "iconLock");
            AssignSprite(ref catalog.iconAudio, "iconAudio");
            AssignSprite(ref catalog.whitePixel, "whitePixel");
        }

        private static void AssignAudio(AssetCatalog catalog)
        {
            if (catalog.menuMusic == null) catalog.menuMusic = LoadAudio("menuMusic");
            if (catalog.explorationMusic == null) catalog.explorationMusic = LoadAudio("explorationMusic");
            if (catalog.endingMusic == null) catalog.endingMusic = LoadAudio("endingMusic");

            if (catalog.ambienceCafe == null) catalog.ambienceCafe = LoadAudio("ambienceCafe");
            if (catalog.ambienceOudaya == null) catalog.ambienceOudaya = LoadAudio("ambienceOudaya");
            if (catalog.ambienceSouk == null) catalog.ambienceSouk = LoadAudio("ambienceSouk");
            if (catalog.ambienceHassan == null) catalog.ambienceHassan = LoadAudio("ambienceHassan");
            if (catalog.ambienceHome == null) catalog.ambienceHome = LoadAudio("ambienceHome");

            if (catalog.sfxShutter == null) catalog.sfxShutter = LoadAudio("sfxShutter");
            if (catalog.sfxKnock == null) catalog.sfxKnock = LoadAudio("sfxKnock");
            if (catalog.sfxFootstep == null) catalog.sfxFootstep = LoadAudio("sfxFootstep");
            if (catalog.sfxUiClick == null) catalog.sfxUiClick = LoadAudio("sfxUiClick");
            if (catalog.sfxMemoryUnlock == null) catalog.sfxMemoryUnlock = LoadAudio("sfxMemoryUnlock");
            if (catalog.sfxRecordComplete == null) catalog.sfxRecordComplete = LoadAudio("sfxRecordComplete");
        }

        private static void AssignSprite(ref Sprite field, string name)
        {
            if (field != null) return;
            field = LoadSprite(name);
        }

        private static Sprite LoadSprite(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(PlaceholderArtGenerator.PathFor(name));
        }

        private static Sprite LoadEnvironmentSprite(string file)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(LWalidaPaths.EnvironmentArt + "/" + file);
        }

        private static AudioClip LoadAudio(string name)
        {
            return AssetDatabase.LoadAssetAtPath<AudioClip>(AudioGenerator.PathFor(name));
        }
    }
}
