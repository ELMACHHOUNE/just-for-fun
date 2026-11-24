using UnityEngine;
using UnityEngine.UI;

namespace LWalida
{
    public class RecordingUI : ScreenBase
    {
        private Image _fill;
        private Text _timer;
        private Text _status;
        private Text _title;
        private Button _cancel;
        private RectTransform _barsRoot;
        private readonly Image[] _bars = new Image[11];
        private bool _complete;

        protected override void OnBuild()
        {
            var dim = UIFactory.CreateImage("Dim", transform, new Color(0.01f, 0.03f, 0.04f, 0.46f), null, true);
            UIFactory.Stretch(dim.rectTransform);

            var panel = UIFactory.CreatePanel("RecorderPanel", transform, Theme.Panel, UIFactory.PanelSprite, true);
            UIFactory.PlaceCenter(panel, 760f, 340f);

            _title = UIFactory.CreateText("Title", panel, "Recording", 34, Theme.HassanSand, TextAnchor.MiddleCenter,
                FontStyle.Bold);
            UIFactory.Anchor(_title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, -26f), new Vector2(680f, 44f));

            if (Content != null && Content.iconRecord != null)
            {
                var icon = UIFactory.CreateImage("RecordIcon", panel, new Color(1f, 1f, 1f, 0.9f),
                    Content.iconRecord);
                UIFactory.Anchor(icon.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(26f, -24f), new Vector2(48f, 48f));
                icon.preserveAspect = true;
            }

            _barsRoot = UIFactory.CreateRect("Waveform", panel);
            UIFactory.Anchor(_barsRoot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -88f), new Vector2(620f, 104f));
            int count = _bars.Length;
            float spacing = 620f / count;
            for (int i = 0; i < count; i++)
            {
                var bar = UIFactory.CreateImage("Bar_" + i, _barsRoot, Theme.HassanSand);
                UIFactory.Anchor(bar.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                    new Vector2(0.5f, 0.5f), new Vector2(spacing * i + spacing * 0.5f, 0f),
                    new Vector2(16f, 20f));
                _bars[i] = bar;
            }

            var progressBackground = UIFactory.CreateImage("ProgressBackground", panel,
                new Color(0.02f, 0.05f, 0.06f, 0.9f));
            UIFactory.Anchor(progressBackground.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f), new Vector2(0f, 96f), new Vector2(620f, 14f));

            var fillRect = UIFactory.CreateImage("ProgressFill", progressBackground.transform, Theme.HassanSand);
            UIFactory.Stretch(fillRect.rectTransform);
            fillRect.type = Image.Type.Filled;
            fillRect.fillMethod = Image.FillMethod.Horizontal;
            fillRect.fillOrigin = 0;
            fillRect.fillAmount = 0f;
            _fill = fillRect;

            _timer = UIFactory.CreateText("Timer", panel, "0.0 / 3.0s", 21, Theme.TextDim, TextAnchor.MiddleCenter);
            UIFactory.Anchor(_timer.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f), new Vector2(0f, 52f), new Vector2(620f, 30f));

            _status = UIFactory.CreateText("Status", panel, "Keep still. Listen to the street.", 21, Theme.TextDim,
                TextAnchor.MiddleCenter, FontStyle.Italic);
            UIFactory.Anchor(_status.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f), new Vector2(0f, 16f), new Vector2(660f, 28f));

            _cancel = UIFactory.CreateButton(panel, "Cancel", () =>
            {
                if (GameManager.Instance != null && GameManager.Instance.Recorder != null)
                {
                    GameManager.Instance.Recorder.Cancel();
                }
            }, 22, 46f, Theme.MidnightBlue);
            UIFactory.Anchor(_cancel.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(1f, 1f), new Vector2(-20f, -18f), new Vector2(170f, 44f));

            Hide();
        }

        public override void Show()
        {
            base.Show();
            _complete = false;
            if (_title != null) _title.text = "Recording";
            if (_status != null) _status.text = "Keep still. Listen to the street.";
            if (_timer != null) _timer.text = "0.0 / 3.0s";
            if (_fill != null) _fill.fillAmount = 0f;
            if (_cancel != null) _cancel.gameObject.SetActive(true);
            if (Root != null) Root.ClearSelection();
        }

        public void SetProgress(float progress)
        {
            if (_fill != null) _fill.fillAmount = Mathf.Clamp01(progress);
            if (_timer != null) _timer.text = (progress * SoundRecorder.RecordDuration).ToString("0.0") + " / 3.0s";
        }

        public void ShowComplete(string message)
        {
            _complete = true;
            if (_title != null) _title.text = "Recorded";
            if (_status != null) _status.text = message;
            if (_fill != null) _fill.fillAmount = 1f;
            if (_timer != null) _timer.text = "3.0 / 3.0s";
            if (_cancel != null) _cancel.gameObject.SetActive(false);
            if (Root != null) Root.ClearSelection();
        }

        private void Update()
        {
            var game = GameManager.Instance;
            if (game == null || game.Mode != GameMode.Recording) return;

            float t = Time.unscaledTime;
            for (int i = 0; i < _bars.Length; i++)
            {
                if (_bars[i] == null) continue;
                float wave = Mathf.Abs(Mathf.Sin(t * (3.1f + i * 0.37f) + i * 0.9f));
                float height = _complete ? 18f : Mathf.Lerp(12f, 84f, wave);
                var size = _bars[i].rectTransform.sizeDelta;
                size.y = Mathf.Lerp(size.y, height, Time.unscaledDeltaTime * 14f);
                _bars[i].rectTransform.sizeDelta = size;
                _bars[i].color = _complete ? Theme.Positive : Theme.HassanSand;
            }
        }
    }
}
