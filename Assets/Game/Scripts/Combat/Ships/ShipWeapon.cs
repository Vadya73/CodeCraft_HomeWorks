using System;
using Game.Scripts.Combat.Bullets;
using Game.Scripts.Combat.Common;
using UnityEngine;

namespace Game.Scripts.Combat.Ships
{
    [Serializable]
    public sealed class ShipWeapon
    {
        [SerializeField] private Transform _firePoint;
        [SerializeField] private float _bulletSpeed;
        [SerializeField] private int _bulletDamage;

        private float _cooldown;
        private float _lastFireTime;

        public void Initialize(float cooldown)
        {
            if (!_firePoint)
                throw new InvalidOperationException("A ship weapon requires a fire point");

            if (_bulletSpeed <= 0)
                throw new InvalidOperationException("Bullet speed must be greater than zero");

            if (_bulletDamage <= 0)
                throw new InvalidOperationException("Bullet damage must be greater than zero");

            if (cooldown < 0)
                throw new ArgumentOutOfRangeException(nameof(cooldown), cooldown, "Fire cooldown cannot be negative");

            _cooldown = cooldown;
            Reset();
        }

        public bool TryFire(TeamType team, Vector2 direction, float time, out BulletData data)
        {
            data = null;

            if (direction.sqrMagnitude == 0 || time - _lastFireTime < _cooldown)
                return false;

            data = new BulletData(
                team,
                _firePoint.position,
                direction,
                _bulletDamage,
                _bulletSpeed
            );

            _lastFireTime = time;
            return true;
        }

        public Vector2 DirectionTo(Vector2 targetPosition) =>
            (targetPosition - (Vector2) _firePoint.position).normalized;

        public Vector2 Forward => _firePoint.up;

        public void Reset()
        {
            _lastFireTime = float.NegativeInfinity;
        }

        internal void Validate()
        {
            _bulletSpeed = Mathf.Max(0, _bulletSpeed);
            _bulletDamage = Mathf.Max(1, _bulletDamage);
        }
    }
}
