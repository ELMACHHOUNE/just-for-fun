using UnityEngine;
using UnityEngine.UI;

namespace LWalida
{
    public class JournalUI : ScreenBase
    {
        private RectTransform _grid;
        private Text _count;
        private Button _closeButton;
        private string _playingMemoryId;

        protected override void OnBuild()
        {
            var overlay = UIFactory.CreateImage("Dim", transform, Theme.Overlay, null, true);
            UIFactory.Stretch(overlay.rectTransform);

            var page = UIFactory.CreatePanel("Page", transform, Theme.ChalkWhite, UIFactory.PaperSprite, true);
            UIFactory.PlaceCenter(page, 1600f, 900f);
            page.GetComponent<Image>().color = new Color(0.949f, 0.929f, 0.890f, 0.97f);

            var title = UIFactory.CreateText("Title", page, "Journal", 52, Theme.MidnightBlue, TextAnchor.MiddleLeft,
                FontStyle.Bold);
            UIFactory.PlaceLeft(title.rectTransform, 56f, 38f, 700f, 60f);

            var subtitle = UIFactory.CreateText("Subtitle", page, "The memories you carry", 24, Theme.MemorySepia,
                TextAnchor.MiddleLeft, FontStyle.Italic);
            UIFactory.PlaceLeft(subtitle.rectTransform, 58f, 96f, 700f, 32f);

            _count = UIFactory.CreateText("Count", page, "", 24, Theme.MidnightBlue, TextAnchor.MiddleRight);
            UIFactory.Anchor(_count.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-56f, -44f), new Vector2(420f, 40f));

            var divider = UIFactory.CreateImage("Divider", page, new Color(0.66f, 0.49f, 0.32f, 0.35f));
            UIFactory.Anchor(divider.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f),
                new Vector2(56f, -128f), new Vector2(-112f, 2f));

            _grid = UIFactory.CreateRect("Grid", page);
            UIFactory.Stretch(_grid, 56f, 100f, 56f, 150f);
            var grid = _grid.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(718f, 300f);
            grid.spacing = new Vector2(24f, 22f);
            grid.padding = new RectOffset(0, 0, 0, 0);
            grid.childAlignment = TextAnchor.UpperLeft;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;

            _closeButton = UIFactory.CreateButton(page, "Close journal", () =>
            {
                if (GameManager.Instance != null) GameManager.Instance.CloseJournal();
            }, 26, 58f, Theme.MidnightBlue);
            UIFactory.Anchor(_closeButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(300f, 58f));

