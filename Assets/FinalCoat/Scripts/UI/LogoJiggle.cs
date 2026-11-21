using UnityEngine;
using UnityEngine.EventSystems;

namespace FinalCoat{
    public class LogoJiggle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler{
        public float wobbleAngle = 3f;
        public float wobbleSpeed = 1.6f;
        public float breathe = 0.025f;
        public float boingStrength = 0.1f;
        public float boingSpeed = 22f;
        public float boingDamping = 5f;
        public float autoBoingEvery = 4f;
        public float phase;
        public float hoverGrow = 0.04f;
        public float startDelay;
        Vector3 baseScale = Vector3.one;
        float boingTime = -10f;
        float nextAutoBoing;
        bool hovered;
        float hover;
        void Awake(){
            baseScale = transform.localScale;
            nextAutoBoing = Time.unscaledTime + 0.4f + startDelay;
        }

        public void Boing() => boingTime = Time.unscaledTime;
        void Update(){
            float t = Time.unscaledTime + phase;
            if (autoBoingEvery > 0f && Time.unscaledTime >= nextAutoBoing){
                Boing();
                nextAutoBoing = Time.unscaledTime + autoBoingEvery;
            }
            float since = Time.unscaledTime - boingTime;
            float boing = since < 2f ? Mathf.Sin(since * boingSpeed) * Mathf.Exp(-since * boingDamping) * boingStrength : 0f;
            float idle = Mathf.Sin(t * wobbleSpeed * 2f) * breathe;
            hover = Mathf.MoveTowards(hover, hovered ? 1f : 0f, Time.unscaledDeltaTime * 4f);
            float grow = 1f + Mathf.SmoothStep(0f, 1f, hover) * hoverGrow;
            transform.localScale = new Vector3(baseScale.x * grow * (1f + boing - idle), baseScale.y * grow * (1f - boing + idle), 1f);
            transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * wobbleSpeed) * wobbleAngle + boing * 6f);
        }

        public void OnPointerEnter(PointerEventData eventData) => hovered = true;
        public void OnPointerExit(PointerEventData eventData) => hovered = false;
        public void OnPointerClick(PointerEventData eventData) => Boing();
    }
}
