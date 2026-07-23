using System;
using Game.Scripts.Combat.Bullets;
using UnityEngine;

namespace Game.Scripts.Combat.Player
{
    // +
    public sealed class PlayerBulletRouter : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour _bulletSpawner;
        [SerializeField] private PlayerShip _player;

        private IBulletSpawner _spawner;

        private void Awake()
        {
            _spawner = _bulletSpawner as IBulletSpawner;

            if (_spawner == null)
                throw new InvalidOperationException("The assigned bullet spawner must implement IBulletSpawner");
        }

        private void OnEnable()
        {
            _player.Fired += OnFired;
        }

        private void OnDisable()
        {
            _player.Fired -= OnFired;
        }

        private void OnFired(BulletData data)
        {
            _spawner.Spawn(data);
        }
    }
}
