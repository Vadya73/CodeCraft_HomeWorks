using System;
using UnityEngine;

namespace Game.Scripts.Combat.Bullets
{
    public sealed class BulletImpactPresenter : MonoBehaviour
    {
        [SerializeField] private BulletSpawner _bulletSpawner;
        [SerializeField] private BulletImpactConfig _config;

        private void Awake()
        {
            if (!_bulletSpawner || !_config || !_config.ExplosionVFX)
                throw new InvalidOperationException("Bullet impact presentation is not fully configured");
        }

        private void OnEnable()
        {
            _bulletSpawner.BulletHit += OnBulletHit;
        }

        private void OnDisable()
        {
            _bulletSpawner.BulletHit -= OnBulletHit;
        }

        private void OnBulletHit(Vector3 position)
        {
            GameObject prefab = _config.ExplosionVFX;
            Instantiate(prefab, position, prefab.transform.rotation);
        }
    }
}
