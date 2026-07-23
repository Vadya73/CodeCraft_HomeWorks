using System;
using System.Collections.Generic;
using Game.Scripts.Infrastructure.Pooling;
using Modules.Utils;
using UnityEngine;

namespace Game.Scripts.Combat.Bullets
{
    // +
    public sealed class BulletSpawner : MonoBehaviour, IBulletSpawner
    {
        [SerializeField] private Bullet _prefab;
        [SerializeField] private Transform _container;
        [SerializeField] private TransformBounds _levelBounds;
        [SerializeField] private int _initialPoolSize = 10;

        private readonly List<Bullet> _bullets = new();
        private ComponentPool<Bullet> _pool;

        public event Action<Vector3> BulletHit;

        private void Awake()
        {
            if (!_levelBounds)
                throw new InvalidOperationException("Bullet bounds are not configured");

            _pool = new ComponentPool<Bullet>(_prefab, _container, _initialPoolSize);
        }

        private void FixedUpdate()
        {
            for (int i = _bullets.Count - 1; i >= 0; i--)
            {
                Bullet bullet = _bullets[i];

                if (!_levelBounds.InBounds(bullet.Position))
                    Despawn(bullet, i);
            }
        }

        public void Spawn(BulletData data)
        {
            Bullet bullet = _pool.Rent(item =>
            {
                item.Launch(data);
                item.Hit += OnBulletHit;
            });

            _bullets.Add(bullet);
        }

        private void OnBulletHit(Bullet bullet)
        {
            Vector3 hitPosition = bullet.Position;
            Despawn(bullet);
            BulletHit?.Invoke(hitPosition);
        }

        private void Despawn(Bullet bullet)
        {
            _bullets.Remove(bullet);
            DespawnInactive(bullet);
        }

        private void Despawn(Bullet bullet, int index)
        {
            _bullets.RemoveAt(index);
            DespawnInactive(bullet);
        }

        private void DespawnInactive(Bullet bullet)
        {
            bullet.Hit -= OnBulletHit;
            bullet.Stop();
            _pool.Return(bullet);
        }

        private void OnValidate()
        {
            _initialPoolSize = Mathf.Max(0, _initialPoolSize);
        }
    }
}
