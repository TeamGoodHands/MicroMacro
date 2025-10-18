using System;
using Constants;
using Module.Player.Component;
using UnityEngine;

namespace Module.Gimmick
{
    public class Thorn : MonoBehaviour
    {
        private const int maxDamage = 99999999;
        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag(Tag.Handle.Player))
            {
                other.gameObject.GetComponent<PlayerStatus>().Damage(maxDamage);
            }
        }
    }
}
