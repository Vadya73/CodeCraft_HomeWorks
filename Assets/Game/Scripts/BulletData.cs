using System;
using UnityEngine;

namespace Game
{
    // +
    public sealed class BulletData : MonoBehaviour
    {
        public event Action<BulletData, Collider2D> OnTriggerEntered;

        public TeamType team = TeamType.None;
        public Vector2 direction;

        public int damage;
        public float speed;
        public GameObject blueVFX;
        public GameObject redVFX;

        private void OnTriggerEnter2D(Collider2D other) => this.OnTriggerEntered?.Invoke(this, other);
    }
}