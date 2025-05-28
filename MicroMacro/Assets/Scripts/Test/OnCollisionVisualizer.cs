using System;
using UnityEngine;

namespace Test
{
    public class OnCollisionVisualizer : MonoBehaviour
    {
        private void Start()
        {
            GetComponent<Rigidbody>().sleepThreshold = -1;
        }

        private void OnCollisionEnter(Collision other)
        {
            Debug.Log("OnCollisionEnter: " + other.gameObject.name);
        }

        private void OnCollisionStay(Collision other)
        {
            foreach (ContactPoint contact in other.contacts)
            {
                Debug.DrawLine(contact.point, contact.point + contact.normal, Color.red, 0.5f);
            }

            Debug.Log("OnCollisionStay: " + other.gameObject.name);
        }

        private void OnCollisionExit(Collision other)
        {
            Debug.Log("OnCollisionExit: " + other.gameObject.name);
        }
    }
}