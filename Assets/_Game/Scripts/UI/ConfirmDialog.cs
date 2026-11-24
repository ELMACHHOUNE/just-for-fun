using System;
using UnityEngine;
using UnityEngine.UI;

namespace LWalida
{
    public class ConfirmDialog : ScreenBase
    {
        private Text _message;
        private Text _yesLabel;
        private Text _noLabel;
        private Button _yesButton;
        private Button _noButton;
        private Action _onYes;
        private Action _onNo;

        protected override void OnBuild()
        {
            var dim = UIFactory.CreateImage("Dim", transform, new Color(0.01f, 0.02f, 0.03f, 0.66f), null, true);
            UIFactory.Stretch(dim.rectTransform);

            var panel = UIFactory.CreatePanel("Panel", transform, Theme.Panel, UIFactory.PanelSprite, true);
            UIFactory.PlaceCenter(panel, 740f, 340f);

            var title = UIFactory.CreateText("Title", panel, "Are you sure?", 30, Theme.HassanSand,
                TextAnchor.MiddleCenter, FontStyle.Bold);
            UIFactory.Anchor(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, -28f), new Vector2(660f, 40f));

            _message = UIFactory.CreateText("Message", panel, "", 25, Theme.ChalkWhite, TextAnchor.MiddleCenter);
            UIFactory.Anchor(_message.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, 22f), new Vector2(620f, 120f));

            _noButton = UIFactory.CreateButton(panel, "Cancel", () => Cancel(), 25, 56f, Theme.MidnightBlue);
            UIFactory.Anchor(_noButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f), new Vector2(-160f, 44f), new Vector2(260f, 56f));
            _noLabel = _noButton.GetComponentInChildren<Text>();

            _yesButton = UIFactory.CreateButton(panel, "Confirm", () => Confirm(), 25, 56f, Theme.Terracotta);
            UIFactory.Anchor(_yesButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f), new Vector2(160f, 44f), new Vector2(260f, 56f));
            _yesLabel = _yesButton.GetComponentInChildren<Text>();

            Hide();
        }

        public void Show(string message, string yesText, string noText, Action onYes, Action onNo)
        {
            _onYes = onYes;
            _onNo = onNo;
            if (_message != null) _message.text = message;
            if (_yesLabel != null) _yesLabel.text = string.IsNullOrEmpty(yesText) ? "Confirm" : yesText;
            if (_noLabel != null) _noLabel.text = string.IsNullOrEmpty(noText) ? "Cancel" : noText;
            base.Show();
            if (Root != null) Root.Select(_noButton);
        }

        private void Confirm()
        {
            var callback = _onYes;
            _onYes = null;
            _onNo = null;
            Hide();
            callback?.Invoke();
        }

        public void Cancel()
        {
            var callback = _onNo;
            _onYes = null;
            _onNo = null;
            Hide();
            callback?.Invoke();
        }

        protected override void OnHide()
        {
            _onYes = null;
            _onNo = null;
        }
    }
}
