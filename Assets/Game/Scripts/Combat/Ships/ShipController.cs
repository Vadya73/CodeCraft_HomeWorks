using System;
using Game.Scripts.Combat.Bullets;
using Game.Scripts.Combat.Common;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Scripts.Combat.Ships
{
    // +
    public abstract class ShipController : MonoBehaviour, IDamageable
    {
        public event Action<int> HealthChanged;
        public event Action Damaged;
        public event Action Died;
        public event Action<BulletData> Fired;

        [SerializeField, FormerlySerializedAs("config")]
        private ShipConfig _config;

        [Header("Combat")]
        [SerializeField] private ShipWeapon _weapon;

        [Header("Movement")]
        [SerializeField, FormerlySerializedAs("_motor")]
        private Mover _mover;
        private Vector3 _moveDirection;

        [Header("Visual")]
        [SerializeField] private ShipView _view;

        private ShipHealth _health;

        public abstract TeamType Team { get; }
        public int Health => GetHealth().Current;
        public int MaxHealth => GetHealth().Maximum;
        public bool IsAlive => GetHealth().IsAlive;
        public Vector2 Position => transform.position;

        private void Awake()
        {
            if (_health == null)
                ResetForReuse();
        }

        private void Initialize()
        {
            if (_health != null)
                return;

            if (!_config)
                throw new InvalidOperationException("A ship requires a ship config");

            _health = new ShipHealth(_config.Health);
            _health.HealthChanged += OnHealthChanged;
            _health.Damaged += OnDamaged;
            _health.Died += OnDied;

            _weapon.Initialize(_config.FireCooldown);
            _mover.SetSpeed(_config.MoveSpeed);
            _view.Initialize();
        }

        private void OnDestroy()
        {
            if (_health == null)
                return;

            _health.HealthChanged -= OnHealthChanged;
            _health.Damaged -= OnDamaged;
            _health.Died -= OnDied;
            _view.Dispose();
        }

        private void FixedUpdate() => _mover.Tick();

        public void Move(Vector2 direction)
        {
            _moveDirection = IsAlive ? direction : Vector2.zero;
            _mover.SetDirection(_moveDirection);
        }

        public void FireForward()
        {
            Fire(_weapon.Forward);
        }

        protected void FireTowards(Vector2 targetPosition)
        {
            Fire(_weapon.DirectionTo(targetPosition));
        }

        private void Fire(Vector2 direction)
        {
            if (!IsAlive || !_weapon.TryFire(Team, direction, Time.time, out BulletData bulletData))
                return;

            _view.PlayFire();
            Fired?.Invoke(bulletData);
        }

        private void LateUpdate()
        {
            _view.Tick(_moveDirection, Time.deltaTime);
        }

        protected void ResetForReuse()
        {
            Initialize();
            _health.Reset();
            _weapon.Reset();
            _moveDirection = Vector2.zero;
            _mover.SetDirection(Vector2.zero);
            _view.Reset();
        }

        public void TakeDamage(int damage)
        {
            GetHealth().TakeDamage(damage);
        }

        private void OnHealthChanged(int health)
        {
            HealthChanged?.Invoke(health);
        }

        private void OnDamaged()
        {
            if (IsAlive)
                _view.PlayDamage();

            Damaged?.Invoke();
        }

        private void OnDied()
        {
            _moveDirection = Vector2.zero;
            _mover.SetDirection(Vector2.zero);
            _view.PlayDeath();
            Died?.Invoke();
            gameObject.SetActive(false);
        }

        private void OnValidate()
        {
            _weapon?.Validate();
        }

        private ShipHealth GetHealth()
        {
            Initialize();
            return _health;
        }
    }
}
