using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LWalida.EditorTools
{
    public static class LWalidaPaths
    {
        public const string GameRoot = "Assets/_Game";
        public const string ArtRoot = GameRoot + "/Art";
        public const string EnvironmentArt = ArtRoot + "/Environments";
        public const string PlaceholderArt = GameRoot + "/Generated/Placeholders/Art";
        public const string AudioRoot = GameRoot + "/Audio";
        public const string PlaceholderAudio = GameRoot + "/Generated/Placeholders/Audio";
        public const string DataRoot = GameRoot + "/Data";
        public const string Memories = DataRoot + "/Memories";
        public const string Dialogues = DataRoot + "/Dialogue";
        public const string Resources = GameRoot + "/Resources";
        public const string Prefabs = GameRoot + "/Prefabs";
        public const string Scenes = GameRoot + "/Scenes";
        public const string CatalogPath = Resources + "/AssetCatalog.asset";
    }

    /// <summary>
    /// Generates small, original placeholder art so the project can run with no
    /// external art. Existing files are never overwritten, so supplied replacements
    /// survive re-running the builder.
    /// </summary>
    public static class PlaceholderArtGenerator
    {
        public static readonly string[] PanelFiles = { "panelDark", "panelLight" };

        public static void GenerateMissing()
        {
            EnsureFolder(LWalidaPaths.PlaceholderArt);

            Sky("skyCafe", new Color(0.98f, 0.72f, 0.44f), new Color(0.44f, 0.32f, 0.32f));
            Sky("skyOudaya", new Color(0.55f, 0.78f, 0.82f), new Color(0.12f, 0.31f, 0.39f));
            Sky("skySouk", new Color(0.97f, 0.67f, 0.36f), new Color(0.42f, 0.24f, 0.18f));
            Sky("skyHassan", new Color(0.99f, 0.58f, 0.31f), new Color(0.37f, 0.19f, 0.22f));
            Sky("skyHome", new Color(0.18f, 0.25f, 0.38f), new Color(0.03f, 0.05f, 0.10f));

            Skyline("farCafe", new Color(0.27f, 0.2f, 0.18f), false);
            Skyline("farOudaya", new Color(0.09f, 0.24f, 0.31f), true);
            Skyline("farSouk", new Color(0.29f, 0.17f, 0.13f), true);
            Skyline("farHassan", new Color(0.31f, 0.16f, 0.16f), false);
            Skyline("farHome", new Color(0.04f, 0.08f, 0.14f), true);

            Ground("groundStrip");

            Character("playerIdle", new Color(0.13f, 0.31f, 0.36f), new Color(0.82f, 0.7f, 0.52f),
                new Color(0.14f, 0.16f, 0.18f));
            Character("aminaIdle", new Color(0.19f, 0.24f, 0.42f), new Color(0.94f, 0.9f, 0.82f),
                new Color(0.5f, 0.52f, 0.55f));
            Character("allalIdle", new Color(0.41f, 0.25f, 0.16f), new Color(0.88f, 0.76f, 0.55f),
                new Color(0.25f, 0.19f, 0.15f));
            Character("neighborIdle", new Color(0.28f, 0.3f, 0.31f), new Color(0.9f, 0.87f, 0.79f),
                new Color(0.55f, 0.56f, 0.56f));

            Portrait("portraitPlayer", new Color(0.13f, 0.31f, 0.36f));
            Portrait("portraitAmina", new Color(0.19f, 0.24f, 0.42f));
            Portrait("portraitAllal", new Color(0.41f, 0.25f, 0.16f));
            Portrait("portraitNeighbor", new Color(0.28f, 0.3f, 0.31f));

            Door("doorCorrect", true, true);
            Door("doorWrong01", false, false);
            Door("doorWrong02", false, true);

            Create("photoProp", 120, 90, Photo);
            Create("referencePhoto", 120, 150, ReferencePhoto);
            Create("teaSet", 90, 60, Tea);
            Create("flowerpot", 70, 90, FlowerPot);
            Create("bougainvillea", 160, 220, Bougainvillea);
            Create("stall", 260, 220, Stall);
            Create("exitMarker", 64, 96, ExitMarker);
            Create("soundMarker", 64, 96, SoundMarker);

            Create("memoryCafe", 512, 384, MemoryCafe);
            Create("memoryDoor", 512, 384, MemoryDoor);
            Create("memoryAmina", 512, 384, MemoryAmina);
            Create("memorySouk", 512, 384, MemorySouk);

            Create("logoEmblem", 256, 256, Emblem);
            Create("menuBackground", 1920, 1080, MenuBackdrop);
            Create("panelDark", 64, 64, p => Panel(p, new Color(0.055f, 0.12f, 0.145f, 0.97f),
                new Color(0.7f, 0.56f, 0.38f, 0.55f)));
            Create("panelLight", 64, 64, p => Panel(p, new Color(0.95f, 0.93f, 0.87f, 0.98f),
                new Color(0.55f, 0.41f, 0.27f, 0.35f)));
            Create("paperTexture", 512, 512, Paper);
            Create("zelligePattern", 256, 256, Zellige);
            Create("vignette", 512, 512, Vignette);
            Create("grain", 256, 256, Grain);
            Create("iconCamera", 96, 96, IconCamera);
            Create("iconJournal", 96, 96, IconJournal);
            Create("iconRecord", 96, 96, IconRecord);
            Create("iconLock", 96, 96, IconLock);
            Create("iconAudio", 96, 96, IconAudio);
            Create("whitePixel", 8, 8, p => p.Fill(Color.white));

            AssetDatabase.Refresh();
            ConfigureImporters();
            AssetDatabase.SaveAssets();
        }

        public static string PathFor(string name)
        {
            return LWalidaPaths.PlaceholderArt + "/" + name + ".png";
        }

        private static void Sky(string name, Color top, Color bottom)
        {
            Create(name, 512, 256, p =>
            {
                p.GradientV(top, bottom);
                p.Ellipse(390, 180, 44, 44, new Color(1f, 0.93f, 0.76f, 0.5f));
                p.Ellipse(390, 180, 30, 30, new Color(1f, 0.97f, 0.86f, 0.7f));
                for (int i = 0; i < 22; i++)
                {
                    float x = (i * 73f) % 512f;
                    float y = 40f + (i * 37f) % 150f;
                    p.Ellipse(x, y, 22f + (i % 4) * 8f, 7f + (i % 3) * 3f,
                        new Color(1f, 1f, 1f, 0.045f));
                }
            });
        }

        private static void Skyline(string name, Color color, bool towers)
        {
            Create(name, 1024, 360, p =>
            {
                var random = new System.Random(name.GetHashCode());
                int x = -20;
                while (x < 1044)
                {
                    int w = 42 + random.Next(90);
                    int h = 70 + random.Next(150);
                    p.Rect(x, 0, w, h, color);
                    if (random.NextDouble() > 0.55)
                    {
                        p.Rect(x + w / 3, h, w / 3, 24 + random.Next(40), color);
                    }
                    for (int wy = 30; wy < h - 12; wy += 34)
                    {
                        for (int wx = x + 10; wx < x + w - 10; wx += 26)
                        {
                            if (random.NextDouble() > 0.72)
                            {
                                p.Rect(wx, wy, 9, 14, new Color(1f, 0.82f, 0.48f, 0.22f));
                            }
                        }
                    }
                    x += w - 8;
                }
                if (towers)
                {
                    p.Rect(720, 100, 42, 230, color);
                    p.Rect(728, 330, 26, 30, color);
                }
            });
        }

        private static void Ground(string name)
        {
            Create(name, 512, 128, p =>
            {
                p.GradientV(new Color(0.35f, 0.29f, 0.23f), new Color(0.19f, 0.16f, 0.14f));
                var random = new System.Random(771);
                for (int i = 0; i < 150; i++)
                {
                    int x = random.Next(512);
                    int y = random.Next(128);
                    int w = 10 + random.Next(30);
                    int h = 4 + random.Next(8);
                    var shade = 0.16f + (float)random.NextDouble() * 0.16f;
                    p.Rect(x, y, w, h, new Color(shade, shade * 0.86f, shade * 0.7f, 0.8f));
                }
                p.Rect(0, 118, 512, 10, new Color(0.62f, 0.53f, 0.4f, 0.55f));
            });
        }

        private static void Character(string name, Color robe, Color scarf, Color hair)
        {
            Create(name, 110, 210, p =>
            {
                p.Ellipse(55, 6, 34, 7, new Color(0f, 0f, 0f, 0.18f));
                p.RoundRect(24, 14, 62, 138, 16, robe);
                p.Rect(30, 74, 50, 78, robe);
                p.RoundRect(18, 92, 16, 52, 8, robe * 0.92f);
                p.RoundRect(76, 92, 16, 52, 8, robe * 0.92f);
                p.Rect(38, 10, 12, 12, new Color(0.16f, 0.13f, 0.12f));
                p.Rect(60, 10, 12, 12, new Color(0.16f, 0.13f, 0.12f));
                p.Ellipse(55, 174, 23, 25, new Color(0.74f, 0.56f, 0.42f));
                p.Ellipse(55, 184, 24, 18, hair);
                p.Rect(35, 150, 40, 18, scarf);
                p.Rect(46, 136, 18, 20, scarf * 0.9f);
                p.Ellipse(47, 174, 2.4f, 2.4f, new Color(0.1f, 0.08f, 0.07f));
                p.Ellipse(63, 174, 2.4f, 2.4f, new Color(0.1f, 0.08f, 0.07f));
                p.Rect(50, 162, 10, 2, new Color(0.55f, 0.35f, 0.28f, 0.7f));
                p.Rect(38, 28, 34, 3, new Color(1f, 1f, 1f, 0.12f));
            });
        }

        private static void Portrait(string name, Color robe)
        {
            Create(name, 256, 256, p =>
            {
                p.Fill(new Color(0.045f, 0.09f, 0.11f, 1f));
                p.Ellipse(128, 280, 150, 130, robe * 0.75f);
                p.Ellipse(128, 82, 58, 64, robe);
                p.Ellipse(128, 168, 44, 48, new Color(0.73f, 0.55f, 0.41f));
                p.Ellipse(128, 190, 48, 36, new Color(0.13f, 0.11f, 0.1f));
                p.Rect(86, 132, 84, 18, new Color(0.92f, 0.88f, 0.8f, 0.9f));
                p.Ellipse(112, 170, 4, 4, new Color(0.1f, 0.08f, 0.07f));
                p.Ellipse(144, 170, 4, 4, new Color(0.1f, 0.08f, 0.07f));
                p.Rect(120, 152, 16, 3, new Color(0.55f, 0.34f, 0.27f, 0.8f));
                for (int i = 0; i < 40; i++)
                {
                    p.Rect((i * 67) % 256, (i * 41) % 256, 2, 2, new Color(1f, 1f, 1f, 0.05f));
                }
            });
        }

        private static void Door(string name, bool correct, bool square)
        {
            Create(name, 160, 320, p =>
            {
                p.Fill(Color.clear);
                Color wood = correct
                    ? new Color(0.16f, 0.36f, 0.52f)
                    : square ? new Color(0.24f, 0.19f, 0.16f) : new Color(0.23f, 0.34f, 0.42f);
                Color frame = correct ? new Color(0.09f, 0.24f, 0.34f) : new Color(0.14f, 0.11f, 0.1f);
                p.Rect(18, 0, 124, 250, frame);
                p.Rect(26, 0, 108, 244, wood);
                if (square)
                {
                    p.Rect(26, 244, 108, 24, frame);
                }
                else if (correct)
                {
                    p.Ellipse(80, 250, 60, 74, frame);
                    p.Ellipse(80, 250, 50, 64, wood);
                    p.Rect(30, 250, 100, 40, wood);
                }
                else
                {
                    p.Triangle(24, 252, 80, 320, 136, 252, frame);
                    p.Triangle(34, 252, 80, 306, 126, 252, wood);
                }

                for (int row = 0; row < 3; row++)
                {
                    for (int col = 0; col < 5; col++)
                    {
                        float x = 44 + col * 18f;
                        float y = 188 - row * 34f;
                        if (correct)
                        {
                            p.Ellipse(x, y, 3.6f, 3.6f, new Color(0.72f, 0.56f, 0.28f));
                        }
                        else
                        {
                            p.Rect((int)x - 2, (int)y - 4, 5, 9, new Color(0.12f, 0.1f, 0.09f));
                        }
                    }
                }

                if (correct)
                {
                    p.Ellipse(58, 128, 9, 9, new Color(0.78f, 0.6f, 0.28f));
                    p.Ellipse(102, 128, 9, 9, new Color(0.78f, 0.6f, 0.28f));
                    p.Ellipse(58, 128, 4, 4, new Color(0.28f, 0.21f, 0.12f));
                    p.Ellipse(102, 128, 4, 4, new Color(0.28f, 0.21f, 0.12f));
                }
                else
                {
                    p.Ellipse(80, 132, 6, 6, new Color(0.35f, 0.28f, 0.18f));
                }

                p.Rect(24, 0, 112, 5, new Color(0f, 0f, 0f, 0.25f));
                p.Ellipse(80, 2, 64, 5, new Color(0f, 0f, 0f, 0.22f));
            });
        }

        private static void Photo(Painter p)
        {
            p.Fill(new Color(0.86f, 0.83f, 0.74f));
            p.Rect(8, 8, 104, 74, new Color(0.72f, 0.62f, 0.47f));
            p.Rect(20, 20, 80, 50, new Color(0.2f, 0.38f, 0.52f));
            p.Ellipse(60, 50, 20, 22, new Color(0.32f, 0.52f, 0.66f));
            p.Rect(44, 20, 32, 22, new Color(0.32f, 0.52f, 0.66f));
            p.Ellipse(49, 38, 3, 3, new Color(0.1f, 0.09f, 0.08f));
            p.Ellipse(71, 38, 3, 3, new Color(0.1f, 0.09f, 0.08f));
            p.Rect(28, 14, 64, 2, new Color(0.4f, 0.32f, 0.22f, 0.6f));
        }

        private static void ReferencePhoto(Painter p)
        {
            p.Fill(new Color(0.86f, 0.83f, 0.74f));
            p.Rect(10, 10, 100, 130, new Color(0.2f, 0.38f, 0.52f));
            p.Ellipse(60, 112, 30, 34, new Color(0.3f, 0.5f, 0.64f));
            p.Rect(30, 10, 60, 104, new Color(0.3f, 0.5f, 0.64f));
            p.Ellipse(46, 76, 5, 5, new Color(0.78f, 0.6f, 0.28f));
            p.Ellipse(74, 76, 5, 5, new Color(0.78f, 0.6f, 0.28f));
            p.Rect(20, 40, 82, 2, new Color(1f, 1f, 1f, 0.2f));
        }

        private static void Tea(Painter p)
        {
            p.Ellipse(45, 14, 40, 7, new Color(0.55f, 0.46f, 0.3f));
            p.Rect(15, 16, 60, 5, new Color(0.7f, 0.58f, 0.38f));
            p.Rect(34, 22, 12, 24, new Color(0.84f, 0.78f, 0.6f, 0.9f));
            p.Rect(50, 22, 12, 24, new Color(0.84f, 0.78f, 0.6f, 0.9f));
            p.Rect(35, 30, 10, 12, new Color(0.34f, 0.45f, 0.22f));
            p.Rect(51, 30, 10, 12, new Color(0.34f, 0.45f, 0.22f));
            p.Rect(28, 42, 34, 3, new Color(0.7f, 0.58f, 0.38f));
        }

        private static void FlowerPot(Painter p)
        {
            p.Ellipse(35, 6, 24, 5, new Color(0f, 0f, 0f, 0.18f));
            p.Rect(20, 12, 30, 28, new Color(0.62f, 0.32f, 0.2f));
            p.Rect(16, 38, 38, 7, new Color(0.72f, 0.4f, 0.24f));
            p.Ellipse(35, 56, 22, 20, new Color(0.19f, 0.36f, 0.22f));
            p.Ellipse(24, 64, 10, 12, new Color(0.23f, 0.42f, 0.25f));
            p.Ellipse(46, 62, 11, 13, new Color(0.16f, 0.33f, 0.2f));
            p.Ellipse(35, 72, 8, 9, new Color(0.26f, 0.45f, 0.27f));
        }

        private static void Bougainvillea(Painter p)
        {
            p.Fill(Color.clear);
            var random = new System.Random(19);
            for (int i = 0; i < 30; i++)
            {
                float x = 20 + (float)random.NextDouble() * 120f;
                float y = 20 + (float)random.NextDouble() * 190f;
                p.Line(80, 20, x, y, 2f, new Color(0.15f, 0.28f, 0.16f));
            }
            for (int i = 0; i < 80; i++)
            {
                float x = 10 + (float)random.NextDouble() * 140f;
                float y = 30 + (float)random.NextDouble() * 180f;
                p.Ellipse(x, y, 3 + (float)random.NextDouble() * 4, 3 + (float)random.NextDouble() * 4,
                    new Color(0.62f, 0.12f, 0.34f, 0.9f));
            }
            for (int i = 0; i < 28; i++)
            {
                float x = 10 + (float)random.NextDouble() * 140f;
                float y = 30 + (float)random.NextDouble() * 180f;
                p.Ellipse(x, y, 3, 2, new Color(0.21f, 0.39f, 0.2f, 0.8f));
            }
        }

        private static void Stall(Painter p)
        {
            p.Ellipse(130, 8, 120, 10, new Color(0f, 0f, 0f, 0.2f));
            p.Rect(20, 60, 220, 110, new Color(0.26f, 0.18f, 0.13f));
            p.Rect(20, 60, 220, 10, new Color(0.18f, 0.12f, 0.09f));
            for (int i = 0; i < 6; i++)
            {
                Color stripe = i % 2 == 0 ? new Color(0.76f, 0.55f, 0.27f) : new Color(0.7f, 0.26f, 0.2f);
                p.Triangle(18 + i * 37, 168, 36 + i * 37, 210, 55 + i * 37, 168, stripe);
            }
            p.Rect(28, 92, 44, 30, new Color(0.72f, 0.42f, 0.16f));
            p.Rect(80, 98, 38, 24, new Color(0.42f, 0.55f, 0.24f));
            p.Rect(126, 90, 42, 32, new Color(0.62f, 0.24f, 0.2f));
            p.Rect(176, 96, 52, 26, new Color(0.32f, 0.42f, 0.46f));
            p.Rect(20, 54, 220, 6, new Color(0.16f, 0.11f, 0.08f));
        }

        private static void ExitMarker(Painter p)
        {
            p.Fill(Color.clear);
            p.Ellipse(32, 36, 24, 26, new Color(0.16f, 0.42f, 0.5f, 0.9f));
            p.Rect(8, 8, 48, 30, new Color(0.16f, 0.42f, 0.5f, 0.9f));
            p.Rect(24, 14, 16, 30, new Color(0.05f, 0.12f, 0.14f));
            p.Triangle(26, 62, 38, 62, 32, 82, new Color(0.95f, 0.85f, 0.62f));
            p.Rect(30, 74, 4, 12, new Color(0.95f, 0.85f, 0.62f));
        }

        private static void SoundMarker(Painter p)
        {
            p.Fill(Color.clear);
            p.Ellipse(32, 46, 20, 20, new Color(0.86f, 0.62f, 0.26f, 0.9f));
            p.Ellipse(32, 46, 12, 12, new Color(0.1f, 0.2f, 0.22f));
            p.Rect(30, 28, 4, 12, new Color(0.1f, 0.2f, 0.22f));
            for (int i = 0; i < 3; i++)
            {
                p.Line(32 + i * 10, 60, 32 + i * 10, 32, 2.5f, new Color(0.95f, 0.85f, 0.62f, 0.9f));
                p.Line(32 - i * 10, 60, 32 - i * 10, 32, 2.5f, new Color(0.95f, 0.85f, 0.62f, 0.9f));
            }
        }

        private static void MemoryCafe(Painter p)
        {
            p.GradientV(new Color(0.55f, 0.37f, 0.24f), new Color(0.2f, 0.13f, 0.11f));
            p.Rect(0, 0, 512, 90, new Color(0.35f, 0.24f, 0.17f));
            p.Rect(60, 80, 392, 190, new Color(0.28f, 0.34f, 0.36f, 0.7f));
            p.Ellipse(190, 270, 120, 84, new Color(0.81f, 0.66f, 0.44f, 0.35f));
            p.Rect(120, 96, 272, 22, new Color(0.5f, 0.36f, 0.24f));
            p.Rect(190, 112, 24, 42, new Color(0.62f, 0.48f, 0.34f));
            p.Rect(300, 112, 24, 42, new Color(0.62f, 0.48f, 0.34f));
            p.Rect(236, 132, 60, 8, new Color(0.7f, 0.56f, 0.36f));
            p.Rect(228, 118, 76, 50, new Color(0.86f, 0.83f, 0.74f));
            p.Rect(240, 128, 52, 32, new Color(0.22f, 0.38f, 0.52f));
            p.Ellipse(266, 146, 12, 14, new Color(0.32f, 0.52f, 0.66f));
            p.Ellipse(266, 146, 3, 3, new Color(0.78f, 0.6f, 0.28f));
        }

        private static void MemoryDoor(Painter p)
        {
            p.GradientV(new Color(0.34f, 0.55f, 0.63f), new Color(0.09f, 0.24f, 0.33f));
            p.Rect(0, 0, 512, 70, new Color(0.38f, 0.31f, 0.24f));
            p.Rect(150, 60, 212, 250, new Color(0.12f, 0.28f, 0.4f));
            p.Ellipse(256, 310, 110, 84, new Color(0.12f, 0.28f, 0.4f));
            p.Ellipse(256, 310, 88, 64, new Color(0.17f, 0.38f, 0.54f));
            p.Rect(168, 60, 176, 244, new Color(0.17f, 0.38f, 0.54f));
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 5; col++)
                {
                    p.Ellipse(196 + col * 30, 220 - row * 42, 5, 5, new Color(0.76f, 0.6f, 0.3f));
                }
            }
            p.Ellipse(222, 160, 12, 12, new Color(0.8f, 0.62f, 0.3f));
            p.Ellipse(290, 160, 12, 12, new Color(0.8f, 0.62f, 0.3f));
            p.Rect(0, 40, 512, 2, new Color(1f, 1f, 1f, 0.12f));
        }

        private static void MemoryAmina(Painter p)
        {
            p.GradientV(new Color(0.23f, 0.27f, 0.42f), new Color(0.08f, 0.09f, 0.16f));
            p.Rect(80, 60, 352, 250, new Color(0.12f, 0.16f, 0.25f));
            p.Rect(120, 90, 90, 190, new Color(0.9f, 0.73f, 0.42f, 0.28f));
            p.Ellipse(256, 268, 80, 86, new Color(0.72f, 0.55f, 0.42f));
            p.Ellipse(256, 304, 92, 66, new Color(0.86f, 0.83f, 0.76f));
            p.Rect(186, 150, 140, 120, new Color(0.2f, 0.24f, 0.4f));
            p.Rect(196, 186, 120, 18, new Color(0.92f, 0.89f, 0.82f));
            p.Ellipse(228, 270, 6, 6, new Color(0.1f, 0.08f, 0.07f));
            p.Ellipse(284, 270, 6, 6, new Color(0.1f, 0.08f, 0.07f));
            p.Rect(236, 240, 40, 5, new Color(0.5f, 0.3f, 0.26f));
            for (int i = 0; i < 18; i++)
            {
                p.Rect((i * 97) % 512, (i * 61) % 384, 3, 3, new Color(1f, 1f, 1f, 0.07f));
            }
        }

        private static void MemorySouk(Painter p)
        {
            p.GradientV(new Color(0.72f, 0.45f, 0.24f), new Color(0.26f, 0.15f, 0.11f));
            for (int i = 0; i < 3; i++)
            {
                int x = 40 + i * 160;
                p.Rect(x, 60, 110, 250, new Color(0.34f, 0.21f, 0.15f));
                p.Ellipse(x + 55, 310, 58, 60, new Color(0.12f, 0.09f, 0.08f));
                p.Ellipse(x + 55, 310, 44, 44, new Color(0.56f, 0.46f, 0.32f, 0.35f));
            }
            for (int i = 0; i < 9; i++)
            {
                float x = 46 + i * 50f;
                p.Ellipse(x, 330, 4, 4, new Color(1f, 0.86f, 0.5f, 0.8f));
                p.Line(x, 334, x, 356, 1.5f, new Color(0.1f, 0.08f, 0.07f));
            }
            p.Rect(0, 0, 512, 70, new Color(0.4f, 0.28f, 0.2f));
            for (int i = 0; i < 14; i++)
            {
                float x = 20 + i * 36f;
                float y = 78 + (i % 3) * 10;
                p.Ellipse(x, y, 10, 20, new Color(0.68f, 0.2f, 0.2f, 0.75f));
            }
        }

        private static void Emblem(Painter p)
        {
            p.Fill(Color.clear);
            p.Ellipse(128, 128, 112, 112, new Color(0.08f, 0.2f, 0.24f, 0.9f));
            p.Ellipse(128, 128, 100, 100, new Color(0.72f, 0.55f, 0.31f, 0.75f));
            p.Ellipse(128, 128, 92, 92, new Color(0.07f, 0.17f, 0.21f, 0.95f));
            p.Rect(84, 66, 88, 90, new Color(0.78f, 0.6f, 0.32f));
            p.Ellipse(128, 156, 44, 40, new Color(0.78f, 0.6f, 0.32f));
            p.Rect(96, 66, 64, 90, new Color(0.1f, 0.22f, 0.28f));
            p.Ellipse(128, 156, 32, 28, new Color(0.1f, 0.22f, 0.28f));
            p.Ellipse(111, 118, 5, 5, new Color(0.8f, 0.62f, 0.3f));
            p.Ellipse(145, 118, 5, 5, new Color(0.8f, 0.62f, 0.3f));
            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI * 0.25f;
                p.Ellipse(128 + Mathf.Cos(angle) * 78, 128 + Mathf.Sin(angle) * 78, 6, 6,
                    new Color(0.86f, 0.72f, 0.46f, 0.8f));
            }
        }

        private static void MenuBackdrop(Painter p)
        {
            p.GradientV(new Color(0.11f, 0.26f, 0.32f), new Color(0.03f, 0.07f, 0.09f));
            p.Ellipse(1450, 760, 220, 220, new Color(1f, 0.82f, 0.52f, 0.25f));
            p.Rect(980, 0, 940, 1080, new Color(0.05f, 0.13f, 0.17f, 0.85f));
            p.Ellipse(1260, 900, 210, 270, new Color(0.08f, 0.2f, 0.25f));
            p.Rect(1080, 0, 360, 840, new Color(0.08f, 0.2f, 0.25f));
            p.Ellipse(1260, 900, 160, 220, new Color(0.03f, 0.07f, 0.09f));
            p.Rect(1120, 0, 280, 830, new Color(0.03f, 0.07f, 0.09f));
            for (int i = 0; i < 40; i++)
            {
                p.Rect((i * 137) % 1920, (i * 89) % 1080, 3, 3, new Color(1f, 1f, 1f, 0.045f));
            }
        }

        private static void Panel(Painter p, Color fill, Color border)
        {
            p.Fill(Color.clear);
            p.RoundRect(0, 0, p.W, p.H, 10, fill);
            p.RoundRect(1, 1, p.W - 2, p.H - 2, 9, border);
            p.RoundRect(4, 4, p.W - 8, p.H - 8, 7, fill);
            for (int i = 0; i < 12; i++)
            {
                p.Rect(6 + i * 5, 6, 2, 2, new Color(0.85f, 0.7f, 0.45f, 0.25f));
            }
        }

        private static void Paper(Painter p)
        {
            p.Fill(new Color(0.94f, 0.92f, 0.87f));
            var random = new System.Random(13);
            for (int i = 0; i < 5200; i++)
            {
                int x = random.Next(512);
                int y = random.Next(512);
                float a = 0.015f + (float)random.NextDouble() * 0.03f;
                p.Rect(x, y, 1 + random.Next(2), 1 + random.Next(2),
                    random.NextDouble() > 0.5 ? new Color(0.6f, 0.5f, 0.36f, a) : new Color(1f, 1f, 1f, a));
            }
        }

        private static void Zellige(Painter p)
        {
            p.Fill(new Color(0.06f, 0.16f, 0.2f, 0.8f));
            Color line = new Color(0.76f, 0.6f, 0.33f, 0.7f);
            for (int y = 0; y < 256; y += 32)
            {
                for (int x = 0; x < 256; x += 32)
                {
                    p.Rect(x + 2, y + 2, 28, 28, new Color(0.09f, 0.22f, 0.27f, 0.7f));
                    p.Triangle(x + 16, y + 5, x + 27, y + 16, x + 16, y + 27, line);
                    p.Triangle(x + 16, y + 5, x + 5, y + 16, x + 16, y + 27, line);
                }
            }
        }

        private static void Vignette(Painter p)
        {
            p.Fill(Color.clear);
            float cx = 256f;
            float cy = 256f;
            for (int y = 0; y < 512; y++)
            {
                for (int x = 0; x < 512; x++)
                {
                    float dx = (x - cx) / cx;
                    float dy = (y - cy) / cy;
                    float d = Mathf.Clamp01(Mathf.Sqrt(dx * dx + dy * dy) - 0.45f);
                    p.Set(x, y, new Color(0f, 0f, 0f, Mathf.Pow(d, 1.7f) * 0.85f));
                }
            }
        }

        private static void Grain(Painter p)
        {
            var random = new System.Random(31);
            for (int y = 0; y < p.H; y++)
            {
                for (int x = 0; x < p.W; x++)
                {
                    float a = (float)random.NextDouble() * 0.5f;
                    p.Set(x, y, new Color(1f, 0.97f, 0.9f, a));
                }
            }
        }

        private static void IconCamera(Painter p)
        {
            p.Fill(Color.clear);
            p.RoundRect(10, 24, 76, 48, 8, new Color(0.94f, 0.91f, 0.84f));
            p.Rect(30, 18, 28, 10, new Color(0.94f, 0.91f, 0.84f));
            p.Ellipse(48, 48, 20, 20, new Color(0.1f, 0.2f, 0.23f));
            p.Ellipse(48, 48, 13, 13, new Color(0.7f, 0.58f, 0.35f));
            p.Ellipse(48, 48, 7, 7, new Color(0.1f, 0.16f, 0.2f));
            p.Rect(68, 62, 10, 6, new Color(0.7f, 0.58f, 0.35f));
        }

        private static void IconJournal(Painter p)
        {
            p.Fill(Color.clear);
            p.RoundRect(16, 12, 64, 74, 7, new Color(0.78f, 0.61f, 0.34f));
            p.Rect(26, 20, 48, 58, new Color(0.93f, 0.9f, 0.83f));
            p.Rect(26, 20, 5, 58, new Color(0.55f, 0.4f, 0.24f));
            for (int i = 0; i < 4; i++) p.Rect(38, 32 + i * 11, 28, 2, new Color(0.3f, 0.38f, 0.4f));
        }

        private static void IconRecord(Painter p)
        {
            p.Fill(Color.clear);
            p.RoundRect(40, 42, 16, 34, 8, new Color(0.9f, 0.87f, 0.8f));
            p.Ellipse(48, 42, 20, 10, new Color(0.9f, 0.87f, 0.8f));
            p.Line(48, 20, 48, 36, 4, new Color(0.9f, 0.87f, 0.8f));
            p.Rect(30, 14, 36, 5, new Color(0.9f, 0.87f, 0.8f));
        }

        private static void IconLock(Painter p)
        {
            p.Fill(Color.clear);
            p.RoundRect(22, 14, 52, 42, 8, new Color(0.9f, 0.86f, 0.78f));
            p.RoundRect(32, 46, 32, 36, 16, new Color(0.9f, 0.86f, 0.78f));
            p.Rect(38, 46, 20, 30, new Color(0.9f, 0.86f, 0.78f));
            p.Ellipse(48, 34, 6, 6, new Color(0.18f, 0.24f, 0.26f));
        }

        private static void IconAudio(Painter p)
        {
            p.Fill(Color.clear);
            p.Triangle(18, 38, 36, 52, 36, 24, new Color(0.92f, 0.88f, 0.8f));
            p.Rect(36, 32, 12, 12, new Color(0.92f, 0.88f, 0.8f));
            for (int i = 0; i < 3; i++)
            {
                p.Line(54 + i * 7, 40 + i * 8, 54 + i * 7, 40 - i * 8, 2.5f, new Color(0.92f, 0.88f, 0.8f));
            }
        }

        private static void Create(string name, int width, int height, Action<Painter> draw)
        {
            string path = PathFor(name);
            if (File.Exists(path)) return;
            var painter = new Painter(width, height, Color.clear);
            draw(painter);
            File.WriteAllBytes(path, painter.ToTexture().EncodeToPNG());
        }

        private static void ConfigureImporters()
        {
            foreach (string name in KnownNames)
            {
                string path = PathFor(name);
                if (!File.Exists(path)) continue;
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 100f;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.maxTextureSize = 2048;

                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteMeshType = SpriteMeshType.FullRect;
                settings.spriteAlignment = (int)SpriteAlignment.Center;
                bool panel = name == "panelDark" || name == "panelLight";
                settings.spriteBorder = panel ? new Vector4(12f, 12f, 12f, 12f) : Vector4.zero;
                importer.SetTextureSettings(settings);
                importer.SaveAndReimport();
            }
        }

        public static readonly string[] KnownNames =
        {
            "skyCafe", "skyOudaya", "skySouk", "skyHassan", "skyHome",
            "farCafe", "farOudaya", "farSouk", "farHassan", "farHome", "groundStrip",
            "playerIdle", "aminaIdle", "allalIdle", "neighborIdle",
            "portraitPlayer", "portraitAmina", "portraitAllal", "portraitNeighbor",
            "doorCorrect", "doorWrong01", "doorWrong02", "photoProp", "referencePhoto", "teaSet",
            "flowerpot", "bougainvillea", "stall", "exitMarker", "soundMarker",
            "memoryCafe", "memoryDoor", "memoryAmina", "memorySouk",
            "logoEmblem", "menuBackground", "panelDark", "panelLight", "paperTexture", "zelligePattern",
            "vignette", "grain", "iconCamera", "iconJournal", "iconRecord", "iconLock", "iconAudio",
            "whitePixel"
        };

        public static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string leaf = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        public class Painter
        {
            public readonly int W;
            public readonly int H;
            public readonly Color[] Pixels;

            public Painter(int width, int height, Color clear)
            {
                W = width;
                H = height;
                Pixels = new Color[width * height];
                Fill(clear);
            }

            public void Fill(Color color)
            {
                for (int i = 0; i < Pixels.Length; i++) Pixels[i] = color;
            }

            public void Set(int x, int y, Color color)
            {
                if (x < 0 || y < 0 || x >= W || y >= H) return;
                Pixels[y * W + x] = color;
            }

            public void Blend(int x, int y, Color color)
            {
                if (color.a <= 0f) return;
                if (x < 0 || y < 0 || x >= W || y >= H) return;
                int index = y * W + x;
                var baseColor = Pixels[index];
                float a = color.a;
                float outA = a + baseColor.a * (1f - a);
                if (outA <= 0.0001f)
                {
                    Pixels[index] = Color.clear;
                    return;
                }
                Pixels[index] = new Color(
                    (color.r * a + baseColor.r * baseColor.a * (1f - a)) / outA,
                    (color.g * a + baseColor.g * baseColor.a * (1f - a)) / outA,
                    (color.b * a + baseColor.b * baseColor.a * (1f - a)) / outA,
                    outA);
            }

            public void Rect(int x, int y, int w, int h, Color color)
            {
                for (int py = y; py < y + h; py++)
                for (int px = x; px < x + w; px++) Blend(px, py, color);
            }

            public void Ellipse(float cx, float cy, float rx, float ry, Color color)
            {
                int x0 = Mathf.FloorToInt(cx - rx);
                int x1 = Mathf.CeilToInt(cx + rx);
                int y0 = Mathf.FloorToInt(cy - ry);
                int y1 = Mathf.CeilToInt(cy + ry);
                for (int y = y0; y <= y1; y++)
                {
                    for (int x = x0; x <= x1; x++)
                    {
                        float dx = (x - cx) / Mathf.Max(0.01f, rx);
                        float dy = (y - cy) / Mathf.Max(0.01f, ry);
                        if (dx * dx + dy * dy <= 1f) Blend(x, y, color);
                    }
                }
            }

            public void RoundRect(int x, int y, int w, int h, int radius, Color color)
            {
                if (radius <= 0)
                {
                    Rect(x, y, w, h, color);
                    return;
                }
                Rect(x + radius, y, w - radius * 2, h, color);
                Rect(x, y + radius, radius, h - radius * 2, color);
                Rect(x + w - radius, y + radius, radius, h - radius * 2, color);
                Ellipse(x + radius, y + radius, radius, radius, color);
                Ellipse(x + w - radius, y + radius, radius, radius, color);
                Ellipse(x + radius, y + h - radius, radius, radius, color);
                Ellipse(x + w - radius, y + h - radius, radius, radius, color);
            }

            public void Triangle(float x0, float y0, float x1, float y1, float x2, float y2, Color color)
            {
                int minX = Mathf.FloorToInt(Mathf.Min(x0, Mathf.Min(x1, x2)));
                int maxX = Mathf.CeilToInt(Mathf.Max(x0, Mathf.Max(x1, x2)));
                int minY = Mathf.FloorToInt(Mathf.Min(y0, Mathf.Min(y1, y2)));
                int maxY = Mathf.CeilToInt(Mathf.Max(y0, Mathf.Max(y1, y2)));
                for (int y = minY; y <= maxY; y++)
                {
                    for (int x = minX; x <= maxX; x++)
                    {
                        if (InsideTriangle(x, y, x0, y0, x1, y1, x2, y2)) Blend(x, y, color);
                    }
                }
            }

            private static bool InsideTriangle(float px, float py, float x0, float y0, float x1, float y1,
                float x2, float y2)
            {
                float d1 = Sign(px, py, x0, y0, x1, y1);
                float d2 = Sign(px, py, x1, y1, x2, y2);
                float d3 = Sign(px, py, x2, y2, x0, y0);
                bool hasNeg = d1 < 0 || d2 < 0 || d3 < 0;
                bool hasPos = d1 > 0 || d2 > 0 || d3 > 0;
                return !(hasNeg && hasPos);
            }

            private static float Sign(float px, float py, float ax, float ay, float bx, float by)
            {
                return (px - bx) * (ay - by) - (ax - bx) * (py - by);
            }

            public void Line(float x0, float y0, float x1, float y1, float thickness, Color color)
            {
                float distance = Vector2.Distance(new Vector2(x0, y0), new Vector2(x1, y1));
                int steps = Mathf.Max(1, Mathf.CeilToInt(distance / 0.6f));
                for (int i = 0; i <= steps; i++)
                {
                    float t = i / (float)steps;
                    Ellipse(Mathf.Lerp(x0, x1, t), Mathf.Lerp(y0, y1, t), thickness * 0.5f, thickness * 0.5f,
                        color);
                }
            }

            public void GradientV(Color top, Color bottom)
            {
                for (int y = 0; y < H; y++)
                {
                    var color = Color.Lerp(bottom, top, y / (float)Mathf.Max(1, H - 1));
                    for (int x = 0; x < W; x++) Set(x, y, color);
                }
            }

            public Texture2D ToTexture()
            {
                var texture = new Texture2D(W, H, TextureFormat.RGBA32, false);
                texture.SetPixels(Pixels);
                texture.Apply();
                return texture;
            }
        }
    }
}
