using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Scripts.Combat.Ships
{
    // +
    [CreateAssetMenu(menuName = "Game/ShipControllerViewConfig", order = 0)]
    public sealed class ShipControllerViewConfig : ScriptableObject
    {
        [field: SerializeField] public Material MaterialPrefab { get; private set; }

        [Header("Damage")]
        [field: SerializeField] public AnimationCurve HitAnimationCurve { get; private set; }

        [field: SerializeField] public string HitPropertyName { get; private set; } = "_HitBlend";

        [field: SerializeField] public float HitDuration { get; private set; } = 0.2f;

        [Header("Move")]
        [field: SerializeField] public float MoveRotationAngle { get; private set; } = 30f;

        [field: SerializeField, FormerlySerializedAs("<MoveSpeed>k__BackingField")]
        public float TiltResponsiveness { get; private set; } = 5;

        [Header("Destroy")]
        [field: SerializeField] public ParticleSystem DestroyEffectPrefab { get; private set; }

        private void OnValidate()
        {
            HitDuration = Mathf.Max(0, HitDuration);
            TiltResponsiveness = Mathf.Max(0, TiltResponsiveness);
        }
    }
}
