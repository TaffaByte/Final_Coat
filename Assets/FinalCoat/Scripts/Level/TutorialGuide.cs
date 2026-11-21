using System.Linq;
using UnityEngine;

namespace FinalCoat{
    public class TutorialGuide : MonoBehaviour{
        public int fontSize = 15;
        public float fadeSpeed = 4f;
        public float bottomMargin = 24f;
        TutorialSign[] signs;
        int current = -1;
        int showing = -1;
        float alpha;
        GUIStyle style;
        void Start(){
            signs = FindObjectsByType<TutorialSign>(FindObjectsSortMode.None).OrderBy(s => s.transform.position.x).ToArray();
        }

        void Update(){
            var gm = GameManager.Instance;
            if (gm == null || gm.toys == null || signs == null) return;
            float lead = float.MinValue;
            foreach (var toy in gm.toys)
                if (toy != null) lead = Mathf.Max(lead, toy.transform.position.x);
            for (int i = signs.Length - 1; i >= 0; i--){
                if (signs[i].transform.position.x <= lead){
                    if (i > current) current = i;
                    break;
                }
            }
            bool hidden = gm.InMenu || gm.IsPaused || gm.IsLevelComplete;
            if (showing != current || hidden){
                alpha = Mathf.MoveTowards(alpha, 0f, Time.unscaledDeltaTime * fadeSpeed);
                if (alpha <= 0f && !hidden) showing = current;
            }
            else alpha = Mathf.MoveTowards(alpha, 1f, Time.unscaledDeltaTime * fadeSpeed);
        }

        void OnGUI(){
            if (alpha <= 0.01f || showing < 0 || signs == null) return;
            if (style == null){
                style = new GUIStyle(GUI.skin.label){
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = fontSize,
                    wordWrap = true,
                    richText = true
                };
                style.normal.textColor = Color.white;
            }
            string text = signs[showing].text;
            float width = Mathf.Min(620f, Screen.width - 40f);
            float height = style.CalcHeight(new GUIContent(text), width - 32f) + 18f;
            var box = new Rect((Screen.width - width) * 0.5f, Screen.height - bottomMargin - height, width, height);
            GUI.color = new Color(0.03f, 0.08f, 0.2f, 0.82f * alpha);
            GUI.DrawTexture(box, Texture2D.whiteTexture);
            GUI.color = new Color(1f, 0.8f, 0.1f, alpha);
            GUI.DrawTexture(new Rect(box.x, box.y, 4f, box.height), Texture2D.whiteTexture);
            GUI.color = new Color(1f, 1f, 1f, alpha);
            GUI.Label(new Rect(box.x + 16f, box.y, box.width - 32f, box.height), text, style);
            GUI.color = Color.white;
        }
    }
}
