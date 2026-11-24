using UnityEngine;

namespace LWalida
{
    public class LevelContentMarker : MonoBehaviour
    {
        public string sceneId;
    }

    public class SceneBootstrap : MonoBehaviour
    {
        [SerializeField] private string sceneId = SceneIds.CafeMaure;

        public string SceneId => sceneId;

        public void SetSceneId(string id)
        {
            sceneId = id;
        }

        private void Awake()
        {
            GameRoot.EnsureExists();
        }

        private void Start()
        {
            var root = GameRoot.EnsureExists();
            var level = LevelCatalog.Get(sceneId);

            var marker = Object.FindFirstObjectByType<LevelContentMarker>();
            if (marker == null && !level.IsMenu && !level.IsCredits && !level.IsCutscene)
            {
                LevelComposer.Compose(sceneId, root.Content, null);
            }
            else if (marker == null && sceneId == SceneIds.Home)
            {
                LevelComposer.Compose(sceneId, root.Content, null);
            }

            root.Game.EnterScene(this);

            if (level.IsCutscene)
            {
                var cutscene = Object.FindFirstObjectByType<CutsceneController>();
                if (cutscene == null)
                {
                    cutscene = gameObject.AddComponent<CutsceneController>();
                    cutscene.ConfigureForScene(sceneId);
                }
                else if (!cutscene.HasStarted)
                {
                    cutscene.ConfigureIfEmpty();
                }
            }

            if (level.IsCredits && root.UI != null)
            {
                root.UI.ShowCredits();
            }
        }
    }
}
