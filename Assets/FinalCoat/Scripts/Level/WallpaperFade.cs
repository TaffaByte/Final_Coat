using UnityEngine;

namespace FinalCoat{
    [RequireComponent(typeof(SpriteRenderer))]
    public class WallpaperFade : MonoBehaviour{
        public float startX = -4f;
        public float endX = 70f;
        public float startAlpha = 0f;
        public float endAlpha = 0.35f;
        public float fadeSpeed = 1.5f;
        SpriteRenderer sr;
        float alpha;
        void Awake(){
            sr = GetComponent<SpriteRenderer>();
            alpha = startAlpha;
            Apply();
        }

        void Update(){
            var gm = GameManager.Instance;
            if (gm == null || gm.toys == null || gm.toys.Length == 0) return;
            float x = 0f;
            int count = 0;
            foreach (var toy in gm.toys){
                if (toy == null) continue;
                x += toy.transform.position.x;
                count++;
            }
            if (count == 0) return;
            float t = Mathf.InverseLerp(startX, endX, x / count);
            float target = Mathf.Lerp(startAlpha, endAlpha, t);
            alpha = Mathf.MoveTowards(alpha, target, Time.deltaTime * fadeSpeed);
            Apply();
        }

        void Apply(){
            var c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }
}
