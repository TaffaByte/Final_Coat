using System.Collections.Generic;
using UnityEngine;

namespace FinalCoat{
    // A solid gate that is open while at least one linked PressurePlate is pressed.
    [RequireComponent(typeof(BoxCollider2D))]
    public class PuzzleGate : MonoBehaviour{
        public SpriteRenderer sprite;
        public BoxCollider2D solid;
        public Color closedColor = new Color(0.85f, 0.35f, 0.35f);
        public Color openColor = new Color(0.45f, 0.85f, 0.5f, 0.25f);
        public bool IsOpen { get; private set; }
        readonly HashSet<PressurePlate> holders = new HashSet<PressurePlate>();
        float lastHintTime = -10f;
        void Awake(){
            if (solid == null) solid = GetComponent<BoxCollider2D>();
            if (sprite == null) sprite = GetComponent<SpriteRenderer>();
            Apply(false);
        }

        public void SetHeld(PressurePlate plate, bool held){
            if (held) holders.Add(plate);
            else holders.Remove(plate);
        }

        void Update(){
            bool want = holders.Count > 0;
            // Never close on top of a toy or crate standing in the doorway.
            if (!want && IsOpen && Blocked()) want = true;
            if (want != IsOpen) Apply(want);
        }

        void Apply(bool open){
            IsOpen = open;
            if (solid != null) solid.enabled = !open;
            if (sprite != null) sprite.color = open ? openColor : closedColor;
        }

        bool Blocked(){
            Vector2 size = Vector2.Scale(solid.size, transform.lossyScale) * 0.9f;
            Vector2 center = transform.TransformPoint(solid.offset);
            var hits = Physics2D.OverlapBoxAll(center, size, 0f);
            foreach (var c in hits){
                if (c.isTrigger) continue;
                var body = c.attachedRigidbody;
                if (body != null && body.bodyType == RigidbodyType2D.Dynamic) return true;
            }
            return false;
        }

        void OnCollisionEnter2D(Collision2D collision){
            if (IsOpen || collision.collider.GetComponentInParent<ToyController>() == null) return;
            if (Time.time - lastHintTime < 4f) return;
            lastHintTime = Time.time;
            GameEvents.RaiseNotice("The gate is shut. Stand on a pressure plate to open it");
        }
    }
}
