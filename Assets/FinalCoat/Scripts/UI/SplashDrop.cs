using UnityEngine;
using UnityEngine.UI;

namespace FinalCoat{
    public class SplashDrop : MonoBehaviour{
        public Vector2 velocity;
        public float gravity;
        public float life = 0.9f;
        float age;
        Image img;
        Vector3 baseScale;
        void Awake(){
            img = GetComponent<Image>();
            baseScale = transform.localScale;
        }

        void Update(){
            float dt = Time.unscaledDeltaTime;
            age += dt;
            velocity.y -= gravity * dt;
            transform.position += (Vector3)(velocity * dt);
            float t = age / life;
            float stretch = Mathf.Clamp(velocity.magnitude / 2000f, 0f, 0.5f);
            transform.localScale = new Vector3(baseScale.x * (1f - stretch * 0.5f), baseScale.y * (1f + stretch), 1f);
            transform.up = velocity.sqrMagnitude > 1f ? (Vector3)velocity.normalized : transform.up;
            if (img != null){
                var c = img.color;
                c.a = Mathf.Clamp01(1f - t * t);
                img.color = c;
            }
            if (age >= life) Destroy(gameObject);
        }
    }
}
