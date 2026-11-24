using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LWalida.Tests
{
    public class GeneratedSceneTests
    {
        [Test]
        public void GeneratedGameplayScenesContainCoreContent()
        {
            for (int i = 0; i < SceneIds.BuildOrder.Length; i++)
            {
                string sceneId = SceneIds.BuildOrder[i];
                var level = LevelCatalog.Get(sceneId);
                if (level.IsMenu || level.IsCredits || level.IsCutscene) continue;

                var scene = EditorSceneManager.OpenScene(SceneIds.AssetPath(sceneId));
                Assert.IsTrue(scene.IsValid(), "Scene did not open: " + sceneId);

                var bootstrap = Object.FindFirstObjectByType<SceneBootstrap>();
                Assert.IsNotNull(bootstrap, "SceneBootstrap missing in " + sceneId);
                Assert.AreEqual(sceneId, bootstrap.SceneId);

                Assert.IsNotNull(Object.FindFirstObjectByType<LevelContentMarker>(),
                    "LevelContentMarker missing in " + sceneId);
                Assert.IsNotNull(Object.FindFirstObjectByType<PlayerController>(),
                    "PlayerController missing in " + sceneId);
                Assert.IsNotNull(Object.FindFirstObjectByType<Camera>(),
                    "Camera missing in " + sceneId);
                Assert.Greater(Object.FindObjectsByType<InteractableBase>(FindObjectsSortMode.None).Length, 0,
                    "No interactables in " + sceneId);
            }
        }
    }
}
