using System;
using UnityEngine;

namespace Module.Gimmick
{
    public class BalloonDismounter : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out SingleBalloon singleBalloon))
            {
                singleBalloon.BeFree();
                
               Rigidbody rigidBody = singleBalloon.GetComponent<Rigidbody>();
               rigidBody.linearVelocity = new Vector3(0f, 10f, 0f);
            }
        }
    }
}