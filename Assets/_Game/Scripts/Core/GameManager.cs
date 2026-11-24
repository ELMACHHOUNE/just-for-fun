using System;
using System.Collections.Generic;
using UnityEngine;

namespace LWalida
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public MemoryManager Memories { get; private set; }
        public AudioManager Audio { get; private set; }
        public UIRoot UI { get; private set; }
        public SceneLoader Loader { get; private set; }
        public InteractionManager Interactions { get; private set; }
        public DialogueManager Dialogue { get; private set; }
        public CameraSystem Camera { get; private set; }
        public SoundRecorder Recorder { get; private set; }
        public ObjectiveManager Objectives { get; private set; }
        public SaveManager Saves { get; private set; }
        public AssetCatalog Content { get; private set; }

        public GameMode Mode { get; private set; } = GameMode.Boot;
        public string CurrentSceneId { get; private set; } = "";
        public HashSet<string> Flags { get; } = new HashSet<string>();

        public event Action<GameMode> ModeChanged;
        public event Action<string> NotificationRequested;

        private GameMode _journalReturnMode = GameMode.Exploring;
        private Vector3 _lastPlayerPosition;
        private float _idleSeconds;
        private bool _idleHintShown;

        public void Initialize(GameRoot root, AssetCatalog content, MemoryManager memories, AudioManager audio,
            UIRoot ui, SceneLoader loader, InteractionManager interactions, DialogueManager dialogue,
            CameraSystem cameraSystem, SoundRecorder recorder, ObjectiveManager objectives, SaveManager saves)
        {
            Instance = this;
            Content = content;
            Memories = memories;
            Audio = audio;
            UI = ui;
            Loader = loader;
            Interactions = interactions;
            Dialogue = dialogue;
            Camera = cameraSystem;
            Recorder = recorder;
            Objectives = objectives;
            Saves = saves;

            SettingsService.Load();
            Audio.ApplySettings();
            Memories.MemoryCollected += OnMemoryCollected;
            Objectives.Initialize(this);
        }

        private void OnDestroy()
        {
            if (Memories != null) Memories.MemoryCollected -= OnMemoryCollected;
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            if (Mode == GameMode.Transition) return;

            if (LWalidaInput.BackPressed)
            {
                if (UI != null && UI.TryHandleBack()) return;
                HandleBack();
            }

            if (LWalidaInput.JournalPressed)
            {
                if (Mode == GameMode.Exploring) OpenJournal(false);
                else if (Mode == GameMode.Journal) CloseJournal();
            }

            UpdateIdleHint();
        }

        private void HandleBack()
        {
            switch (Mode)
            {
                case GameMode.Exploring:
                    PauseGame();
                    break;
                case GameMode.Paused:
                    ResumeGame();
                    break;
                case GameMode.Journal:
                    CloseJournal();
                    break;
                case GameMode.Camera:
                    if (Camera != null) Camera.Close();
                    break;
                case GameMode.Recording:
                    if (Recorder != null) Recorder.Cancel();
                    break;
                case GameMode.Cutscene:
                    if (CutsceneController.Active != null) CutsceneController.Active.RequestSkip();
                    break;
                case GameMode.Dialogue:
                    // Dialogue is intentionally not skippable by Esc to avoid accidental story loss.
                    break;
                case GameMode.Credits:
                    ReturnToMainMenu();
                    break;
            }
        }

        public void SetMode(GameMode mode)
        {
            if (Mode == mode) return;
            Mode = mode;
            Time.timeScale = mode == GameMode.Paused || mode == GameMode.Journal ? 0f : 1f;
            ModeChanged?.Invoke(mode);
            if (UI != null) UI.OnModeChanged(mode);
            if (Interactions != null) Interactions.OnModeChanged(mode);
            if (mode == GameMode.Exploring) ResetIdleHints();
        }

        public void EnterScene(SceneBootstrap bootstrap)
        {
            if (bootstrap == null) return;
            var level = LevelCatalog.Get(bootstrap.SceneId);
            CurrentSceneId = bootstrap.SceneId;

            AudioClip ambience = Content != null ? Content.GetAmbience(level.AmbienceKey) : null;
            AudioClip music = Content != null ? Content.GetMusic(level.MusicKey) : null;
            if (Audio != null) Audio.SetSceneAudio(ambience, music);

            if (UI != null) UI.SetSceneTint(level.Tint);

            if (level.IsMenu) SetMode(GameMode.Menu);
            else if (level.IsCredits) SetMode(GameMode.Credits);
            else if (level.IsCutscene) SetMode(GameMode.Cutscene);
            else SetMode(GameMode.Exploring);

            if (Objectives != null) Objectives.Refresh();

            if (level.IsMenu || level.IsCredits || level.SceneId == SceneIds.Intro) return;
            SaveCheckpoint();
        }

        public bool HasMemory(string id) => Memories != null && Memories.Has(id);

        public bool HasFlag(string flag) => !string.IsNullOrEmpty(flag) && Flags.Contains(flag);

        public void SetFlag(string flag)
        {
            if (string.IsNullOrEmpty(flag)) return;
            if (!Flags.Add(flag)) return;
            if (Objectives != null) Objectives.Refresh();
            if (Mode == GameMode.Exploring) SaveCheckpoint();
        }

        public bool CollectMemory(string id)
        {
            if (Memories == null) return false;
            if (!Memories.TryCollect(id)) return false;
            if (Objectives != null) Objectives.Refresh();
            if (Saves != null) SaveCheckpoint();
            return true;
        }

        private void OnMemoryCollected(MemoryData memory)
        {
            if (memory == null) return;
            if (Audio != null)
            {
                Audio.PlaySfx(Content != null ? Content.sfxMemoryUnlock : null);
            }

            string message = "Memory recorded \u2014 " + memory.title;
            if (ProgressionRules.HasAllMemories(Memories.CollectedIds))
            {
                message = "All four memories. It is time to go home.";
            }
            Notify(message);
        }

        public void Notify(string message, float duration = 3.5f)
        {
            if (string.IsNullOrEmpty(message)) return;
            NotificationRequested?.Invoke(message);
            if (UI != null) UI.ShowNotification(message, duration);
        }

        public void PauseGame()
        {
            if (Mode != GameMode.Exploring) return;
            SetMode(GameMode.Paused);
        }

        public void ResumeGame()
        {
            if (Mode != GameMode.Paused) return;
            SetMode(GameMode.Exploring);
        }

        public void OpenJournal(bool fromPause)
        {
            if (Mode != GameMode.Exploring && Mode != GameMode.Paused) return;
            _journalReturnMode = fromPause ? GameMode.Paused : GameMode.Exploring;
            SetMode(GameMode.Journal);
            if (UI != null) UI.ShowJournal();
        }

        public void CloseJournal()
        {
            if (Mode != GameMode.Journal) return;
            if (UI != null) UI.HideJournal();
            SetMode(_journalReturnMode);
        }

        public void NewJourney()
        {
            ResetState();
            if (Saves != null) Saves.Delete();
            if (Loader != null) Loader.Load(SceneIds.Intro);
        }

        public void ResetState()
        {
            Time.timeScale = 1f;
            if (Memories != null) Memories.Clear();
            Flags.Clear();
            _journalReturnMode = GameMode.Exploring;
            if (UI != null)
            {
                UI.HideAllModals();
                if (UI.Journal != null) UI.Journal.StopAudio();
            }
        }

        public void RestartJourney()
        {
            NewJourney();
        }

        public void ReturnToMainMenu()
        {
            if (Saves != null) SaveCheckpoint();
            if (UI != null)
            {
                UI.HideAllModals();
                if (UI.Journal != null) UI.Journal.StopAudio();
            }
            if (Loader != null) Loader.Load(SceneIds.MainMenu);
        }

        public void ApplySave(SaveData data)
        {
            if (data == null) return;
            SettingsService.Apply(data.settings);
            if (Audio != null) Audio.ApplySettings();
            Flags.Clear();
            if (data.flags != null)
            {
                for (int i = 0; i < data.flags.Count; i++) Flags.Add(data.flags[i]);
            }
            if (Memories != null) Memories.Restore(data.memoryIds);
            if (Objectives != null) Objectives.Refresh();
        }

        public bool CanContinueFrom(SaveData data)
        {
            if (data == null || string.IsNullOrEmpty(data.sceneId)) return false;
            if (!SceneIds.IsKnown(data.sceneId)) return false;
            return data.sceneId != SceneIds.MainMenu && data.sceneId != SceneIds.Credits &&
                   data.sceneId != SceneIds.Intro;
        }

        public void ContinueJourney()
        {
            if (Saves == null || Loader == null) return;
            if (!Saves.TryLoad(out var data, out string error))
            {
                Notify("No readable journey was found" + (string.IsNullOrEmpty(error) ? "." : ": " + error));
                return;
            }

            if (!CanContinueFrom(data))
            {
                Notify("That journey cannot be resumed.");
                return;
            }

            ApplySave(data);
            Loader.Load(data.sceneId);
        }

        public void SaveCheckpoint()
        {
            if (Saves == null) return;
            if (string.IsNullOrEmpty(CurrentSceneId)) return;
            if (CurrentSceneId == SceneIds.MainMenu || CurrentSceneId == SceneIds.Intro ||
                CurrentSceneId == SceneIds.Credits) return;
            if (Mode == GameMode.Dialogue || Mode == GameMode.Transition) return;
            Saves.Save(CurrentSceneId, Memories != null ? Memories.CollectedIds : null, Flags);
        }

        public void SetJournalReturnModeForTests(GameMode mode)
        {
            _journalReturnMode = mode;
        }

        private void ResetIdleHints()
        {
            _idleSeconds = 0f;
            _idleHintShown = false;
            var player = PlayerController.Instance;
            _lastPlayerPosition = player != null ? player.transform.position : Vector3.zero;
        }

        private void UpdateIdleHint()
        {
            if (Mode != GameMode.Exploring) return;
            var player = PlayerController.Instance;
            if (player == null) return;

            if ((player.transform.position - _lastPlayerPosition).sqrMagnitude > 0.01f)
            {
                _lastPlayerPosition = player.transform.position;
                _idleSeconds = 0f;
                _idleHintShown = false;
                return;
            }

            _idleSeconds += Time.unscaledDeltaTime;
            if (_idleSeconds > 28f && !_idleHintShown)
            {
                _idleHintShown = true;
                string objective = ProgressionRules.ObjectiveFor(CurrentSceneId,
                    Memories != null ? Memories.CollectedIds : null, Flags);
                if (!string.IsNullOrEmpty(objective))
                {
                    Notify("Hint: " + objective, 5f);
                }
            }
        }

        private void OnApplicationQuit()
        {
            SaveCheckpoint();
        }
    }
}
