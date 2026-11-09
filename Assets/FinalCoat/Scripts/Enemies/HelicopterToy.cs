using System.Collections.Generic;
using UnityEngine;

namespace FinalCoat{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class HelicopterToy : MonoBehaviour{
        [Header("Patrol")]
        public float patrolHeight = 1.5f;
        public float patrolSpeed = 1.2f;
        [Header("Chase")]
        public float chaseRadius = 3.5f;
        public float escapeBuffer = 0.5f;
        public float chaseSpeed = 2.6f;
        public float steering = 6f;
        public float retargetInterval = 0.25f;
        [Header("After losing a player")]
        public float hoverTime = 1f;
        public float returnSpeed = 1.8f;
        [Header("After a hit")]
        public float hitPause = 0.3f;
        public float backOffTime = 0.8f;
        public float backOffSpeed = 2f;
        [Header("Visuals")]
        public Transform body;
        public Transform blades;
        public Transform tailRotor;
        public SpriteRenderer bodyRenderer;
        public Color calmColor = Color.white;
        public Color alertColor = new Color(1f, 0.72f, 0.72f);
        [Header("Debug")]
        public bool showStateLabel = true;
        public Rigidbody2D Body { get; private set; }
        public Vector2 Home { get; private set; }
        public ToyController Target { get; set; }
        public Vector2 HitFrom { get; private set; }
        public int Facing { get; set; } = 1;
        public float RotorSpeed { get; set; } = 1f;
        public bool Alert { get; set; }
        public float LoseRadius => chaseRadius + escapeBuffer;
        public HeliPatrolState Patrol { get; private set; }
        public HeliChaseState Chase { get; private set; }
        public HeliHitState Hit { get; private set; }
        public HeliHoverState Hover { get; private set; }
        public HeliReturnState Return { get; private set; }
        public HeliState Current { get; private set; }
        Vector2 spawnPoint;
        Vector3 bodyBaseScale = Vector3.one;
        Vector3 bladesBaseScale = Vector3.one;
        float bladeTurn;
        readonly List<RaycastHit2D> castHits = new List<RaycastHit2D>(4);
        ContactFilter2D solidOnly;
        GUIStyle labelStyle;
        void Awake(){
            Body = GetComponent<Rigidbody2D>();
            Body.gravityScale = 0f;
            Body.freezeRotation = true;
            spawnPoint = transform.position;
            Home = spawnPoint;
            solidOnly = new ContactFilter2D { useTriggers = false };
            if (body != null) bodyBaseScale = body.localScale;
            if (blades != null) bladesBaseScale = blades.localScale;
            if (bodyRenderer != null) bodyRenderer.color = calmColor;
            Patrol = new HeliPatrolState(this);
            Chase = new HeliChaseState(this);
            Hit = new HeliHitState(this);
            Hover = new HeliHoverState(this);
            Return = new HeliReturnState(this);
        }

        void Start() => ChangeState(Patrol);
        void Update(){
            var gm = GameManager.Instance;
            if (gm != null && gm.IsLevelComplete) return;
            Current?.Tick(Time.deltaTime);
            Animate();
        }

        void FixedUpdate(){
            var gm = GameManager.Instance;
            if (gm != null && gm.IsLevelComplete) { Body.SetVelocity(Vector2.zero); return; }
            Current?.FixedTick(Time.fixedDeltaTime);
        }

        public void ChangeState(HeliState next){
            Current?.Exit();
            Current = next;
            Current.Enter();
        }

        public ToyController FindNearestToy(float range){
            var gm = GameManager.Instance;
            if (gm == null) return null;
            ToyController best = null;
            float bestDistance = range;
            foreach (var toy in gm.toys){
                if (toy == null) continue;
                float distance = Vector2.Distance(toy.transform.position, transform.position);
                if (distance <= bestDistance){
                    best = toy;
                    bestDistance = distance;
                }
            }
            return best;
        }

        public void FlyTowards(Vector2 desiredVelocity){
            Vector2 v = Vector2.MoveTowards(Body.GetVelocity(), desiredVelocity, steering * Time.fixedDeltaTime);
            Body.SetVelocity(v);
        }

        public void FaceTowards(float x){
            float dx = x - transform.position.x;
            if (Mathf.Abs(dx) > 0.1f) Facing = dx > 0f ? 1 : -1;
        }

        public bool IsBlocked(Vector2 direction, float distance){
            castHits.Clear();
            int count = Body.Cast(direction, solidOnly, castHits, distance);
            for (int i = 0; i < count; i++)
                if (castHits[i].collider.GetComponentInParent<ToyController>() == null) return true;
            return false;
        }

        public void SetHome(Vector2 position) => Home = position;
        public void ResetToHome(){
            transform.position = spawnPoint;
            Body.position = spawnPoint;
            Body.SetVelocity(Vector2.zero);
            Home = spawnPoint;
            ChangeState(Patrol);
        }

        void OnCollisionEnter2D(Collision2D collision) => TryHit(collision.collider);
        void OnCollisionStay2D(Collision2D collision) => TryHit(collision.collider);
        void TryHit(Collider2D other){
            if (Current is HeliHitState) return;
            var toy = other.GetComponentInParent<ToyController>();
            if (toy == null || toy.IsInvulnerable) return;
            toy.TakeHit(transform.position);
            Target = toy;
            HitFrom = toy.transform.position;
            ChangeState(Hit);
        }

        void Animate(){
            if (blades != null){
                bladeTurn += RotorSpeed * 1440f * Time.deltaTime;
                blades.localScale = new Vector3(bladesBaseScale.x * Mathf.Cos(bladeTurn * Mathf.Deg2Rad), bladesBaseScale.y, 1f);
            }
            if (tailRotor != null) tailRotor.Rotate(0f, 0f, -RotorSpeed * 900f * Time.deltaTime);
            if (body != null){
                float bob = Mathf.Sin(Time.time * 3f) * 0.06f;
                float tilt = Mathf.Clamp(-Body.GetVelocity().x * 6f, -15f, 15f);
                body.localPosition = new Vector3(0f, bob, 0f);
                body.localRotation = Quaternion.Euler(0f, 0f, tilt);
                body.localScale = new Vector3(bodyBaseScale.x * Facing, bodyBaseScale.y, 1f);
            }
            if (bodyRenderer != null)
                bodyRenderer.color = Color.Lerp(bodyRenderer.color, Alert ? alertColor : calmColor, Time.deltaTime * 6f);
        }

        void OnGUI(){
            if (!showStateLabel || Current == null) return;
            var cam = Camera.main;
            if (cam == null) return;
            Vector3 screen = cam.WorldToScreenPoint(transform.position + Vector3.up * 0.9f);
            if (screen.z < 0f) return;
            if (labelStyle == null){
                labelStyle = new GUIStyle(GUI.skin.label){
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    fontSize = 13
                };
                labelStyle.normal.textColor = new Color(0.15f, 0.15f, 0.2f);
            }
            GUI.Label(new Rect(screen.x - 80f, Screen.height - screen.y - 12f, 160f, 24f), Current.Name, labelStyle);
        }

        void OnDrawGizmosSelected(){
            Vector3 home = Application.isPlaying ? (Vector3)Home : transform.position;
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.6f);
            Gizmos.DrawWireSphere(transform.position, chaseRadius);
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, chaseRadius + escapeBuffer);
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(home + Vector3.down * patrolHeight, home + Vector3.up * patrolHeight);
        }
    }
}
