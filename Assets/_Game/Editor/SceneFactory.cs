using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LWalida.EditorTools
{
    /// <summary>
    /// Creates the eight MVP scenes, composes level content through the same
    /// LevelComposer used at runtime, keeps cutscene controllers wired, and registers
    /// the build scene order.
    /// </summary>
    public static class SceneFactory
    {
        public static void BuildAll(AssetCatalog catalog, bool overwrite)
        {
            PlaceholderArtGenerator.EnsureFolder(LWalidaPaths.Scenes);
            for (int i = 0; i < SceneIds.BuildOrder.Length; i++)
            {
                string sceneId = SceneIds.BuildOrder[i];
                string path = SceneIds.AssetPath(sceneId);
                if (File.Exists(path) && !overwrite)
                {
                    Debug.Log("[LWalida] Scene exists, skipped: " + path);
                    continue;
                }
                BuildScene(sceneId, catalog, path);
            }
            UpdateBuildSettings();
            AssetDatabase.SaveAssets();
        }

        private static void BuildScene(string sceneId, AssetCatalog catalog, string path)
        {
            var level = LevelCatalog.Get(sceneId);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            catalog = AssetDatabase.LoadAssetAtPath<AssetCatalog>(LWalidaPaths.CatalogPath);

            var bootstrapGo = new GameObject("SceneBootstrap");
            var bootstrap = bootstrapGo.AddComponent<SceneBootstrap>();
            bootstrap.SetSceneId(sceneId);

            if (!level.IsMenu && !level.IsCredits)
            {
                Debug.Log("[LWalida Diagnose] Compose " + sceneId +
                          " catalog=" + (catalog != null) +
                          " playerIdle=" + (catalog != null && catalog.playerIdle != null) +
                          " envCafe=" + (catalog != null && catalog.envCafe != null) +
                          " playerPrefab=" + (catalog != null && catalog.playerPrefab != null));
                LevelComposer.Compose(sceneId, catalog, null);
            }

            if (level.IsMenu || level.IsCredits || level.IsCutscene)
            {
                EnsureCamera(level);
            }

            if (sceneId == SceneIds.Intro || sceneId == SceneIds.Home)
            {
                var cutsceneGo = new GameObject("Cutscene");
                var cutscene = cutsceneGo.AddComponent<CutsceneController>();
                cutscene.ConfigureForScene(sceneId);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            bool saved = EditorSceneManager.SaveScene(scene, path);
            if (!saved)
            {
                Debug.LogError("[LWalida] Failed to save scene: " + path);
            }
            else
            {
                Debug.Log("[LWalida] Built scene: " + path);
            }
        }

        private static void EnsureCamera(LevelInfo level)
        {
            Camera cam = Camera.main;
            if (cam == null) cam = Object.FindFirstObjectByType<Camera>();
            if (cam == null)
            {
                var go = new GameObject("Main Camera");
                cam = go.AddComponent<Camera>();
            }

            cam.gameObject.tag = "MainCamera";
            cam.orthographic = true;
            cam.orthographicSize = 5.4f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = level.IsMenu || level.IsCredits
                ? new Color(0.035f, 0.07f, 0.09f)
                : Color.black;
            cam.transform.rotation = Quaternion.identity;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            if (cam.GetComponent<AudioListener>() == null &&
                Object.FindFirstObjectByType<AudioListener>() == null)
            {
                cam.gameObject.AddComponent<AudioListener>();
            }
        }

        public static void UpdateBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>();
            for (int i = 0; i < SceneIds.BuildOrder.Length; i++)
            {
                string path = SceneIds.AssetPath(SceneIds.BuildOrder[i]);
                if (File.Exists(path)) scenes.Add(new EditorBuildSettingsScene(path, true));
            }
            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log("[LWalida] Build settings scenes: " + scenes.Count);
        }
    }
}
