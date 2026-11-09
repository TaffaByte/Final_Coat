#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FinalCoat.EditorTools{
    public static class HelicopterBuilder{
        const string ArtFolder = "Assets/FinalCoat/Art/Enemies";
        const float ArtScale = 2f;
        [MenuItem("Final Coat/Add Helicopter Toy To Open Scene")]
        static void AddToOpenScene(){
            var parent = GameObject.Find("Enemies");
            if (parent == null){
                parent = new GameObject("Enemies");
                Undo.RegisterCreatedObjectUndo(parent, "Add Helicopter Toy");
            }
            var view = SceneView.lastActiveSceneView;
            Vector2 spot = view != null ? (Vector2)view.pivot : Vector2.zero;
            var heli = Make("Helicopter Toy", spot, parent.transform);
            if (heli == null) return;
            Undo.RegisterCreatedObjectUndo(heli.gameObject, "Add Helicopter Toy");
            Selection.activeGameObject = heli.gameObject;
            EditorSceneManager.MarkSceneDirty(heli.gameObject.scene);
        }

        public static HelicopterToy Make(string name, Vector2 position, Transform parent, float patrolHeight = 1.5f){
            var bodyArt = Art("heli_body");
            var bladesArt = Art("heli_blades");
            var tailArt = Art("heli_tail_rotor");
            if (bodyArt == null || bladesArt == null || tailArt == null) return null;
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.mass = 4f;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.55f, 0.72f);
            col.offset = new Vector2(0f, -0.02f);
            var body = new GameObject("Body").transform;
            body.SetParent(go.transform, false);
            body.localScale = new Vector3(-ArtScale, ArtScale, 1f);
            var bodySr = Layer(body, bodyArt, 10);
            var blades = new GameObject("Blades").transform;
            blades.SetParent(body, false);
            blades.localPosition = PivotOffset(bladesArt);
            Layer(blades, bladesArt, 11);
            var tail = new GameObject("Tail Rotor").transform;
            tail.SetParent(body, false);
            tail.localPosition = PivotOffset(tailArt);
            Layer(tail, tailArt, 11);
            var heli = go.AddComponent<HelicopterToy>();
            heli.body = body;
            heli.blades = blades;
            heli.tailRotor = tail;
            heli.bodyRenderer = bodySr;
            heli.patrolHeight = patrolHeight;
            return heli;
        }

        static SpriteRenderer Layer(Transform target, Sprite sprite, int order){
            var sr = target.gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
            return sr;
        }

        static Vector3 PivotOffset(Sprite sprite){
            Vector2 offset = (sprite.pivot - sprite.rect.size * 0.5f) / sprite.pixelsPerUnit;
            return new Vector3(offset.x, offset.y, 0f);
        }

        static Sprite Art(string name){
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtFolder}/{name}.png");
            if (sprite == null) Debug.LogError("Final Coat: missing helicopter art " + name);
            return sprite;
        }
    }
}
#endif
