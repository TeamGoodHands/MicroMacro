using System;
using UnityEngine;

namespace Module.Enemy
{
    public class EnemyStatus : MonoBehaviour
    {
        [SerializeField] private int maxHealth;
        [SerializeField] private int currentHealth;

        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;

        public event Action<int> OnDamage;
        public event Action OnDeath;

        private void Start()
        {
            currentHealth = maxHealth;
        }

        public void Damage(int damage)
        {
            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            OnDamage?.Invoke(currentHealth);

            if (currentHealth == 0)
            {
                OnDeath?.Invoke();
            }
        }
    }
}