#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FinalCoat.EditorTools{
    public static class Level1{
        const string FurnitureFolder = "Assets/FinalCoat/Art/Furniture";
        const string WallpaperPath = "Assets/FinalCoat/Art/Background/wallpaper.png";
        const float ArtUnit = 0.5f;
        const int ChairCushionRow = 30;
        static readonly Vector2 WallpaperCenter = new Vector2(17f, 6f);
        static readonly Vector2 WallpaperSize = new Vector2(88f, 32f);
        const float CameraMinSize = 8f;
        const float CameraMaxSize = 9f;
        const float SeesawWidth = 5.6f;
        const float SmallBrushHeight = 1.1f;
        static Transform furniture;
        [MenuItem("Final Coat/Level 1")]
        static void ApplyFromMenu(){
            if (!Apply()) return;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("Final Coat: level 1 art added. Save the scene to keep it.");
        }

        public static bool Apply(){
            var level = GameObject.Find("Level");
            if (level == null){
                Debug.LogError("Final Coat: open the Prototype scene first (no 'Level' object found).");
                return false;
            }
            var old = level.transform.Find("Furniture");
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            var root = new GameObject("Furniture");
            Undo.RegisterCreatedObjectUndo(root, "Level 1");
            root.transform.SetParent(level.transform, false);
            furniture = root.transform;
            AddWallpaper(level.transform);
            AddChair(level.transform);
            AddTable(level.transform, "Floor 2");
            AddTable(level.transform, "Floor 3");
            AddSeesaw(level.transform);
            AddLedge(level.transform, "Low Shelf");
            AddLedge(level.transform, "High Ledge");
            AddLedge(level.transform, "Solo Ledge");
            HideBox(level.transform, "Left Wall");
            HideBox(level.transform, "Right Wall");
            AddLadderExit(level.transform);
            MoveSpawnsOffChairBack(level.transform);
            ZoomOutCamera();
            UsePaintbrushes();
            return true;
        }

        static void AddWallpaper(Transform level){
            var art = AssetDatabase.LoadAssetAtPath<Sprite>(WallpaperPath);
            if (art == null) return;
            var sr = Place("Wallpaper Art", art, WallpaperCenter, -20);
            Vector2 native = art.bounds.size;
            sr.transform.localScale = new Vector3(WallpaperSize.x / native.x, WallpaperSize.y / native.y, 1f);
            HideBox(level, "Wallpaper");
        }

        static void AddChair(Transform level){
            var floor = Box(level, "Floor 1");
            if (floor == null) return;
            var art = Art("chair_wood");
            Bounds b = Measure(floor);
            float scale = b.size.x / (art.rect.width * ArtUnit);
            float width = art.rect.width * ArtUnit * scale;
            float height = art.rect.height * ArtUnit * scale;
            float top = b.max.y + ChairCushionRow * ArtUnit * scale;
            var sr = Place("Chair", art, new Vector2(b.min.x + width * 0.5f, top - height * 0.5f), -3);
            sr.transform.localScale = new Vector3(scale, scale, 1f);
            Hide(floor);
        }

        static void AddTable(Transform level, string floorName){
            var floor = Box(level, floorName);
            if (floor == null) return;
            Bounds b = Measure(floor);
            var top = Place(floorName + " Tabletop", Art("tabletop"), b.center, 0);
            top.drawMode = SpriteDrawMode.Tiled;
            top.size = b.size;
            var legArt = Art("table_leg");
            float legWidth = legArt.bounds.size.x;
            float legHeight = legArt.bounds.size.y;
            float inset = Mathf.Min(1f, b.size.x * 0.1f);
            Place(floorName + " Leg L", legArt, new Vector2(b.min.x + inset + legWidth * 0.5f, b.min.y - legHeight * 0.5f), -1);
            Place(floorName + " Leg R", legArt, new Vector2(b.max.x - inset - legWidth * 0.5f, b.min.y - legHeight * 0.5f), -1);
            Hide(floor);
        }

        static void AddSeesaw(Transform level){
            var plank = Box(level, "Seesaw Plank");
            var pillar = Box(level, "Seesaw Pillar");
            if (pillar != null){
                Bounds b = Measure(pillar);
                var post = Place("Seesaw Post", Art("table_leg_bead"), b.center, -1);
                post.drawMode = SpriteDrawMode.Tiled;
                post.size = new Vector2(post.sprite.bounds.size.x, b.size.y);
                Hide(pillar);
            }
            if (plank == null) return;
            var t = plank.transform;
            if (!Mathf.Approximately(t.localScale.x, SeesawWidth)){
                Undo.RecordObject(t, "Level 1");
                t.localScale = new Vector3(SeesawWidth, t.localScale.y, 1f);
            }
            for (int i = t.childCount - 1; i >= 0; i--)
                if (t.GetChild(i).name == "Seesaw Art") Undo.DestroyObjectImmediate(t.GetChild(i).gameObject);
            var art = new GameObject("Seesaw Art");
            Undo.RegisterCreatedObjectUndo(art, "Level 1");
            art.transform.SetParent(t, false);
            Vector3 s = t.lossyScale;
            art.transform.localScale = new Vector3(1f / s.x, 1f / s.y, 1f);
            var sr = art.AddComponent<SpriteRenderer>();
            sr.sprite = Art("seesaw_plank");
            sr.drawMode = SpriteDrawMode.Tiled;
            float thick = sr.sprite.bounds.size.y;
            sr.size = new Vector2(s.x, thick);
            sr.sortingOrder = plank.sortingOrder;
            float drop = (s.y - thick) * 0.5f;
            art.transform.localPosition = new Vector3(0f, drop / s.y, 0f);
            Hide(plank);
        }

        static void AddLedge(Transform level, string name){
            var box = Box(level, name);
            if (box == null) return;
            Bounds b = Measure(box);
            var sr = Place(name + " Plank", Art("seesaw_plank"), Vector2.zero, box.sortingOrder);
            sr.drawMode = SpriteDrawMode.Tiled;
            float thick = sr.sprite.bounds.size.y;
            sr.size = new Vector2(b.size.x, thick);
            sr.transform.position = new Vector3(b.center.x, b.max.y - thick * 0.5f, 0f);
            Hide(box);
        }

        static void AddLadderExit(Transform level){
            var exit = level.Find("Exit");
            var floor = Box(level, "Floor 3");
            if (exit == null || floor == null) return;
            Bounds b = Measure(floor);
            var art = Art("ladder");
            float height = art.bounds.size.y;
            float lean = -12f;
            Vector2 foot = new Vector2(b.max.x - 1.9f, b.max.y);
            Vector2 up = Quaternion.Euler(0f, 0f, lean) * Vector2.up;
            var ladder = Place("Ladder", art, foot + up * height * 0.5f, 1);
            ladder.transform.rotation = Quaternion.Euler(0f, 0f, lean);
            Undo.RecordObject(exit, "Level 1");
            exit.position = new Vector3(foot.x + 0.4f, b.max.y + 1.25f, 0f);
            var trigger = exit.GetComponent<BoxCollider2D>();
            if (trigger != null){
                Undo.RecordObject(trigger, "Level 1");
                trigger.size = new Vector2(2.6f, 2.5f);
            }
            var levelExit = exit.GetComponent<LevelExit>();
            if (levelExit != null){
                Undo.RecordObject(levelExit, "Level 1");
                levelExit.door = null;
            }
            foreach (var sr in exit.GetComponentsInChildren<SpriteRenderer>()) Hide(sr);
            var shelfArt = Art("bookshelf");
            Vector2 shelfSize = shelfArt.bounds.size;
            var wall = Box(level, "Right Wall");
            float wallLeft = wall != null ? Measure(wall).min.x : b.max.x;
            Place("Bookshelf", shelfArt, new Vector2(wallLeft + shelfSize.x * 0.5f, 22f - shelfSize.y * 0.5f), -10);
        }

        [MenuItem("Final Coat/Paintbrushes")]
        static void PaintbrushesFromMenu(){
            int swapped = UsePaintbrushes();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log($"Final Coat: {swapped} paint pickups now use the paintbrush art. Save the scene to keep it.");
        }

        public static int UsePaintbrushes(){
            var small = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/FinalCoat/Art/Props/paintbrush_blue.png");
            var large = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/FinalCoat/Art/Props/paintbrush_red.png");
            if (small == null || large == null){
                Debug.LogError("Final Coat: missing Art/Props/paintbrush_blue.png or paintbrush_red.png");
                return 0;
            }
            int swapped = 0;
            foreach (var blob in Object.FindObjectsByType<PaintBlob>(FindObjectsInactive.Include, FindObjectsSortMode.None)){
                if (blob.kind == BlobKind.SoloPot || blob.visual == null) continue;
                var sr = blob.visual.GetComponentInChildren<SpriteRenderer>(true);
                if (sr == null) continue;
                bool isLarge = blob.kind == BlobKind.Large;
                Undo.RecordObject(sr, "Paintbrushes");
                Undo.RecordObject(sr.transform, "Paintbrushes");
                sr.sprite = isLarge ? large : small;
                sr.color = Color.white;
                float height = isLarge ? SmallBrushHeight * 1.4f : SmallBrushHeight;
                sr.transform.localScale = new Vector3(height, height, 1f);
                swapped++;
            }
            return swapped;
        }

        static void ZoomOutCamera(){
            var follow = Object.FindFirstObjectByType<CameraFollow>();
            if (follow == null) return;
            Undo.RecordObject(follow, "Level 1");
            follow.minSize = CameraMinSize;
            follow.maxSize = CameraMaxSize;
            var cam = follow.GetComponent<Camera>();
            if (cam == null) return;
            Undo.RecordObject(cam, "Level 1");
            cam.orthographicSize = CameraMinSize;
        }

        static void MoveSpawnsOffChairBack(Transform level){
            var floor = Box(level, "Floor 1");
            var start = GameObject.Find("Start Point");
            if (floor == null || start == null) return;
            float x = Measure(floor).min.x + 3.2f;
            Undo.RecordObject(start.transform, "Level 1");
            start.transform.position = new Vector3(x, start.transform.position.y, 0f);
            MovePlayer("Players/Player 1", x - 0.6f);
            MovePlayer("Players/Player 2", x + 0.8f);
        }

        static void MovePlayer(string path, float x){
            var player = GameObject.Find(path);
            if (player == null) return;
            Undo.RecordObject(player.transform, "Level 1");
            player.transform.position = new Vector3(x, player.transform.position.y, 0f);
        }

        static SpriteRenderer Place(string name, Sprite sprite, Vector2 position, int order){
            var go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, "Level 1");
            go.transform.SetParent(furniture, false);
            go.transform.position = position;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
            return sr;
        }

        static SpriteRenderer Box(Transform level, string name){
            var t = level.Find(name);
            if (t == null){
                Debug.LogWarning("Final Coat: couldn't find '" + name + "' in Level, skipped it.");
                return null;
            }
            return t.GetComponent<SpriteRenderer>();
        }

        static Bounds Measure(SpriteRenderer box){
            var t = box.transform;
            Vector3 size = Vector3.Scale(box.sprite.bounds.size, t.lossyScale);
            return new Bounds(t.position, new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), 0f));
        }

        static void HideBox(Transform level, string name){
            var sr = Box(level, name);
            if (sr != null) Hide(sr);
        }

        static void Hide(SpriteRenderer sr){
            Undo.RecordObject(sr, "Level 1");
            sr.enabled = false;
        }

        static Sprite Art(string name){
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{FurnitureFolder}/{name}.png");
            if (sprite == null) Debug.LogError("Final Coat: missing furniture art " + name);
            return sprite;
        }
    }
}
#endif
