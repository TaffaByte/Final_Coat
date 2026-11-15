using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FinalCoat{
    [RequireComponent(typeof(Collider2D))]
    public class WindowFall : MonoBehaviour{
        public string nextSceneName = "Level3";
        public string titleText = "Level 3";
        public float fadeTime = 1f;
        public float holdTime = 1f;
        readonly HashSet<ToyController> inside = new HashSet<ToyController>();
        Texture2D black;
        GUIStyle style;
        float startTime = -1f;
        bool loaded;
        void Reset() => GetComponent<Collider2D>().isTrigger = true;
        void OnTriggerEnter2D(Collider2D other){
            var toy = other.GetComponentInParent<ToyController>();
            if (toy != null) inside.Add(toy);
        }

        void OnTriggerExit2D(Collider2D other){
            var toy = other.GetComponentInParent<ToyController>();
            if (toy != null) inside.Remove(toy);
        }

        void Update(){
            if (startTime < 0f){
                var gm = GameManager.Instance;
                int needed = gm != null ? gm.toys.Length : 2;
                if (inside.Count < needed) return;
                startTime = Time.unscaledTime;
                foreach (var zone in FindObjectsByType<KillZone>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                    zone.gameObject.SetActive(false);
                return;
            }
            if (loaded || Time.unscaledTime - startTime < fadeTime + holdTime) return;
            loaded = true;
            if (Application.CanStreamedLevelBeLoaded(nextSceneName)){
                Time.timeScale = 1f;
                SceneManager.LoadScene(nextSceneName);
            }
            else Debug.LogWarning("Final Coat: scene '" + nextSceneName + "' is not in the build yet.");
        }

        void OnGUI(){
            if (startTime < 0f) return;
            if (black == null){
                black = new Texture2D(1, 1);
                black.SetPixel(0, 0, Color.white);
                black.Apply();
            }
            float alpha = Mathf.Clamp01((Time.unscaledTime - startTime) / fadeTime);
            GUI.depth = -1000;
            GUI.color = new Color(0f, 0f, 0f, alpha);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), black);
            GUI.color = Color.white;
            if (alpha < 1f) return;
            if (style == null){
                style = new GUIStyle(GUI.skin.label){ alignment = TextAnchor.MiddleCenter, fontSize = 32, fontStyle = FontStyle.Bold };
                style.normal.textColor = Color.white;
            }
            GUI.Label(new Rect(0f, 0f, Screen.width, Screen.height), titleText, style);
        }
    }
}
