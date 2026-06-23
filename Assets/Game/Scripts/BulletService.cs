using System;
using System.Collections.Generic;
using Modules.Utils;
using UnityEngine;

namespace Game
{
    // +
    public sealed class BulletService : MonoBehaviour
    {
        private const string DefaultLayer = "Default";
        private const string PlayerBulletLayer = "PlayerBullet";
        private const string EnemyBulletLayer = "EnemyBullet";

        [SerializeField] private Bullet _prefab;
        [SerializeField] private Transform _container;
        [SerializeField] private BulletViewConfig _configView;
        [SerializeField] private TransformBounds _levelBounds;
        [SerializeField] private int _initialPoolSize = 10;

        private readonly Stack<Bullet> _pool = new();
        private readonly List<Bullet> _bullets = new();

        private void Awake()
        {
            for (var i = 0; i < _initialPoolSize; i++)
            {
                Bullet bullet = Instantiate(_prefab, _container);
                bullet.gameObject.SetActive(false);
                _pool.Push(bullet);
            }
        }

        private void FixedUpdate()
        {
            for (int i = _bullets.Count - 1; i >= 0; i--)
            {
                Bullet bullet = _bullets[i];
                Vector3 moveStep = bullet.Data.Direction * bullet.Data.Speed * Time.fixedDeltaTime;
                bullet.transform.position += moveStep;

                if (!_levelBounds.InBounds(bullet.transform.position))
                    Despawn(bullet, i);
            }
        }

        public void Spawn(Vector2 position, Vector2 direction, float speed, int damage, TeamType team)
        {
            Bullet bullet = GetBullet();
            BulletData bulletData = new BulletData(team, direction, damage, speed);
            Quaternion bulletRotation = Quaternion.LookRotation(direction, Vector3.forward);

            bullet.Initialize(bulletData, position, bulletRotation, GetLayer(team));
            bullet.OnTriggerEntered += this.OnTriggerEntered;
            _bullets.Add(bullet);
        }

        private void OnTriggerEntered(Bullet bullet, Collider2D other)
        {
            if (!other.TryGetComponent(out ShipController ship))
                return;

            if (!CanHit(bullet.Data.Team, ship))
                return;

            ship.TakeDamage(bullet.Data.Damage);
            Despawn(bullet);
            SpawnExplosion(bullet.transform.position);
        }

        private Bullet GetBullet()
        {
            if (!_pool.TryPop(out Bullet bullet))
                bullet = Instantiate(_prefab, _container);

            bullet.gameObject.SetActive(true);
            return bullet;
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
            bullet.OnTriggerEntered -= this.OnTriggerEntered;
            bullet.gameObject.SetActive(false);
            _pool.Push(bullet);
        }

        private static bool CanHit(TeamType bulletTeam, ShipController ship)
        {
            return bulletTeam == TeamType.Player && ship is Enemy ||
                   bulletTeam == TeamType.Enemy && ship is PlayerShip;
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
