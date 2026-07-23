using Modules.Utils;
using UnityEngine;

namespace Game.Scripts.Combat.Player
{
    public sealed class PlayerInputController : MonoBehaviour
    {
        private const string HorizontalAxis = "Horizontal";
        private const string VerticalAxis = "Vertical";

        [SerializeField] private PlayerShip _player;
        [SerializeField] private TransformBounds _playerArea;
        [SerializeField] private KeyCode _fireKey = KeyCode.Space;

        private void Update()
        {
            if (Input.GetKeyDown(_fireKey))
                _player.FireForward();

            float horizontal = Input.GetAxisRaw(HorizontalAxis);
            float vertical = Input.GetAxisRaw(VerticalAxis);
            _player.Move(new Vector2(horizontal, vertical));
        }

        private void LateUpdate()
        {
            _player.transform.position = _playerArea.ClampInBounds(_player.transform.position);
        }

        private void OnDisable()
        {
            if (_player)
                _player.Move(Vector2.zero);
        }
    }
}
