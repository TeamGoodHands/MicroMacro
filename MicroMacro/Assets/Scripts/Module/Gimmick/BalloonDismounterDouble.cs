using System;
using UnityEngine;

namespace Module.Gimmick
{
    public class BalloonDismounterDouble : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out DoubleBalloon balloon))
            {
                balloon.BeFree();
                
               Rigidbody rigidBody = balloon.GetComponent<Rigidbody>();
               rigidBody.linearVelocity = new Vector3(0f, 10f, 0f);
            }
        }
    }
}