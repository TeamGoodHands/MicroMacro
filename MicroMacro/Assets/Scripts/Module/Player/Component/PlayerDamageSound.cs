using System;
using Module.Management;
using Module.UI;
using UnityEngine;

namespace Module.Player.Component
{
    public class PlayerDamageSound : MonoBehaviour
    {
        [SerializeField] private HealthStatus healthStatus;

        private void Start()
        {
            healthStatus.OnDamage += _ => SoundManager.instance.Play("プレイヤーダメージ音");
        }
    }
}