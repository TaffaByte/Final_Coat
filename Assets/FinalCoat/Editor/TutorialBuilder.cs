#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace FinalCoat.EditorTools{
    public static class TutorialBuilder{
        const string FurnitureFolder = "Assets/FinalCoat/Art/Furniture";
        const string WallpaperPath = "Assets/FinalCoat/Art/Background/wallpaper.png";
        const float TrapX = 66f;
        const float ChairLeft = 70.5f;
        const float SeatHeight = 11.5f;
        const float SignHeight = 5f;
        static readonly Color Floor = new Color(0.62f, 0.45f, 0.32f);
        static readonly Color RoomFloor = new Color(0.36f, 0.24f, 0.18f);
        static readonly Color RoomFloorEdge = new Color(0.5f, 0.35f, 0.25f);
        static readonly Color Shelf = new Color(0.74f, 0.56f, 0.38f);
        static readonly Color SmallBlob = new Color(0.2f, 0.75f, 0.85f);
        static readonly Color LargeBlob = new Color(0.98f, 0.78f, 0.15f);
        static readonly Color SoloPot = new Color(0.65f, 0.3f, 0.85f);
        static Transform furniture;
        public static GameManager BuildWorld(){
            if (!PrototypeSceneBuilder.Prepare()) return null;
            var level = new GameObject("Level").transform;
            var pickups = new GameObject("Paint").transform;
            var enemies = new GameObject("Enemies").transform;
            var players = new GameObject("Players").transform;
            var signs = new GameObject("Signs").transform;
            furniture = new GameObject("Furniture").transform;
            furniture.SetParent(level, false);
            BuildLevel(level);
            BuildPaint(pickups);
            BuildSigns(signs);
            var start = new GameObject("Start Point").transform;
            start.position = new Vector3(-6f, 1f, 0f);
            var p1 = PrototypeSceneBuilder.MakeToy(0, new Vector2(-6.6f, 1f), GameSettings.Colours[0], ControlScheme.WASD, players);
            var p2 = PrototypeSceneBuilder.MakeToy(1, new Vector2(-5.2f, 1f), GameSettings.Colours[1], ControlScheme.Arrows, players);
            var strandGo = new GameObject("Paint Strand");
            strandGo.AddComponent<LineRenderer>();
            var strand = strandGo.AddComponent<PaintStrand>();
            strand.toyA = p1;
            strand.toyB = p2;
            PrototypeSceneBuilder.MakeWindUp("Wind-up Toy", new Vector2(56f, 0.6f), 2.5f, PrototypeSceneBuilder.bot1, PrototypeSceneBuilder.bot1Key, new Vector2(0.09f, 0.035f), 2f, new Vector2(0.55f, 1.17f), enemies);
            var managers = new GameObject("Game Manager");
            var gm = managers.AddComponent<GameManager>();
            gm.toys = new[] { p1, p2 };
            gm.strand = strand;
            gm.startPoint = start;
            var hud = managers.AddComponent<PrototypeHUD>();
            hud.completeTitle = "TUTORIAL COMPLETE!";
            hud.completeMessage = "SNAP! Up onto the chair...";
            hud.showControls = false;
            managers.AddComponent<TutorialGuide>();
            var loader = managers.AddComponent<NextLevelLoader>();
            loader.nextSceneName = "Prototype";
            loader.delay = 2.2f;
            PrototypeSceneBuilder.MakeCamera();
            var follow = Object.FindFirstObjectByType<CameraFollow>();
            follow.minSize = 7f;
            follow.maxSize = 8.5f;
            follow.useLeftLimit = true;
            follow.leftLimit = -10f;
            var cam = follow.GetComponent<Camera>();
            cam.orthographicSize = 7f;
            Level1.UsePaintbrushes();
            return gm;
        }

        static void BuildLevel(Transform level){
            AddWallpaper();
            AddFloor(level, "Floor A", -9f, 20f);
            PrototypeSceneBuilder.Box("Gap Shadow", new Vector2(21.25f, -9f), new Vector2(2.5f, 18f), new Color(0.12f, 0.08f, 0.06f), level, -3, false);
            AddFloor(level, "Floor B", 22.5f, 92f);
            AddWall(level, "Left Wall", -9.5f);
            AddWall(level, "Right Wall", ChairLeft - 0.5f);
            AddLedge(level, "Jump Ledge", 8f, 1.8f, 3f);
            AddLedge(level, "High Ledge (tug up)", 33f, 3.8f, 3f);
            AddLedge(level, "Solo Ledge", 45f, 2.6f, 2f);
            AddCheckpoint(level, 24.5f);
            AddCheckpoint(level, 60f);
            AddMouseTrapExit(level);
            var kill = new GameObject("Kill Zone");
            kill.transform.SetParent(level, false);
            kill.transform.position = new Vector2(41f, -9f);
            var killCol = kill.AddComponent<BoxCollider2D>();
            killCol.isTrigger = true;
            killCol.size = new Vector2(120f, 2f);
            kill.AddComponent<KillZone>();
        }

        static void BuildPaint(Transform parent){
            Small(7.2f, 2.4f, parent); Small(8.8f, 2.4f, parent);
            Small(12f, 0.6f, parent); Small(13.5f, 0.6f, parent); Small(15f, 0.6f, parent); Small(16.5f, 0.6f, parent);
            PrototypeSceneBuilder.MakeBlob("Large Blob", new Vector2(18.5f, 3f), BlobKind.Large, 50, LargeBlob, 0.75f, parent);
            Small(26f, 0.6f, parent); Small(28f, 0.6f, parent);
            PrototypeSceneBuilder.MakeBlob("Large Blob", new Vector2(33f, 4.6f), BlobKind.Large, 50, LargeBlob, 0.75f, parent);
            Small(38f, 0.6f, parent); Small(40f, 0.6f, parent);
            PrototypeSceneBuilder.MakeBlob("Solo Paint Pot", new Vector2(45f, 3.35f), BlobKind.SoloPot, 75, SoloPot, 0.7f, parent);
            Small(49f, 0.6f, parent); Small(51f, 0.6f, parent); Small(57f, 0.6f, parent);
            Small(64f, 0.6f, parent); Small(66f, 0.6f, parent); Small(68f, 0.6f, parent);
        }

        static void Small(float x, float y, Transform parent) =>
            PrototypeSceneBuilder.MakeBlob("Blob", new Vector2(x, y), BlobKind.Small, 10, SmallBlob, 0.45f, parent);
        static void BuildSigns(Transform parent){
            Sign(parent, -12f, "Two unfinished toys got left behind in the toy room, still <b>blank</b>.");
            Sign(parent, -5f, "Tonight the window is open. Time to <b>escape</b>!\nP1 moves with <b>A / D</b>, P2 with the <b>arrow keys</b>.");
            Sign(parent, 3.5f, "Jump with <b>W</b>, <b>Up</b> or the <b>A button</b>.");
            Sign(parent, 10.5f, "Grab brushes to paint yourself in.\n<b>Green brush +10</b>     <b>Gold brush +50</b>");
            Sign(parent, 16.5f, "Your toy fills in as you collect paint.\nMost paint at the end gets the <b>FINAL COAT</b>!");
            Sign(parent, 19.5f, "Jump the gap! Falling costs paint: <b>-10</b> together, <b>-30</b> solo.");
            Sign(parent, 26f, "You two are stuck together with a <b>wet paint strand</b>.\nWander too far and it pulls you back.");
            Sign(parent, 30f, "Tap <b>Shift</b> or <b>X</b> to <b>TUG</b> your partner, like up onto that high ledge.");
            Sign(parent, 36.5f, "Hold <b>S</b>, <b>Down</b> or <b>B</b> to <b>HOLD ON</b>. You get heavy so your partner can swing.");
            Sign(parent, 42f, "Paint buckets give <b>+75</b>, but only <b>solo</b> toys can grab them.");
            Sign(parent, 44.5f, "Hold the strand button for 1 second to <b>SNAP</b> it.\nTap it next to your partner to rejoin.");
            Sign(parent, 49f, "Solo toys lose <b>double</b> paint. Share the bucket... or betray your friend?");
            Sign(parent, 52.5f, "Wind-up toys chase whoever is closest. A hit costs <b>-20</b> (<b>-40</b> solo).");
            Sign(parent, 59f, "Flags are checkpoints. Fall off and you come back here.");
            Sign(parent, 64f, "That <b>mouse trap</b> is your way up! First toy on it gets <b>+25</b>.\nStep on it together and it flings you onto the chair.");
        }

        static void Sign(Transform parent, float x, string text, float y = SignHeight){
            var go = new GameObject("Sign");
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector2(x, y);
            var sign = go.AddComponent<TutorialSign>();
            sign.text = text;
        }

        static void AddWallpaper(){
            var art = AssetDatabase.LoadAssetAtPath<Sprite>(WallpaperPath);
            if (art == null) return;
            Vector2 size = new Vector2(104f, 104f / 2.75f);
            Vector2 native = art.bounds.size;
            var sr = Place("Wallpaper Art", art, new Vector2(41f, -2f + size.y * 0.5f), -20);
            sr.transform.localScale = new Vector3(size.x / native.x, size.y / native.y, 1f);
        }

        static void AddFloor(Transform level, string name, float left, float right){
            var box = PrototypeSceneBuilder.Box(name, new Vector2((left + right) * 0.5f, -1f), new Vector2(right - left, 2f), Floor, level);
            box.GetComponent<SpriteRenderer>().enabled = false;
            float visualLeft = left < 0f ? left - 4f : left;
            float width = right - visualLeft;
            float centerX = visualLeft + width * 0.5f;
            PrototypeSceneBuilder.Box(name + " Ground", new Vector2(centerX, -9f), new Vector2(width, 18f), RoomFloor, level, -2, false);
            PrototypeSceneBuilder.Box(name + " Edge", new Vector2(centerX, -0.08f), new Vector2(width, 0.16f), RoomFloorEdge, level, -1, false);
        }

        static void AddWall(Transform level, string name, float x){
            var box = PrototypeSceneBuilder.Box(name, new Vector2(x, 10f), new Vector2(1f, 30f), Floor, level);
            box.GetComponent<SpriteRenderer>().enabled = false;
        }

        static void AddLedge(Transform level, string name, float centerX, float topY, float width){
            var box = PrototypeSceneBuilder.Box(name, new Vector2(centerX, topY - 0.2f), new Vector2(width, 0.4f), Shelf, level);
            var sr = Place(name + " Plank", Art("seesaw_plank"), Vector2.zero, 0);
            sr.drawMode = SpriteDrawMode.Tiled;
            float thick = sr.sprite.bounds.size.y;
            sr.size = new Vector2(width, thick);
            sr.transform.position = new Vector3(centerX, topY - thick * 0.5f, 0f);
            box.GetComponent<SpriteRenderer>().enabled = false;
        }

        static void AddCheckpoint(Transform level, float x){
            var checkpoint = new GameObject("Checkpoint");
            checkpoint.transform.SetParent(level, false);
            checkpoint.transform.position = new Vector2(x, 1f);
            var trigger = checkpoint.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(1f, 2f);
            PrototypeSceneBuilder.Shape("Pole", PrototypeSceneBuilder.square, checkpoint.transform, Vector2.zero, new Vector2(0.1f, 2f), new Color(0.2f, 0.18f, 0.22f), 2);
            var flag = PrototypeSceneBuilder.Shape("Flag", PrototypeSceneBuilder.square, checkpoint.transform, new Vector2(0.35f, 0.7f), new Vector2(0.6f, 0.4f), Color.gray, 3);
            checkpoint.AddComponent<Checkpoint>().flag = flag;
        }

        static void AddMouseTrapExit(Transform level){
            var sq = PrototypeSceneBuilder.square;
            var circle = PrototypeSceneBuilder.circle;
            var trap = new GameObject("Mouse Trap").transform;
            trap.SetParent(level, false);
            trap.position = new Vector2(TrapX, 0f);
            var ink = new Color(0.2f, 0.18f, 0.22f);
            var wood = new Color(0.82f, 0.62f, 0.38f);
            var metal = new Color(0.78f, 0.8f, 0.84f);
            PrototypeSceneBuilder.Shape("Base Outline", sq, trap, new Vector2(0f, 0.2f), new Vector2(3.3f, 0.48f), ink, 2);
            PrototypeSceneBuilder.Shape("Base", sq, trap, new Vector2(0f, 0.2f), new Vector2(3.2f, 0.38f), wood, 3);
            PrototypeSceneBuilder.Shape("Cheese", sq, trap, new Vector2(1.1f, 0.55f), new Vector2(0.45f, 0.3f), new Color(1f, 0.82f, 0.2f), 4);
            PrototypeSceneBuilder.Shape("Spring", circle, trap, new Vector2(0f, 0.45f), new Vector2(0.4f, 0.4f), metal, 5);
            var hinge = new GameObject("Hinge").transform;
            hinge.SetParent(trap, false);
            hinge.localPosition = new Vector2(0f, 0.45f);
            PrototypeSceneBuilder.Shape("Bar", sq, hinge, new Vector2(0f, 0.75f), new Vector2(0.12f, 1.5f), metal, 6);
            var launch = trap.gameObject.AddComponent<MouseTrapLaunch>();
            launch.hinge = hinge;
            launch.landPoints = new[] { new Vector2(ChairLeft + 2.2f, SeatHeight + 0.5f), new Vector2(ChairLeft + 3.6f, SeatHeight + 0.5f) };
            var chairArt = Art("chair_wood");
            Vector2 chairSize = chairArt.bounds.size;
            Place("Chair", chairArt, new Vector2(ChairLeft + chairSize.x * 0.5f, chairSize.y * 0.5f), -3);
            var exit = new GameObject("Exit");
            exit.transform.SetParent(level, false);
            exit.transform.position = new Vector2(TrapX, 1.25f);
            var trigger = exit.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(3.2f, 2.5f);
            exit.AddComponent<LevelExit>();
        }

        static SpriteRenderer Place(string name, Sprite sprite, Vector2 position, int order){
            var go = new GameObject(name);
            go.transform.SetParent(furniture, false);
            go.transform.position = position;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
            return sr;
        }

        static Sprite Art(string name){
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{FurnitureFolder}/{name}.png");
            if (sprite == null) Debug.LogError("Final Coat: missing furniture art " + name);
            return sprite;
        }
    }
}
#endif
