using UnityEngine;

namespace LWalida
{
    /// <summary>
    /// Builds the visible, playable content of a level from the shared AssetCatalog.
    /// Editor scene generation and the runtime fallback both call this, so generated
    /// scenes and directly-played scenes stay identical.
    /// </summary>
    public static class LevelComposer
    {
        private const int OrderSky = -100;
        private const int OrderFar = -80;
        private const int OrderEnvironment = -50;
        private const int OrderGround = -8;
        private const int OrderDoor = -1;
        private const int OrderNpc = 2;
        private const int OrderProp = 3;
        private const int OrderPlayer = 10;

        public static GameObject Compose(string sceneId, AssetCatalog catalog, Transform parent)
        {
            var level = LevelCatalog.Get(sceneId);
            var root = new GameObject("LevelContent");
            if (parent != null) root.transform.SetParent(parent, false);

            var marker = root.AddComponent<LevelContentMarker>();
            marker.sceneId = sceneId;

            if (level.IsMenu || level.IsCredits)
            {
                return root;
            }

            BuildCamera(level, catalog);
            BuildBackground(root.transform, level, catalog);
            BuildGround(root.transform, level, catalog);

            if (level.IsCutscene)
            {
                BuildHome(root.transform, level, catalog);
            }
            else
            {
                BuildGameplay(root.transform, level, catalog);
            }

            return root;
        }

        private static void BuildCamera(LevelInfo level, AssetCatalog catalog)
        {
            Camera cam = Camera.main;
            if (cam == null) cam = Object.FindFirstObjectByType<Camera>();
            if (cam == null)
            {
                var go = new GameObject("Main Camera");
                cam = go.AddComponent<Camera>();
            }

            cam.gameObject.tag = "MainCamera";
            cam.orthographic = true;
            cam.orthographicSize = 5.4f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = AmbientColor(level.ArtKey);
            cam.transform.rotation = Quaternion.identity;
            cam.transform.position = new Vector3(InitialCameraX(level), level.CameraY, -10f);

            if (cam.GetComponent<AudioListener>() == null &&
                Object.FindFirstObjectByType<AudioListener>() == null)
            {
                cam.gameObject.AddComponent<AudioListener>();
            }

            var follow = cam.GetComponent<CameraFollow>();
            if (follow == null) follow = cam.gameObject.AddComponent<CameraFollow>();
            follow.Configure(0f, level.WorldWidth, level.CameraY);
        }

        private static float InitialCameraX(LevelInfo level)
        {
            const float halfView = 9.6f;
            float min = Mathf.Min(halfView, level.WorldWidth * 0.5f);
            float max = Mathf.Max(min, level.WorldWidth - halfView);
            return Mathf.Clamp(level.SpawnX, min, max);
        }

        private static void BuildBackground(Transform parent, LevelInfo level, AssetCatalog catalog)
        {
            if (catalog == null) return;
            var art = level.ArtKey;
            var skyRoot = CreateLayer(parent, "Sky", 0.04f);
            var farRoot = CreateLayer(parent, "FarSkyline", 0.24f);
            var envRoot = CreateLayer(parent, "Environment", 0.82f);

            var sky = catalog.GetSky(art);
            if (sky != null)
            {
                CreateBlock("SkyFill", skyRoot, catalog.whitePixel, AmbientColor(art) * new Color(0.6f, 0.6f, 0.6f, 1f),
                    new Vector2(level.WorldWidth + 60f, 26f), 4f, OrderSky, null);
                StretchSprite("SkyImage", skyRoot, sky, new Vector2(level.WorldWidth + 60f, 26f), 4f,
                    OrderSky + 2);
            }
            else
            {
                CreateBlock("SkyFill", skyRoot, catalog.whitePixel, AmbientColor(art),
                    new Vector2(level.WorldWidth + 60f, 26f), 4f, OrderSky, null);
            }

            var far = catalog.GetFar(art);
            if (far != null)
            {
                TileSprite("FarTile", farRoot, far, level.WorldWidth + 30f, 5.2f, 3.6f, OrderFar);
            }

            var env = catalog.GetEnvironment(art);
            if (env != null)
            {
                TileSprite("EnvironmentTile", envRoot, env, level.WorldWidth + 30f, 11.8f, level.CameraY + 0.1f,
                    OrderEnvironment);
            }
            else
            {
                CreateBlock("EnvironmentFill", envRoot, catalog.whitePixel,
                    AmbientColor(art) * new Color(0.7f, 0.7f, 0.7f, 1f),
                    new Vector2(level.WorldWidth + 40f, 12f), level.CameraY, OrderEnvironment, null);
            }
        }

