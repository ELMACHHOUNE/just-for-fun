using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LWalida.EditorTools
{
    /// <summary>
    /// Static project validation. Never modifies assets; safe to run repeatedly and
    /// from batchmode.
    /// </summary>
    public static class ProjectValidator
    {
        public static bool ValidateAndLog(AssetCatalog catalog)
        {
            int errors = 0;
            int warnings = 0;

            void Error(string message)
            {
                errors++;
                Debug.LogError("[LWalida Validation] " + message);
            }

            void Warn(string message)
            {
                warnings++;
                Debug.LogWarning("[LWalida Validation] " + message);
            }

            if (catalog == null)
            {
                Error("AssetCatalog is missing at " + LWalidaPaths.CatalogPath + ".");
                return Report(errors, warnings);
            }

            ValidateMemories(catalog, Error, Warn);
            ValidateDialogue(catalog, Error);
            ValidateScenes(Error, Warn);
            ValidatePrefabs(catalog, Error);
            ValidateArt(catalog, Warn);
            ValidateAudio(catalog, Warn);

            for (int i = 0; i < LevelCatalog.All.Length; i++)
            {
                var level = LevelCatalog.All[i];
                if (level.IsMenu || level.IsCredits || level.IsCutscene) continue;
                if (level.WorldWidth < 10f) Error("Level '" + level.SceneId + "' has an invalid world width.");
                if (level.SpawnX <= 0f || level.SpawnX >= level.WorldWidth)
                {
                    Warn("Level '" + level.SceneId + "' spawn point is outside its world width.");
                }
            }

            if (catalog.titleArabic == null)
            {
                Warn("Arabic title art is not assigned; the menu and credits fall back to built-in font text. " +
                     "Arabic shaping is not guaranteed by the built-in font.");
            }

            return Report(errors, warnings);
        }

        private static void ValidateMemories(AssetCatalog catalog, System.Action<string> error,
            System.Action<string> warn)
        {
            if (catalog.memories == null || catalog.memories.Length != MemoryIds.RequiredCount)
            {
                error("Catalog must contain exactly four memories.");
                return;
            }

            var ids = new HashSet<string>();
            for (int i = 0; i < catalog.memories.Length; i++)
            {
                var memory = catalog.memories[i];
                if (memory == null)
                {
                    error("Memory at index " + i + " is null.");
                    continue;
                }
                if (!memory.IsValid()) error("Memory at index " + i + " has no id or title.");
                if (!ids.Add(memory.id)) error("Duplicate memory id: " + memory.id);
                if (memory.image == null) warn("Memory '" + memory.id + "' has no image.");
                if (memory.id == MemoryIds.Souk && memory.audio == null)
                {
                    error("The sound memory is missing its audio clip.");
                }
                if (memory.id != MemoryIds.Souk && memory.audio != null)
                {
                    warn("Memory '" + memory.id + "' has audio but is not a sound memory.");
                }
            }

            for (int i = 0; i < MemoryIds.All.Length; i++)
            {
                if (!ids.Contains(MemoryIds.All[i])) error("Missing required memory id: " + MemoryIds.All[i]);
            }
        }

        private static void ValidateDialogue(AssetCatalog catalog, System.Action<string> error)
        {
            var errors = new List<string>();
            ValidateDialogueAsset(catalog.aminaDialogue, "Amina", errors);
            ValidateDialogueAsset(catalog.aminaAfterDialogue, "Amina after", errors);
            ValidateDialogueAsset(catalog.allalDialogue, "Allal", errors);
            ValidateDialogueAsset(catalog.allalAfterDialogue, "Allal after", errors);
            for (int i = 0; i < errors.Count; i++) error(errors[i]);
        }

        private static void ValidateDialogueAsset(DialogueData data, string label, List<string> errors)
        {
            if (data == null)
            {
                errors.Add(label + " dialogue asset is missing.");
                return;
            }
            data.Validate(errors);
        }

        private static void ValidateScenes(System.Action<string> error, System.Action<string> warn)
        {
            var settings = EditorBuildSettings.scenes;
            if (settings == null || settings.Length != SceneIds.BuildOrder.Length)
            {
                error("Build settings should contain exactly " + SceneIds.BuildOrder.Length + " scenes.");
            }

            for (int i = 0; i < SceneIds.BuildOrder.Length; i++)
            {
                string sceneId = SceneIds.BuildOrder[i];
                string path = SceneIds.AssetPath(sceneId);
                if (!File.Exists(path))
                {
                    error("Scene file is missing: " + path);
                    continue;
                }
                if (settings == null || i >= settings.Length)
                {
                    continue;
                }
                if (settings[i].path != path)
                {
                    warn("Build scene " + i + " is '" + settings[i].path + "', expected '" + path + "'.");
                }
                if (!settings[i].enabled) error("Build scene is disabled: " + path);
            }

            if (File.Exists("Assets/Scenes/SampleScene.unity"))
            {
                Debug.Log("[LWalida Validation] Original SampleScene preserved.");
            }
        }

        private static void ValidatePrefabs(AssetCatalog catalog, System.Action<string> error)
        {
            if (catalog.playerPrefab == null) error("Catalog player prefab is missing.");
            if (catalog.npcPrefab == null) error("Catalog NPC prefab is missing.");
            if (catalog.doorPrefab == null) error("Catalog door prefab is missing.");
            if (catalog.propPrefab == null) error("Catalog prop prefab is missing.");
            if (catalog.exitPrefab == null) error("Catalog exit prefab is missing.");
            if (catalog.zonePrefab == null) error("Catalog recording zone prefab is missing.");

            string[] required = { "Player", "NPC", "Door", "Prop", "Exit", "RecordingZone" };
            for (int i = 0; i < required.Length; i++)
            {
                string path = LWalidaPaths.Prefabs + "/" + required[i] + ".prefab";
                if (!File.Exists(path)) error("Prefab file is missing: " + path);
            }
        }

        private static void ValidateArt(AssetCatalog catalog, System.Action<string> warn)
        {
            if (catalog.envCafe == null) warn("Cafe environment art is not assigned.");
            if (catalog.envOudaya == null) warn("Oudaya environment art is not assigned.");
            if (catalog.envSouk == null) warn("Souk environment art is not assigned.");
            if (catalog.envHassan == null) warn("Hassan environment art is not assigned.");
            if (catalog.envHome == null) warn("Home environment art is not assigned.");
            if (catalog.playerIdle == null) warn("Player sprite is missing.");
            if (catalog.panelDark == null) warn("Panel sprite is missing; UI will use a plain rectangle.");
            if (catalog.whitePixel == null) warn("White pixel sprite is missing.");
            if (catalog.menuBackground == null) warn("Menu background is not assigned.");
        }

        private static void ValidateAudio(AssetCatalog catalog, System.Action<string> warn)
        {
            if (catalog.menuMusic == null) warn("Menu music is missing.");
            if (catalog.explorationMusic == null) warn("Exploration music is missing.");
            if (catalog.endingMusic == null) warn("Ending music is missing.");
            if (catalog.ambienceCafe == null) warn("Cafe ambience is missing.");
            if (catalog.ambienceOudaya == null) warn("Oudaya ambience is missing.");
            if (catalog.ambienceSouk == null) warn("Souk ambience is missing.");
            if (catalog.ambienceHassan == null) warn("Hassan ambience is missing.");
            if (catalog.ambienceHome == null) warn("Home ambience is missing.");
            if (catalog.sfxShutter == null) warn("Shutter SFX is missing.");
            if (catalog.sfxKnock == null) warn("Knock SFX is missing.");
            if (catalog.sfxFootstep == null) warn("Footstep SFX is missing.");
            if (catalog.sfxUiClick == null) warn("UI click SFX is missing.");
            if (catalog.sfxMemoryUnlock == null) warn("Memory unlock SFX is missing.");
            if (catalog.sfxRecordComplete == null) warn("Recording complete SFX is missing.");
        }

        private static bool Report(int errors, int warnings)
        {
            if (errors == 0 && warnings == 0)
            {
                Debug.Log("[LWalida Validation] Passed with no errors or warnings.");
            }
            else
            {
                Debug.Log("[LWalida Validation] Completed: " + errors + " error(s), " + warnings + " warning(s).");
            }
            return errors == 0;
        }
    }
}
