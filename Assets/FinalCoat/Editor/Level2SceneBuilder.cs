#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FinalCoat.EditorTools{
    public static class Level2SceneBuilder{
        const string RootFolder = "Assets/FinalCoat";
        const string SpriteFolder = RootFolder + "/Placeholders";
        const string SceneFolder = RootFolder + "/Scenes";
        const string ScenePath = SceneFolder + "/Level2.unity";

        const float SwingLength = 5.6f;
        const float SwingAngle = 0.35f;
        const float SwingPeriod = 3.4f;
        const float SwingWidth = 2f;

        static readonly Color Background = new Color(0.95f, 0.91f, 0.84f);
        static readonly Color WallPaper = new Color(0.89f, 0.84f, 0.92f);
        static readonly Color Floor = new Color(0.62f, 0.45f, 0.32f);
        static readonly Color Shelf = new Color(0.74f, 0.56f, 0.38f);
        static readonly Color Crate = new Color(0.86f, 0.66f, 0.36f);
        static readonly Color Outline = new Color(0.2f, 0.18f, 0.22f);
        static readonly Color SmallBlob = new Color(0.2f, 0.75f, 0.85f);
        static readonly Color LargeBlob = new Color(0.98f, 0.78f, 0.15f);
        static readonly Color SoloPot = new Color(0.65f, 0.3f, 0.85f);
        static readonly Color Player1 = new Color(0.93f, 0.3f, 0.3f);
        static readonly Color Player2 = new Color(0.25f, 0.5f, 0.95f);
        static Sprite square;
        static Sprite circle;
        [MenuItem("Final Coat/Build Level 2 Scene")]
        public static void Build(){
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (File.Exists(ScenePath) && !EditorUtility.DisplayDialog(
                    "Rebuild level 2?",
                    "This replaces " + ScenePath + ". Anything changed by hand in that scene will be lost.",
                    "Rebuild", "Cancel"))
                return;
            EnsureFolder(SpriteFolder);
            EnsureFolder(SceneFolder);
            square = MakeSprite("Square", 32, false);
            circle = MakeSprite("Circle", 64, true);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var level = new GameObject("Level").transform;
            var props = new GameObject("Props").transform;
            var pickups = new GameObject("Paint").transform;
            var players = new GameObject("Players").transform;
            var enemies = new GameObject("Enemies").transform;
            BuildLevel(level);
            BuildProps(props);
            BuildPaint(pickups);
            BuildEnemies(enemies);
            var start = new GameObject("Start Point").transform;
            start.position = new Vector3(-6f, 1f, 0f);
            var p1 = MakeToy(0, new Vector2(-6.6f, 1f), Player1, ControlScheme.WASD, players);
            var p2 = MakeToy(1, new Vector2(-5.2f, 1f), Player2, ControlScheme.Arrows, players);
            var strandGo = new GameObject("Paint Strand");
            strandGo.AddComponent<LineRenderer>();
            var strand = strandGo.AddComponent<PaintStrand>();
            strand.toyA = p1;
            strand.toyB = p2;
            var managers = new GameObject("Game Manager");
            var gm = managers.AddComponent<GameManager>();
            gm.toys = new[] { p1, p2 };
            gm.strand = strand;
            gm.startPoint = start;
            managers.AddComponent<PrototypeHUD>();
            MakeCamera();
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings(ScenePath);
            Selection.activeGameObject = managers;
            Debug.Log("Final Coat: level 2 built at " + ScenePath + ". Press Play.");
        }

        static void BuildLevel(Transform parent){
            Box("Wallpaper", new Vector2(40f, 6f), new Vector2(110f, 30f), WallPaper, parent, -20, false);
            Box("Floor 1", new Vector2(2f, -1f), new Vector2(20f, 2f), Floor, parent);
            Box("Floor 2", new Vector2(66.75f, -1f), new Vector2(38.5f, 2f), Floor, parent);
            Box("Left Wall", new Vector2(-8.5f, 6f), new Vector2(1f, 16f), Floor, parent);
            Box("Right Wall", new Vector2(86.5f, 6f), new Vector2(1f, 16f), Floor, parent);
            Ledge("Step 1", 7f, 1.8f, 3f, parent);
            Ledge("Step 2", 10.5f, 3.6f, 3f, parent);
            Ledge("Swing Start Ledge", 14f, 5.4f, 3f, parent);
            MakeSwing("Swing 1", 18.8f, 5.4f, 0f, parent);
            MakeSwing("Swing 2", 24.8f, 5.4f, Mathf.PI, parent);
            MakeSwing("Swing 3", 30.8f, 5.4f, 0f, parent);
            Ledge("Swing End Ledge", 36.85f, 5.4f, 5.3f, parent);
            Ledge("Down Step 1", 42f, 3.6f, 3f, parent);
            Ledge("Down Step 2", 45.5f, 1.8f, 3f, parent);
            Ledge("Solo Ledge", 54f, 2.8f, 2f, parent);
            var gate1 = MakeGate("Gate 1", new Vector2(58f, 3.25f), new Vector2(0.8f, 6.5f), parent);
            MakePlate("Plate A (left of gate 1)", new Vector2(50f, 0f), parent, gate1);
            MakePlate("Plate A2 (right of gate 1)", new Vector2(62f, 0f), parent, gate1);
            var gate2 = MakeGate("Gate 2", new Vector2(75f, 3.25f), new Vector2(0.8f, 6.5f), parent);
            MakePlate("Plate B (crate plate)", new Vector2(73.6f, 0f), parent, gate2);
            MakeCheckpoint(new Vector2(14.2f, 1f + 5.4f), parent);
            MakeCheckpoint(new Vector2(36.5f, 1f + 5.4f), parent);
            MakeCheckpoint(new Vector2(48.5f, 1f), parent);
            MakeExit(new Vector2(83f, 1.25f), parent);
            var kill = new GameObject("Kill Zone");
            kill.transform.SetParent(parent, false);
            kill.transform.position = new Vector2(43f, -7f);
            var killCol = kill.AddComponent<BoxCollider2D>();
            killCol.isTrigger = true;
            killCol.size = new Vector2(130f, 2f);
            kill.AddComponent<KillZone>();
        }

        static void BuildProps(Transform parent){
            MakeCrate("Crate (push onto Plate B)", new Vector2(66f, 0.5f), parent);
        }

        static void BuildEnemies(Transform parent){
            HelicopterBuilder.Make("Helicopter (climb)", new Vector2(3.5f, 3.5f), parent, 1.5f);
            HelicopterBuilder.Make("Helicopter (swings)", new Vector2(21.8f, 7.5f), parent, 1.2f);
            HelicopterBuilder.Make("Helicopter (solo pot)", new Vector2(54f, 5f), parent, 1.3f);
        }

        static void BuildPaint(Transform parent){
            Blob(-4f, 0.6f, parent); Blob(-2.5f, 0.6f, parent); Blob(-1f, 0.6f, parent); Blob(3f, 0.6f, parent);
            Blob(7f, 2.4f, parent); Blob(10.5f, 4.2f, parent);
            MakeBlob("Large Blob", new Vector2(10.5f, 6.5f), BlobKind.Large, 50, LargeBlob, 0.75f, parent);
            Blob(18.8f, 6.3f, parent); Blob(24.8f, 6.3f, parent); Blob(30.8f, 6.3f, parent);
            MakeBlob("Large Blob", new Vector2(24.8f, 8.7f), BlobKind.Large, 50, LargeBlob, 0.75f, parent);
            Blob(35.5f, 6.3f, parent); Blob(37.5f, 6.3f, parent); Blob(42f, 4.3f, parent); Blob(45.5f, 2.5f, parent);
            Blob(52f, 0.6f, parent);
            MakeBlob("Solo Paint Pot", new Vector2(54f, 3.55f), BlobKind.SoloPot, 75, SoloPot, 0.7f, parent);
            Blob(56f, 0.6f, parent); Blob(60f, 0.6f, parent); Blob(64f, 0.6f, parent);
            Blob(70f, 0.6f, parent); Blob(78f, 0.6f, parent);
            MakeBlob("Large Blob", new Vector2(79.5f, 2.2f), BlobKind.Large, 50, LargeBlob, 0.75f, parent);
        }

        static void Blob(float x, float y, Transform parent) =>
            MakeBlob("Blob", new Vector2(x, y), BlobKind.Small, 10, SmallBlob, 0.45f, parent);
        static GameObject Ledge(string name, float centerX, float topY, float width, Transform parent) =>
            Box(name, new Vector2(centerX, topY - 0.2f), new Vector2(width, 0.4f), Shelf, parent);

        static void MakeSwing(string name, float anchorX, float restTopY, float phase, Transform parent){
            var anchor = new Vector2(anchorX, restTopY + SwingLength);
            var rope = Box(name + " Rope", anchor, new Vector2(0.08f, SwingLength), Outline, parent, 1, false);
            Shape(name + " Hook", circle, parent, anchor, new Vector2(0.35f, 0.35f), Outline, 3);
            var plat = Box(name, new Vector2(anchorX, restTopY - 0.2f), new Vector2(SwingWidth, 0.4f), Shelf, parent, 2);
            var rb = plat.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            var swing = plat.AddComponent<SwingingPlatform>();
            swing.anchor = anchor;
            swing.ropeLength = SwingLength;
            swing.maxAngle = SwingAngle;
            swing.period = SwingPeriod;
            swing.phase = phase;
            swing.rope = rope.transform;
        }

        static PuzzleGate MakeGate(string name, Vector2 center, Vector2 size, Transform parent){
            var go = Box(name, center, size, new Color(0.85f, 0.35f, 0.35f), parent, 3);
            var gate = go.AddComponent<PuzzleGate>();
            gate.sprite = go.GetComponent<SpriteRenderer>();
            gate.solid = go.GetComponent<BoxCollider2D>();
            return gate;
        }

        static PressurePlate MakePlate(string name, Vector2 floorPoint, Transform parent, params PuzzleGate[] gates){
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = floorPoint;
            Shape("Slab", square, go.transform, new Vector2(0f, 0.06f), new Vector2(1.7f, 0.12f), Outline, 1);
            var button = Shape("Button", square, go.transform, new Vector2(0f, 0.17f), new Vector2(1.2f, 0.1f), new Color(0.9f, 0.75f, 0.3f), 2);
            var plate = go.AddComponent<PressurePlate>();
            plate.gates = gates;
            plate.button = button.transform;
            plate.buttonRenderer = button;
            return plate;
        }

        static void MakeCheckpoint(Vector2 position, Transform parent){
            var checkpoint = new GameObject("Checkpoint");
            checkpoint.transform.SetParent(parent, false);
            checkpoint.transform.position = position;
            var trigger = checkpoint.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(1f, 2f);
            Shape("Pole", square, checkpoint.transform, new Vector2(0f, 0f), new Vector2(0.1f, 2f), Outline, 2);
            var flag = Shape("Flag", square, checkpoint.transform, new Vector2(0.35f, 0.7f), new Vector2(0.6f, 0.4f), Color.gray, 3);
            checkpoint.AddComponent<Checkpoint>().flag = flag;
        }

        static void MakeExit(Vector2 position, Transform parent){
            var exit = new GameObject("Exit");
            exit.transform.SetParent(parent, false);
            exit.transform.position = position;
            var trigger = exit.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(1.6f, 2.5f);
            Shape("Frame", square, exit.transform, Vector2.zero, new Vector2(1.7f, 2.6f), Outline, 1);
            var door = Shape("Door", square, exit.transform, Vector2.zero, new Vector2(1.4f, 2.3f), Color.gray, 2);
            exit.AddComponent<LevelExit>().door = door;
        }

        static ToyController MakeToy(int index, Vector2 position, Color paint, ControlScheme controls, Transform parent){
            var go = new GameObject($"Player {index + 1}");
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.mass = 1f;
            rb.gravityScale = 2.5f;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.8f, 0.9f);
            var visual = new GameObject("Visual").transform;
            visual.SetParent(go.transform, false);
            Shape("Outline", square, visual, Vector2.zero, new Vector2(0.88f, 0.98f), Outline, 9);
            var bodySr = Shape("Body", square, visual, Vector2.zero, new Vector2(0.8f, 0.9f), Color.white, 10);
            Shape("Eye L", circle, visual, new Vector2(0.06f, 0.14f), new Vector2(0.13f, 0.18f), Outline, 11);
            Shape("Eye R", circle, visual, new Vector2(0.26f, 0.14f), new Vector2(0.13f, 0.18f), Outline, 11);
            Shape("Player Dot", circle, visual, new Vector2(0f, 0.56f), new Vector2(0.2f, 0.2f), paint, 11);
            var toy = go.AddComponent<ToyController>();
            toy.playerIndex = index;
            toy.controls = controls;
            toy.paintColor = paint;
            toy.visual = visual;
            var toyPaint = go.AddComponent<ToyPaint>();
            toyPaint.toy = toy;
            toyPaint.body = bodySr;
            return toy;
        }

        static PaintBlob MakeBlob(string name, Vector2 position, BlobKind kind, int value, Color color, float size, Transform parent){
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 2f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            var trigger = go.AddComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = size * 0.6f;
            var visual = new GameObject("Visual").transform;
            visual.SetParent(go.transform, false);
            if (kind == BlobKind.SoloPot){
                Shape("Pot", square, visual, new Vector2(0f, -0.1f), new Vector2(size, size * 0.75f), Outline, 7);
                Shape("Paint", circle, visual, new Vector2(0f, 0.18f), new Vector2(size * 0.9f, size * 0.55f), color, 8);
            }
            else{
                Shape("Drop", circle, visual, Vector2.zero, new Vector2(size, size), color, 8);
            }
            var blob = go.AddComponent<PaintBlob>();
            blob.kind = kind;
            blob.value = value;
            blob.visual = visual;
            blob.pickupTrigger = trigger;
            return blob;
        }

        static void MakeCrate(string name, Vector2 position, Transform parent){
            var crate = Box(name, position, Vector2.one, Crate, parent, 4);
            Shape("Lid", square, crate.transform, new Vector2(0f, 0.4f), new Vector2(1f, 0.12f), Shelf, 5);
            var rb = crate.AddComponent<Rigidbody2D>();
            rb.mass = 1f;
            rb.gravityScale = 2.5f;
            crate.AddComponent<Respawnable>();
        }

        static void MakeCamera(){
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            go.transform.position = new Vector3(-4f, 2f, -10f);
            var cam = go.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 6f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Background;
            go.AddComponent<AudioListener>();
            go.AddComponent<CameraFollow>();
        }

        static GameObject Box(string name, Vector2 center, Vector2 size, Color color, Transform parent, int order = 0, bool withCollider = true){
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = center;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = square;
            sr.color = color;
            sr.sortingOrder = order;
            if (withCollider) go.AddComponent<BoxCollider2D>();
            return go;
        }

        static SpriteRenderer Shape(string name, Sprite sprite, Transform parent, Vector2 localPosition, Vector2 size, Color color, int order){
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Vector3 ps = parent.lossyScale;
            go.transform.localPosition = new Vector3(localPosition.x / ps.x, localPosition.y / ps.y, 0f);
            go.transform.localScale = new Vector3(size.x / ps.x, size.y / ps.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = order;
            return sr;
        }

        static Sprite MakeSprite(string name, int size, bool round){
            string path = $"{SpriteFolder}/{name}.png";
            if (!File.Exists(path)){
                var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                var pixels = new Color32[size * size];
                float r = size * 0.5f;
                for (int y = 0; y < size; y++){
                    for (int x = 0; x < size; x++){
                        byte alpha = 255;
                        if (round){
                            float dx = x + 0.5f - r, dy = y + 0.5f - r;
                            float edge = Mathf.Clamp01(r - Mathf.Sqrt(dx * dx + dy * dy));
                            alpha = (byte)(edge * 255f);
                        }
                        pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                    }
                }
                tex.SetPixels32(pixels);
                tex.Apply();
                File.WriteAllBytes(path, tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
                AssetDatabase.ImportAsset(path);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = size;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.filterMode = FilterMode.Bilinear;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        static void EnsureFolder(string path){
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        static void AddToBuildSettings(string path){
            var scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.Any(s => s.path == path)) return;
            scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}

#endif
