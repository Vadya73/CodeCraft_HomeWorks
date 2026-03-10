using DG.Tweening;
using UnityEngine;

namespace Modules.Utils
{
    public sealed class CameraShaker : MonoBehaviour
    {
        [SerializeField] private Transform _transform;
        [SerializeField] private float _duration;
        [SerializeField] private Vector3 _strength;
        [SerializeField] private int _vibrato;
        [SerializeField] private ShakeRandomnessMode _randomnessMode;
        
        private Tweener _tween;

        public void Shake()
        {
            if (_tween.IsActive()) 
                _tween.Complete();

            _tween = _transform.DOShakePosition(_duration, _strength, _vibrato, randomnessMode: _randomnessMode)
                .SetLink(_transform.gameObject);
        }
    }
}