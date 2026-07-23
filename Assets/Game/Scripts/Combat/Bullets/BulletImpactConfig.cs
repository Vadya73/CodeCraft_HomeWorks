using UnityEngine;

namespace Game.Scripts.Combat.Bullets
{
    // +
    [CreateAssetMenu(fileName = "BulletImpactConfig", menuName = "Game/New Bullet Impact Config")]
    public sealed class BulletImpactConfig : ScriptableObject
    {
        [field: SerializeField] public GameObject ExplosionVFX  { get; private set; }
    }
}
