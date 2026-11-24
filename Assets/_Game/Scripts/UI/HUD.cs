using UnityEngine;
using UnityEngine.UI;

namespace LWalida
{
    public class HUD : ScreenBase
    {
        private Text _objective;
        private Text _memory;
        private Text _prompt;
        private RectTransform _promptRoot;
        private Text _shortcuts;

        protected override void OnBuild()
        {
            var group = gameObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;

            var objectivePanel = UIFactory.CreatePanel("ObjectivePanel", transform, Theme.Panel);
            UIFactory.Anchor(objectivePanel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(48f, -40f), new Vector2(650f, 112f));

            var label = UIFactory.CreateText("Label", objectivePanel, "OBJECTIVE", 19, Theme.HassanSand,
                TextAnchor.MiddleLeft, FontStyle.Bold);
            UIFactory.PlaceLeft(label.rectTransform, 24f, 14f, 520f, 24f);

            _objective = UIFactory.CreateText("Objective", objectivePanel, "", 27, Theme.ChalkWhite,
                TextAnchor.UpperLeft);
            UIFactory.Stretch(_objective.rectTransform, 24f, 14f, 24f, 42f);

            var memoryPanel = UIFactory.CreatePanel("MemoryPanel", transform, Theme.Panel);
            UIFactory.Anchor(memoryPanel, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-48f, -40f), new Vector2(286f, 86f));

            if (Content != null && Content.iconJournal != null)
            {
                var icon = UIFactory.CreateImage("Icon", memoryPanel, Color.white, Content.iconJournal);
                UIFactory.Anchor(icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                    new Vector2(0f, 0.5f), new Vector2(22f, 0f), new Vector2(48f, 48f));
                icon.preserveAspect = true;
            }

            _memory = UIFactory.CreateText("Memory", memoryPanel, "0 / 4 memories", 25, Theme.ChalkWhite,
                TextAnchor.MiddleRight);
            UIFactory.Stretch(_memory.rectTransform, 76f, 8f, 22f, 8f);

            var promptPanel = UIFactory.CreatePanel("Prompt", transform, Theme.PanelSoft);
            UIFactory.Anchor(promptPanel, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 62f), new Vector2(920f, 72f));
            _promptRoot = promptPanel;
            _prompt = UIFactory.CreateText("PromptText", promptPanel, "", 27, Theme.ChalkWhite,
                TextAnchor.MiddleCenter);
            UIFactory.Stretch(_prompt.rectTransform, 26f, 8f, 26f, 8f);
            promptPanel.gameObject.SetActive(false);

            _shortcuts = UIFactory.CreateText("Shortcuts", transform,
                "A/D Move   \u00b7   E Interact   \u00b7   C Camera   \u00b7   J Journal   \u00b7   Esc Pause",
                21, Theme.TextDim, TextAnchor.LowerRight);
            UIFactory.Anchor(_shortcuts.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-48f, 18f), new Vector2(900f, 34f));

            SetObjective("");
            SetMemoryCount(0, MemoryIds.RequiredCount);
            ClearPrompt();
        }

        public void SetVisible(bool visible)
        {
            if (visible)
            {
                Show();
            }
            else
            {
                ClearPrompt();
                Hide();
            }
        }

        public void SetObjective(string text)
        {
            if (_objective != null) _objective.text = text;
        }

        public void SetMemoryCount(int count, int total)
        {
            if (_memory != null) _memory.text = count + " / " + total + " memories";
        }

        public void SetPrompt(string text)
        {
            if (_promptRoot == null) return;
            if (string.IsNullOrEmpty(text))
            {
                ClearPrompt();
                return;
            }
            _promptRoot.gameObject.SetActive(true);
            _prompt.text = text;
        }

        public void ClearPrompt()
        {
            if (_promptRoot != null) _promptRoot.gameObject.SetActive(false);
            if (_prompt != null) _prompt.text = "";
        }
    }
}
