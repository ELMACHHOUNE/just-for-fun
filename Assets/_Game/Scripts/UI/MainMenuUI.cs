using UnityEngine;
using UnityEngine.UI;

namespace LWalida
{
    public class MainMenuUI : ScreenBase
    {
        private Button _newJourney;
        private Button _continue;
        private Text _continueLabel;
        private Image _background;

        protected override void OnBuild()
        {
            _background = UIFactory.CreateImage("Background", transform, Color.white, null);
            UIFactory.Stretch(_background.rectTransform);
            _background.color = Theme.MidnightBlue;

            var leftShade = UIFactory.CreateImage("LeftShade", transform, new Color(0.055f, 0.11f, 0.14f, 0.9f));
            UIFactory.Anchor(leftShade.rectTransform, new Vector2(0f, 0f), new Vector2(0.52f, 1f), new Vector2(0f, 0f),
                Vector2.zero, Vector2.zero);

            var bottomShade = UIFactory.CreateImage("BottomShade", transform, new Color(0.03f, 0.06f, 0.07f, 0.55f));
            UIFactory.Anchor(bottomShade.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f),
                Vector2.zero, new Vector2(0f, 220f));

            var emblemRect = UIFactory.CreateRect("Emblem", transform);
            UIFactory.PlaceLeft(emblemRect, 112f, 96f, 150f, 150f);
            if (Content != null && Content.logoEmblem != null)
            {
                var emblem = UIFactory.CreateImage("EmblemImage", emblemRect, Color.white, Content.logoEmblem);
                UIFactory.Stretch(emblem.rectTransform);
                emblem.preserveAspect = true;
            }

            var title = UIFactory.CreateText("Title", transform, "L'WALIDA", 88, Theme.ChalkWhite,
                TextAnchor.MiddleLeft, FontStyle.Bold);
            UIFactory.PlaceLeft(title.rectTransform, 288f, 96f, 700f, 96f);
            var shadow = title.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.55f);
            shadow.effectDistance = new Vector2(3f, -3f);

            if (Content != null && Content.titleArabic != null)
            {
                var arabic = UIFactory.CreateImage("ArabicTitle", transform, Color.white, Content.titleArabic);
                UIFactory.PlaceLeft(arabic.rectTransform, 290f, 188f, 380f, 120f);
                arabic.preserveAspect = true;
            }
            else
            {
                var arabic = UIFactory.CreateText("ArabicTitle", transform, "\u0627\u0644\u0648\u0627\u0644\u062f\u0629",
                    46, Theme.HassanSand, TextAnchor.MiddleLeft);
                UIFactory.PlaceLeft(arabic.rectTransform, 292f, 196f, 400f, 64f);
            }

            var tagline = UIFactory.CreateText("Tagline", transform, "Every street holds a memory.", 26,
                Theme.HassanSand, TextAnchor.MiddleLeft, FontStyle.Italic);
            UIFactory.PlaceLeft(tagline.rectTransform, 294f, 324f, 700f, 40f);

            var location = UIFactory.CreateText("Location", transform, "RABAT  \u00b7  A STORY ABOUT REMEMBERING", 18,
                Theme.TextDim, TextAnchor.MiddleLeft);
            UIFactory.PlaceLeft(location.rectTransform, 296f, 364f, 700f, 28f);

            var container = UIFactory.CreateRect("MenuButtons", transform);
            UIFactory.PlaceLeft(container, 112f, 448f, 430f, 420f);
            var layout = container.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            _newJourney = UIFactory.CreateButton(container, "New Journey", OnNewJourney);
            _continue = UIFactory.CreateButton(container, "Continue", OnContinue);
            _continueLabel = _continue.GetComponentInChildren<Text>();
            UIFactory.CreateButton(container, "Settings", () => Root.SettingsUI.Open());
            UIFactory.CreateButton(container, "Credits", () => Root.ShowCredits());
            UIFactory.CreateButton(container, "Exit", OnExit, 28, 62f, new Color(0.28f, 0.19f, 0.16f, 1f));

            var notice = UIFactory.CreateText("ContentNotice", transform,
                "A story about family, memory and grief.\nContains themes of loss and bereavement.",
                18, Theme.TextDim, TextAnchor.LowerLeft);
            UIFactory.Anchor(notice.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(114f, 52f), new Vector2(820f, 74f));

            var version = UIFactory.CreateText("Version", transform, "MVP build \u00b7 Unity 6", 16,
                new Color(0.95f, 0.93f, 0.89f, 0.45f), TextAnchor.LowerRight);
            UIFactory.Anchor(version.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-48f, 24f), new Vector2(520f, 26f));

            Hide();
        }

        public override void Show()
        {
            base.Show();
            Refresh();
            if (Root != null) Root.Select(_continue != null && _continue.interactable ? _continue : _newJourney);
        }

        public void Refresh()
        {
            var game = GameManager.Instance;
            bool hasSave = game != null && game.Saves != null && game.Saves.HasValidSave();
            if (_continue != null)
            {
                _continue.interactable = hasSave;
                if (_continueLabel != null)
                {
                    _continueLabel.color = hasSave ? Theme.ChalkWhite : new Color(1f, 1f, 1f, 0.4f);
                }
            }

            if (_background != null && Content != null)
            {
                Sprite sprite = Content.menuBackground != null ? Content.menuBackground : Content.envOudaya;
                if (sprite != null)
                {
                    _background.sprite = sprite;
                    _background.color = Color.white;
                    _background.preserveAspect = false;
                }
            }
        }

        private void OnNewJourney()
        {
            var game = GameManager.Instance;
            if (game == null) return;
            bool hasSave = game.Saves != null && game.Saves.HasValidSave();
            if (hasSave)
            {
                Root.ShowConfirm("Start a new journey? Your saved journey will be replaced.", "Begin", "Cancel",
                    () => game.NewJourney(), null);
            }
            else
            {
                game.NewJourney();
            }
        }

        private void OnContinue()
        {
            var game = GameManager.Instance;
            if (game == null) return;
            game.ContinueJourney();
        }

        private void OnExit()
        {
            Root.ShowConfirm("Leave L'Walida?", "Quit", "Stay", Quit, null);
        }

        private static void Quit()
        {
#if UNITY_EDITOR
            Debug.Log("[LWalida] Quit requested. Stop Play mode in the editor.");
#else
            Application.Quit();
#endif
        }

        public void RequestBack()
        {
            OnExit();
        }
    }
}
