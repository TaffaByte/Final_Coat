using System.Collections;
using UnityEngine;

namespace FinalCoat{
    [RequireComponent(typeof(SpriteRenderer))]
    public class LadderClimb : MonoBehaviour{
        public float hopTime = 0.4f;
        public float hopHeight = 0.35f;
        public float rungSpacing = 1.5f;
        public float stagger = 0.3f;
        public Vector2 landOffset = new Vector2(1.6f, 0.7f);
        public float landSpacing = 1f;
        void Start() => GameEvents.LevelCompleted += Climb;
        void OnDestroy() => GameEvents.LevelCompleted -= Climb;
        void Climb(){
            var gm = GameManager.Instance;
            if (gm == null) return;
            var sr = GetComponent<SpriteRenderer>();
            float height = sr.sprite.bounds.size.y * transform.lossyScale.y;
            Vector2 up = transform.up;
            Vector2 foot = (Vector2)transform.position - up * height * 0.5f;
            Vector2 top = (Vector2)transform.position + up * height * 0.5f;
            for (int i = 0; i < gm.toys.Length; i++){
                Vector2 land = top + landOffset + Vector2.right * landSpacing * (gm.toys.Length - 1 - i);
                StartCoroutine(ClimbToy(gm.toys[i], foot, top, land, i * stagger));
            }
        }

        IEnumerator ClimbToy(ToyController toy, Vector2 foot, Vector2 top, Vector2 land, float wait){
            if (toy == null) yield break;
            if (toy.Body != null) toy.Body.simulated = false;
            yield return new WaitForSecondsRealtime(wait);
            int hops = Mathf.Max(1, Mathf.RoundToInt(Vector2.Distance(foot, top) / rungSpacing));
            Vector2 from = toy.transform.position;
            for (int k = 0; k <= hops + 1; k++){
                Vector2 to = k > hops ? land : Vector2.Lerp(foot, top, k / (float)hops) + Vector2.up * 0.5f;
                float t = 0f;
                while (t < 1f){
                    t = Mathf.Min(1f, t + Time.unscaledDeltaTime / hopTime);
                    float height = k > hops ? hopHeight * 2f : hopHeight;
                    toy.transform.position = Vector2.Lerp(from, to, t) + Vector2.up * Mathf.Sin(t * Mathf.PI) * height;
                    yield return null;
                }
                from = to;
            }
        }
    }
}
