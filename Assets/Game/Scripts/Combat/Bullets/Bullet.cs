using System;
using Game.Scripts.Combat.Common;
using UnityEngine;

namespace Game.Scripts.Combat.Bullets
{
    // +
    public sealed class Bullet : MonoBehaviour
    {
        private const string DefaultLayer = "Default";
        private const string PlayerBulletLayer = "PlayerBullet";
        private const string EnemyBulletLayer = "EnemyBullet";

        [SerializeField] private GameObject blueVFX;
        [SerializeField] private GameObject redVFX;

        private TeamType _team;
        private Vector2 _direction;
        private int _damage;
        private float _speed;
        private bool _isLaunched;

        public Vector3 Position => transform.position;
        public event Action<Bullet> Hit;

        public void Launch(BulletData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            _team = data.Team;
            _direction = data.Direction;
            _damage = data.Damage;
            _speed = data.Speed;
            _isLaunched = true;

            transform.position = data.Position;
            transform.rotation = Quaternion.LookRotation(data.Direction, Vector3.forward);
            gameObject.layer = GetLayer(data.Team);

            SetTeamView(data.Team);
        }

        public void Stop()
        {
            _isLaunched = false;
            _team = TeamType.None;
            _direction = Vector2.zero;
            _damage = 0;
            _speed = 0;
        }

        private void FixedUpdate()
        {
            if (!_isLaunched)
                return;

            transform.position += (Vector3) (_direction * (_speed * Time.fixedDeltaTime));
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_isLaunched)
                return;

            IDamageable damageable = FindDamageable(other);

            if (damageable == null || !damageable.IsAlive || damageable.Team == _team)
                return;

            int damage = _damage;
            Stop();
            damageable.TakeDamage(damage);
            Hit?.Invoke(this);
        }

        private static IDamageable FindDamageable(Collider2D other)
        {
            MonoBehaviour[] behaviours = other.GetComponentsInParent<MonoBehaviour>(true);

            for (int i = 0; i < behaviours.Length; i++)
            {
                IDamageable damageable = behaviours[i] as IDamageable;

                if (damageable != null)
                    return damageable;
            }

            return null;
        }

        private void SetTeamView(TeamType team)
        {
            blueVFX.SetActive(team == TeamType.Player);
            redVFX.SetActive(team == TeamType.Enemy);
        }

        private static int GetLayer(TeamType team)
        {
            string layerName;

            switch (team)
            {
                case TeamType.Player:
                    layerName = PlayerBulletLayer;
                    break;
                case TeamType.Enemy:
                    layerName = EnemyBulletLayer;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(team), team, null);
            }

            int layer = LayerMask.NameToLayer(layerName);
            return layer >= 0 ? layer : LayerMask.NameToLayer(DefaultLayer);
        }
    }
}
