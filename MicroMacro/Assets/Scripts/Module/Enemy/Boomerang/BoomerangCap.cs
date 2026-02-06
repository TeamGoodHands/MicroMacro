using System;
using Constants;
using Module.Player.Component;
using Module.Scaling;
using Module.UI;
using UnityEngine;

namespace Module.Enemy.Boomerang
{
    public class BoomerangCap : MonoBehaviour
    {
        [SerializeField] private int damage;

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag(Tag.Handle.Player) && other.transform.root.TryGetComponent<HealthStatus>(out var playerStatus))
            {
                playerStatus.Damage(damage);
            }
        }
    }
}