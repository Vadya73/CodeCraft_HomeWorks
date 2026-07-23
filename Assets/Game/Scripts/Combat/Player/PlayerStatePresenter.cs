using Modules.UI;
using Modules.Utils;
using UnityEngine;

namespace Game.Scripts.Combat.Player
{
    public sealed class PlayerStatePresenter : MonoBehaviour
    {
        [SerializeField] private PlayerShip _player;
        [SerializeField] private HealthView _healthView;
        [SerializeField] private CameraShaker _cameraShaker;
        [SerializeField] private GameOverView _gameOverView;

        private void OnEnable()
        {
            _player.HealthChanged += OnHealthChanged;
            _player.Damaged += OnDamaged;
            _player.Died += OnDied;
            _healthView.SetHealth(_player.Health, _player.MaxHealth);
        }

        private void OnDisable()
        {
            _player.HealthChanged -= OnHealthChanged;
            _player.Damaged -= OnDamaged;
            _player.Died -= OnDied;
        }

        private void OnHealthChanged(int health)
        {
            _healthView.SetHealth(health, _player.MaxHealth);
        }

        private void OnDamaged()
        {
            _cameraShaker.Shake();
        }

        private void OnDied()
        {
            _gameOverView.Show();
        }
    }
}
