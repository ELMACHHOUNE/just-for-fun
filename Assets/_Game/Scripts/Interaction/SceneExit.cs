using UnityEngine;

namespace LWalida
{
    public class SceneExit : InteractableBase
    {
        public string targetSceneId;
        public string openPrompt = "Continue";
        public string lockedMessage;
        public string[] requiredMemoryIds = new string[0];
        public string[] requiredFlags = new string[0];
        public bool returnToTitle;

        public void Configure(string label, string target, string prompt, string locked,
            string[] memories, string[] flags)
        {
            SetupDisplay(label, 2.2f);
            targetSceneId = target;
            openPrompt = prompt;
            lockedMessage = locked;
            requiredMemoryIds = memories ?? new string[0];
            requiredFlags = flags ?? new string[0];
        }

        public bool IsOpen
        {
            get
            {
                var game = GameManager.Instance;
                if (game == null) return false;
                return ProgressionRules.RequirementsMet(requiredMemoryIds, requiredFlags,
                    game.Memories != null ? game.Memories.CollectedIds : null, game.Flags);
            }
        }

        public override string GetPromptText()
        {
            if (IsOpen) return "E \u2014 " + openPrompt;
            return "E \u2014 " + openPrompt + " (locked)";
        }

        public override void Interact()
        {
            var game = GameManager.Instance;
            if (game == null) return;

            if (!IsOpen)
            {
                string message = lockedMessage;
                if (string.IsNullOrEmpty(message))
                {
                    string objective = game.Objectives != null
                        ? game.Objectives.CurrentObjective
                        : ProgressionRules.ObjectiveFor(game.CurrentSceneId,
                            game.Memories != null ? game.Memories.CollectedIds : null, game.Flags);
                    message = "Not yet. " + objective;
                }
                game.Notify(message, 4.5f);
                return;
            }

            game.SaveCheckpoint();
            if (returnToTitle) game.ReturnToMainMenu();
            else game.Loader.Load(targetSceneId);
        }
    }
}
