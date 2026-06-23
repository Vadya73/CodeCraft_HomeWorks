using UnityEngine;

namespace Game
{
    // +
    public sealed class Enemy : ShipController
    {
        [SerializeField] private float _fireCooldown = 1.25f;
        [SerializeField] private float _stoppingDistance = 0.25f;

        private ShipController _target;
        private Vector2 _destination;

        private float _fireTime;
        private IEnemyDespawner _despawner;

        public void SetDespawner(IEnemyDespawner despawner) => _despawner = despawner;

        private void OnEnable() => this.OnDead += this.OnCharacterDead;

        private void OnDisable() => this.OnDead -= this.OnCharacterDead;

        private void OnCharacterDead() => _despawner?.Despawn(this);

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!IsAlive || _target == null || !_target.IsAlive)
                return;

            Vector2 distance = _destination - (Vector2) this.transform.position;
            bool isNotReached = distance.sqrMagnitude > _stoppingDistance * _stoppingDistance;
            
            moveDirection = isNotReached ? distance.normalized : Vector3.zero;

            if (isNotReached)
            {
                _mover.MoveStep(distance.normalized);
            }
            else
            {
                float time = Time.time;
                if (time - _fireTime >= _fireCooldown)
                {
                    this.Fire();
                    _fireTime = time;
                }
            }
        }

        public void SetDestination(Vector3 nextDestination)
        {
            _destination = nextDestination;
        }

        public void SetTarget(ShipController player)
        {
            _target = player;
        }
    }
}