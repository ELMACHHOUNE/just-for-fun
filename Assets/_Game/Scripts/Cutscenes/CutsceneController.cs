using System.Collections;
using UnityEngine;

namespace LWalida
{
    public class CutsceneController : MonoBehaviour
    {
        public static CutsceneController Active { get; private set; }

        public string cutsceneId = "";
        public CutsceneLine[] lines = new CutsceneLine[0];
        public string targetSceneId = "";
        public string completionFlag = "";
        public bool blackBackground;
        public int knockOnLineIndex = -1;

        public bool HasStarted { get; private set; }

        private int _index = -1;
        private float _lineTimer;
        private bool _playing;
        private bool _confirmOpen;
        private bool _finishing;

        private void Awake()
        {
            GameRoot.EnsureExists();
        }

        private void Start()
        {
            StartCoroutine(DelayedPlay());
        }

        private IEnumerator DelayedPlay()
        {
            yield return null;
            if (!HasStarted) Play();
        }

        public void ConfigureForScene(string sceneId)
        {
            cutsceneId = sceneId;
            if (sceneId == SceneIds.Intro)
            {
                lines = NarrativeDefaults.IntroLines();
                targetSceneId = SceneIds.CafeMaure;
                completionFlag = GameFlags.IntroSeen;
                blackBackground = true;
                knockOnLineIndex = -1;
            }
            else if (sceneId == SceneIds.Home)
            {
                lines = NarrativeDefaults.HomeLines();
                targetSceneId = SceneIds.Credits;
                completionFlag = GameFlags.HomeSeen;
                blackBackground = false;
                knockOnLineIndex = 2;
            }
        }

        public void ConfigureIfEmpty()
        {
            if (lines == null || lines.Length == 0)
            {
                if (string.IsNullOrEmpty(cutsceneId))
                {
                    var level = LevelCatalog.Get(SceneIds.Home);
                    cutsceneId = gameObject.scene.name;
                    if (cutsceneId == SceneIds.Intro || cutsceneId == SceneIds.Home)
                    {
                        ConfigureForScene(cutsceneId);
                    }
                    else
                    {
                        cutsceneId = SceneIds.Intro;
                        ConfigureForScene(cutsceneId);
                    }
                }
                else
                {
                    ConfigureForScene(cutsceneId);
                }
            }
        }

        public void Play()
        {
            if (HasStarted) return;
            HasStarted = true;
            Active = this;
            ConfigureIfEmpty();
            var game = GameManager.Instance;
            if (game != null)
            {
                game.SetMode(GameMode.Cutscene);
                if (game.UI != null) game.UI.ShowCutscene(blackBackground);
            }
            _playing = true;
            _index = -1;
            Advance();
        }

        public void Advance()
        {
            if (!_playing || _confirmOpen || _finishing) return;
            _index++;
            if (lines == null || _index >= lines.Length)
            {
                Finish();
                return;
            }

            var line = lines[_index];
            _lineTimer = Mathf.Max(1.5f, line.autoAdvanceSeconds);
            var game = GameManager.Instance;
            if (game != null && game.UI != null) game.UI.ShowCutsceneLine(line);

            if (_index == knockOnLineIndex && game != null && game.Audio != null)
            {
                game.Audio.PlayKnock();
            }
        }

        private void Update()
        {
            if (!_playing || _confirmOpen || _finishing) return;
            if (LWalidaInput.AdvancePressed)
            {
                Advance();
                return;
            }

            _lineTimer -= Time.unscaledDeltaTime;
            if (_lineTimer <= 0f) Advance();
        }

        public void RequestSkip()
        {
            if (!_playing || _confirmOpen || _finishing) return;
            var game = GameManager.Instance;
            if (game == null || game.UI == null)
            {
                Finish();
                return;
            }

            _confirmOpen = true;
            game.UI.ShowConfirm("Skip this scene?", "Skip", "Stay", () =>
            {
                _confirmOpen = false;
                Finish();
            }, () =>
            {
                _confirmOpen = false;
            });
        }

        private void Finish()
        {
            if (_finishing) return;
            _finishing = true;
            _playing = false;
            var game = GameManager.Instance;
            if (game != null)
            {
                if (!string.IsNullOrEmpty(completionFlag)) game.SetFlag(completionFlag);
                if (game.UI != null) game.UI.HideCutscene();
                if (!string.IsNullOrEmpty(targetSceneId)) game.Loader.Load(targetSceneId);
                else game.SetMode(GameMode.Exploring);
            }
        }

        private void OnDestroy()
        {
            if (Active == this) Active = null;
        }
    }
}
