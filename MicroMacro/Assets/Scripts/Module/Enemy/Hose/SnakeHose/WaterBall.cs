using System;
using Constants;
using Module.Player;
using UnityEngine;

public class WaterBall : MonoBehaviour
{
    [SerializeField] private Rigidbody rigidBody;
    [SerializeField] private float bounceForce = 55f;
    [SerializeField] private Vector2 acceleration;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(Tag.Player) && other.gameObject.TryGetComponent(out PlayerBehaviour behaviour))
        {
            behaviour.Component.PlayerMovement.AddExternalForce(-Vector3.right * bounceForce);
        }
    }

    private void FixedUpdate()
    {
        rigidBody.AddForce(acceleration * Time.fixedDeltaTime, ForceMode.Acceleration);
    }
}