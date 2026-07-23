using UnityEngine;

namespace Game.Scripts.Combat.Ships
{
    // +
    [CreateAssetMenu(fileName = "ShipConfig", menuName = "Game/New Ship Config", order = 0)]
    public sealed class ShipConfig : ScriptableObject
    {
        [Header("Core")]
        [field: SerializeField] public int Health { get; private set; } = 5;

        [field: SerializeField] public float MoveSpeed { get; private set; } = 5;

        [field: SerializeField] public float FireCooldown { get; private set; } = 0.25f;

        private void OnValidate()
        {
            Health = Mathf.Max(1, Health);
            MoveSpeed = Mathf.Max(0, MoveSpeed);
            FireCooldown = Mathf.Max(0, FireCooldown);
        }
    }
}
