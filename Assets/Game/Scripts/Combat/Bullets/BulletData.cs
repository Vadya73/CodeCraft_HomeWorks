using UnityEngine;

namespace Game
{
    public sealed class BulletData
    {
        public TeamType Team { get; }
        public Vector2 Position { get; }
        public Vector2 Direction { get; }
        public int Damage { get; }
        public float Speed { get; }

        public BulletData(
            TeamType team,
            Vector2 position,
            Vector2 direction,
            int damage,
            float speed)
        {
            Team = team;
            Position = position;
            Direction = direction.normalized;
            Damage = damage;
            Speed = speed;
        }
    }
}
