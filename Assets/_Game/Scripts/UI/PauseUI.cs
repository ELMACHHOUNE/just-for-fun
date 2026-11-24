using UnityEngine;
using UnityEngine.UI;

namespace LWalida
{
    public class PauseUI : ScreenBase
    {
        private Button _resume;

        protected override void OnBuild()
        {
            var dim = UIFactory.CreateImage("Dim", transform, Theme.Overlay, null, true);
            UIFactory.Stretch(dim.rectTransform);

            var panel = UIFactory.CreatePanel("Panel", transform, Theme.Panel, UIFactory.PanelSprite, true);
            UIFactory.PlaceCenter(panel, 560f, 620f);

            var title = UIFactory.CreateText("Title", panel, "Paused", 46, Theme.ChalkWhite, TextAnchor.MiddleCenter,
                FontStyle.Bold);
            UIFactory.Anchor(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, -38f), new Vector2(480f, 58f));

            var subtitle = UIFactory.CreateText("Subtitle", panel, "Every street can wait a moment.", 20,
                Theme.TextDim, TextAnchor.MiddleCenter, FontStyle.Italic);
            UIFactory.Anchor(subtitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, -96f), new Vector2(480f, 28f));

            var container = UIFactory.CreateRect("Buttons", panel);
            container.anchorMin = new Vector2(0.5f, 0f);
            container.anchorMax = new Vector2(0.5f, 1f);
            container.pivot = new Vector2(0.5f, 0.5f);
            container.offsetMin = new Vector2(-210f, 34f);
            container.offsetMax = new Vector2(210f, -150f);
            var layout = container.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 14f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            _resume = UIFactory.CreateButton(container, "Resume", () =>
            {
                if (GameManager.Instance != null) GameManager.Instance.ResumeGame();
            });
            UIFactory.CreateButton(container, "Journal", () =>
            {
                if (GameManager.Instance != null) GameManager.Instance.OpenJournal(true);
            });
            UIFactory.CreateButton(container, "Settings", () => Root.SettingsUI.Open());
            UIFactory.CreateButton(container, "Restart Journey", () =>
            {
                Root.ShowConfirm("Restart the journey? Your memories will be cleared.", "Restart", "Cancel",
                    () =>
                    {
                        if (GameManager.Instance != null) GameManager.Instance.RestartJourney();
                    }, null);
            });
            UIFactory.CreateButton(container, "Main Menu", () =>
            {
                Root.ShowConfirm("Return to the main menu? Your journey is saved at this scene.", "Main Menu", "Cancel",
                    () =>
                    {
                        if (GameManager.Instance != null) GameManager.Instance.ReturnToMainMenu();
                    }, null);
            });

            Hide();
        }

        public override void Show()
        {
            base.Show();
            if (Root != null) Root.Select(_resume);
        }
    }
}
