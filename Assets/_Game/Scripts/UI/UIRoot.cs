using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace LWalida
{
    public abstract class ScreenBase : MonoBehaviour
    {
        protected UIRoot Root;
        protected AssetCatalog Content;

        public bool IsVisible => gameObject.activeSelf;

        public void Build(UIRoot root, AssetCatalog content)
        {
            Root = root;
            Content = content;
            OnBuild();
        }

        protected abstract void OnBuild();
        public virtual void Show() { gameObject.SetActive(true); OnShow(); }
        public virtual void Hide() { OnHide(); gameObject.SetActive(false); }
        protected virtual void OnShow() { }
        protected virtual void OnHide() { }

        protected static RectTransform FullScreen(string name, Transform parent)
        {
            var rect = UIFactory.CreateRect(name, parent);
            UIFactory.Stretch(rect);
            return rect;
        }
    }

    public class UIRoot : MonoBehaviour
    {
        public HUD HUD { get; private set; }
        public DialogueUI DialogueUI { get; private set; }
        public JournalUI Journal { get; private set; }
        public CameraUI CameraUI { get; private set; }
        public RecordingUI RecordingUI { get; private set; }
        public PauseUI PauseUI { get; private set; }
        public MainMenuUI MainMenuUI { get; private set; }
        public SettingsUI SettingsUI { get; private set; }
        public CreditsUI CreditsUI { get; private set; }
        public CutsceneUI CutsceneUI { get; private set; }
        public ConfirmDialog ConfirmUI { get; private set; }

        public Canvas Canvas { get; private set; }
        public Image FadeImage { get; private set; }
        public Image TintImage { get; private set; }

        private RectTransform _screenLayer;
        private RectTransform _notificationRoot;
        private Text _notificationText;
        private CanvasGroup _notificationGroup;
        private Coroutine _notificationRoutine;
        private Coroutine _fadeRoutine;
        private float _fadeTarget;
        private bool _built;

        public void Build(AssetCatalog content)
        {
            if (_built) return;
            _built = true;
            UIFactory.Initialize(content);

            var canvasGo = new GameObject("LWalidaCanvas", typeof(RectTransform));
            canvasGo.transform.SetParent(transform, false);
            Canvas = canvasGo.AddComponent<Canvas>();
            Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            Canvas.sortingOrder = 40;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            EnsureEventSystem();

            var tint = UIFactory.CreateImage("SceneTint", canvasGo.transform, new Color(1f, 1f, 1f, 0f));
            UIFactory.Stretch(tint.rectTransform);
            TintImage = tint;

            if (UIFactory.GrainSprite != null)
            {
                var grain = UIFactory.CreateImage("PaperGrain", canvasGo.transform, new Color(1f, 1f, 1f, 0.075f),
                    UIFactory.GrainSprite);
                UIFactory.Stretch(grain.rectTransform);
            }

            if (UIFactory.VignetteSprite != null)
            {
                var vignette = UIFactory.CreateImage("Vignette", canvasGo.transform, new Color(1f, 1f, 1f, 0.5f),
                    UIFactory.VignetteSprite);
                UIFactory.Stretch(vignette.rectTransform);
            }

            _screenLayer = UIFactory.CreateRect("Screens", canvasGo.transform);
            UIFactory.Stretch(_screenLayer);

            MainMenuUI = CreateScreen<MainMenuUI>("MainMenu");
            CreditsUI = CreateScreen<CreditsUI>("Credits");
            HUD = CreateScreen<HUD>("HUD");
            DialogueUI = CreateScreen<DialogueUI>("Dialogue");
            Journal = CreateScreen<JournalUI>("Journal");
            CameraUI = CreateScreen<CameraUI>("CameraUI");
            RecordingUI = CreateScreen<RecordingUI>("RecordingUI");
            PauseUI = CreateScreen<PauseUI>("Pause");
            CutsceneUI = CreateScreen<CutsceneUI>("Cutscene");
            SettingsUI = CreateScreen<SettingsUI>("Settings");
            ConfirmUI = CreateScreen<ConfirmDialog>("Confirm");

            var notification = UIFactory.CreatePanel("Notification", _screenLayer, Theme.Panel,
                UIFactory.PanelSprite, false);
            UIFactory.Anchor(notification, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -54f), new Vector2(980f, 72f));
            _notificationRoot = notification;
            _notificationGroup = UIFactory.AddGroup(notification.gameObject, 0f);
            _notificationGroup.blocksRaycasts = false;
            _notificationText = UIFactory.CreateText("Text", notification, "", 27, Theme.ChalkWhite,
                TextAnchor.MiddleCenter);
            UIFactory.Stretch(_notificationText.rectTransform, 24f, 8f, 24f, 8f);
            notification.gameObject.SetActive(false);

            var fade = UIFactory.CreateImage("Fade", canvasGo.transform, Color.black, null, true);
            UIFactory.Stretch(fade.rectTransform);
            FadeImage = fade;

            HideAllModals();
            if (HUD != null) HUD.SetVisible(false);

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            SettingsService.ApplyScreenSettings();
        }

        private T CreateScreen<T>(string name) where T : ScreenBase
        {
            var rect = UIFactory.CreateRect(name, _screenLayer);
            UIFactory.Stretch(rect);
            var screen = rect.gameObject.AddComponent<T>();
            screen.Build(this, GameRoot.Instance != null ? GameRoot.Instance.Content : null);
            screen.Hide();
            return screen;
        }

        private void EnsureEventSystem()
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null) eventSystem = FindFirstObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                var go = new GameObject("EventSystem");
                go.transform.SetParent(transform, false);
                eventSystem = go.AddComponent<EventSystem>();
            }

            var module = eventSystem.GetComponent<InputSystemUIInputModule>();
            if (module == null)
            {
                var legacy = eventSystem.GetComponent<StandaloneInputModule>();
                if (legacy != null) Destroy(legacy);
                module = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            }
            if (module.actionsAsset == null)
            {
                module.AssignDefaultActions();
            }
        }

        private void Start()
        {
            StartFade(0f, 0.9f);
        }

        public void Select(Selectable selectable)
        {
            if (EventSystem.current == null || selectable == null) return;
            if (!selectable.IsInteractable() || !selectable.gameObject.activeInHierarchy) return;
            EventSystem.current.SetSelectedGameObject(selectable.gameObject);
        }

        public void ClearSelection()
        {
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        }

        public void OnModeChanged(GameMode mode)
        {
            if (!_built) return;

            if (mode != GameMode.Menu) MainMenuUI.Hide();
            if (mode != GameMode.Paused) PauseUI.Hide();
            if (mode != GameMode.Dialogue) DialogueUI.Hide();
            if (mode != GameMode.Journal) Journal.Hide();
            if (mode != GameMode.Camera) CameraUI.Hide();
            if (mode != GameMode.Recording) RecordingUI.Hide();
            if (mode != GameMode.Cutscene) CutsceneUI.Hide();
            if (mode != GameMode.Credits) CreditsUI.Hide();
            SettingsUI.Hide();
            ConfirmUI.Hide();

            HUD.SetVisible(mode == GameMode.Exploring);

            switch (mode)
            {
                case GameMode.Menu:
                    MainMenuUI.Show();
                    break;
                case GameMode.Paused:
                    PauseUI.Show();
                    break;
                case GameMode.Credits:
                    CreditsUI.Show();
                    break;
            }
        }

        public bool TryHandleBack()
        {
            if (ConfirmUI != null && ConfirmUI.IsVisible)
            {
                ConfirmUI.Cancel();
                return true;
            }
            if (SettingsUI != null && SettingsUI.IsVisible)
            {
                SettingsUI.Close();
                return true;
            }
            if (CreditsUI != null && CreditsUI.IsVisible)
            {
                ReturnFromCredits();
                return true;
            }
            if (MainMenuUI != null && MainMenuUI.IsVisible)
            {
                MainMenuUI.RequestBack();
                return true;
            }
            return false;
        }

        public void HideAllModals()
        {
            if (DialogueUI != null) DialogueUI.Hide();
            if (Journal != null) Journal.Hide();
            if (CameraUI != null) CameraUI.Hide();
            if (RecordingUI != null) RecordingUI.Hide();
            if (PauseUI != null) PauseUI.Hide();
            if (SettingsUI != null) SettingsUI.Hide();
            if (CutsceneUI != null) CutsceneUI.Hide();
            if (CreditsUI != null) CreditsUI.Hide();
            if (ConfirmUI != null) ConfirmUI.Hide();
        }

        public void SetSceneTint(Color color)
        {
            if (TintImage != null) TintImage.color = color;
        }

        public void ShowDialogue()
        {
            if (DialogueUI != null) DialogueUI.Show();
        }

        public void HideDialogue()
        {
            if (DialogueUI != null) DialogueUI.Hide();
        }

        public void ShowJournal()
        {
            if (Journal != null) Journal.Show();
        }

        public void HideJournal()
        {
            if (Journal != null) Journal.Hide();
        }

        public void ShowCamera(PhotoDoor door, Sprite reference)
        {
            if (CameraUI != null) CameraUI.Show(door, reference);
        }

        public void SetCameraFeedback(string text, bool positive)
        {
            if (CameraUI != null) CameraUI.SetFeedback(text, positive);
        }

        public void HideCamera()
        {
            if (CameraUI != null) CameraUI.Hide();
        }

        public void ShowRecording()
        {
            if (RecordingUI != null) RecordingUI.Show();
        }

        public void SetRecordingProgress(float progress)
        {
            if (RecordingUI != null) RecordingUI.SetProgress(progress);
        }

        public void ShowRecordingComplete(string message)
        {
            if (RecordingUI != null) RecordingUI.ShowComplete(message);
        }

        public void HideRecording()
        {
            if (RecordingUI != null) RecordingUI.Hide();
        }

        public void ShowCutscene(bool blackBackground)
        {
            if (CutsceneUI != null) CutsceneUI.Show(blackBackground);
        }

        public void ShowCutsceneLine(CutsceneLine line)
        {
            if (CutsceneUI != null) CutsceneUI.ShowLine(line);
        }

        public void HideCutscene()
        {
            if (CutsceneUI != null) CutsceneUI.Hide();
        }

        public void ShowCredits()
        {
            if (MainMenuUI != null) MainMenuUI.Hide();
            if (CreditsUI != null) CreditsUI.Show();
        }

        public void ReturnFromCredits()
        {
            if (GameManager.Instance != null && GameManager.Instance.Mode == GameMode.Credits)
            {
                GameManager.Instance.Loader.Load(SceneIds.MainMenu);
                return;
            }
            if (CreditsUI != null) CreditsUI.Hide();
            if (MainMenuUI != null) MainMenuUI.Show();
        }

        public void ShowConfirm(string message, string yesLabel, string noLabel, Action onYes, Action onNo)
        {
            if (ConfirmUI != null) ConfirmUI.Show(message, yesLabel, noLabel, onYes, onNo);
        }

        public void StopNotification()
        {
            if (_notificationRoutine != null)
            {
                StopCoroutine(_notificationRoutine);
                _notificationRoutine = null;
            }
            if (_notificationRoot != null) _notificationRoot.gameObject.SetActive(false);
        }

        public void ShowNotification(string message, float duration = 3.5f)
        {
            if (!_built || _notificationRoot == null) return;
            if (_notificationRoutine != null) StopCoroutine(_notificationRoutine);
            _notificationRoot.gameObject.SetActive(true);
            _notificationRoot.SetAsLastSibling();
            if (FadeImage != null) FadeImage.transform.SetAsLastSibling();
            _notificationText.text = message;
            _notificationRoutine = StartCoroutine(NotificationRoutine(duration));
        }

        private IEnumerator NotificationRoutine(float duration)
        {
            _notificationGroup.alpha = 0f;
            float t = 0f;
            while (t < 0.2f)
            {
                t += Time.unscaledDeltaTime;
                _notificationGroup.alpha = Mathf.Clamp01(t / 0.2f);
                yield return null;
            }
            _notificationGroup.alpha = 1f;
            yield return new WaitForSecondsRealtime(duration);
            t = 0f;
            while (t < 0.4f)
            {
                t += Time.unscaledDeltaTime;
                _notificationGroup.alpha = 1f - Mathf.Clamp01(t / 0.4f);
                yield return null;
            }
            _notificationRoot.gameObject.SetActive(false);
            _notificationRoutine = null;
        }

        public IEnumerator FadeOut(float duration)
        {
            StartFade(1f, duration);
            while (FadeImage != null && FadeImage.color.a < 0.995f) yield return null;
        }

        public IEnumerator FadeIn(float duration)
        {
            StartFade(0f, duration);
            while (FadeImage != null && FadeImage.color.a > 0.005f) yield return null;
        }

        private void StartFade(float target, float duration)
        {
            _fadeTarget = target;
            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(FadeRoutine(target, duration));
        }

        private IEnumerator FadeRoutine(float target, float duration)
        {
            if (FadeImage == null) yield break;
            var color = FadeImage.color;
            float start = color.a;
            if (duration <= 0.01f)
            {
                color.a = target;
                FadeImage.color = color;
                FadeImage.raycastTarget = target > 0.01f;
                yield break;
            }
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                color.a = Mathf.Lerp(start, target, Mathf.Clamp01(t / duration));
                FadeImage.color = color;
                FadeImage.raycastTarget = color.a > 0.01f;
                yield return null;
            }
            color.a = target;
            FadeImage.color = color;
            FadeImage.raycastTarget = target > 0.01f;
            _fadeRoutine = null;
        }
    }
}
