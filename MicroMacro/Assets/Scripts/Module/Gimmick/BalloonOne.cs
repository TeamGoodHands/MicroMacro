using System;
using Constants;
using CoreModule.Input;
using Module.Player;
using Module.Player.Component;
using Module.Scaling;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Module.Gimmick
{
    public class BalloonOne : MonoBehaviour
    {
        [SerializeField, Header("スケール量に対するY軸の力")] private float forceMultiplier;
        [SerializeField, Header("X軸方向の移動スピード")] private float moveSpeed;
        [SerializeField, Header("X軸方向の最大スピード")] private float maxSpeed;
        [SerializeField, Header("風船を降りるときのオフセット")] private float dismountOffset;

        [SerializeField] private Scaler scaler;
        [SerializeField] private Rigidbody rigidBody;
        [SerializeField] private Transform ridePivot;
        [SerializeField] private CinemachineCamera balloonCamera;

        private Transform playerTransform;
        private PlayerCondition playerCondition;
        private InputEvent jumpEvent;
        private bool isRiding;

        private void Start()
        {
            rigidBody.isKinematic = true;
            jumpEvent = InputProvider.CreateEvent(ActionGuid.Player.Jump);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(Tag.Handle.Player) && other.TryGetComponent(out PlayerCondition condition))
            {
                playerCondition = condition;
                OnRide();
            }
        }

        private void OnRide()
        {
            isRiding = true;
            playerCondition.IsRiding = true;
            playerTransform = playerCondition.transform;
            rigidBody.isKinematic = false;
            balloonCamera.Priority = 1000;

            jumpEvent.Started += OnDismount;
        }

        private void OnDismount(InputAction.CallbackContext _)
        {
            isRiding = false;
            playerCondition.IsRiding = false;
            balloonCamera.Priority = 0;

            playerTransform.position += Vector3.right * dismountOffset;

            jumpEvent.Started -= OnDismount;
        }

        private void FixedUpdate()
        {
            if (isRiding)
            {
                rigidBody.AddForce(Vector2.up * (forceMultiplier * scaler.CurrentStep));
                rigidBody.AddForce(Vector2.right * moveSpeed);

                Vector3 velocity = rigidBody.linearVelocity;
                velocity.x = Mathf.Clamp(velocity.x, -maxSpeed, maxSpeed);
                rigidBody.linearVelocity = velocity;
            }
        }

        private void Update()
        {
            if (isRiding)
            {
                playerTransform.position = ridePivot.position;
            }
        }
    }
}