using TMPro;
using UnityEngine;

namespace Modules.UI
{
    public sealed class ScoreView : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _text;

        public void SetValue(int score)
        {
            _text.text = $"SCORE: {score}";
        }
    }
}