        private static Transform CreateLayer(Transform parent, string name, float factor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var parallax = go.AddComponent<ParallaxLayer>();
            parallax.SetFactor(factor);
            return go.transform;
        }

        private static void TileSprite(string name, Transform parent, Sprite sprite, float coverage,
            float height, float centerY, int order)
        {
            if (sprite == null) return;
            float spriteHeight = Mathf.Max(0.01f, sprite.bounds.size.y);
            float scale = height / spriteHeight;
            float spriteWidth = sprite.bounds.size.x * scale;
            float step = Mathf.Max(2f, spriteWidth * 0.92f);
            int count = Mathf.CeilToInt((coverage + Mathf.Abs(centerY)) / step) + 2;
            float start = -step * 0.5f;

            for (int i = 0; i < count; i++)
            {
                var go = new GameObject(name + "_" + i);
                go.transform.SetParent(parent, false);
                go.transform.localPosition = new Vector3(start + i * step, centerY, 0f);
                go.transform.localScale = new Vector3(scale, scale, 1f);
                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.sortingOrder = order;
            }
        }

        private static void StretchSprite(string name, Transform parent, Sprite sprite, Vector2 size, float centerY,
            int order)
        {
            if (sprite == null) return;
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, centerY, 0f);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = order;
            var bounds = sprite.bounds.size;
            go.transform.localScale = new Vector3(size.x / Mathf.Max(0.01f, bounds.x),
                size.y / Mathf.Max(0.01f, bounds.y), 1f);
        }

