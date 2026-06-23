using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game
{
    // +
    public abstract class ShipController : MonoBehaviour
    {
        public event Action<int> OnHealthChanged;
        public event Action OnDead;
        public event Action<ShipController> OnFire;

        [SerializeField, FormerlySerializedAs("config")]
        private ShipControllerSO _config;

        [Header("Health")]
        [SerializeField, FormerlySerializedAs("currentHealth")]
        private int _currentHealth;

        [Header("Combat")]
        [SerializeField, FormerlySerializedAs("firePoint")]
        private Transform _firePoint;
        [SerializeField, FormerlySerializedAs("bulletSpeed")]
        private float _bulletSpeed;
        [SerializeField, FormerlySerializedAs("bulletDamage")]
        private int _bulletDamage;
        private float _fireTime;

        [Header("Movement")]
        [SerializeField, FormerlySerializedAs("_motor")]
        protected Mover _mover;
        protected Vector3 moveDirection;

        [Header("Visual")]
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Transform _viewTransform;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private ShipControllerViewConfig _viewConfig;
        [SerializeField] private ParticleSystem _fireVFX;
        [SerializeField] private AudioClip _fireSFX;
        [SerializeField] private AudioClip _damageSFX;

        private Material _material;
        private Tweener _damageAnimation;

        public int Health => _currentHealth;
        public int MaxHealth => _config.Health;
        public bool IsAlive => _currentHealth > 0;
        public Transform FirePoint => _firePoint;
        public float BulletSpeed => _bulletSpeed;
        public int BulletDamage => _bulletDamage;

        private void Awake()
        {
            ResetHealth();
            _mover.SetSpeed(_config.MoveSpeed);

            _material = new Material(_viewConfig.MaterialPrefab);
            _renderer.material = _material;
        }

        protected virtual void FixedUpdate() => _mover.FixedUpdate();

        protected void Fire()
        {
            float time = Time.time;
            if (time - _fireTime < _config.FireCooldown || !IsAlive)
                return;

            if (_fireSFX)
                _audioSource.PlayOneShot(_fireSFX);

            if (_fireVFX)
                _fireVFX.Play();

            this.OnFire?.Invoke(this);
            _fireTime = time;
        }

        protected virtual void LateUpdate()
        {
            this.AnimateMovement(Time.deltaTime);
        }

        private void AnimateMovement(float deltaTime)
        {
            Vector3 shipAngles = _viewTransform.localEulerAngles;
            shipAngles.x = _viewConfig.MoveRotationAngle * moveDirection.y;
            shipAngles.y = _viewConfig.MoveRotationAngle / 2 * moveDirection.x * -1f;

            Quaternion shipRotation = Quaternion.Euler(shipAngles);
            float t = _viewConfig.MoveSpeed * deltaTime;
            _viewTransform.localRotation = Quaternion.Lerp(_viewTransform.localRotation, shipRotation, t);
        }

        public void ResetHealth()
        {
            _currentHealth = _config.Health;
            this.OnHealthChanged?.Invoke(_currentHealth);
        }

        public void TakeDamage(int damage)
        {
            if (damage <= 0 || !IsAlive)
                return;

            _currentHealth = Mathf.Clamp(_currentHealth - damage, 0, MaxHealth);
            if (IsAlive)
                this.AnimateDamage();

            this.OnHealthChanged?.Invoke(_currentHealth);

            if (!IsAlive)
                Die();
        }

        private void Die()
        {
            ParticleSystem prefab = _viewConfig.DestroyEffectPrefab;
            Instantiate(prefab, _viewTransform.position, prefab.transform.rotation);

            this.OnDead?.Invoke();
            gameObject.SetActive(false);
        }

        private void AnimateDamage()
        {
            if (_damageAnimation != null && _damageAnimation.IsActive())
                _damageAnimation.Kill();

            _damageAnimation = DOVirtual.Float(
                0f,
                1f,
                _viewConfig.HitDuration,
                progress => _material?.SetFloat(_viewConfig.HitPropertyName,
                    _viewConfig.HitAnimationCurve.Evaluate(progress))
            ).SetLink(_renderer.gameObject);

            if (_damageSFX)
                _audioSource.PlayOneShot(_damageSFX);
        }
    }
}