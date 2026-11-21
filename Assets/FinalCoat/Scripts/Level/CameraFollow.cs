using UnityEngine;

namespace FinalCoat{
    [RequireComponent(typeof(Camera))]
    public class CameraFollow : MonoBehaviour{
        public float smoothTime = 0.25f;
        public Vector2 offset = new Vector2(0f, 1.5f);
        public float minSize = 5.5f;
        public float maxSize = 9f;
        public float sizePerUnitApart = 0.35f;
        public bool useLeftLimit;
        public float leftLimit;
        Camera cam;
        Vector3 moveVelocity;
        float zoomVelocity;
        bool snapped;
        float holdUntil;
        void Awake(){
            cam = GetComponent<Camera>();
            cam.orthographic = true;
        }

        public void SnapTo(Vector2 center){
            snapped = true;
            moveVelocity = Vector3.zero;
            transform.position = new Vector3(center.x + offset.x, center.y + offset.y, transform.position.z);
        }

        public float ClampX(float x, float size){
            if (!useLeftLimit) return x;
            if (cam == null) cam = GetComponent<Camera>();
            return Mathf.Max(x, leftLimit + size * cam.aspect);
        }

        public void Hold(float seconds) => holdUntil = Time.unscaledTime + seconds;
        void LateUpdate(){
            if (Time.unscaledTime < holdUntil) return;
            var gm = GameManager.Instance;
            if (gm == null || gm.toys == null || gm.toys.Length == 0) return;
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);
            foreach (var toy in gm.toys){
                if (toy == null) continue;
                min = Vector2.Min(min, toy.transform.position);
                max = Vector2.Max(max, toy.transform.position);
            }
            Vector2 center = (min + max) * 0.5f + offset;
            Vector3 target = new Vector3(ClampX(center.x, cam.orthographicSize), center.y, transform.position.z);
            if (!snapped){
                snapped = true;
                transform.position = target;
            }
            transform.position = Vector3.SmoothDamp(transform.position, target, ref moveVelocity, smoothTime);
            float spread = Mathf.Max(max.x - min.x, (max.y - min.y) * cam.aspect);
            float targetSize = Mathf.Clamp(minSize + spread * sizePerUnitApart, minSize, maxSize);
            cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, targetSize, ref zoomVelocity, smoothTime);
        }
    }
}
