#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FinalCoat.EditorTools{
    public static class Level2Art{
        const string FurnitureFolder = "Assets/FinalCoat/Art/Furniture";
        const string WallpaperPath = "Assets/FinalCoat/Art/Background/wallpaper.png";
        const string WindowPath = "Assets/FinalCoat/Art/Background/window.png";
        const string CratePath = "Assets/FinalCoat/Art/Props/crate.png";
        const string SquarePath = "Assets/FinalCoat/Placeholders/Square.png";
        static readonly Vector2 WindowScale = new Vector2(5f, 3.4f);
        const float LedgeTop = 1.2f;
        const float LedgeWidth = 7.6f;
        const string BucketPath = "Assets/FinalCoat/Art/Props/paint_bucket.png";
        const int WindowOrder = -15;
        static readonly float[] WindowX = { 97.3f };
        static readonly string[] LedgeNames = { "Step 1", "Step 2", "Swing Start Ledge", "Swing End Ledge", "Down Step 1", "Down Step 2", "Solo Ledge" };
        static readonly string[] SwingNames = { "Swing 1", "Swing 2", "Swing 3" };
        static Transform furniture;
        [MenuItem("Final Coat/Level 2")]
        static void ApplyFromMenu(){
            if (!Apply()) return;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("Final Coat: level 2 art added. Save the scene to keep it.");
        }

        public static bool Apply(){
            var level = GameObject.Find("Level");
            if (level == null){
                Debug.LogError("Final Coat: open the Level2 scene first (no 'Level' object found).");
                return false;
            }
            var old = level.transform.Find("Furniture");
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            var root = new GameObject("Furniture");
            Undo.RegisterCreatedObjectUndo(root, "Level 2 Art");
            root.transform.SetParent(level.transform, false);
            furniture = root.transform;
            AddWallpaper(level.transform);
            AddBookshelfStart(level.transform, "Floor 1");
            AddDresser(level.transform, "Floor 2");
            foreach (var n in LedgeNames) AddLedge(level.transform, n);
            foreach (var n in SwingNames) AddSwingPlank(level.transform, n);
            AddCoatRack(level.transform);
            AddArrivalLadder();
            ZoomOutCamera();
            AddWalls(level.transform);
            AddCrate();
            AddPaintBucket();
            AddEndCheckpoint(level.transform);
            Level1.UsePaintbrushes();
            AddWindowLedge(level.transform);
            AddWindowFall(level.transform);
            AddHint(level.transform);
            return AddWindows(WindowX);
        }

        static void AddWallpaper(Transform level){
            var art = AssetDatabase.LoadAssetAtPath<Sprite>(WallpaperPath);
            var box = Box(level, "Wallpaper");
            if (art == null || box == null) return;
            Bounds b = Measure(box);
            float scale = b.size.y / art.bounds.size.y;
            float width = art.bounds.size.x * scale;
            int tiles = Mathf.CeilToInt(b.size.x / width);
            for (int i = 0; i < tiles; i++){
                var sr = Place("Wallpaper Art " + (i + 1), art, new Vector2(b.min.x + width * (i + 0.5f), b.center.y), -20);
                sr.transform.localScale = new Vector3(scale, scale, 1f);
                sr.flipX = i % 2 == 1;
            }
            Hide(box);
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

        static void AddLedge(Transform level, string name){
            var box = Box(level, name);
            if (box == null) return;
            Bounds b = Measure(box);
            var sr = Place(name + " Plank", Art("seesaw_plank_cream"), Vector2.zero, box.sortingOrder);
            sr.drawMode = SpriteDrawMode.Tiled;
            float thick = sr.sprite.bounds.size.y;
            sr.size = new Vector2(b.size.x, thick);
            sr.transform.position = new Vector3(b.center.x, b.max.y - thick * 0.5f, 0f);
            Hide(box);
        }

        static void AddSwingPlank(Transform level, string name){
            var plat = Box(level, name);
            if (plat == null) return;
            var t = plat.transform;
            for (int i = t.childCount - 1; i >= 0; i--)
                if (t.GetChild(i).name == name + " Art") Undo.DestroyObjectImmediate(t.GetChild(i).gameObject);
            var art = new GameObject(name + " Art");
            Undo.RegisterCreatedObjectUndo(art, "Level 2 Art");
            art.transform.SetParent(t, false);
            Vector3 s = t.lossyScale;
            art.transform.localScale = new Vector3(1f / s.x, 1f / s.y, 1f);
            var sr = art.AddComponent<SpriteRenderer>();
            sr.sprite = Art("seesaw_plank_cream");
            sr.drawMode = SpriteDrawMode.Tiled;
            float thick = sr.sprite.bounds.size.y;
            sr.size = new Vector2(s.x, thick);
            sr.sortingOrder = plat.sortingOrder;
            float drop = (s.y - thick) * 0.5f;
            art.transform.localPosition = new Vector3(0f, drop / s.y, 0f);
            Hide(plat);
        }

        static void AddDresser(Transform level, string floorName){
            var floor = Box(level, floorName);
            var art = Art("dresser_wide");
            if (floor == null || art == null) return;
            Bounds b = Measure(floor);
            float scale = b.size.x / art.bounds.size.x;
            float height = art.bounds.size.y * scale;
            var sr = Place("Dresser", art, new Vector2(b.center.x, b.max.y - height * 0.5f), 0);
            sr.transform.localScale = new Vector3(scale, scale, 1f);
            Hide(floor);
        }

        static void AddBookshelfStart(Transform level, string floorName){
            var floor = Box(level, floorName);
            var art = Art("bookshelf_walnut");
            if (floor == null || art == null) return;
            Bounds b = Measure(floor);
            float perPixel = b.size.x / art.rect.width;
            float scale = perPixel * art.pixelsPerUnit;
            float shelfRow = 115f;
            var sr = Place("Bookshelf", art, new Vector2(b.center.x, b.max.y + (shelfRow - art.rect.height * 0.5f) * perPixel), -10);
            sr.transform.localScale = new Vector3(scale, scale, 1f);
            Hide(floor);
        }

        static void AddArrivalLadder(){
            var art = Art("ladder");
            if (art == null) return;
            float height = art.bounds.size.y;
            float lean = -12f;
            Vector2 top = new Vector2(-7.3f, 1.6f);
            Vector2 up = Quaternion.Euler(0f, 0f, lean) * Vector2.up;
            var sr = Place("Arrival Ladder", art, top - up * height * 0.5f, -6);
            sr.transform.rotation = Quaternion.Euler(0f, 0f, lean);
        }

        static void AddCoatRack(Transform level){
            var art = Art("coat_rack_walnut");
            if (art == null) return;
            Vector2[] tips = { new Vector2(3.5f, 11.5f), new Vector2(14f, 1f), new Vector2(22.5f, 15.5f) };
            float firstX = 18.8f;
            float lastX = 30.8f;
            float firstY = 10f;
            float restTop = 5.4f;
            float perPixel = (lastX - firstX) / (tips[2].x - tips[0].x);
            Vector2 mid = new Vector2(art.rect.width * 0.5f, art.rect.height * 0.5f);
            Vector2 center = new Vector2(firstX - (tips[0].x - mid.x) * perPixel, firstY + (tips[0].y - mid.y) * perPixel);
            var sr = Place("Coat Rack", art, center, -5);
            float scale = perPixel * art.pixelsPerUnit;
            sr.transform.localScale = new Vector3(scale, scale, 1f);
            for (int i = 0; i < tips.Length; i++){
                Vector2 anchor = center + new Vector2(tips[i].x - mid.x, mid.y - tips[i].y) * perPixel;
                string name = "Swing " + (i + 1);
                var plat = level.Find(name);
                if (plat == null) continue;
                var swing = plat.GetComponent<SwingingPlatform>();
                if (swing == null) continue;
                float length = anchor.y - restTop;
                Undo.RecordObject(swing, "Level 2 Art");
                swing.ropeWidth = 0.14f;
                swing.anchor = anchor;
                swing.ropeLength = length;
                swing.maxAngle = Mathf.Min(0.35f * 5.6f / length, 0.6f);
                Undo.RecordObject(plat, "Level 2 Art");
                plat.position = new Vector3(anchor.x, restTop - 0.2f, 0f);
                var hook = level.Find(name + " Hook");
                if (hook != null){
                    Undo.RecordObject(hook, "Level 2 Art");
                    hook.position = anchor;
                    var hookSr = hook.GetComponent<SpriteRenderer>();
                    if (hookSr != null) Hide(hookSr);
                }
                var rope = level.Find(name + " Rope");
                if (rope != null){
                    Undo.RecordObject(rope, "Level 2 Art");
                    rope.position = new Vector3(anchor.x, (anchor.y + restTop) * 0.5f, 0f);
                    rope.localScale = new Vector3(0.14f, length, 1f);
                    var ropeSr = rope.GetComponent<SpriteRenderer>();
                    if (ropeSr != null){
                        Undo.RecordObject(ropeSr, "Level 2 Art");
                        ropeSr.color = Color.white;
                    }
                }
            }
        }

        static void ZoomOutCamera(){
            var follow = Object.FindFirstObjectByType<CameraFollow>();
            if (follow == null) return;
            Undo.RecordObject(follow, "Level 2 Art");
            follow.minSize = 8f;
            follow.maxSize = 9f;
            var cam = follow.GetComponent<Camera>();
            if (cam == null) return;
            Undo.RecordObject(cam, "Level 2 Art");
            cam.orthographicSize = 8f;
            cam.backgroundColor = Color.black;
        }

        static void AddWalls(Transform level){
            var left = Box(level, "Left Wall");
            if (left != null) Hide(left);
            var right = level.Find("Right Wall");
            if (right == null) return;
            Undo.RecordObject(right.gameObject, "Level 2 Art");
            right.gameObject.SetActive(false);
        }

        static void AddWindowLedge(Transform level){
            var old = level.Find("Window Ledge");
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            var square = AssetDatabase.LoadAssetAtPath<Sprite>(SquarePath);
            if (square == null) return;
            var go = new GameObject("Window Ledge");
            Undo.RegisterCreatedObjectUndo(go, "Level 2 Art");
            go.transform.SetParent(level, false);
            go.transform.position = new Vector3(WindowX[0], LedgeTop - 0.2f, 0f);
            go.transform.localScale = new Vector3(LedgeWidth, 0.4f, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = square;
            go.AddComponent<BoxCollider2D>();
            AddLedge(level, "Window Ledge");
        }

        static void AddCrate(){
            var props = GameObject.Find("Props");
            var art = AssetDatabase.LoadAssetAtPath<Sprite>(CratePath);
            if (props == null || art == null) return;
            foreach (var sr in props.GetComponentsInChildren<SpriteRenderer>(true)){
                if (!sr.name.StartsWith("Crate")) continue;
                Undo.RecordObject(sr, "Level 2 Art");
                sr.sprite = art;
                sr.color = Color.white;
                foreach (var lid in sr.GetComponentsInChildren<SpriteRenderer>(true))
                    if (lid != sr) Hide(lid);
            }
        }

        static void AddWindowFall(Transform level){
            var old = level.Find("Window Fall Zone");
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            var go = new GameObject("Window Fall Zone");
            Undo.RegisterCreatedObjectUndo(go, "Level 2 Art");
            go.transform.SetParent(level, false);
            go.transform.position = new Vector3(95.25f, -2.5f, 0f);
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(18f, 8f);
            go.AddComponent<WindowFall>();
            var exit = level.Find("Exit");
            if (exit == null) return;
            Undo.RecordObject(exit.gameObject, "Level 2 Art");
            exit.gameObject.SetActive(false);
        }

        static void AddEndCheckpoint(Transform level){
            var old = level.Find("Checkpoint (end)");
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            var square = AssetDatabase.LoadAssetAtPath<Sprite>(SquarePath);
            var go = new GameObject("Checkpoint (end)");
            Undo.RegisterCreatedObjectUndo(go, "Level 2 Art");
            go.transform.SetParent(level, false);
            go.transform.position = new Vector3(81f, 1f, 0f);
            var trigger = go.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(1f, 2f);
            var pole = new GameObject("Pole");
            pole.transform.SetParent(go.transform, false);
            pole.transform.localScale = new Vector3(0.1f, 2f, 1f);
            var poleSr = pole.AddComponent<SpriteRenderer>();
            poleSr.sprite = square;
            poleSr.color = new Color(0.2f, 0.18f, 0.22f);
            poleSr.sortingOrder = 2;
            var flag = new GameObject("Flag");
            flag.transform.SetParent(go.transform, false);
            flag.transform.localPosition = new Vector3(0.35f, 0.7f, 0f);
            flag.transform.localScale = new Vector3(0.6f, 0.4f, 1f);
            var flagSr = flag.AddComponent<SpriteRenderer>();
            flagSr.sprite = square;
            flagSr.sortingOrder = 3;
            go.AddComponent<Checkpoint>().flag = flagSr;
        }

        static void AddHint(Transform level){
            var old = level.Find("Jump Hint");
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            var root = new GameObject("Jump Hint");
            Undo.RegisterCreatedObjectUndo(root, "Level 2 Art");
            root.transform.SetParent(level, false);
            root.transform.position = new Vector3(89.5f, 5.5f, 0f);
            root.AddComponent<HintLabel>();
        }

        static void AddPaintBucket(){
            var art = AssetDatabase.LoadAssetAtPath<Sprite>(BucketPath);
            if (art == null) return;
            foreach (var blob in Object.FindObjectsByType<PaintBlob>(FindObjectsInactive.Include, FindObjectsSortMode.None)){
                if (blob.kind != BlobKind.SoloPot || blob.visual == null) continue;
                var old = blob.visual.Find("Bucket");
                if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
                foreach (var sr in blob.visual.GetComponentsInChildren<SpriteRenderer>(true)) Hide(sr);
                var go = new GameObject("Bucket");
                Undo.RegisterCreatedObjectUndo(go, "Level 2 Art");
                go.transform.SetParent(blob.visual, false);
                go.transform.localPosition = new Vector3(0f, -0.3f, 0f);
                go.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
                var bucket = go.AddComponent<SpriteRenderer>();
                bucket.sprite = art;
                bucket.sortingOrder = 8;
            }
        }

        public static bool AddWindows(float[] xs){
            var level = GameObject.Find("Level");
            if (level == null){
                Debug.LogError("Final Coat: no 'Level' object found in the open scene.");
                return false;
            }
            var art = AssetDatabase.LoadAssetAtPath<Sprite>(WindowPath);
            if (art == null){
                Debug.LogError("Final Coat: missing " + WindowPath);
                return false;
            }
            var old = level.transform.Find("Windows");
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            var root = new GameObject("Windows");
            Undo.RegisterCreatedObjectUndo(root, "Windows");
            root.transform.SetParent(level.transform, false);
            for (int i = 0; i < xs.Length; i++){
                var go = new GameObject("Window " + (i + 1));
                go.transform.SetParent(root.transform, false);
                float height = art.bounds.size.y * WindowScale.y;
                go.transform.position = new Vector3(xs[i], LedgeTop + 0.4f + height * 0.277f, 0f);
                go.transform.localScale = new Vector3(WindowScale.x, WindowScale.y, 1f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = art;
                sr.sortingOrder = WindowOrder;
            }
            return true;
        }

        static SpriteRenderer Place(string name, Sprite sprite, Vector2 position, int order){
            var go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, "Level 2 Art");
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

        static void Hide(SpriteRenderer sr){
            Undo.RecordObject(sr, "Level 2 Art");
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
