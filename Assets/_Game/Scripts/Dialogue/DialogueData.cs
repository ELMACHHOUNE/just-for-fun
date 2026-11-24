using System;
using System.Collections.Generic;
using UnityEngine;

namespace LWalida
{
    [Serializable]
    public class DialogueChoice
    {
        [TextArea(1, 3)] public string text;
        public string nextNodeId;

        public DialogueChoice() { }

        public DialogueChoice(string text, string nextNodeId)
        {
            this.text = text;
            this.nextNodeId = nextNodeId;
        }
    }

    [Serializable]
    public class DialogueNode
    {
        public string id;
        public string speakerName;
        public Sprite portrait;
        public AudioClip voice;
        [TextArea(2, 6)] public string text;
        public string nextNodeId;
        public List<DialogueChoice> choices = new List<DialogueChoice>();
        public bool endConversation;
        public string awardMemoryId;
        public string awardFlag;

        public bool HasChoices()
        {
            return choices != null && choices.Count > 0;
        }
    }

    [CreateAssetMenu(fileName = "Dialogue", menuName = "LWalida/Dialogue", order = 1)]
    public class DialogueData : ScriptableObject
    {
        public string dialogueId;
        public string speakerName;
        public string startNodeId;
        public List<DialogueNode> nodes = new List<DialogueNode>();

        public DialogueNode FindNode(string id)
        {
            if (string.IsNullOrEmpty(id) || nodes == null) return null;
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] != null && nodes[i].id == id) return nodes[i];
            }
            return null;
        }

        public bool Validate(List<string> errors)
        {
            if (errors == null) return false;
            int startCount = errors.Count;
            if (string.IsNullOrEmpty(startNodeId))
            {
                errors.Add(name + ": startNodeId is empty.");
            }
            else if (FindNode(startNodeId) == null)
            {
                errors.Add(name + ": start node '" + startNodeId + "' does not exist.");
            }

            if (nodes == null || nodes.Count == 0)
            {
                errors.Add(name + ": no nodes.");
                return errors.Count == startCount;
            }

            var ids = new HashSet<string>();
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                if (node == null)
                {
                    errors.Add(name + ": null node at index " + i + ".");
                    continue;
                }

                if (string.IsNullOrEmpty(node.id))
                {
                    errors.Add(name + ": node at index " + i + " has an empty id.");
                }
                else if (!ids.Add(node.id))
                {
                    errors.Add(name + ": duplicate node id '" + node.id + "'.");
                }

                if (!string.IsNullOrEmpty(node.nextNodeId) && FindNode(node.nextNodeId) == null)
                {
                    errors.Add(name + ": node '" + node.id + "' points to missing next node '" + node.nextNodeId + "'.");
                }

                if (node.choices != null)
                {
                    for (int c = 0; c < node.choices.Count; c++)
                    {
                        var choice = node.choices[c];
                        if (choice == null)
                        {
                            errors.Add(name + ": node '" + node.id + "' has a null choice.");
                            continue;
                        }

                        if (string.IsNullOrEmpty(choice.text))
                        {
                            errors.Add(name + ": node '" + node.id + "' has a choice with empty text.");
                        }

                        if (string.IsNullOrEmpty(choice.nextNodeId))
                        {
                            errors.Add(name + ": node '" + node.id + "' choice '" + choice.text + "' has no next node.");
                        }
                        else if (FindNode(choice.nextNodeId) == null)
                        {
                            errors.Add(name + ": node '" + node.id + "' choice points to missing node '" +
                                       choice.nextNodeId + "'.");
                        }
                    }
                }
                else if (string.IsNullOrEmpty(node.nextNodeId) && !node.endConversation)
                {
                    errors.Add(name + ": terminal node '" + node.id + "' should set endConversation.");
                }
            }

            return errors.Count == startCount;
        }
    }
}
