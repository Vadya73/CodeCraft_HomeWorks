using System;
using System.Collections.Generic;
using Modules.Utils;
using UnityEngine;

namespace Game
{
    // +
    public sealed class BulletSpawner : MonoBehaviour
    {
        private const string DefaultLayer = "Default";
        private const string PlayerBulletLayer = "PlayerBullet";
        private const string EnemyBulletLayer = "EnemyBullet";

        [SerializeField] private Bullet _prefab;
        [SerializeField] private Transform _container;
        [SerializeField] private BulletViewConfig _configView;
        [SerializeField] private TransformBounds _levelBounds;
        [SerializeField] private int _initialPoolSize = 10;

        private readonly List<Bullet> _bullets = new();
        private ComponentPool<Bullet> _pool;

        private void Awake()
        {
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
            Bullet bullet = _pool.Get();
            bullet.Hit += OnBulletHit;
            bullet.Launch(data, GetLayer(data.Team));
            _bullets.Add(bullet);
        }

        private void OnBulletHit(Bullet bullet)
        {
            Vector3 hitPosition = bullet.Position;
            Despawn(bullet);
            SpawnExplosion(hitPosition);
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
            _pool.Release(bullet);
        }

        private static int GetLayer(TeamType team)
        {
            string layerName;

            switch (team)
            {
                case TeamType.None:
                    layerName = DefaultLayer;
                    break;
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

        private void SpawnExplosion(Vector3 position)
        {
            GameObject prefab = _configView.ExplosionVFX;
            Instantiate(prefab, position, prefab.transform.rotation);
        }
    }
}
