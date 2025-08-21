using System;
using Constants;
using Module.Player.Component;
using UnityEngine;

namespace Module.Gimmick
{
    public class Thorn : MonoBehaviour
    {
        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag(Tag.Handle.Player))
            {
                other.gameObject.GetComponent<PlayerStatus>().Damage(1);
            }
        }
    }
}
