using UnityEngine;

namespace Game
{
    // +
    public sealed class Enemy : ShipController
    {
        [Header("Enemy")]
        public ShipController target;
        public Vector2 destination;

        [SerializeField]
        private float _fireCooldown = 1.25f;

        [SerializeField]
        private float _stoppingDistance = 0.25f;

        private float _fireTime;

        private IEnemyDespawner _despawner;

        public void SetDespawner(IEnemyDespawner despawner) => _despawner = despawner;

        private void OnEnable() => this.OnDead += this.OnCharacterDead;

        private void OnDisable() => this.OnDead -= this.OnCharacterDead;

        private void OnCharacterDead() => _despawner.Despawn(this);

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (this.currentHealth <= 0 || this.target == null || this.target.currentHealth <= 0)
                return;

            Vector2 distance = destination - (Vector2) this.transform.position;
            bool isNotReached = distance.sqrMagnitude > _stoppingDistance * _stoppingDistance;
            
            moveDirection = isNotReached ? distance.normalized : Vector3.zero;

            if (isNotReached)
            {
                _motor.MoveStep(distance.normalized);
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
    }
}