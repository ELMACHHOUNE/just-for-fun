using UnityEngine;

namespace LWalida
{
    /// <summary>
    /// Persistent composition root. Created on demand by any scene so pressing Play
    /// on any generated scene works, while duplicates are destroyed on reload.
    /// </summary>
    public class GameRoot : MonoBehaviour
    {
        public static GameRoot Instance { get; private set; }

        public GameManager Game { get; private set; }
        public AssetCatalog Content { get; private set; }
        public UIRoot UI { get; private set; }

        private MemoryData[] _fallbackMemories;

        public static GameRoot EnsureExists()
        {
            if (Instance != null) return Instance;

            var existing = Object.FindFirstObjectByType<GameRoot>();
            if (existing != null)
            {
                Instance = existing;
                return existing;
            }

            var go = new GameObject("GameRoot");
            var root = go.AddComponent<GameRoot>();
            root.Build();
            return root;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Build()
        {
            if (Game != null) return;

            Content = Resources.Load<AssetCatalog>("AssetCatalog");
            if (Content == null)
            {
                Debug.LogWarning("[LWalida] AssetCatalog not found in Resources. Using narrative fallback content " +
                                 "with colour-only placeholders. Run LWalida/Build or Update MVP Scenes.");
                Content = ScriptableObject.CreateInstance<AssetCatalog>();
                var fallback = NarrativeDefaults.CreateMemoryInstances();
                _fallbackMemories = fallback.ToArray();
                Content.memories = _fallbackMemories;
            }

            Game = gameObject.AddComponent<GameManager>();
            var memories = Child<MemoryManager>("MemoryManager");
            var audio = Child<AudioManager>("AudioManager");
            var loader = Child<SceneLoader>("SceneLoader");
            var interactions = Child<InteractionManager>("InteractionManager");
            var dialogue = Child<DialogueManager>("DialogueManager");
            var cameras = Child<CameraSystem>("CameraSystem");
            var recorder = Child<SoundRecorder>("SoundRecorder");
            var objectives = Child<ObjectiveManager>("ObjectiveManager");
            var saves = Child<SaveManager>("SaveManager");

            audio.Build();

            var ui = gameObject.AddComponent<UIRoot>();
            ui.Build(Content);
            UI = ui;

            var catalogMemories = Content.memories;
            if (catalogMemories == null || catalogMemories.Length == 0)
            {
                if (_fallbackMemories == null) _fallbackMemories = NarrativeDefaults.CreateMemoryInstances().ToArray();
                catalogMemories = _fallbackMemories;
            }
            memories.Initialize(catalogMemories);

            Game.Initialize(this, Content, memories, audio, ui, loader, interactions, dialogue, cameras, recorder,
                objectives, saves);

            DontDestroyOnLoad(gameObject);
        }

        private T Child<T>(string childName) where T : Component
        {
            var go = new GameObject(childName);
            go.transform.SetParent(transform, false);
            return go.AddComponent<T>();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
