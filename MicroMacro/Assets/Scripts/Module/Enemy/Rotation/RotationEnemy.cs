using System;
using Constants;
using Module.Player.Component;
using Module.UI;
using UnityEngine;

namespace Module.Enemy.Rotation
{
    public class RotationEnemy : MonoBehaviour
    {
        [SerializeField] private float rotationSpeed;
        [SerializeField] private float detectDistance;
        [SerializeField] private float gravity;
        [SerializeField] private Rigidbody rigidBody;

        private Transform playerTransform;
        private bool isDetected;

        private void Start()
        {
            playerTransform = GameObject.FindWithTag(Tag.Player).transform;
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag(Tag.Handle.Player)
                && other.transform.root.TryGetComponent(out HealthStatus playerStatus))
            {
                playerStatus.Damage(1);
            }
        }

        private void FixedUpdate()
        {
            Vector3 diff = playerTransform.position - transform.position;

            // 探知範囲外なら回転しない
            if (diff.sqrMagnitude > detectDistance * detectDistance)
            {
                if (isDetected)
                {
                    rigidBody.angularVelocity = Vector3.zero;
                }
                
                return;
            }
            
            isDetected = true;

            // 重力を加える
            rigidBody.AddForce(Vector3.down * gravity, ForceMode.Acceleration);

            // プレイヤーの方向に回転力を加える
            float direction = Mathf.Sign(-diff.x);
            rigidBody.angularVelocity = Vector3.forward * (rotationSpeed * direction);
        }
    }
}