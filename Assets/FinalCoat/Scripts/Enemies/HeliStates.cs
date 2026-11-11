using UnityEngine;

namespace FinalCoat{
    public abstract class HeliState{
        protected readonly HelicopterToy owner;
        protected float timeInState;
        protected HeliState(HelicopterToy owner) { this.owner = owner; }
        public abstract string Name { get; }
        public virtual void Enter() { timeInState = 0f; }
        public virtual void Tick(float dt) { timeInState += dt; }
        public virtual void FixedTick(float dt) { }
        public virtual void Exit() { }
        protected bool SpotPlayer(){
            var seen = owner.FindNearestToy(owner.chaseRadius);
            if (seen == null) return false;
            owner.Target = seen;
            owner.ChangeState(owner.Chase);
            return true;
        }
    }

    public class HeliPatrolState : HeliState{
        int direction = 1;
        public HeliPatrolState(HelicopterToy owner) : base(owner) { }
        public override string Name => "Patrol";
        public override void Enter(){
            base.Enter();
            owner.Target = null;
            owner.Alert = false;
            owner.RotorSpeed = 1f;
        }

        public override void Tick(float dt){
            base.Tick(dt);
            SpotPlayer();
        }

        public override void FixedTick(float dt){
            float offset = owner.transform.position.y - owner.Home.y;
            if (offset > owner.patrolHeight) direction = -1;
            else if (offset < -owner.patrolHeight) direction = 1;
            if (owner.IsBlocked(Vector2.up * direction, 0.15f)) direction = -direction;
            float drift = Mathf.Clamp(owner.Home.x - owner.transform.position.x, -1f, 1f);
            owner.FlyTowards(new Vector2(drift, direction * owner.patrolSpeed));
        }
    }

    public class HeliChaseState : HeliState{
        float retargetTimer;
        public HeliChaseState(HelicopterToy owner) : base(owner) { }
        public override string Name => owner.Target != null ? $"Chase P{owner.Target.playerIndex + 1}" : "Chase";
        public override void Enter(){
            base.Enter();
            retargetTimer = 0f;
            owner.Alert = true;
            owner.RotorSpeed = 1.6f;
        }

        public override void Tick(float dt){
            base.Tick(dt);
            retargetTimer -= dt;
            if (retargetTimer > 0f) return;
            retargetTimer = owner.retargetInterval;
            var nearest = owner.FindNearestToy(owner.LoseRadius);
            if (nearest == null){
                owner.ChangeState(owner.Hover);
                return;
            }
            owner.Target = nearest;
        }

        public override void FixedTick(float dt){
            if (owner.Target == null) return;
            Vector2 toTarget = owner.Target.transform.position - owner.transform.position;
            owner.FaceTowards(owner.Target.transform.position.x);
            owner.FlyTowards(toTarget.sqrMagnitude < 0.01f ? Vector2.zero : toTarget.normalized * owner.chaseSpeed);
        }
    }

    public class HeliHitState : HeliState{
        public HeliHitState(HelicopterToy owner) : base(owner) { }
        public override string Name => "Hit!";
        public override void Enter(){
            base.Enter();
            owner.RotorSpeed = 0.5f;
            owner.Body.SetVelocity(Vector2.zero);
        }

        public override void Tick(float dt){
            base.Tick(dt);
            if (timeInState < owner.hitPause + owner.backOffTime) return;
            if (!SpotPlayer()) owner.ChangeState(owner.Hover);
        }

        public override void FixedTick(float dt){
            if (timeInState < owner.hitPause){
                owner.FlyTowards(Vector2.zero);
                return;
            }
            Vector2 away = (Vector2)owner.transform.position - owner.HitFrom;
            away.y = Mathf.Max(away.y, 0.5f);
            owner.FlyTowards(away.normalized * owner.backOffSpeed);
        }
    }

    public class HeliHoverState : HeliState{
        public HeliHoverState(HelicopterToy owner) : base(owner) { }
        public override string Name => "Lost them";
        public override void Enter(){
            base.Enter();
            owner.Target = null;
            owner.Alert = false;
            owner.RotorSpeed = 1f;
        }

        public override void Tick(float dt){
            base.Tick(dt);
            if (SpotPlayer()) return;
            if (timeInState >= owner.hoverTime) owner.ChangeState(owner.Return);
        }

        public override void FixedTick(float dt) => owner.FlyTowards(Vector2.zero);
    }

    public class HeliReturnState : HeliState{
        const float GiveUpTime = 6f;
        public HeliReturnState(HelicopterToy owner) : base(owner) { }
        public override string Name => "Return";
        public override void Enter(){
            base.Enter();
            owner.RotorSpeed = 1.2f;
        }

        public override void Tick(float dt){
            base.Tick(dt);
            SpotPlayer();
        }

        public override void FixedTick(float dt){
            Vector2 toHome = owner.Home - (Vector2)owner.transform.position;
            if (toHome.magnitude < 0.2f){
                owner.ChangeState(owner.Patrol);
                return;
            }
            if (timeInState > GiveUpTime){
                owner.SetHome(owner.transform.position);
                owner.ChangeState(owner.Patrol);
                return;
            }
            owner.FaceTowards(owner.Home.x);
            owner.FlyTowards(toHome.normalized * owner.returnSpeed);
        }
    }
}
