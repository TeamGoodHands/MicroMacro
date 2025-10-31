using System;
using Constants;
using UnityEngine;

namespace Module.Application.Dialogue
{
    public class SectionGate : MonoBehaviour
    {
        public event Action OnPlayerExit;
        
        private void Awake()
        {
            GetComponent<Renderer>().enabled = false;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag(Tag.Handle.Player))
            {
                OnPlayerExit?.Invoke();
            }
        }
    }
}