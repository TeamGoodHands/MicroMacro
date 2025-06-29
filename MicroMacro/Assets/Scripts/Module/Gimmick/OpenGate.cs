using System;
using Constants;
using UnityEngine;
using Module.Scaling;

namespace Module.Gimmick
{
    public class OpenGate : MonoBehaviour
    {
        [SerializeField] private GameObject gate;
        private void OnCollisionEnter(Collision other)
        {
            if (!gate.activeSelf)
                return;
            
            if (other.gameObject.CompareTag(Tag.Handle.Untagged))
            {
                gate.SetActive(false);
            }
        }
    }
}