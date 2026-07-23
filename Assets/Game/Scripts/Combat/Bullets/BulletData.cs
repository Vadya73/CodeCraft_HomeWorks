using System;
using Game.Scripts.Combat.Common;
using UnityEngine;

namespace Game.Scripts.Combat.Bullets
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
            if (team == TeamType.None)
                throw new ArgumentOutOfRangeException(nameof(team), team, "A bullet must belong to a team");

            if (direction.sqrMagnitude == 0)
                throw new ArgumentException("Bullet direction cannot be zero.", nameof(direction));

            if (damage <= 0)
                throw new ArgumentOutOfRangeException(nameof(damage), damage, "Bullet damage must be positive");

            if (speed <= 0)
                throw new ArgumentOutOfRangeException(nameof(speed), speed, "Bullet speed must be positive");

            Team = team;
            Position = position;
            Direction = direction.normalized;
            Damage = damage;
            Speed = speed;
        }
    }
}
