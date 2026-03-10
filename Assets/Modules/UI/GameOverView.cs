using UnityEngine;

namespace Modules.UI
{
    public sealed class GameOverView : MonoBehaviour
    {
        private void Awake()
        {
            this.gameObject.SetActive(false);
        }

        public void Show()
        {
            this.gameObject.SetActive(true);
        }
    }
}