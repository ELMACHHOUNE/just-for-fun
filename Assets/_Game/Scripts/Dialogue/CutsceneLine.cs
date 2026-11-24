using System;

namespace LWalida
{
    [Serializable]
    public class CutsceneLine
    {
        public string speaker;
        [UnityEngine.TextArea(2, 6)] public string text;
        public float autoAdvanceSeconds = 6f;
        public bool showMemories;
        public bool fadeToBlack;

        public CutsceneLine() { }

        public CutsceneLine(string speaker, string text, float autoAdvanceSeconds, bool showMemories = false,
            bool fadeToBlack = false)
        {
            this.speaker = speaker;
            this.text = text;
            this.autoAdvanceSeconds = autoAdvanceSeconds;
            this.showMemories = showMemories;
            this.fadeToBlack = fadeToBlack;
        }
    }

    public struct MemoryTemplate
    {
        public string id;
        public string title;
        public string description;
        public MemoryType type;
        public int order;

        public MemoryTemplate(string id, string title, string description, MemoryType type, int order)
        {
            this.id = id;
            this.title = title;
            this.description = description;
            this.type = type;
            this.order = order;
        }
    }
}
