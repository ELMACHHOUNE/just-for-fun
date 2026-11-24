using UnityEditor;
using UnityEngine;

namespace LWalida.EditorTools
{
    /// <summary>
    /// Builds reusable prefabs and wires them into the AssetCatalog. Existing prefabs
    /// and non-null catalog references are preserved.
    /// </summary>
    public static class PrefabFactory
    {
        public static void BuildAll(AssetCatalog catalog)
        {
            PlaceholderArtGenerator.EnsureFolder(LWalidaPaths.Prefabs);
            if (catalog == null) return;
            var art = catalog;

            var player = EnsurePrefab("Player", () => CreatePlayer(art));
            var npc = EnsurePrefab("NPC", () => CreateNpc(art));
            var door = EnsurePrefab("Door", () => CreateDoor(art));
            var prop = EnsurePrefab("Prop", () => CreateProp(art));
            var exit = EnsurePrefab("Exit", () => CreateExit(art));
            var zone = EnsurePrefab("RecordingZone", () => CreateZone(art));

            if (catalog.playerPrefab == null) catalog.playerPrefab = player;
            if (catalog.npcPrefab == null) catalog.npcPrefab = npc;
            if (catalog.doorPrefab == null) catalog.doorPrefab = door;
            if (catalog.propPrefab == null) catalog.propPrefab = prop;
            if (catalog.exitPrefab == null) catalog.exitPrefab = exit;
            if (catalog.zonePrefab == null) catalog.zonePrefab = zone;

            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
        }

        public static GameObject PrefabPath(string name)
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(LWalidaPaths.Prefabs + "/" + name + ".prefab");
        }

        private static GameObject EnsurePrefab(string name, System.Func<GameObject> create)
        {
            string path = LWalidaPaths.Prefabs + "/" + name + ".prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var temp = create();
            temp.name = name;
            var prefab = PrefabUtility.SaveAsPrefabAsset(temp, path);
            Object.DestroyImmediate(temp);
            return prefab != null ? prefab : AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        private static GameObject CreatePlayer(AssetCatalog c)
        {
            var go = new GameObject("Player");
            var body = go.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionY;
            var collider = go.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.7f, 1.8f);
            collider.offset = new Vector2(0f, 0.9f);

            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform, false);
            visual.transform.localPosition = new Vector3(0f, 1.05f, 0f);
            var renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = c != null ? c.playerIdle : null;
            renderer.sortingOrder = 10;

            go.AddComponent<PlayerController>();
            return go;
        }

        private static GameObject CreateNpc(AssetCatalog c)
        {
            var go = new GameObject("NPC");
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = c != null ? c.aminaIdle : null;
            renderer.sortingOrder = 2;
            go.AddComponent<NpcInteractable>();
            return go;
        }

        private static GameObject CreateDoor(AssetCatalog c)
        {
            var go = new GameObject("Door");
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = c != null ? c.doorCorrect : null;
            renderer.sortingOrder = -1;
            var door = go.AddComponent<PhotoDoor>();
            door.spriteRenderer = renderer;
            return go;
        }

        private static GameObject CreateProp(AssetCatalog c)
        {
            var go = new GameObject("Prop");
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = c != null ? c.photoProp : null;
            renderer.sortingOrder = 3;
            var prop = go.AddComponent<InspectableProp>();
            prop.spriteRenderer = renderer;
            return go;
        }

        private static GameObject CreateExit(AssetCatalog c)
        {
            var go = new GameObject("Exit");
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = c != null ? c.exitMarker : null;
            renderer.sortingOrder = 4;
            go.AddComponent<SceneExit>();
            return go;
        }

        private static GameObject CreateZone(AssetCatalog c)
        {
            var go = new GameObject("RecordingZone");
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = c != null ? c.soundMarker : null;
            renderer.sortingOrder = 3;
            var zone = go.AddComponent<RecordingZone>();
            zone.marker = renderer;
            return go;
        }
    }
}
