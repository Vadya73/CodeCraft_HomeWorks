using System;
using UnityEngine;

namespace Game
{
    // +
    [Serializable]
    public sealed class Motor
    {
        public event Action<Vector3> OnMoved;
        
        [SerializeField]
        private Rigidbody2D _rigidbody;

        [SerializeField]
        private float _speed;

        private Vector2? _direction;

        public void SetSpeed(float speed) => _speed = speed;

        public void MoveStep(Vector2 direction) => _direction = direction;

        public void FixedUpdate()
        {
            if (!_direction.HasValue)
                return;

            Vector2 direction = _direction.Value;
            Vector2 newPosition = _rigidbody.position + direction * (_speed * Time.fixedDeltaTime);
            _rigidbody.MovePosition(newPosition);
            _direction = null;
            
            this.OnMoved?.Invoke(direction);
        }
    }
}