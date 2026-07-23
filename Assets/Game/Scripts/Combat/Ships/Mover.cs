using System;
using UnityEngine;

namespace Game.Scripts.Combat.Ships
{
    // +
    [Serializable]
    public sealed class Mover
    {
        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private float _speed;

        private Vector2? _direction;

        public void SetSpeed(float speed)
        {
            if (speed < 0)
                throw new ArgumentOutOfRangeException(nameof(speed), speed, "Movement speed cannot be negative");

            _speed = speed;
        }

        public void SetDirection(Vector2 direction) => _direction = direction;

        public void Tick()
        {
            if (!_direction.HasValue)
                return;

            Vector2 direction = _direction.Value;
            Vector2 newPosition = _rigidbody.position + direction * (_speed * Time.fixedDeltaTime);
            _rigidbody.MovePosition(newPosition);
            _direction = null;
        }
    }
}
