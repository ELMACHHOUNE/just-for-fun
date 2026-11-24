using UnityEngine;
using UnityEngine.UI;

namespace LWalida
{
    public class CreditsUI : ScreenBase
    {
        private Button _return;

        protected override void OnBuild()
        {
            var background = UIFactory.CreateImage("Background", transform, new Color(0.043f, 0.075f, 0.106f, 1f));
            UIFactory.Stretch(background.rectTransform);

            var band = UIFactory.CreatePanel("Band", transform, new Color(0.09f, 0.22f, 0.27f, 0.8f),
                UIFactory.PanelSprite, false);
            UIFactory.Anchor(band, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, 0f), new Vector2(0f, 420f));

            var title = UIFactory.CreateText("Title", transform, "L'WALIDA", 76, Theme.ChalkWhite,
                TextAnchor.MiddleCenter, FontStyle.Bold);
            UIFactory.Anchor(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, -64f), new Vector2(1000f, 92f));

            if (Content != null && Content.titleArabic != null)
            {
                var arabic = UIFactory.CreateImage("ArabicTitle", transform, Color.white, Content.titleArabic);
                UIFactory.Anchor(arabic.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                    new Vector2(0.5f, 1f), new Vector2(0f, -162f), new Vector2(280f, 96f));
                arabic.preserveAspect = true;
            }

            var tagline = UIFactory.CreateText("Tagline", transform, "Every street holds a memory.", 26,
                Theme.HassanSand, TextAnchor.MiddleCenter, FontStyle.Italic);
            UIFactory.Anchor(tagline.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, -272f), new Vector2(900f, 34f));

            var closing = UIFactory.CreateText("Closing", transform,
                "Some people leave us.\nBut the places they loved never forget them.", 30, Theme.ChalkWhite,
                TextAnchor.MiddleCenter);
            UIFactory.Anchor(closing.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, -326f), new Vector2(1100f, 84f));

            var body = UIFactory.CreateText("CreditsBody", transform,
                "<b>L'WALIDA \u2014 The Mother</b>\n" +
                "A short narrative game about memory, family and the streets of Rabat.\n" +
                "Made for the <i>Lost in the Medina</i> hackathon.\n\n" +
                "<b>TEAM</b>\n" +
                "Game Design / Narrative \u00b7 [Team member]\n" +
                "Unity Development \u00b7 [Team member]\n" +
                "Art Direction \u00b7 [Team member]\n" +
                "Sound Design \u00b7 [Team member]\n\n" +
                "<b>ASSETS</b>\n" +
                "Environment references supplied by the project owner.\n" +
                "Character, prop, door, portrait and UI art in this MVP are procedurally generated placeholders.\n" +
                "Music, ambience and sound effects are procedurally generated placeholder audio.\n" +
                "No voice recordings were generated.\n" +
                "UI type uses Unity's built-in runtime font.\n\n" +
                "<b>CONTENT NOTICE</b>\n" +
                "This story contains themes of grief and bereavement. It is not a medical depiction.\n\n" +
                "Thank you for walking these streets.",
                18, Theme.ChalkWhite, TextAnchor.UpperCenter);
            UIFactory.Anchor(body.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, -425f), new Vector2(1400f, 550f));

            _return = UIFactory.CreateButton(transform, "Return to main menu", () =>
            {
                if (Root != null) Root.ReturnFromCredits();
            }, 25, 58f, Theme.OudayaBlue);
            UIFactory.Anchor(_return.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f), new Vector2(0f, 42f), new Vector2(360f, 58f));

            Hide();
        }

        public override void Show()
        {
            base.Show();
            if (Root != null) Root.Select(_return);
        }
    }
}
