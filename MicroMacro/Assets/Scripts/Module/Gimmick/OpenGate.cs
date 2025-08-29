using System;
using Constants;
using Module.Management;
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
                SoundManager.instance.Play("スイッチ");
            }
        }
    }
}