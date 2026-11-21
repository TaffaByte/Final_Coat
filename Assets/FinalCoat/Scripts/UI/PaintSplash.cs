using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FinalCoat{
    public class PaintSplash : MonoBehaviour, IPointerClickHandler{
        public Sprite dropSprite;
        public LogoJiggle jiggle;
        public Color[] paintColours = { new Color(0.35f, 0f, 0f), new Color(0.5f, 0.04f, 0.04f), new Color(0.24f, 0f, 0f), new Color(0.62f, 0.1f, 0.08f) };
        public int drops = 16;
        public Vector2 speedRange = new Vector2(350f, 850f);
        public Vector2 sizeRange = new Vector2(12f, 34f);
        public float gravity = 2400f;
        public float life = 0.9f;
        public void OnPointerClick(PointerEventData eventData){
            if (jiggle != null) jiggle.Boing();
            Splash(eventData.position);
        }

        public void Splash(Vector2 screenPoint){
            var canvas = GetComponentInParent<Canvas>().rootCanvas;
            float scale = canvas.scaleFactor;
            for (int i = 0; i < drops; i++){
                var go = new GameObject("Paint Drop", typeof(RectTransform));
                go.transform.SetParent(canvas.transform, false);
                var rt = (RectTransform)go.transform;
                rt.position = screenPoint;
                rt.sizeDelta = Vector2.one * Random.Range(sizeRange.x, sizeRange.y);
                var img = go.AddComponent<Image>();
                img.sprite = dropSprite;
                img.color = PickColour();
                img.raycastTarget = false;
                float angle = Random.Range(15f, 165f) * Mathf.Deg2Rad;
                float speed = Random.Range(speedRange.x, speedRange.y) * scale;
                var drop = go.AddComponent<SplashDrop>();
                drop.velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;
                drop.gravity = gravity * scale;
                drop.life = life * Random.Range(0.8f, 1.2f);
            }
        }

        Color PickColour() => paintColours[Random.Range(0, paintColours.Length)];
    }
}
