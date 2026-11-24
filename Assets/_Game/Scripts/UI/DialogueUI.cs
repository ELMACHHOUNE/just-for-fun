using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LWalida
{
    public class DialogueUI : ScreenBase
    {
        private Image _portrait;
        private RectTransform _portraitRoot;
        private Text _speaker;
        private Text _body;
        private Text _hint;
        private VerticalLayoutGroup _choiceLayout;
        private RectTransform _choiceContainer;
        private readonly List<Button> _choiceButtons = new List<Button>();
        private DialogueNode _node;
        private DialogueManager _manager;
        private string _fullText = "";
        private float _revealed;
        private bool _typing;
        private bool _choicesActive;
        private int _choiceIndex = -1;

        protected override void OnBuild()
        {
            var panel = UIFactory.CreatePanel("DialoguePanel", transform, Theme.Panel, UIFactory.PanelSprite, true);
            UIFactory.Anchor(panel, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 40f), new Vector2(1640f, 420f));

            _portraitRoot = UIFactory.CreateRect("PortraitFrame", panel);
            UIFactory.Anchor(_portraitRoot, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(36f, 0f), new Vector2(230f, 230f));

            var portraitBackground = UIFactory.CreateImage("PortraitBackground", _portraitRoot, Theme.MidnightBlue);
            UIFactory.Stretch(portraitBackground.rectTransform);

            _portrait = UIFactory.CreateImage("Portrait", _portraitRoot, Color.white, null);
            UIFactory.Stretch(_portrait.rectTransform, 6f, 6f, 6f, 6f);
            _portrait.preserveAspect = true;

            _speaker = UIFactory.CreateText("Speaker", panel, "", 31, Theme.HassanSand, TextAnchor.MiddleLeft,
                FontStyle.Bold);
            UIFactory.Anchor(_speaker.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f),
                new Vector2(290f, -28f), new Vector2(-330f, 42f));

            _body = UIFactory.CreateText("Body", panel, "", 31, Theme.ChalkWhite, TextAnchor.UpperLeft);
            UIFactory.Anchor(_body.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f),
                new Vector2(290f, -80f), new Vector2(-330f, 132f));

            _hint = UIFactory.CreateText("Hint", panel, "", 21, Theme.TextDim, TextAnchor.LowerRight);
            UIFactory.Anchor(_hint.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-30f, 18f), new Vector2(820f, 28f));

            _choiceContainer = UIFactory.CreateRect("Choices", panel);
            UIFactory.Anchor(_choiceContainer, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(290f, 22f), new Vector2(1280f, 180f));
            _choiceLayout = _choiceContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            _choiceLayout.spacing = 6f;
            _choiceLayout.childAlignment = TextAnchor.LowerLeft;
            _choiceLayout.childControlHeight = true;
            _choiceLayout.childControlWidth = true;
            _choiceLayout.childForceExpandHeight = false;
            _choiceLayout.childForceExpandWidth = true;
            _choiceContainer.gameObject.SetActive(false);

            Hide();
        }

        public override void Show()
        {
            base.Show();
            if (Root != null) Root.ClearSelection();
        }

        public void Display(DialogueNode node, DialogueManager manager)
        {
            if (!gameObject.activeSelf) Show();
            _node = node;
            _manager = manager;
            _fullText = node != null ? node.text ?? "" : "";
            _revealed = 0f;
            _typing = true;
            _choicesActive = false;
            _choiceIndex = -1;
            _body.text = "";
            _speaker.text = node != null ? node.speakerName : "";
            _hint.text = "Space \u2014 reveal";

            bool hasPortrait = node != null && node.portrait != null;
            _portraitRoot.gameObject.SetActive(hasPortrait);
            if (hasPortrait) _portrait.sprite = node.portrait;

            ClearChoices();
            Root.ClearSelection();
        }

        private void Update()
        {
            var game = GameManager.Instance;
            if (game == null || game.Mode != GameMode.Dialogue) return;

            bool advance = LWalidaInput.SpacePressed || LWalidaInput.ConfirmPressed;

            if (_typing)
            {
                if (advance)
                {
                    CompleteReveal();
                    return;
                }

                _revealed += Time.unscaledDeltaTime * 46f;
                int count = Mathf.Clamp(Mathf.FloorToInt(_revealed), 0, _fullText.Length);
                _body.text = _fullText.Substring(0, count);
                if (count >= _fullText.Length) CompleteReveal();
                return;
            }

            if (_choicesActive)
            {
                int number = LWalidaInput.ChoiceNumberPressed;
                if (number > 0)
                {
                    Choose(number - 1);
                    return;
                }
                if (LWalidaInput.MenuUpPressed) MoveSelection(-1);
                else if (LWalidaInput.MenuDownPressed) MoveSelection(1);
                else if (LWalidaInput.ConfirmPressed && _choiceIndex >= 0) Choose(_choiceIndex);
                return;
            }

            if (advance && _manager != null) _manager.Advance();
        }

        private void CompleteReveal()
        {
            _typing = false;
            _body.text = _fullText;
            if (_node != null && _node.HasChoices())
            {
                BuildChoices();
            }
            else
            {
                _hint.text = _node != null && _node.endConversation
                    ? "Space \u2014 close"
                    : "Space \u2014 continue";
            }
        }

        private void BuildChoices()
        {
            ClearChoices();
            _choicesActive = true;
            _choiceIndex = -1;
            _choiceContainer.gameObject.SetActive(true);
            _hint.text = "1 / 2 / 3 or \u2191\u2193 + Enter";

            for (int i = 0; i < _node.choices.Count; i++)
            {
                int index = i;
                var choice = _node.choices[i];
                string label = (i + 1) + ".  " + (choice != null ? choice.text : "...");
                var button = UIFactory.CreateButton(_choiceContainer, label, () => Choose(index), 25, 52f);
                _choiceButtons.Add(button);
            }
            MoveSelection(0);
        }

        private void Choose(int index)
        {
            if (!_choicesActive || _manager == null) return;
            if (index < 0 || index >= _choiceButtons.Count) return;
            _choicesActive = false;
            _choiceContainer.gameObject.SetActive(false);
            _manager.Choose(index);
        }

        private void MoveSelection(int delta)
        {
            if (_choiceButtons.Count == 0) return;
            _choiceIndex += delta;
            if (_choiceIndex < 0) _choiceIndex = _choiceButtons.Count - 1;
            if (_choiceIndex >= _choiceButtons.Count) _choiceIndex = 0;

            for (int i = 0; i < _choiceButtons.Count; i++)
            {
                UIFactory.SetButtonColor(_choiceButtons[i], i == _choiceIndex ? Theme.HassanSand : Theme.OudayaBlue);
            }
        }

        private void ClearChoices()
        {
            for (int i = 0; i < _choiceButtons.Count; i++)
            {
                if (_choiceButtons[i] != null) Destroy(_choiceButtons[i].gameObject);
            }
            _choiceButtons.Clear();
            _choicesActive = false;
            _choiceIndex = -1;
            if (_choiceContainer != null) _choiceContainer.gameObject.SetActive(false);
        }

        protected override void OnHide()
        {
            ClearChoices();
            _node = null;
            _manager = null;
            _typing = false;
        }
    }
}
