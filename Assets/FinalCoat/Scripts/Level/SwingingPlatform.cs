using System.Collections.Generic;
using UnityEngine;

namespace FinalCoat{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class SwingingPlatform : MonoBehaviour{
        [Header("Pendulum")]
        public Vector2 anchor;
        public float ropeLength = 5.6f;
        public float maxAngle = 0.35f;
        public float period = 3.4f;
        public float phase;
        [Header("Visuals")]
        public Transform rope;
        public float ropeWidth = 0.08f;
        Rigidbody2D rb;
        BoxCollider2D box;
        float halfThickness;
        ContactFilter2D solidOnly;
        readonly List<Collider2D> overlaps = new List<Collider2D>(8);
        readonly HashSet<Rigidbody2D> riders = new HashSet<Rigidbody2D>();
        void Awake(){
            rb = GetComponent<Rigidbody2D>();
            box = GetComponent<BoxCollider2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            halfThickness = box.size.y * transform.lossyScale.y * 0.5f;
            solidOnly = new ContactFilter2D { useTriggers = false };
            Vector2 start = CenterAt(Time.fixedTime);
            transform.position = start;
            rb.position = start;
        }

        Vector2 CenterAt(float time){
            float angle = maxAngle * Mathf.Sin(time * Mathf.PI * 2f / period + phase);
            Vector2 top = anchor + new Vector2(Mathf.Sin(angle), -Mathf.Cos(angle)) * ropeLength;
            return top - Vector2.up * halfThickness;
        }

        void FixedUpdate(){
            Vector2 next = CenterAt(Time.fixedTime + Time.fixedDeltaTime);
            Vector2 delta = next - rb.position;
            CarryRiders(delta);
            rb.MovePosition(next);
        }

        void CarryRiders(Vector2 delta){
            Bounds b = box.bounds;
            Vector2 center = new Vector2(b.center.x, b.max.y + 0.06f);
            Vector2 size = new Vector2(b.size.x * 0.9f, 0.12f);
            overlaps.Clear();
            riders.Clear();
            Physics2D.OverlapBox(center, size, 0f, solidOnly, overlaps);
            foreach (var c in overlaps){
                var body = c.attachedRigidbody;
                if (body == null || body == rb || body.bodyType != RigidbodyType2D.Dynamic) continue;
                if (!riders.Add(body)) continue;
                if (body.GetVelocity().y > 1f) continue;
                body.position += delta;
            }
        }

        void LateUpdate(){
            if (rope == null) return;
            Vector2 top = (Vector2)transform.position + Vector2.up * halfThickness;
            Vector2 d = anchor - top;
            float length = d.magnitude;
            if (length < 0.01f) return;
            rope.position = new Vector3((top.x + anchor.x) * 0.5f, (top.y + anchor.y) * 0.5f, 0f);
            rope.rotation = Quaternion.FromToRotation(Vector3.up, new Vector3(d.x, d.y, 0f));
            rope.localScale = new Vector3(ropeWidth, length, 1f);
        }

        void OnDrawGizmosSelected(){
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(anchor, 0.2f);
            Gizmos.DrawLine(anchor, anchor + Vector2.down * ropeLength);
        }
    }
}
