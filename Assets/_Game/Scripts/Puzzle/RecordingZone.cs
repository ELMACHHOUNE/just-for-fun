using System.Collections.Generic;
using UnityEngine;

namespace LWalida
{
    public class RecordingZone : InteractableBase
    {
        public static readonly List<RecordingZone> All = new List<RecordingZone>();

        [SerializeField] private AudioClip previewClip;
        public SpriteRenderer marker;

        public bool IsUnlocked
        {
            get { return GameManager.Instance != null && GameManager.Instance.HasFlag(GameFlags.RecordingUnlocked); }
        }

        public bool IsRecorded
        {
            get { return GameManager.Instance != null && GameManager.Instance.HasMemory(MemoryIds.Souk); }
        }

        public bool Ready => IsUnlocked && !IsRecorded;

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

        public void Configure(string label, AudioClip preview)
        {
            SetupDisplay(label, 2.6f);
            previewClip = preview;
        }

        public override string GetPromptText()
        {
            if (IsRecorded) return "The sound of the medina is already in your journal.";
            if (!IsUnlocked) return "E \u2014 Listen   \u00b7   Allal might know about your mother.";
            return "R \u2014 Record the sound of the souk   \u00b7   E \u2014 Listen";
        }

        public override void Interact()
        {
            var game = GameManager.Instance;
            if (game == null) return;
            if (IsRecorded)
            {
                game.Notify("You can listen to this memory in your journal. Press J.", 4f);
                return;
            }
            if (!IsUnlocked)
            {
                game.Notify("The street is alive with sound. Perhaps Allal would understand why you want to keep it.", 5f);
                return;
            }

            game.Notify("Press R to record three seconds of the medina.", 4f);
            if (previewClip != null && game.Audio != null)
            {
                game.Audio.PlaySfx(previewClip, 0.2f);
            }
        }

        private void Update()
        {
            if (marker == null) return;
            float pulse = 1f + Mathf.Sin(Time.time * 2.2f) * 0.025f;
            marker.transform.localScale = new Vector3(pulse, pulse, 1f);
        }
    }
}
