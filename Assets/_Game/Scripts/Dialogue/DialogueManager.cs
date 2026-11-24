using UnityEngine;

namespace LWalida
{
    public class DialogueManager : MonoBehaviour
    {
        private DialogueData _data;
        private DialogueNode _node;

        public bool IsActive => _data != null;
        public DialogueData CurrentData => _data;
        public DialogueNode CurrentNode => _node;

        public void Begin(DialogueData data)
        {
            var game = GameManager.Instance;
            if (game == null || data == null) return;
            if (game.Mode != GameMode.Exploring) return;

            _data = data;
            _node = null;
            game.SetMode(GameMode.Dialogue);
            if (game.UI != null) game.UI.ShowDialogue();
            EnterNode(data.startNodeId);
        }

        private void EnterNode(string nodeId)
        {
            var game = GameManager.Instance;
            if (game == null || _data == null) return;

            var node = _data.FindNode(nodeId);
            if (node == null)
            {
                Debug.LogWarning("[LWalida] Dialogue '" + _data.dialogueId + "' has no node '" + nodeId + "'.");
                End();
                return;
            }

            _node = node;

            if (!string.IsNullOrEmpty(node.awardMemoryId))
            {
                game.CollectMemory(node.awardMemoryId);
            }
            if (!string.IsNullOrEmpty(node.awardFlag))
            {
                game.SetFlag(node.awardFlag);
            }

            if (game.Audio != null)
            {
                game.Audio.PlayDialogueVoice(node.voice);
            }

            if (game.UI != null && game.UI.DialogueUI != null)
            {
                game.UI.DialogueUI.Display(node, this);
            }
        }

        public void Advance()
        {
            if (_node == null)
            {
                End();
                return;
            }

            if (_node.HasChoices())
            {
                // Choices must be selected explicitly.
                return;
            }

            if (_node.endConversation || string.IsNullOrEmpty(_node.nextNodeId))
            {
                End();
                return;
            }

            EnterNode(_node.nextNodeId);
        }

        public void Choose(int index)
        {
            if (_node == null || _node.choices == null) return;
            if (index < 0 || index >= _node.choices.Count) return;
            var choice = _node.choices[index];
            if (choice == null) return;
            if (string.IsNullOrEmpty(choice.nextNodeId))
            {
                End();
                return;
            }
            EnterNode(choice.nextNodeId);
        }

        public void End()
        {
            var game = GameManager.Instance;
            _data = null;
            _node = null;
            if (game != null)
            {
                if (game.Audio != null) game.Audio.StopDialogueVoice();
                if (game.UI != null) game.UI.HideDialogue();
                if (game.Mode == GameMode.Dialogue) game.SetMode(GameMode.Exploring);
                game.SaveCheckpoint();
            }
        }
    }
}
