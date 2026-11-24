using UnityEngine;
using UnityEngine.UI;

namespace LWalida
{
    public class CutsceneUI : ScreenBase
    {
        private Image _background;
        private Image _fade;
        private Text _speaker;
        private Text _body;
        private RectTransform _subtitlePanel;
        private RectTransform _photoRow;
        private Image[] _photos = new Image[4];
        private float _fadeTarget;
        private bool _showPhotos;

        protected override void OnBuild()
        {
            _background = UIFactory.CreateImage("Black", transform, Color.black, null, false);
            UIFactory.Stretch(_background.rectTransform);

            _photoRow = UIFactory.CreateRect("MemoryPhotos", transform);
            UIFactory.Anchor(_photoRow, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 130f), new Vector2(1280f, 280f));

            var photosTitle = UIFactory.CreateText("PhotosTitle", _photoRow, "The memories you carry", 24,
                Theme.HassanSand, TextAnchor.UpperCenter, FontStyle.Bold);
            UIFactory.Anchor(photosTitle.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, 0f), new Vector2(0f, 30f));

            for (int i = 0; i < 4; i++)
            {
                var frame = UIFactory.CreatePanel("PhotoFrame_" + i, _photoRow, Theme.ChalkWhite,
                    UIFactory.PanelSprite, false);
                UIFactory.Anchor(frame, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
                    new Vector2(40f + i * 310f, 20f), new Vector2(270f, 200f));

                var image = UIFactory.CreateImage("Photo_" + i, frame, Color.white, null);
                UIFactory.Stretch(image.rectTransform, 10f, 10f, 10f, 10f);
                image.preserveAspect = true;
                _photos[i] = image;
            }
            _photoRow.gameObject.SetActive(false);

            _subtitlePanel = UIFactory.CreatePanel("SubtitlePanel", transform,
                new Color(0.03f, 0.05f, 0.06f, 0.88f), UIFactory.PanelSprite, false);
            UIFactory.Anchor(_subtitlePanel, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 54f), new Vector2(1580f, 260f));

            _speaker = UIFactory.CreateText("Speaker", _subtitlePanel, "", 26, Theme.HassanSand,
                TextAnchor.MiddleLeft, FontStyle.Bold);
            UIFactory.Anchor(_speaker.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f),
                new Vector2(46f, -24f), new Vector2(-92f, 34f));

            _body = UIFactory.CreateText("Body", _subtitlePanel, "", 32, Theme.ChalkWhite, TextAnchor.UpperLeft);
            UIFactory.Anchor(_body.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f),
                new Vector2(46f, -72f), new Vector2(-92f, 148f));
            var shadow = _body.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
            shadow.effectDistance = new Vector2(2f, -2f);

            var continueHint = UIFactory.CreateText("ContinueHint", transform,
                "Space \u2014 continue     \u00b7     Esc \u2014 skip", 20,
                new Color(0.95f, 0.93f, 0.89f, 0.7f), TextAnchor.LowerRight);
            UIFactory.Anchor(continueHint.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(1f, 0f), new Vector2(-48f, 16f), new Vector2(760f, 28f));

            var skipButton = UIFactory.CreateButton(transform, "Skip", () =>
            {
                if (CutsceneController.Active != null) CutsceneController.Active.RequestSkip();
            }, 20, 44f, Theme.MidnightBlue);
            UIFactory.Anchor(skipButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(1f, 1f), new Vector2(-44f, -32f), new Vector2(150f, 44f));

            _fade = UIFactory.CreateImage("CutsceneFade", transform, new Color(0f, 0f, 0f, 0f), null, false);
            UIFactory.Stretch(_fade.rectTransform);

            Hide();
        }

        public override void Show()
        {
            base.Show();
            Root.ClearSelection();
        }

        public void Show(bool blackBackground)
        {
            if (!gameObject.activeSelf) Show();
            _background.color = blackBackground ? Color.black : new Color(0f, 0f, 0f, 0.18f);
            _speaker.text = "";
            _body.text = "";
            _photoRow.gameObject.SetActive(false);
            _showPhotos = false;
            _fadeTarget = 0f;
            _fade.color = new Color(0f, 0f, 0f, 0f);
            if (Root != null) Root.ClearSelection();
        }

        public void ShowLine(CutsceneLine line)
        {
            if (line == null) return;
            if (_speaker != null) _speaker.text = string.IsNullOrEmpty(line.speaker) ? "" : line.speaker.ToUpperInvariant();
            if (_body != null) _body.text = line.text;
            if (_photoRow != null && line.showMemories) ShowMemories();
            if (line.fadeToBlack) _fadeTarget = 0.94f;
            else if (!_showPhotos) _fadeTarget = 0f;
            if (line.showMemories) _showPhotos = true;
            _fade.transform.SetAsLastSibling();
            if (_subtitlePanel != null) _subtitlePanel.SetAsLastSibling();
        }

        private void ShowMemories()
        {
            _photoRow.gameObject.SetActive(true);
            _photoRow.SetAsLastSibling();
            if (_subtitlePanel != null) _subtitlePanel.SetAsLastSibling();
            _fade.transform.SetAsLastSibling();

            var game = GameManager.Instance;
            var catalog = game != null ? game.Content : Content;
            for (int i = 0; i < _photos.Length; i++)
            {
                MemoryData memory = catalog != null ? catalog.GetMemory(MemoryIds.All[i]) : null;
                bool collected = game != null && game.Memories != null && game.Memories.Has(MemoryIds.All[i]);
                if (_photos[i] != null)
                {
                    _photos[i].sprite = memory != null ? memory.image : null;
                    _photos[i].enabled = collected && memory != null && memory.image != null;
                    _photos[i].color = collected ? Color.white : new Color(0.2f, 0.24f, 0.26f, 0.5f);
                }
            }
        }

        private void Update()
        {
            if (_fade == null) return;
            float current = _fade.color.a;
            float next = Mathf.MoveTowards(current, _fadeTarget, Time.unscaledDeltaTime * 0.5f);
            _fade.color = new Color(0f, 0f, 0f, next);
        }

        protected override void OnHide()
        {
            _showPhotos = false;
            _fadeTarget = 0f;
        }
    }
}
