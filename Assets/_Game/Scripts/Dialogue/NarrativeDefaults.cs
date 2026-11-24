using System.Collections.Generic;
using UnityEngine;

namespace LWalida
{
    /// <summary>
    /// Single source of truth for narrative content. The editor asset factory uses
    /// these builders to create ScriptableObjects, and the runtime falls back to
    /// them if data assets are missing, so a missing asset can never dead-end the game.
    /// </summary>
    public static class NarrativeDefaults
    {
        public static readonly MemoryTemplate[] MemoryTemplates =
        {
            new MemoryTemplate(MemoryIds.Cafe, "Where it began",
                "A worn photograph of a blue door, kept safe in a coat pocket. This is where the search begins.",
                MemoryType.Photograph, 0),
            new MemoryTemplate(MemoryIds.Door, "The childhood house",
                "The horseshoe arch, the paired brass handles, the studs. After all these years, the door still remembers.",
                MemoryType.Photograph, 1),
            new MemoryTemplate(MemoryIds.Amina, "Zohra's childhood",
                "Lalla Amina remembers a girl running through the blue streets, laughing.",
                MemoryType.Photograph, 2),
            new MemoryTemplate(MemoryIds.Souk, "Voices of the medina",
                "The medina sounds like music: vendors, footsteps, copper, water, life.",
                MemoryType.Sound, 3)
        };

        public static List<MemoryData> CreateMemoryInstances()
        {
            var list = new List<MemoryData>();
            for (int i = 0; i < MemoryTemplates.Length; i++)
            {
                var t = MemoryTemplates[i];
                var memory = ScriptableObject.CreateInstance<MemoryData>();
                memory.id = t.id;
                memory.title = t.title;
                memory.description = t.description;
                memory.type = t.type;
                memory.order = t.order;
                memory.name = t.id;
                list.Add(memory);
            }
            return list;
        }

        public static DialogueData BuildAminaDialogue()
        {
            var data = CreateDialogue("amina", "Lalla Amina", "amina_intro");
            data.nodes.Add(Node("amina_intro", "Lalla Amina",
                "Hello, son. Are you looking for something?",
                choices: new[]
                {
                    new DialogueChoice("Actually, yes. I'm looking for this house.", "amina_photo"),
                    new DialogueChoice("No, I'm just looking around.", "amina_goodbye")
                }));
            data.nodes.Add(Node("amina_goodbye", "Lalla Amina",
                "Of course, son. Take your time. These streets have many stories.", end: true));
            data.nodes.Add(Node("amina_photo", "Lalla Amina",
                "Ah, yes! That's the house. But it's been empty for a long time.",
                choices: new[]
                {
                    new DialogueChoice("I know. I'm collecting my mother's childhood memories.", "amina_mother"),
                    new DialogueChoice("Thank you.", "amina_thanks")
                }));
            data.nodes.Add(Node("amina_thanks", "You",
                "Thank you, Lalla. I'll come back if I find the courage.", end: true));
            data.nodes.Add(Node("amina_mother", "Lalla Amina",
                "Your mother?",
                choices: new[]
                {
                    new DialogueChoice("Zohra. She used to live here.", "amina_zohra"),
                    new DialogueChoice("It's the last photo of her childhood neighborhood.", "amina_hint")
                }));
            data.nodes.Add(Node("amina_hint", "Lalla Amina",
                "Tell me her name when you're ready.", end: true));
            data.nodes.Add(Node("amina_zohra", "Lalla Amina",
                "Zohra? Oh, the memories! She used to run through these streets. How is she doing these days?",
                choices: new[] { new DialogueChoice("Thank God. She's a little unwell.", "amina_end") }));
            data.nodes.Add(Node("amina_end", "Lalla Amina",
                "May God make things easy for you, son.", end: true, awardMemory: MemoryIds.Amina));
            return data;
        }

