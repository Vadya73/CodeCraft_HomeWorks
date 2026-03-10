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
            this.OnHealthChanged += health =>
            {
                _healthView.SetHealth(health, this.config.Health);
                _cameraShaker.Shake();
            };
            this.OnDead += _gameOverView.Show;
        }

        private void OnDisable()
        {
            this.OnHealthChanged -= health =>
            {
                _healthView.SetHealth(health, this.config.Health);
                _cameraShaker.Shake();
            };
            this.OnDead -= _gameOverView.Show;
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
                this.Fire();

            float dx = Input.GetAxisRaw("Horizontal");
            float dy = Input.GetAxisRaw("Vertical");
            this.moveDirection = new Vector2(dx, dy);

            if (this.currentHealth > 0)
            {
                _motor.MoveStep(this.moveDirection);
            }
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            this.transform.position = _playerArea.ClampInBounds(this.transform.position);
        }
    }
}