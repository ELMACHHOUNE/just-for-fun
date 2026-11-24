using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace LWalida.EditorTools
{
    /// <summary>
    /// Menu entry points for the whole MVP pipeline. Existing scenes are never
    /// replaced by "Build or Update"; use the explicit rebuild entry for that.
    /// </summary>
    public static class MVPBuilder
    {
        private const string BuildFolder = "Builds/Windows";
        private const string BuildFile = "LWalida.exe";

        [MenuItem("LWalida/Build or Update MVP Scenes", priority = 20)]
        public static void BuildOrUpdate()
        {
            var catalog = BuildCore(false);
            ProjectValidator.ValidateAndLog(catalog);
            EditorUtility.DisplayDialog("L'Walida",
                "MVP assets, data, prefabs and scenes are ready.\n\n" +
                "Existing scenes were preserved. Open Assets/_Game/Scenes/03_CafeMaure.unity and press Play.",
                "Close");
        }

        [MenuItem("LWalida/Rebuild MVP Scenes (overwrite generated scenes)", priority = 21)]
        public static void RebuildScenes()
        {
            if (!EditorUtility.DisplayDialog("Rebuild generated scenes?",
                    "This overwrites the eight generated MVP scenes. User-authored scenes outside these paths are untouched.\n\nContinue?",
                    "Rebuild", "Cancel"))
            {
                return;
            }
            var catalog = BuildCore(true);
            ProjectValidator.ValidateAndLog(catalog);
        }

        [MenuItem("LWalida/Generate Missing Placeholders", priority = 22)]
        public static void GeneratePlaceholders()
        {
            PlaceholderArtGenerator.GenerateMissing();
            AudioGenerator.GenerateMissing();
            AssetDatabase.Refresh();
            var catalog = DataAssetFactory.BuildAll();
            PrefabFactory.BuildAll(catalog);
            EditorUtility.DisplayDialog("L'Walida", "Missing placeholder art and audio were generated.\n" +
                                                     "Existing files were not overwritten.", "Close");
        }

        [MenuItem("LWalida/Validate Project", priority = 23)]
        public static void Validate()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<AssetCatalog>(LWalidaPaths.CatalogPath);
            bool ok = ProjectValidator.ValidateAndLog(catalog);
            EditorUtility.DisplayDialog("L'Walida validation",
                ok ? "Validation passed. Check the Console for details." :
                    "Validation found problems. Check the Console for details.", "Close");
        }

        [MenuItem("LWalida/Build Windows", priority = 40)]
        public static void BuildWindows()
        {
            var catalog = DataAssetFactory.BuildAll();
            PrefabFactory.BuildAll(catalog);
            SceneFactory.UpdateBuildSettings();

            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64))
            {
                Debug.LogError("[LWalida] Windows build support is not installed for this Unity editor. " +
                               "Install 'Windows Build Support (Mono)' from Unity Hub, then rerun LWalida/Build Windows.");
                EditorUtility.DisplayDialog("Windows build unavailable",
                    "Windows Build Support is not installed.\n\n" +
                    "Install it from Unity Hub > Installs > Add Modules, then run LWalida/Build Windows again.", "Close");
                return;
            }

            Directory.CreateDirectory(BuildFolder);
            string path = BuildFolder + "/" + BuildFile;
            var options = new BuildPlayerOptions
            {
                scenes = BuildScenePaths(),
                locationPathName = path,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };
            var report = BuildPipeline.BuildPlayer(options);
            if (report != null && report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log("[LWalida] Windows build succeeded: " + path + " (" + report.summary.totalSize + " bytes)");
                EditorUtility.DisplayDialog("Build complete", "Windows build created at " + path, "Close");
            }
            else
            {
                Debug.LogError("[LWalida] Windows build failed: " +
                               (report != null ? report.summary.result.ToString() : "no report"));
                EditorUtility.DisplayDialog("Build failed", "See the Console for build errors.", "Close");
            }
        }

        [MenuItem("LWalida/Build All (one click)", priority = 0)]
        public static void BuildEverything()
        {
            bool overwrite = EditorUtility.DisplayDialog("Build L'Walida MVP",
                "Generate placeholders, data and prefabs, then build the eight scenes.\n\n" +
                "Choose 'Keep scenes' to preserve existing scene files, or 'Rebuild scenes' to overwrite the generated ones.",
                "Keep scenes", "Rebuild scenes");
            if (EditorUtility.DisplayDialog("Confirm",
                    "Proceed with " + (overwrite ? "rebuilding" : "preserving") + " the generated MVP scenes?",
                    "Build", "Cancel"))
            {
                var catalog = BuildCore(!overwrite);
                bool ok = ProjectValidator.ValidateAndLog(catalog);
                EditorUtility.DisplayDialog("L'Walida MVP",
                    ok ? "Build complete. Open 01_MainMenu.unity and press Play." :
                        "Build finished with validation errors. See the Console.",
                    "Close");
            }
        }

        public static AssetCatalog BuildCore(bool overwriteScenes)
        {
            PlaceholderArtGenerator.EnsureFolder("Assets/_Game");
            PlaceholderArtGenerator.EnsureFolder(LWalidaPaths.GameRoot + "/Generated");
            PlaceholderArtGenerator.GenerateMissing();
            AudioGenerator.GenerateMissing();
            AssetDatabase.Refresh();

            var catalog = DataAssetFactory.BuildAll();
            Debug.Log("[LWalida Diagnose] BuildCore catalog=" + (catalog != null));
            PrefabFactory.BuildAll(catalog);
            Debug.Log("[LWalida Diagnose] After prefabs catalog=" + (catalog != null));
            SceneFactory.BuildAll(catalog, overwriteScenes);
            ApplyPlayerSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[LWalida] MVP build finished. Scenes generated: " + SceneIds.BuildOrder.Length);
            return AssetDatabase.LoadAssetAtPath<AssetCatalog>(LWalidaPaths.CatalogPath);
        }

        private static string[] BuildScenePaths()
        {
            var paths = new string[SceneIds.BuildOrder.Length];
            for (int i = 0; i < paths.Length; i++) paths[i] = SceneIds.AssetPath(SceneIds.BuildOrder[i]);
            return paths;
        }

        private static void ApplyPlayerSettings()
        {
            PlayerSettings.companyName = "L'Walida";
            PlayerSettings.productName = "L'Walida - The Mother";
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = true;
        }

        [MenuItem("LWalida/Batch/Validate", priority = 80)]
        public static void BatchValidate()
        {
            bool ok = ProjectValidator.ValidateAndLog(
                AssetDatabase.LoadAssetAtPath<AssetCatalog>(LWalidaPaths.CatalogPath));
            EditorApplication.Exit(ok ? 0 : 1);
        }

        [MenuItem("LWalida/Batch/Build All", priority = 81)]
        public static void BatchBuildAll()
        {
            try
            {
                var catalog = BuildCore(false);
                bool ok = ProjectValidator.ValidateAndLog(catalog);
                EditorApplication.Exit(ok ? 0 : 1);
            }
            catch (System.Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        [MenuItem("LWalida/Batch/Rebuild All", priority = 83)]
        public static void BatchRebuildAll()
        {
            try
            {
                var catalog = BuildCore(true);
                bool ok = ProjectValidator.ValidateAndLog(catalog);
                EditorApplication.Exit(ok ? 0 : 1);
            }
            catch (System.Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        [MenuItem("LWalida/Batch/Diagnose Sprites", priority = 84)]
        public static void BatchDiagnoseSprites()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<AssetCatalog>(LWalidaPaths.CatalogPath);
            Debug.Log("[LWalida Diagnose] catalog=" + (catalog != null));
            if (catalog != null)
            {
                Debug.Log("[LWalida Diagnose] playerIdle=" + (catalog.playerIdle != null) +
                          " envCafe=" + (catalog.envCafe != null) +
                          " photoProp=" + (catalog.photoProp != null) +
                          " whitePixel=" + (catalog.whitePixel != null));
                Debug.Log("[LWalida Diagnose] playerPrefab=" + (catalog.playerPrefab != null) +
                          " propPrefab=" + (catalog.propPrefab != null));
            }
            var loaded = AssetDatabase.LoadAssetAtPath<Sprite>(PlaceholderArtGenerator.PathFor("playerIdle"));
            Debug.Log("[LWalida Diagnose] directLoad playerIdle=" + (loaded != null) +
                      " path=" + PlaceholderArtGenerator.PathFor("playerIdle"));
            var env = AssetDatabase.LoadAssetAtPath<Sprite>(LWalidaPaths.EnvironmentArt + "/cafe_maure.jpg");
            Debug.Log("[LWalida Diagnose] directLoad envCafe=" + (env != null));
            EditorApplication.Exit(0);
        }

        [MenuItem("LWalida/Batch/Build Windows", priority = 82)]
        public static void BatchBuildWindows()
        {
            try
            {
                BuildCore(false);
                if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone,
                        BuildTarget.StandaloneWindows64))
                {
                    Debug.LogError("[LWalida] Windows build support is not installed.");
                    EditorApplication.Exit(2);
                    return;
                }
                Directory.CreateDirectory(BuildFolder);
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = BuildScenePaths(),
                    locationPathName = BuildFolder + "/" + BuildFile,
                    target = BuildTarget.StandaloneWindows64,
                    options = BuildOptions.None
                });
                EditorApplication.Exit(report != null && report.summary.result == BuildResult.Succeeded ? 0 : 1);
            }
            catch (System.Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }
    }
}

