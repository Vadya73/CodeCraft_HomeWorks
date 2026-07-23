using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public sealed class EnemyBulletInstantiator : MonoBehaviour
    {
        [SerializeField] private EnemySpawner _enemySpawner;
        [SerializeField] private BulletSpawner _bulletSpawner;

        private readonly List<Enemy> _enemies = new();

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
                _enemies[i].OnFire -= OnFire;

            _enemies.Clear();
        }

        private void OnEnemySpawned(Enemy enemy)
        {
            enemy.OnFire += OnFire;
            _enemies.Add(enemy);
        }

        private void OnEnemyDestroyed(Enemy enemy)
        {
            enemy.OnFire -= OnFire;
            _enemies.Remove(enemy);
        }

        private void OnFire(BulletData data)
        {
            _bulletSpawner.Spawn(data);
        }
    }
}
