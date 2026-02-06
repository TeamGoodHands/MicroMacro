using System;
using Module.Management;
using UnityEngine;

namespace Module.Gimmick
{
    public class ObjectLocker : MonoBehaviour
    {
        [SerializeField] private GameObject target;
        [SerializeField] private GameObject waterPivot;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == target)
            {
                other.attachedRigidbody.isKinematic = true;
                other.gameObject.transform.position = transform.position;

                SoundManager.instance.Play("糸切り");
                
                if (waterPivot != null)
                {
                    waterPivot.SetActive(false);
                }
            }
        }
    }
}