using System;
using Constants;
using Module.Player.Component;
using UnityEngine;

public class DeathArea : MonoBehaviour
{
    private const int maxDamage = 99999999;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Tag.Player) && 
            other.transform.root.TryGetComponent(out PlayerStatus player))
        {
            player.Damage(maxDamage);
        };
    }
}
