using UnityEngine;
using UnityEngine.UI;

namespace Modules.UI
{
    public class HealthView : MonoBehaviour
    {
        [SerializeField]
        private Image[] blocks; 

        public void SetHealth(int current, int max)
        {
            if (blocks == null || blocks.Length == 0) return;

            max = Mathf.Max(1, max);

            int filled = Mathf.RoundToInt((current / (float) max) * blocks.Length);
            filled = Mathf.Clamp(filled, 0, blocks.Length);

            for (int i = 0; i < blocks.Length; i++)
            {
                bool isFull = i < filled;
                blocks[i].gameObject.SetActive(isFull);
            }
        }
    }
}