        private static void CreateBlock(string name, Transform parent, Sprite pixel, Color color, Vector2 size,
            float centerY, int order, Transform existing)
        {
            if (pixel == null) return;
            var go = existing != null ? existing.gameObject : new GameObject(name);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, centerY, 0f);
            var renderer = go.GetComponent<SpriteRenderer>();
            if (renderer == null) renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = pixel;
            renderer.color = color;
            renderer.sortingOrder = order;
            var bounds = pixel.bounds.size;
            go.transform.localScale = new Vector3(size.x / Mathf.Max(0.01f, bounds.x),
                size.y / Mathf.Max(0.01f, bounds.y), 1f);
        }

        private static void BuildGround(Transform parent, LevelInfo level, AssetCatalog catalog)
        {
            var ground = new GameObject("GroundCollider");
            ground.transform.SetParent(parent, false);
            ground.transform.position = new Vector3(level.WorldWidth * 0.5f, level.GroundY - 0.5f, 0f);
            var collider = ground.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(level.WorldWidth + 20f, 1f);

            var leftWall = new GameObject("LeftBoundary");
            leftWall.transform.SetParent(parent, false);
            leftWall.transform.position = new Vector3(-1f, level.GroundY + 5f, 0f);
            var leftCollider = leftWall.AddComponent<BoxCollider2D>();
            leftCollider.size = new Vector2(1f, 12f);

            var rightWall = new GameObject("RightBoundary");
            rightWall.transform.SetParent(parent, false);
            rightWall.transform.position = new Vector3(level.WorldWidth + 1f, level.GroundY + 5f, 0f);
            var rightCollider = rightWall.AddComponent<BoxCollider2D>();
            rightCollider.size = new Vector2(1f, 12f);

            var strip = catalog != null ? catalog.groundStrip : null;
            if (strip != null)
            {
                TileSprite("GroundStrip", parent, strip, level.WorldWidth + 20f, 1.3f, level.GroundY - 0.58f,
                    OrderGround);
            }
            else
            {
                CreateBlock("GroundFill", parent, catalog != null ? catalog.whitePixel : null,
                    new Color(0.24f, 0.2f, 0.17f, 1f),
                    new Vector2(level.WorldWidth + 20f, 1.3f), level.GroundY - 0.58f, OrderGround, null);
            }

            if (catalog != null && catalog.whitePixel != null)
            {
                CreateBlock("CurbLine", parent, catalog.whitePixel, new Color(0.85f, 0.76f, 0.6f, 0.55f),
                    new Vector2(level.WorldWidth + 20f, 0.07f), level.GroundY - 0.02f, OrderGround + 1, null);
            }
        }

        private static void BuildGameplay(Transform parent, LevelInfo level, AssetCatalog catalog)
        {
            var player = CreatePlayer(parent, level, catalog);

            switch (level.SceneId)
            {
                case SceneIds.CafeMaure:
                    BuildCafe(parent, level, catalog, player);
                    break;
                case SceneIds.Oudaya:
                    BuildOudaya(parent, level, catalog, player);
                    break;
                case SceneIds.Souk:
                    BuildSouk(parent, level, catalog, player);
                    break;
                case SceneIds.Hassan:
                    BuildHassan(parent, level, catalog, player);
                    break;
            }
        }

        private static void BuildCafe(Transform parent, LevelInfo level, AssetCatalog c, PlayerController player)
        {
            var photo = CreateProp(c, parent, "Photograph");
            var prop = Ensure<InspectableProp>(photo);
            prop.Configure("the photograph", MemoryIds.Cafe,
                "A blue horseshoe arch. Two round brass handles. Three rows of studs. On the back, in faded ink: 'Zohra, our door.' " +
                "You slip the photograph into your journal.",
                "The photograph is already safe in your journal.", c != null ? c.photoProp : null);
            Place(photo, c != null ? c.photoProp : null, 8f, level.GroundY, OrderProp, 0.9f);

            var tea = CreateProp(c, parent, "Mint tea");
            var teaProp = Ensure<InspectableProp>(tea);
            teaProp.Configure("the tea tray", null,
                "Mint tea, still warm. Your mother used to order two glasses and drink both while they argued about the sea.",
                null, c != null ? c.teaSet : null);
            Place(tea, c != null ? c.teaSet : null, 5.3f, level.GroundY, OrderProp + 1, 0.8f);

            Decorate(parent, c, c != null ? c.flowerpot : null, "CafeFlowerpot", 3.4f, level, 0.9f);
            Decorate(parent, c, c != null ? c.bougainvillea : null, "CafeBougainvillea", 13.5f, level, 1.1f, 2.4f);
            Decorate(parent, c, c != null ? c.flowerpot : null, "CafeFlowerpot2", 19.5f, level, 0.8f);
            Decorate(parent, c, c != null ? c.bougainvillea : null, "CafeBougainvillea2", 22.2f, level, 0.9f, 2.1f);

            CreateExit(c, parent, "the Kasbah gate", SceneIds.Oudaya, "Enter the Kasbah",
                "The photograph on the tray is still waiting for you.",
                new[] { MemoryIds.Cafe }, null, 23.5f, level.GroundY);
        }

        private static void BuildOudaya(Transform parent, LevelInfo level, AssetCatalog c, PlayerController player)
        {
            var reminder = CreateProp(c, parent, "Pinned photograph");
            var reminderProp = Ensure<InspectableProp>(reminder);
            reminderProp.Configure("the pinned photograph", null,
                "Horseshoe arch. Two round brass handles. Three rows of studs. Press C near a door to compare it with the photograph.",
                "The details are already in your mind: two brass handles, three rows of studs.", 
                c != null ? c.referencePhoto : null);
            Place(reminder, c != null ? c.referencePhoto : null, 5.5f, level.GroundY, OrderProp, 0.75f);

            CreateDoor(c, parent, "DoorWrong01", "the weathered blue door", false,
                "A pointed arch, and a single handle. The paint is newer than the photograph.",
                "The arch is pointed, not horseshoe.", c != null ? c.doorWrong01 : null, 10f, level.GroundY);
            CreateDoor(c, parent, "DoorCorrect", "the blue door with brass handles", true,
                "A blue horseshoe arch. Two round brass handles, worn smooth by hands that are gone. Your own hands are shaking.",
                "Two round brass handles. Three rows of studs. This is the door.", 
                c != null ? c.doorCorrect : null, 16f, level.GroundY);
            CreateDoor(c, parent, "DoorWrong02", "the narrow cedar door", false,
                "A square top, and iron studs in a different pattern. The wood is dark with age.",
                "The studs and handles are different from the photograph.", c != null ? c.doorWrong02 : null,
                22f, level.GroundY);

            var amina = CreateNpc(c, parent, "Lalla Amina");
            var aminaNpc = Ensure<NpcInteractable>(amina);
            aminaNpc.Configure("Lalla Amina", c != null ? c.aminaDialogue : null,
                c != null ? c.aminaAfterDialogue : null, MemoryIds.Amina, null, c != null ? c.portraitAmina : null);
            Place(amina, c != null ? c.aminaIdle : null, 30f, level.GroundY, OrderNpc, 1f);

            Decorate(parent, c, c != null ? c.bougainvillea : null, "OudayaBougainvillea", 7f, level, 1.05f, 2.5f);
            Decorate(parent, c, c != null ? c.flowerpot : null, "OudayaPot1", 13.5f, level, 0.85f);
            Decorate(parent, c, c != null ? c.bougainvillea : null, "OudayaBougainvillea2", 26.5f, level, 0.9f, 2.2f);
            Decorate(parent, c, c != null ? c.flowerpot : null, "OudayaPot2", 35f, level, 0.9f);

            CreateExit(c, parent, "the alleys to the souk", SceneIds.Souk, "Follow the alley to the souk",
                "Two things still feel unfinished: the door, and Lalla Amina's story.",
                new[] { MemoryIds.Door, MemoryIds.Amina }, null, 39f, level.GroundY);
        }

        private static void BuildSouk(Transform parent, LevelInfo level, AssetCatalog c, PlayerController player)
        {
            var allal = CreateNpc(c, parent, "Allal");
            var allalNpc = Ensure<NpcInteractable>(allal);
            allalNpc.Configure("Allal", c != null ? c.allalDialogue : null,
                c != null ? c.allalAfterDialogue : null, null, GameFlags.RecordingUnlocked,
                null);
            Place(allal, c != null ? c.allalIdle : null, 9f, level.GroundY, OrderNpc, 1f);

            var zone = CreateZone(c, parent, "SoundOfTheSouk");
            var recording = Ensure<RecordingZone>(zone);
            recording.Configure("the sound of the souk", c != null ? c.ambienceSouk : null);
            Place(zone, c != null ? c.soundMarker : null, 17f, level.GroundY, OrderProp, 0.9f, 1.6f);

            Decorate(parent, c, c != null ? c.stall : null, "Stall1", 5f, level, 1f);
            Decorate(parent, c, c != null ? c.stall : null, "Stall2", 21f, level, 0.95f);
            Decorate(parent, c, c != null ? c.stall : null, "Stall3", 25.8f, level, 1f);
            Decorate(parent, c, c != null ? c.flowerpot : null, "SoukPot", 13f, level, 0.8f);
            Decorate(parent, c, c != null ? c.teaSet : null, "SoukTea", 12.2f, level, 0.7f);

            CreateExit(c, parent, "home", SceneIds.Home, "Return home",
                "Not yet. The souk still has a sound to give you, and the other memories to gather.",
                MemoryIds.All, null, 31f, level.GroundY);
            CreateExit(c, parent, "the Hassan Tower", SceneIds.Hassan, "Visit Hassan Tower",
                "Not yet. Four memories must be gathered before you can face the river.",
                MemoryIds.All, null, 34.5f, level.GroundY);
        }

        private static void BuildHassan(Transform parent, LevelInfo level, AssetCatalog c, PlayerController player)
        {
            Decorate(parent, c, c != null ? c.bougainvillea : null, "HassanBougainvillea", 6f, level, 1.2f, 2.4f);
            Decorate(parent, c, c != null ? c.flowerpot : null, "HassanPot1", 11f, level, 0.9f);
            Decorate(parent, c, c != null ? c.flowerpot : null, "HassanPot2", 18f, level, 0.85f);
            Decorate(parent, c, c != null ? c.stall : null, "HassanBench", 23f, level, 0.8f);

            CreateExit(c, parent, "home", SceneIds.Home, "Return home",
                "Four memories, and the light on the river. Home is waiting.",
                MemoryIds.All, null, 27f, level.GroundY);
        }

        private static void BuildHome(Transform parent, LevelInfo level, AssetCatalog c)
        {
            var player = CreatePlayer(parent, level, c);
            player.transform.position = new Vector3(level.SpawnX, level.GroundY, 0f);

            var neighbor = new GameObject("Neighbor");
            neighbor.transform.SetParent(parent, false);
            var renderer = neighbor.AddComponent<SpriteRenderer>();
            renderer.sprite = c != null ? c.neighborIdle : null;
            renderer.sortingOrder = OrderNpc;
            Place(neighbor, c != null ? c.neighborIdle : null, 13.5f, level.GroundY, OrderNpc, 1f);

            Decorate(parent, c, c != null ? c.flowerpot : null, "HomePot", 7f, level, 0.75f);
            Decorate(parent, c, c != null ? c.teaSet : null, "HomeTea", 9.6f, level, 0.7f);
        }

        private static PlayerController CreatePlayer(Transform parent, LevelInfo level, AssetCatalog c)
        {
            GameObject go = null;
            if (c != null && c.playerPrefab != null)
            {
                go = Object.Instantiate(c.playerPrefab, parent);
            }
            if (go == null)
            {
                go = new GameObject("Player");
                go.transform.SetParent(parent, false);
                var body = go.AddComponent<Rigidbody2D>();
                body.gravityScale = 0f;
                var collider = go.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(0.7f, 1.8f);
                collider.offset = new Vector2(0f, 0.9f);

                var visual = new GameObject("Visual");
                visual.transform.SetParent(go.transform, false);
                visual.transform.localPosition = new Vector3(0f, 1.05f, 0f);
                var sprite = visual.AddComponent<SpriteRenderer>();
                sprite.sprite = c != null ? c.playerIdle : null;
                sprite.sortingOrder = OrderPlayer;
            }

            go.name = "Player";
            if (go.transform.parent != parent) go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(level.SpawnX, level.GroundY, 0f);

            var controller = Ensure<PlayerController>(go);
            controller.SetBounds(0.8f, Mathf.Max(1.6f, level.WorldWidth - 0.8f));
            if (c != null) controller.SetVisual(c.playerIdle);
            return controller;
        }

        private static void CreateDoor(AssetCatalog c, Transform parent, string name, string label, bool correct,
            string inspect, string mismatch, Sprite sprite, float x, float groundY)
        {
            var go = CreateInteractable(c != null ? c.doorPrefab : null, parent, name);
            var door = Ensure<PhotoDoor>(go);
            door.Configure(label, correct, inspect, mismatch, sprite);
            Place(go, sprite, x, groundY, OrderDoor, 1f);
        }

        private static void CreateExit(AssetCatalog c, Transform parent, string label, string target, string prompt,
            string locked, string[] memories, string[] flags, float x, float groundY)
        {
            var go = CreateInteractable(c != null ? c.exitPrefab : null, parent, "Exit_" + target);
            var exit = Ensure<SceneExit>(go);
            exit.Configure(label, target, prompt, locked, memories, flags);
            Place(go, c != null ? c.exitMarker : null, x, groundY, OrderProp + 1, 0.9f, 0.35f);
        }

        private static void Decorate(Transform parent, AssetCatalog c, Sprite sprite, string name, float x,
            LevelInfo level, float scale, float lift = 0f)
        {
            if (sprite == null) return;
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = OrderProp - 1;
            Place(go, sprite, x, level.GroundY, OrderProp - 1, scale, lift);
        }

        private static GameObject CreateProp(AssetCatalog c, Transform parent, string name)
        {
            return CreateInteractable(c != null ? c.propPrefab : null, parent, name);
        }

        private static GameObject CreateNpc(AssetCatalog c, Transform parent, string name)
        {
            return CreateInteractable(c != null ? c.npcPrefab : null, parent, name);
        }

        private static GameObject CreateZone(AssetCatalog c, Transform parent, string name)
        {
            return CreateInteractable(c != null ? c.zonePrefab : null, parent, name);
        }

        private static GameObject CreateInteractable(GameObject prefab, Transform parent, string name)
        {
            GameObject go = prefab != null ? Object.Instantiate(prefab, parent) : new GameObject(name);
            go.name = name;
            if (go.transform.parent != parent) go.transform.SetParent(parent, false);
            if (go.GetComponent<SpriteRenderer>() == null) go.AddComponent<SpriteRenderer>();
            return go;
        }

        private static T Ensure<T>(GameObject go) where T : Component
        {
            var component = go.GetComponent<T>();
            if (component == null) component = go.AddComponent<T>();
            return component;
        }

        private static void Place(GameObject go, Sprite sprite, float x, float groundY, int order, float scale,
            float lift = 0f)
        {
            var renderer = go.GetComponent<SpriteRenderer>();
            if (renderer == null) renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = order;
            float height = sprite != null ? sprite.bounds.size.y : 1.8f;
            go.transform.localScale = new Vector3(scale, scale, 1f);
            go.transform.position = new Vector3(x, groundY + height * 0.5f * scale + lift, 0f);
        }

        private static Color AmbientColor(string artKey)
        {
            switch (artKey)
            {
                case "cafe": return new Color(0.36f, 0.28f, 0.2f);
                case "oudaya": return new Color(0.16f, 0.36f, 0.42f);
                case "souk": return new Color(0.42f, 0.27f, 0.15f);
                case "hassan": return new Color(0.46f, 0.28f, 0.17f);
                case "home": return new Color(0.07f, 0.11f, 0.2f);
                default: return new Color(0.2f, 0.3f, 0.35f);
            }
        }
    }
}
