using UnityEngine;

namespace FinalCoat{
    public class ScreenFade : MonoBehaviour{
        public Color colour = new Color(0.95f, 0.91f, 0.84f);
        public float hold = 0.15f;
        public float duration = 0.6f;
        float startTime;
        void Awake() => startTime = Time.unscaledTime;
        void OnGUI(){
            float t = (Time.unscaledTime - startTime - hold) / duration;
            if (t >= 1f){
                Destroy(this);
                return;
            }
            GUI.depth = -1000;
            var c = colour;
            c.a = 1f - Mathf.Clamp01(t);
            GUI.color = c;
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
    }
}
