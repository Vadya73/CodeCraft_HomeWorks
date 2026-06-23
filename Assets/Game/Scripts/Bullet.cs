using System;
using UnityEngine;

namespace Game
{
    // +
    public sealed class Bullet : MonoBehaviour
    {
        [SerializeField] private GameObject blueVFX;
        [SerializeField] private GameObject redVFX;

        private BulletData _data;

        public BulletData Data => _data;
        public Transform ObjectTransform => gameObject.transform;
        public event Action<Bullet, Collider2D> OnTriggerEntered;

        private void OnTriggerEnter2D(Collider2D other) => this.OnTriggerEntered?.Invoke(this, other);

        public void Initialize(BulletData data, Vector2 position, Quaternion lookRotation, int layer)
        {
            _data = data;
            transform.position = position;
            transform.rotation = lookRotation;
            gameObject.layer = layer;

            SetTeamView(data.Team);
        }

        private void SetTeamView(TeamType team)
        {
            blueVFX.SetActive(team == TeamType.Player);
            redVFX.SetActive(team == TeamType.Enemy);
        }
    }

    public sealed class BulletData
    {
        public TeamType Team { get; }
        public Vector2 Direction { get; }
        public int Damage { get; }
        public float Speed { get; }

        public BulletData(TeamType team, Vector2 direction, int damage, float speed)
        {
            Team = team;
            Direction = direction;
            Damage = damage;
            Speed = speed;
        }
    }
}