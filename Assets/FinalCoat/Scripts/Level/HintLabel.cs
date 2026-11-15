using UnityEngine;

namespace FinalCoat{
    public class HintLabel : MonoBehaviour{
        public string text = "JUMP TOGETHER!";
        public int fontSize = 16;
        GUIStyle style;
        void OnGUI(){
            var cam = Camera.main;
            if (cam == null) return;
            Vector3 screen = cam.WorldToScreenPoint(transform.position);
            if (screen.z < 0f) return;
            if (style == null){
                style = new GUIStyle(GUI.skin.label){
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    fontSize = fontSize
                };
                style.normal.textColor = Color.white;
            }
            Vector2 size = style.CalcSize(new GUIContent(text));
            var box = new Rect(screen.x - size.x * 0.5f - 10f, Screen.height - screen.y - size.y * 0.5f - 4f, size.x + 20f, size.y + 8f);
            GUI.color = new Color(0f, 0f, 0f, 0.7f);
            GUI.DrawTexture(box, Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(box, text, style);
        }
    }
}
