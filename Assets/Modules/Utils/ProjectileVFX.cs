using UnityEngine;

namespace Modules.Utils
{
    public sealed class ProjectileVFX : MonoBehaviour
    {
        [SerializeField]
        private TrailRenderer _trail;

        private void OnEnable()
        {
            _trail.Clear();
        }

        private void OnDisable()
        {
            _trail.Clear();
        }
    }
}