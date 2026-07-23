using System;
using System.Collections.Generic;
using Game.Scripts.Combat.Bullets;
using UnityEngine;

namespace Game.Scripts.Combat.Enemies
{
    public sealed class EnemyBulletRouter : MonoBehaviour
    {
        [SerializeField] private EnemySpawner _enemySpawner;
        [SerializeField] private MonoBehaviour _bulletSpawner;

        private readonly List<Enemy> _enemies = new();
        private IBulletSpawner _spawner;

        private void Awake()
        {
            _spawner = _bulletSpawner as IBulletSpawner;

            if (_spawner == null)
                throw new InvalidOperationException("The assigned bullet spawner must implement IBulletSpawner");
        }

        private void OnEnable()
        {
            _enemySpawner.EnemySpawned += OnEnemySpawned;
            _enemySpawner.EnemyDestroyed += OnEnemyDestroyed;
        }

        private void OnDisable()
        {
            _enemySpawner.EnemySpawned -= OnEnemySpawned;
            _enemySpawner.EnemyDestroyed -= OnEnemyDestroyed;

            for (int i = 0; i < _enemies.Count; i++)
                _enemies[i].Fired -= OnFired;

            _enemies.Clear();
        }

        private void OnEnemySpawned(Enemy enemy)
        {
            enemy.Fired += OnFired;
            _enemies.Add(enemy);
        }

        private void OnEnemyDestroyed(Enemy enemy)
        {
            enemy.Fired -= OnFired;
            _enemies.Remove(enemy);
        }

        private void OnFired(BulletData data)
        {
            _spawner.Spawn(data);
        }
    }
}
