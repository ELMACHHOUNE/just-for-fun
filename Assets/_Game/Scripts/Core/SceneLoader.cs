using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LWalida
{
    public class SceneLoader : MonoBehaviour
    {
        private bool _loading;

        public bool IsLoading => _loading;

        public void Load(string sceneId, float fadeOut = 0.4f, float fadeIn = 0.55f)
        {
            if (_loading) return;
            if (string.IsNullOrEmpty(sceneId))
            {
                Debug.LogWarning("[LWalida] SceneLoader.Load called with an empty scene id.");
                return;
            }
            StartCoroutine(LoadRoutine(sceneId, fadeOut, fadeIn));
        }

        private IEnumerator LoadRoutine(string sceneId, float fadeOut, float fadeIn)
        {
            _loading = true;
            var game = GameManager.Instance;
            var ui = game != null ? game.UI : null;
            GameMode previousMode = game != null ? game.Mode : GameMode.Exploring;
            if (game != null) game.SetMode(GameMode.Transition);

            if (ui != null) yield return ui.FadeOut(fadeOut);

            Time.timeScale = 1f;
            if (!Application.CanStreamedLevelBeLoaded(sceneId))
            {
                Debug.LogError("[LWalida] Scene '" + sceneId +
                               "' is not in the build settings. Run LWalida/Build or Update MVP Scenes.");
                if (game != null)
                {
                    game.Notify("Scene is not registered in Build Settings. Re-run the LWalida scene builder.", 6f);
                    game.SetMode(previousMode);
                }
                if (ui != null) yield return ui.FadeIn(fadeIn);
                _loading = false;
                yield break;
            }

            AsyncOperation operation = null;
            try
            {
                operation = SceneManager.LoadSceneAsync(sceneId, LoadSceneMode.Single);
            }
            catch (System.Exception exception)
            {
                Debug.LogError("[LWalida] Failed to load scene '" + sceneId + "': " + exception.Message);
            }

            if (operation == null)
            {
                if (game != null) game.SetMode(previousMode);
                if (ui != null) yield return ui.FadeIn(fadeIn);
                _loading = false;
                yield break;
            }

            while (!operation.isDone) yield return null;

            // Let the new scene's SceneBootstrap start and switch mode before revealing it.
            yield return null;
            if (ui != null) yield return ui.FadeIn(fadeIn);
            _loading = false;
        }
    }
}
