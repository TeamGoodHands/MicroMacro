using System;
using Module.Enemy;
using UnityEngine;

namespace Module.UI
{
    public class EnemyHpPresenter : MonoBehaviour
    {
        [SerializeField] private HealthStatus enemyStatus;
        [SerializeField] private HpBar hpBar;

        private void Start()
        {
            enemyStatus.OnDamage += OnDamage;
        }

        private void OnDestroy()
        {
            enemyStatus.OnDamage -= OnDamage;
        }

        private void OnDamage(int damage)
        {
            hpBar.ApplyHp(damage, enemyStatus.MaxHealth);
        }
    }
}