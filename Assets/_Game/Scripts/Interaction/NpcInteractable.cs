using UnityEngine;

namespace LWalida
{
    public class NpcInteractable : InteractableBase
    {
        public DialogueData dialogue;
        public DialogueData dialogueAfter;
        public string afterMemoryId;
        public string afterFlag;
        public Sprite portrait;

        public void Configure(string label, DialogueData first, DialogueData after, string afterMemory,
            string flag, Sprite portraitSprite)
        {
            SetupDisplay(label, 2.7f);
            dialogue = first;
            dialogueAfter = after;
            afterMemoryId = afterMemory;
            afterFlag = flag;
            portrait = portraitSprite;
            var renderer = GetComponent<SpriteRenderer>();
            if (renderer != null && portraitSprite != null) renderer.sprite = portraitSprite;
        }

        public override bool CanInteract
        {
            get { return GameManager.Instance != null && GameManager.Instance.Mode == GameMode.Exploring; }
        }

        public override string GetPromptText()
        {
            return "E \u2014 Talk to " + displayName;
        }

        public DialogueData CurrentDialogue
        {
            get
            {
                var game = GameManager.Instance;
                if (game == null) return dialogue;
                if (dialogueAfter != null && !string.IsNullOrEmpty(afterMemoryId) && game.HasMemory(afterMemoryId))
                {
                    return dialogueAfter;
                }
                if (dialogueAfter != null && !string.IsNullOrEmpty(afterFlag) && game.HasFlag(afterFlag))
                {
                    return dialogueAfter;
                }
                return dialogue;
            }
        }

        public override void Interact()
        {
            var game = GameManager.Instance;
            if (game == null || game.Dialogue == null) return;
            var data = CurrentDialogue;
            if (data == null)
            {
                game.Notify("There is nothing to say right now.");
                return;
            }
            game.Dialogue.Begin(data);
        }
    }
}