        public static DialogueData BuildAminaAfterDialogue()
        {
            var data = CreateDialogue("amina_after", "Lalla Amina", "amina_after");
            data.nodes.Add(Node("amina_after", "Lalla Amina",
                "May God make things easy for you, son. Take your time in these streets.", end: true));
            return data;
        }

        public static DialogueData BuildAllalDialogue()
        {
            var data = CreateDialogue("allal", "Allal", "allal_intro");
            data.nodes.Add(Node("allal_intro", "Allal",
                "Zohra? Of course I remember her. She used to run through this street every morning.",
                choices: new[]
                {
                    new DialogueChoice("Do you remember anything else about her?", "allal_memory"),
                    new DialogueChoice("I should let you get back to work.", "allal_goodbye")
                }));
            data.nodes.Add(Node("allal_memory", "Allal",
                "She said the medina sounded like music.",
                choices: new[]
                {
                    new DialogueChoice("Would you mind if I recorded the sound?", "allal_record"),
                    new DialogueChoice("Thank you, Allal.", "allal_thanks")
                }));
            data.nodes.Add(Node("allal_record", "Allal",
                "Of course, son. Some things are worth keeping.", end: true,
                awardFlag: GameFlags.RecordingUnlocked));
            data.nodes.Add(Node("allal_thanks", "Allal",
                "Come back if you want to listen. The street is generous today.", end: true));
            data.nodes.Add(Node("allal_goodbye", "Allal",
                "Take care, son. And mind the cats.", end: true));
            return data;
        }

        public static DialogueData BuildAllalAfterDialogue()
        {
            var data = CreateDialogue("allal_after", "Allal", "allal_after");
            data.nodes.Add(Node("allal_after", "Allal",
                "Have you listened to the medina yet? The good places keep their sounds.", end: true));
            return data;
        }

        public static CutsceneLine[] IntroLines()
        {
            return new[]
            {
                new CutsceneLine("Doctor",
                    "Sometimes, familiar places and sounds can help us reconnect with memories. You could try showing her the places she once knew.",
                    7f),
                new CutsceneLine("Narrator",
                    "A taxi door closes. The river is close now, and so is the smell of mint tea.", 6f),
                new CutsceneLine("You",
                    "The only photograph I have left of my mother's childhood home.", 6f),
                new CutsceneLine("You",
                    "If the medina remembers her, maybe she will too.", 5.5f)
            };
        }

        public static CutsceneLine[] HomeLines()
        {
            return new[]
            {
                new CutsceneLine("You", "Mama? I'm home. I found your house.", 5f),
                new CutsceneLine("Narrator", "Silence. A kettle ticks somewhere behind the wall.", 5f),
                new CutsceneLine("Narrator", "Then: a knock.", 4f),
                new CutsceneLine("Neighbor",
                    "Salam, weldi. I just wanted to offer my condolences. Allah yerhamha. Your mother was a wonderful woman.",
                    8f, true),
                new CutsceneLine("You", "Thank you. She loved this street.", 5f),
                new CutsceneLine("Narrator",
                    "Some people leave us. But the places they loved never forget them.", 7f, true, true)
            };
        }

        private static DialogueData CreateDialogue(string id, string speaker, string startNode)
        {
            var data = ScriptableObject.CreateInstance<DialogueData>();
            data.dialogueId = id;
            data.speakerName = speaker;
            data.startNodeId = startNode;
            data.name = id;
            return data;
        }

        private static DialogueNode Node(string id, string speaker, string text, DialogueChoice[] choices = null,
            bool end = false, string awardMemory = null, string awardFlag = null)
        {
            var node = new DialogueNode
            {
                id = id,
                speakerName = speaker,
                text = text,
                endConversation = end,
                awardMemoryId = awardMemory,
                awardFlag = awardFlag
            };
            if (choices != null)
            {
                for (int i = 0; i < choices.Length; i++) node.choices.Add(choices[i]);
            }
            return node;
        }
    }
}
