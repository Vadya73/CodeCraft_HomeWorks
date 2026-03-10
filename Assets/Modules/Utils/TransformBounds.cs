using UnityEngine;

namespace Modules.Utils
{
    public sealed class TransformBounds : MonoBehaviour
    {
        [SerializeField]
        private Transform leftBorder;

        [SerializeField]
        private Transform rightBorder;

        [SerializeField]
        private Transform downBorder;

        [SerializeField]
        private Transform topBorder;

        public bool InBounds(Vector3 position)
        {
            var positionX = position.x;
            var positionY = position.y;
            return positionX > this.leftBorder.position.x
                   && positionX < this.rightBorder.position.x
                   && positionY > this.downBorder.position.y
                   && positionY < this.topBorder.position.y;
        }

        public Vector2 ClampInBounds(Vector2 position) => new()
        {
            x = Mathf.Clamp(position.x, this.leftBorder.position.x, this.rightBorder.position.x),
            y = Mathf.Clamp(position.y, this.downBorder.position.y, this.topBorder.position.y)
        };
    }
}