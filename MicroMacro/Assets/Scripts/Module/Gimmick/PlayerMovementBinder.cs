using System;
using Constants;
using UnityEngine;

namespace Module.Gimmick
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovementBinder : MonoBehaviour
    {
        [SerializeField] private Rigidbody rigidBody;
        private Rigidbody playerRigidBody;
        private Vector3 prevPosition;
        private Vector3 moveDelta;

        private void FixedUpdate()
        {
            if (playerRigidBody != null)
            {
                // プレイヤーのRigidbodyに移動量を適用
                playerRigidBody.position += moveDelta;
            }

            moveDelta = rigidBody.position - prevPosition;
            prevPosition = rigidBody.position;
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.transform.root.CompareTag(Tag.Handle.Player) &&
                other.transform.root.TryGetComponent(out Rigidbody playerRigidbody))
            {
                playerRigidBody = playerRigidbody;
            }
        }

        private void OnCollisionExit(Collision other)
        {
            if (other.transform.root.CompareTag(Tag.Handle.Player))
            {
                playerRigidBody = null;
            }
        }
    }
}