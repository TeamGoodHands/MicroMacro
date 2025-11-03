using System;
using UnityEngine;

namespace Module.Player.Component
{
    public class PlayerStatus : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHealth;
        [SerializeField] private int currentHealth;

        public event Action<int> OnDamage;
        public event Action OnDeath;
        
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        public void Damage(int damage)
        {
            int prevHealth = currentHealth;
            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            // HPが変わってなければダメージを受けていないことにする
            if (currentHealth == prevHealth)
                return;
            
            OnDamage?.Invoke(currentHealth);

            if (currentHealth == 0)
            {
                OnDeath?.Invoke();
            }
            Debug.Log("ダメージを受けました");
        }

        public void Reset()
        {
            currentHealth = maxHealth;
        }
    }
}