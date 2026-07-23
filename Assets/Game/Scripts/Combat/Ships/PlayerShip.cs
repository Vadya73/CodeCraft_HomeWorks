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

        public override TeamType Team => TeamType.Player;

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

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
                FireForward();

            float dx = Input.GetAxisRaw("Horizontal");
            float dy = Input.GetAxisRaw("Vertical");
            Move(new Vector2(dx, dy));
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            transform.position = _playerArea.ClampInBounds(transform.position);
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
