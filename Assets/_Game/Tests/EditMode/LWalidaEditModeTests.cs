using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace LWalida.Tests
{
    public class ProgressionTests
    {
        [Test]
        public void RequirementsNeedBothMemoriesAndFlags()
        {
            var memories = new HashSet<string> { MemoryIds.Cafe, MemoryIds.Door };
            var flags = new HashSet<string>();
            Assert.IsTrue(ProgressionRules.RequirementsMet(new[] { MemoryIds.Cafe }, null, memories, flags));
            Assert.IsFalse(ProgressionRules.RequirementsMet(new[] { MemoryIds.Amina }, null, memories, flags));
            Assert.IsFalse(ProgressionRules.RequirementsMet(new[] { MemoryIds.Cafe },
                new[] { GameFlags.RecordingUnlocked }, memories, flags));
            flags.Add(GameFlags.RecordingUnlocked);
            Assert.IsTrue(ProgressionRules.RequirementsMet(new[] { MemoryIds.Cafe },
                new[] { GameFlags.RecordingUnlocked }, memories, flags));
        }

        [Test]
        public void AllFourMemoriesAreRequiredForEnding()
        {
            var memories = new HashSet<string>();
            Assert.IsFalse(ProgressionRules.HasAllMemories(memories));
            for (int i = 0; i < MemoryIds.All.Length - 1; i++) memories.Add(MemoryIds.All[i]);
            Assert.IsFalse(ProgressionRules.HasAllMemories(memories));
            memories.Add(MemoryIds.Souk);
            Assert.IsTrue(ProgressionRules.HasAllMemories(memories));
            Assert.AreEqual(4, ProgressionRules.CountMemories(memories));
        }

        [Test]
        public void MemoryIdsAreUnique()
        {
            var ids = new HashSet<string>();
            for (int i = 0; i < MemoryIds.All.Length; i++)
            {
                Assert.IsTrue(ids.Add(MemoryIds.All[i]), "Duplicate memory id " + MemoryIds.All[i]);
            }
            Assert.AreEqual(MemoryIds.RequiredCount, ids.Count);
        }

        [Test]
        public void ObjectiveChangesWithProgress()
        {
            var memories = new HashSet<string>();
            string cafe = ProgressionRules.ObjectiveFor(SceneIds.CafeMaure, memories, new HashSet<string>());
            Assert.IsNotEmpty(cafe);
            memories.Add(MemoryIds.Cafe);
            string oudaya = ProgressionRules.ObjectiveFor(SceneIds.Oudaya, memories, new HashSet<string>());
            Assert.IsNotEmpty(oudaya);
            Assert.AreNotEqual(cafe, oudaya);
        }

        [Test]
        public void LevelCatalogHasStableSceneOrder()
        {
            Assert.AreEqual(8, LevelCatalog.All.Length);
            for (int i = 0; i < LevelCatalog.All.Length; i++)
            {
                Assert.AreEqual(SceneIds.BuildOrder[i], LevelCatalog.All[i].SceneId);
                if (!LevelCatalog.All[i].IsMenu && !LevelCatalog.All[i].IsCredits &&
                    !LevelCatalog.All[i].IsCutscene)
                {
                    Assert.Greater(LevelCatalog.All[i].WorldWidth, 10f);
                }
            }
        }
    }

    public class NarrativeTests
    {
        [Test]
        public void AminaDialogueReferencesAreValid()
        {
            AssertDialogueValid(NarrativeDefaults.BuildAminaDialogue());
            AssertDialogueValid(NarrativeDefaults.BuildAminaAfterDialogue());
        }

        [Test]
        public void AllalDialogueReferencesAreValid()
        {
            AssertDialogueValid(NarrativeDefaults.BuildAllalDialogue());
            AssertDialogueValid(NarrativeDefaults.BuildAllalAfterDialogue());
        }

        [Test]
        public void AminaDialogueAwardsTheMemory()
        {
            var data = NarrativeDefaults.BuildAminaDialogue();
            var nodes = data.nodes;
            bool found = false;
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].awardMemoryId == MemoryIds.Amina) found = true;
            }
            Assert.IsTrue(found, "Amina dialogue never awards amina_01.");
        }

        [Test]
        public void AllalDialogueUnlocksRecording()
        {
            var data = NarrativeDefaults.BuildAllalDialogue();
            bool found = false;
            for (int i = 0; i < data.nodes.Count; i++)
            {
                if (data.nodes[i].awardFlag == GameFlags.RecordingUnlocked) found = true;
            }
            Assert.IsTrue(found, "Allal dialogue never sets recording_unlocked.");
        }

        [Test]
        public void CutsceneLinesAreNotBlank()
        {
            var intro = NarrativeDefaults.IntroLines();
            var home = NarrativeDefaults.HomeLines();
            Assert.Greater(intro.Length, 0);
            Assert.Greater(home.Length, 0);
            for (int i = 0; i < intro.Length; i++) Assert.IsNotEmpty(intro[i].text);
            for (int i = 0; i < home.Length; i++) Assert.IsNotEmpty(home[i].text);
        }

        private static void AssertDialogueValid(DialogueData data)
        {
            var errors = new List<string>();
            bool valid = data.Validate(errors);
            Assert.IsTrue(valid, string.Join("\n", errors.ToArray()));
        }
    }

    public class SaveTests
    {
        [Test]
        public void SaveDataRoundTripsThroughJson()
        {
            var save = new SaveData
            {
                sceneId = SceneIds.Oudaya,
                memoryIds = new List<string> { MemoryIds.Cafe, MemoryIds.Door },
                flags = new List<string> { GameFlags.RecordingUnlocked },
                savedAtUtc = "2026-01-01T00:00:00Z"
            };
            save.settings.master = 0.42f;
            save.settings.subtitles = false;

            string json = JsonUtility.ToJson(save);
            var restored = JsonUtility.FromJson<SaveData>(json);

            Assert.AreEqual(save.sceneId, restored.sceneId);
            Assert.AreEqual(2, restored.memoryIds.Count);
            Assert.Contains(MemoryIds.Door, restored.memoryIds);
            Assert.Contains(GameFlags.RecordingUnlocked, restored.flags);
            Assert.AreEqual(0.42f, restored.settings.master, 0.0001f);
            Assert.IsFalse(restored.settings.subtitles);
            Assert.AreEqual(1, restored.version);
        }

        [Test]
        public void RestoredSaveRejectsUnknownScenes()
        {
            Assert.IsFalse(SceneIds.IsKnown("99_Unknown"));
            Assert.IsTrue(SceneIds.IsKnown(SceneIds.CafeMaure));
        }
    }
}
