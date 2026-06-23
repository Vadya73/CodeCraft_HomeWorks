using Modules.UI;
using Modules.Utils;
using UnityEngine;

namespace Game
{
    // +
    public sealed class PlayerShip : ShipController
    {
        [SerializeField]
        private TransformBounds _playerArea;

        [SerializeField]
        private CameraShaker _cameraShaker;

        [Header("UI")]
        [SerializeField]
        private GameOverView _gameOverView;

        [SerializeField]
        private HealthView _healthView;

        private void OnEnable()
        {
            this.OnHealthChanged += HandleHealthChanged;
            this.OnDead += HandleDead;
        }

        private void OnDisable()
        {
            this.OnHealthChanged -= HandleHealthChanged;
            this.OnDead -= HandleDead;
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
                this.Fire();

            float dx = Input.GetAxisRaw("Horizontal");
            float dy = Input.GetAxisRaw("Vertical");
            this.moveDirection = new Vector2(dx, dy);

            if (IsAlive)
                _mover.MoveStep(this.moveDirection);
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            this.transform.position = _playerArea.ClampInBounds(this.transform.position);
        }

        private void HandleHealthChanged(int health)
        {
            _healthView.SetHealth(health, MaxHealth);
            _cameraShaker.Shake();
        }

        private void HandleDead()
        {
            _gameOverView.Show();
        }
    }
}