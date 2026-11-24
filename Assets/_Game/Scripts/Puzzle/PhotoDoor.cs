using System.Collections.Generic;
using UnityEngine;

namespace LWalida
{
    public class PhotoDoor : InteractableBase
    {
        public static readonly List<PhotoDoor> All = new List<PhotoDoor>();

        public string doorId = "door";
        public bool isCorrect;
        public SpriteRenderer spriteRenderer;
        [TextArea(2, 4)] public string inspectMessage;
        [TextArea(1, 3)] public string mismatchHint;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!All.Contains(this)) All.Add(this);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            All.Remove(this);
        }

        public void Configure(string label, bool correct, string inspect, string mismatch, Sprite sprite)
        {
            SetupDisplay(label, 2.7f);
            isCorrect = correct;
            inspectMessage = inspect;
            mismatchHint = mismatch;
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null) spriteRenderer.sprite = sprite;
        }

        public override string GetPromptText()
        {
            return "E \u2014 Inspect door   \u00b7   C \u2014 Camera";
        }

        public override void Interact()
        {
            var game = GameManager.Instance;
            if (game == null) return;
            string message = string.IsNullOrEmpty(inspectMessage)
                ? "An old wooden door. The details are worn by the sea air."
                : inspectMessage;
            game.Notify(message + "  Press C to compare it with the photograph.", 5f);
        }
    }
}
