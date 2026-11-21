using System.Collections;
using UnityEngine;

namespace FinalCoat{
    public class MouseTrapLaunch : MonoBehaviour{
        public Transform hinge;
        public float armedAngle = 80f;
        public float snappedAngle = -80f;
        public float snapTime = 0.1f;
        public Vector2[] landPoints;
        public float flightTime = 0.9f;
        public float arcHeight = 3f;
        public float stagger = 0.08f;
        void Start(){
            if (hinge != null) hinge.localRotation = Quaternion.Euler(0f, 0f, armedAngle);
            GameEvents.LevelCompleted += Snap;
        }

        void OnDestroy() => GameEvents.LevelCompleted -= Snap;
        void Snap() => StartCoroutine(SnapRoutine());
        IEnumerator SnapRoutine(){
            float t = 0f;
            while (t < 1f){
                t = Mathf.Min(1f, t + Time.unscaledDeltaTime / snapTime);
                if (hinge != null) hinge.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(armedAngle, snappedAngle, t * t));
                yield return null;
            }
            var gm = GameManager.Instance;
            if (gm == null || landPoints == null) yield break;
            for (int i = 0; i < gm.toys.Length && i < landPoints.Length; i++)
                if (gm.toys[i] != null) StartCoroutine(Fly(gm.toys[i], landPoints[i], i * stagger));
        }

        IEnumerator Fly(ToyController toy, Vector2 to, float wait){
            if (toy.Body != null) toy.Body.simulated = false;
            yield return new WaitForSecondsRealtime(wait);
            Vector2 from = toy.transform.position;
            float t = 0f;
            while (t < 1f){
                t = Mathf.Min(1f, t + Time.unscaledDeltaTime / flightTime);
                float eased = 1f - (1f - t) * (1f - t);
                Vector2 p = Vector2.Lerp(from, to, eased) + Vector2.up * arcHeight * Mathf.Sin(t * Mathf.PI);
                toy.transform.position = p;
                yield return null;
            }
            toy.transform.position = to;
        }
    }
}
