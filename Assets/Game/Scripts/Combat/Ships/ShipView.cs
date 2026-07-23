using System;
using DG.Tweening;
using UnityEngine;

namespace Game.Scripts.Combat.Ships
{
    [Serializable]
    public sealed class ShipView
    {
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Transform _viewTransform;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private ShipControllerViewConfig _config;
        [SerializeField] private ParticleSystem _fireVFX;
        [SerializeField] private AudioClip _fireSFX;
        [SerializeField] private AudioClip _damageSFX;

        private Material _material;
        private Tweener _damageAnimation;

        public void Initialize()
        {
            if (!_renderer || !_viewTransform || !_config || !_config.MaterialPrefab)
                throw new InvalidOperationException("A ship view requires a renderer, view transform, material, and view config");

            if (_material)
                UnityEngine.Object.Destroy(_material);

            _material = new Material(_config.MaterialPrefab);
            _renderer.material = _material;
        }

        public void Tick(Vector2 moveDirection, float deltaTime)
        {
            Vector3 shipAngles = _viewTransform.localEulerAngles;
            shipAngles.x = _config.MoveRotationAngle * moveDirection.y;
            shipAngles.y = _config.MoveRotationAngle / 2 * moveDirection.x * -1f;

            Quaternion shipRotation = Quaternion.Euler(shipAngles);
            float interpolation = _config.TiltResponsiveness * deltaTime;
            _viewTransform.localRotation =
                Quaternion.Lerp(_viewTransform.localRotation, shipRotation, interpolation);
        }

        public void PlayFire()
        {
            if (_fireSFX && _audioSource)
                _audioSource.PlayOneShot(_fireSFX);

            if (_fireVFX)
                _fireVFX.Play();
        }

        public void PlayDamage()
        {
            if (_damageAnimation != null && _damageAnimation.IsActive())
                _damageAnimation.Kill();

            _damageAnimation = DOVirtual.Float(
                0f,
                1f,
                _config.HitDuration,
                progress => _material?.SetFloat(
                    _config.HitPropertyName,
                    _config.HitAnimationCurve.Evaluate(progress))
            ).SetLink(_renderer.gameObject);

            if (_damageSFX && _audioSource)
                _audioSource.PlayOneShot(_damageSFX);
        }

        public void PlayDeath()
        {
            ParticleSystem prefab = _config.DestroyEffectPrefab;

            if (prefab)
                UnityEngine.Object.Instantiate(prefab, _viewTransform.position, prefab.transform.rotation);
        }

        public void Reset()
        {
            if (_damageAnimation != null && _damageAnimation.IsActive())
                _damageAnimation.Kill();

            _damageAnimation = null;
            _material?.SetFloat(_config.HitPropertyName, 0f);
            _viewTransform.localRotation = Quaternion.identity;

            if (_fireVFX)
                _fireVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        public void Dispose()
        {
            if (_damageAnimation != null && _damageAnimation.IsActive())
                _damageAnimation.Kill();

            if (_material)
                UnityEngine.Object.Destroy(_material);
        }
    }
}