            Hide();
        }

        public override void Show()
        {
            base.Show();
            BuildCards();
            if (Root != null) Root.Select(_closeButton);
        }

        public override void Hide()
        {
            StopAudio();
            base.Hide();
        }

        protected override void OnHide()
        {
            StopAudio();
        }

        public void StopAudio()
        {
            var game = GameManager.Instance;
            if (game != null && game.Audio != null) game.Audio.StopMemoryClip();
            _playingMemoryId = null;
        }

        private void BuildCards()
        {
            UIFactory.DestroyChildren(_grid);

            var game = GameManager.Instance;
            var catalog = game != null ? game.Content : Content;
            int collected = 0;

            for (int i = 0; i < MemoryIds.All.Length; i++)
            {
                string id = MemoryIds.All[i];
                MemoryData memory = catalog != null ? catalog.GetMemory(id) : null;
                bool has = game != null && game.Memories != null && game.Memories.Has(id);
                if (has) collected++;
                CreateCard(_grid, id, memory, has);
            }

            if (_count != null) _count.text = collected + " of 4 memories";
        }

        private void CreateCard(Transform parent, string id, MemoryData memory, bool collected)
        {
            var card = UIFactory.CreatePanel("Card_" + id, parent, collected ? Theme.Panel : new Color(0.09f, 0.16f, 0.19f, 0.9f),
                UIFactory.PanelSprite, true);

            if (collected && memory != null && memory.image != null)
            {
                var photo = UIFactory.CreateImage("Photo", card, Color.white, memory.image);
                UIFactory.PlaceLeft(photo.rectTransform, 18f, 18f, 250f, 264f);
                photo.preserveAspect = true;
            }
            else
            {
                var photo = UIFactory.CreatePanel("PhotoPlaceholder", card, new Color(0.03f, 0.06f, 0.08f, 0.85f),
                    UIFactory.PanelSprite, false);
                UIFactory.PlaceLeft(photo, 18f, 18f, 250f, 264f);

                if (!collected && Content != null && Content.iconLock != null)
                {
                    var lockIcon = UIFactory.CreateImage("Lock", photo, new Color(1f, 1f, 1f, 0.55f),
                        Content.iconLock);
                    UIFactory.Anchor(lockIcon.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                        new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(96f, 96f));
                    lockIcon.preserveAspect = true;
                }
            }

            string title = collected && memory != null ? memory.title : "Locked memory";
            string description = collected && memory != null ? memory.description : LockedHint(id);

            var titleText = UIFactory.CreateText("Title", card, title, 27,
                collected ? Theme.HassanSand : Theme.TextDim, TextAnchor.UpperLeft, FontStyle.Bold);
            UIFactory.Anchor(titleText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f),
                new Vector2(288f, -22f), new Vector2(-312f, 40f));

            var descriptionText = UIFactory.CreateText("Description", card, description, 21,
                collected ? Theme.ChalkWhite : Theme.TextDim, TextAnchor.UpperLeft);
            UIFactory.Anchor(descriptionText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0f, 1f), new Vector2(288f, -70f), new Vector2(-312f, 126f));

            if (collected && memory != null)
            {
                bool hasAudio = memory.audio != null;
                var button = UIFactory.CreateButton(card, hasAudio ? "Play sound" : "No audio",
                    () => ToggleAudio(memory, id), 21, 44f, hasAudio ? Theme.OudayaBlue : Theme.Disabled);
                UIFactory.Anchor(button.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(0f, 0f),
                    new Vector2(0f, 0f), new Vector2(288f, 20f), new Vector2(230f, 44f));
                button.interactable = hasAudio;
                if (hasAudio && memory.audioIsPlaceholder)
                {
                    var note = UIFactory.CreateText("AudioNote", card, "Placeholder ambience", 17, Theme.TextDim,
                        TextAnchor.LowerLeft, FontStyle.Italic);
                    UIFactory.Anchor(note.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f),
                        new Vector2(0f, 0f), new Vector2(288f, 66f), new Vector2(-312f, 22f));
                }

                var type = UIFactory.CreateText("Type", card,
                    memory.type == MemoryType.Sound ? "SOUND MEMORY" : "PHOTOGRAPH", 16, Theme.MemorySepia,
                    TextAnchor.LowerRight, FontStyle.Bold);
                UIFactory.Anchor(type.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
                    new Vector2(-20f, 26f), new Vector2(150f, 22f));
            }
            else
            {
                var hint = UIFactory.CreateText("Hint", card, "Return to the medina to find it.", 18, Theme.TextDim,
                    TextAnchor.LowerRight, FontStyle.Italic);
                UIFactory.Anchor(hint.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
                    new Vector2(-20f, 20f), new Vector2(320f, 22f));
            }
        }

        private void ToggleAudio(MemoryData memory, string id)
        {
            var game = GameManager.Instance;
            if (game == null || game.Audio == null || memory == null) return;

            if (_playingMemoryId == id && game.Audio.IsMemoryClipPlaying)
            {
                game.Audio.StopMemoryClip();
                _playingMemoryId = null;
                return;
            }

            game.Audio.PlayMemoryClip(memory.audio);
            _playingMemoryId = id;
        }

        private static string LockedHint(string id)
        {
            switch (id)
            {
                case MemoryIds.Cafe: return "A photograph on a caf\u00e9 table.";
                case MemoryIds.Door: return "Somewhere in the blue streets, a door is waiting.";
                case MemoryIds.Amina: return "Someone here remembers your mother.";
                case MemoryIds.Souk: return "The medina has a sound worth keeping.";
                default: return "Not found yet.";
            }
        }
    }
}
