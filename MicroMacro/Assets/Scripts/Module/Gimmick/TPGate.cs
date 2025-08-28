using System;
using Constants;
using UnityEngine;

namespace Module.Gimmick
{
    public class TPGate : MonoBehaviour
    {
        [Header("移動先")] 
        [SerializeField] private Transform destination;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(Tag.Handle.Player))
            {
                other.transform.position = destination.position;
            }
        }
    }
}