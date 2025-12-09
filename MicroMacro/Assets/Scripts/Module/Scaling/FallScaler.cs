using System;
using UnityEngine;

public class FallScaler : MonoBehaviour
{
    [SerializeField] private float fallSpeed; 
    [SerializeField] private Rigidbody rigidBody;
    
    private void FixedUpdate()
    {
        rigidBody.linearVelocity = Vector3.down * fallSpeed;
    }
}
