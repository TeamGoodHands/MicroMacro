using System;
using Constants;
using Unity.VisualScripting;
using UnityEngine;

namespace Module.Scaling
{
    public class ObjectKiller : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(Tag.DeathArea))
            {
                Destroy(gameObject);
            }
        }
    }
}