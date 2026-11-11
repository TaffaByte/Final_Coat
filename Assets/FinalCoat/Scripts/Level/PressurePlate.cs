using System.Collections.Generic;
using UnityEngine;

namespace FinalCoat{
    public class PressurePlate : MonoBehaviour{
        public PuzzleGate[] gates;
        public Transform button;
        public SpriteRenderer buttonRenderer;
        public Vector2 detectSize = new Vector2(1.4f, 0.3f);
        public float pressDepth = 0.06f;
        public Color idleColor = new Color(0.9f, 0.75f, 0.3f);
        public Color pressedColor = new Color(0.4f, 0.8f, 0.45f);
        public bool IsPressed { get; private set; }
        readonly List<Collider2D> results = new List<Collider2D>(8);
        ContactFilter2D solidOnly;
        float buttonRestY;
        void Awake(){
            solidOnly = new ContactFilter2D { useTriggers = false };
            if (button != null) buttonRestY = button.localPosition.y;
            ApplyVisual();
        }

        void FixedUpdate(){
            Vector2 center = (Vector2)transform.position + new Vector2(0f, detectSize.y * 0.5f);
            results.Clear();
            Physics2D.OverlapBox(center, detectSize, 0f, solidOnly, results);
            bool pressed = false;
            foreach (var c in results){
                var body = c.attachedRigidbody;
                if (body != null && body.bodyType == RigidbodyType2D.Dynamic){
                    pressed = true;
                    break;
                }
            }
            if (pressed == IsPressed) return;
            IsPressed = pressed;
            if (gates != null)
                foreach (var g in gates)
                    if (g != null) g.SetHeld(this, pressed);
            ApplyVisual();
        }

        void OnDisable(){
            if (!IsPressed || gates == null) return;
            IsPressed = false;
            foreach (var g in gates)
                if (g != null) g.SetHeld(this, false);
        }

        void ApplyVisual(){
            if (button != null){
                Vector3 p = button.localPosition;
                p.y = buttonRestY - (IsPressed ? pressDepth : 0f);
                button.localPosition = p;
            }
            if (buttonRenderer != null) buttonRenderer.color = IsPressed ? pressedColor : idleColor;
        }

        void OnDrawGizmosSelected(){
            Gizmos.color = new Color(0.4f, 0.8f, 0.45f, 0.6f);
            Gizmos.DrawWireCube(transform.position + new Vector3(0f, detectSize.y * 0.5f, 0f), detectSize);
            if (gates == null) return;
            Gizmos.color = Color.red;
            foreach (var g in gates)
                if (g != null) Gizmos.DrawLine(transform.position, g.transform.position);
        }
    }
}
