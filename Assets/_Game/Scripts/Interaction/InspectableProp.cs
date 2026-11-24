using UnityEngine;

namespace LWalida
{
    public class InspectableProp : InteractableBase
    {
        public string awardMemoryId;
        [TextArea(2, 5)] public string inspectMessage;
        public string alreadyInspectedMessage;
        public SpriteRenderer spriteRenderer;

        public void Configure(string label, string memoryId, string message, string repeatMessage, Sprite sprite)
        {
            SetupDisplay(label, 2.4f);
            awardMemoryId = memoryId;
            inspectMessage = message;
            alreadyInspectedMessage = repeatMessage;
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null) spriteRenderer.sprite = sprite;
        }

        public override string GetPromptText()
        {
            return "E \u2014 Inspect " + displayName;
        }

        public override void Interact()
        {
            var game = GameManager.Instance;
            if (game == null) return;

            if (!string.IsNullOrEmpty(awardMemoryId))
            {
                if (game.HasMemory(awardMemoryId))
                {
                    if (!string.IsNullOrEmpty(alreadyInspectedMessage))
                    {
                        game.Notify(alreadyInspectedMessage, 4f);
                    }
                    else if (!string.IsNullOrEmpty(inspectMessage))
                    {
                        game.Notify(inspectMessage, 4f);
                    }
                    return;
                }

                bool added = game.CollectMemory(awardMemoryId);
                if (added && !string.IsNullOrEmpty(inspectMessage))
                {
                    game.Notify(inspectMessage, 5.5f);
                }
                return;
            }

            if (!string.IsNullOrEmpty(inspectMessage))
            {
                game.Notify(inspectMessage, 4.5f);
            }
        }
    }
}
