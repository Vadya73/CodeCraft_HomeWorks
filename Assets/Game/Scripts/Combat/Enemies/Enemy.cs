using Game.Scripts.Combat.Common;
using Game.Scripts.Combat.Ships;
using UnityEngine;

namespace Game.Scripts.Combat.Enemies
{
    // +
    public sealed class Enemy : ShipController
    {
        [SerializeField] private float _stoppingDistance = 0.25f;

        private ShipController _target;
        private Vector2 _destination;
        private IEnemyDespawner _despawner;

        public override TeamType Team => TeamType.Enemy;

        private void OnEnable() => Died += OnCharacterDied;

        private void OnDisable() => Died -= OnCharacterDied;

        private void OnCharacterDied() => _despawner?.Despawn(this);

        private void Update()
        {
            if (!IsAlive || _target == null || !_target.IsAlive)
                return;

            Vector2 distance = _destination - Position;
            bool isNotReached = distance.sqrMagnitude > _stoppingDistance * _stoppingDistance;

            if (isNotReached)
            {
                Move(distance.normalized);
            }
            else
            {
                Move(Vector2.zero);
                FireTowards(_target.Position);
            }
        }

        internal void Initialize(
            ShipController target,
            Vector3 destination,
            IEnemyDespawner despawner)
        {
            _target = target;
            _destination = destination;
            _despawner = despawner;
            ResetForReuse();
        }

        internal void ResetState()
        {
            _target = null;
            _destination = Vector2.zero;
            _despawner = null;
            Move(Vector2.zero);
        }
    }
}
