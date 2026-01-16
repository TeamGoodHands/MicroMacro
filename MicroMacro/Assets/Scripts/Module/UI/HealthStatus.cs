using System;
using Module.Player.Component;
using UnityEngine;

namespace Module.UI
{
    public class HealthStatus : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHealth;
        [SerializeField] private int currentHealth;
        [SerializeField] private int previousHealth;

        public event Action<int> OnDamage;
        public event Action OnDeath;
        public event Action OnReset;

        public int PreviousHealth => previousHealth;
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;

        private void Awake()
        {
            currentHealth = maxHealth;
            previousHealth = currentHealth;
        }

        public void SetHealth(int health)
        {
            previousHealth = currentHealth;
            currentHealth = health;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            SendEvent();
        }

        public void Damage(int damage)
        {
            previousHealth = currentHealth;
            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            // HPが変わってなければダメージを受けていないことにする
            if (currentHealth == previousHealth)
                return;

            SendEvent();
        }

        public void SendEvent()
        {
            OnDamage?.Invoke(currentHealth);

            if (currentHealth == 0)
            {
                OnDeath?.Invoke();
            }
        }

        public void Reset()
        {
            currentHealth = maxHealth;
            previousHealth = currentHealth;
            OnReset?.Invoke();
        }
    }
}