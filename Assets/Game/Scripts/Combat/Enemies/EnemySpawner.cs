using System;
using System.Collections;
using Game.Scripts.Combat.Ships;
using Game.Scripts.Infrastructure.Pooling;
using Modules.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Scripts.Combat.Enemies
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

        public event Action<Enemy> EnemySpawned;
        public event Action<Enemy> EnemyDestroyed;

        private void Awake()
        {
            ValidateConfiguration();
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

            Enemy enemy = _pool.Rent(item =>
            {
                item.transform.position = NextSpawnPosition();
                item.Initialize(_player, NextDestination(), this);
            });

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
            _pool.Return(enemy);
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

        private void ValidateConfiguration()
        {
            if (!_player)
                throw new InvalidOperationException("Enemy target is not configured");

            if (_spawnPositions == null || _spawnPositions.Length == 0)
                throw new InvalidOperationException("At least one enemy spawn point is required");

            if (_attackPositions == null || _attackPositions.Length == 0)
                throw new InvalidOperationException("At least one enemy attack point is required");
        }

        private void OnValidate()
        {
            _minSpawnCooldown = Mathf.Max(0, _minSpawnCooldown);
            _maxSpawnCooldown = Mathf.Max(_minSpawnCooldown, _maxSpawnCooldown);
            _initialPoolSize = Mathf.Max(0, _initialPoolSize);
        }
    }
}
