using System;
using Constants;
using Module.Player.Component;
using UnityEngine;

namespace Module.Gimmick
{
    public class Thorn : MonoBehaviour
    {
        [SerializeField] private int damage = 1;

        public event Action OnThornDamaged;

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag(Tag.Handle.Player) && other.gameObject.TryGetComponent<PlayerStatus>(out var playerStatus))
            {
                int hpBefore = playerStatus.CurrentHealth;
                playerStatus.Damage(damage);

                // 実際にHPが減った場合のみイベント発火
                if (playerStatus.CurrentHealth < hpBefore)
                {
                    OnThornDamaged?.Invoke();
                }
            }
        }
    }
}