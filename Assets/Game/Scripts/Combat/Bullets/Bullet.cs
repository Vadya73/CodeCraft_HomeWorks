using System;
using UnityEngine;

namespace Game
{
    // +
    public sealed class Bullet : MonoBehaviour
    {
        [SerializeField] private GameObject blueVFX;
        [SerializeField] private GameObject redVFX;

        private TeamType _team;
        private Vector2 _direction;
        private int _damage;
        private float _speed;
        private bool _isLaunched;

        public Vector3 Position => transform.position;
        public event Action<Bullet> Hit;

        public void Launch(BulletData data, int layer)
        {
            _team = data.Team;
            _direction = data.Direction;
            _damage = data.Damage;
            _speed = data.Speed;
            _isLaunched = true;

            transform.position = data.Position;
            transform.rotation = Quaternion.LookRotation(data.Direction, Vector3.forward);
            gameObject.layer = layer;

            SetTeamView(data.Team);
        }

        public void Stop()
        {
            _isLaunched = false;
            _direction = Vector2.zero;
        }

        private void FixedUpdate()
        {
            if (!_isLaunched)
                return;

            transform.position += (Vector3) (_direction * (_speed * Time.fixedDeltaTime));
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_isLaunched || !other.TryGetComponent(out ShipController ship) || ship.Team == _team)
                return;

            _isLaunched = false;
            ship.TakeDamage(_damage);
            Hit?.Invoke(this);
        }

        private void SetTeamView(TeamType team)
        {
            blueVFX.SetActive(team == TeamType.Player);
            redVFX.SetActive(team == TeamType.Enemy);
        }
    }
}
