using System.Collections;
using Modules.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game
{
    public sealed class EnemySpawner : MonoBehaviour, IEnemyDespawner
    {
        [Header("Spawn")]
        [SerializeField] private float _minSpawnCooldown = 2;
        [SerializeField] private float _maxSpawnCooldown = 3;
        [SerializeField] private int _initialPoolSize = 5;
        private float _spawnCooldown;
        private float _spawnTime;

        [Header("Pool")]
        [SerializeField] private Enemy _prefab;
        [SerializeField] private Transform _container;
        private ComponentPool<Enemy> _pool;

        [Header("Target")]
        [SerializeField] private ShipController _player;

        [Header("Points")]
        [SerializeField] private Transform[] _spawnPositions;
        [SerializeField] private Transform[] _attackPositions;
        private int _spawnIndex;
        private int _attackIndex;

        public event System.Action<Enemy> EnemySpawned;
        public event System.Action<Enemy> EnemyDestroyed;

        private void Awake()
        {
            _pool = new ComponentPool<Enemy>(_prefab, _container, _initialPoolSize);
            _spawnPositions.Shuffle();
            _attackPositions.Shuffle();
        }

        private void Start()
        {
            ResetSpawnCooldown();
        }

        private void FixedUpdate()
        {
            float time = Time.fixedTime;

            if (time - _spawnTime < _spawnCooldown || !_player.IsAlive)
                return;

            Enemy enemy = _pool.Get();
            enemy.transform.position = NextSpawnPosition();
            enemy.Initialize(_player, NextDestination(), this);
            EnemySpawned?.Invoke(enemy);

            ResetSpawnCooldown();
        }

        public void Despawn(Enemy enemy)
        {
            EnemyDestroyed?.Invoke(enemy);
            StartCoroutine(DespawnInNextFrame(enemy));
        }

        private IEnumerator DespawnInNextFrame(Enemy enemy)
        {
            yield return null;
            enemy.ResetState();
            _pool.Release(enemy);
        }

        private void ResetSpawnCooldown()
        {
            _spawnCooldown = Random.Range(_minSpawnCooldown, _maxSpawnCooldown);
            _spawnTime = Time.fixedTime;
        }

        private Vector3 NextSpawnPosition()
        {
            if (_spawnIndex >= _spawnPositions.Length)
            {
                _spawnPositions.Shuffle();
                _spawnIndex = 0;
            }

            return _spawnPositions[_spawnIndex++].position;
        }

        private Vector3 NextDestination()
        {
            if (_attackIndex >= _attackPositions.Length)
            {
                _attackPositions.Shuffle();
                _attackIndex = 0;
            }

            return _attackPositions[_attackIndex++].position;
        }
    }
}
