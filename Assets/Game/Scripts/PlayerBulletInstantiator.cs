using UnityEngine;

namespace Game
{
    // +
    public sealed class PlayerBulletInstantiator : MonoBehaviour
    {
        [SerializeField]
        private BulletService _bulletWorld;

        [SerializeField]
        private PlayerShip _player;

        private void OnEnable()
        {
            _player.OnFire += this.OnFire;
        }

        private void OnDisable()
        {
            _player.OnFire -= this.OnFire;
        }

        private void OnFire(ShipController _)
        {
            _bulletWorld.Spawn(
                _player.FirePoint.position,
                _player.FirePoint.up,
                _player.BulletSpeed,
                _player.BulletDamage,
                TeamType.Player
            );
        }
    }
}