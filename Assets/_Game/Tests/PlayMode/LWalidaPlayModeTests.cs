using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace LWalida.Tests
{
    public class PlayModeSmokeTests
    {
        [UnityTest]
        public IEnumerator MainMenuBootsPersistentGameRootAndUI()
        {
            SceneManager.LoadScene(SceneIds.MainMenu);
            yield return null;
            yield return null;

            Assert.IsNotNull(GameRoot.Instance, "GameRoot was not created by the main menu scene.");
            Assert.IsNotNull(GameManager.Instance, "GameManager was not initialized.");
            Assert.IsNotNull(GameManager.Instance.Content, "AssetCatalog was not loaded from Resources.");
            Assert.IsNotNull(GameManager.Instance.UI, "UIRoot was not built.");
            Assert.AreEqual(GameMode.Menu, GameManager.Instance.Mode);
            Assert.IsTrue(GameManager.Instance.UI.MainMenuUI.IsVisible, "Main menu is not visible.");
            Assert.IsTrue(Time.timeScale > 0f, "Main menu left time scale paused.");
        }
    }
}
