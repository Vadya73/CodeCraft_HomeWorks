using UnityEngine;

namespace Game
{
    // +
    public sealed class PlayerBulletInstantiator : MonoBehaviour
    {
        [SerializeField]
        private BulletSpawner _bulletSpawner;

        [SerializeField]
        private PlayerShip _player;

        private void OnEnable()
        {
            _player.OnFire += OnFire;
        }

        private void OnDisable()
        {
            _player.OnFire -= OnFire;
        }

        private void OnFire(BulletData data)
        {
            _bulletSpawner.Spawn(data);
        }
    }
}
