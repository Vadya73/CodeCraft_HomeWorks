using System;
using UnityEngine;

namespace Game.Scripts.Combat.Ships
{
    public sealed class ShipHealth
    {
        private readonly int _maxHealth;
        private int _health;

        public ShipHealth(int maxHealth)
        {
            if (maxHealth < 1)
                throw new ArgumentOutOfRangeException(nameof(maxHealth), maxHealth, "Maximum health must be at least 1");

            _maxHealth = maxHealth;
            _health = maxHealth;
        }

        public event Action<int> HealthChanged;
        public event Action Damaged;
        public event Action Died;

        public int Current => _health;
        public int Maximum => _maxHealth;
        public bool IsAlive => _health > 0;

        public void Reset()
        {
            _health = _maxHealth;
            HealthChanged?.Invoke(_health);
        }

        public void TakeDamage(int damage)
        {
            if (damage <= 0 || !IsAlive)
                return;

            _health = Mathf.Clamp(_health - damage, 0, _maxHealth);
            HealthChanged?.Invoke(_health);
            Damaged?.Invoke();

            if (!IsAlive)
                Died?.Invoke();
        }
    }
}
