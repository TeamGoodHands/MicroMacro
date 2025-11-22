using Module.Player.Component;
using Module.Scaling;
using Unity.Cinemachine;
using UnityEngine;

namespace Module.Gimmick
{
    public class SingleBalloon : MonoBehaviour
    {
        [SerializeField, Header("スケール量に対するY軸の力")] private float forceMultiplier;
        [SerializeField, Header("X軸方向の移動スピード")] private float moveSpeed;
        [SerializeField, Header("最大スピード")] private Vector2 maxSpeed;

        [SerializeField] private VehicleRider vehicleRider;
        [SerializeField] private Scaler scaler;
        [SerializeField] private Rigidbody rigidBody;
        [SerializeField] private CinemachineCamera balloonCamera;

        private PlayerCondition playerCondition;

        private void Start()
        {
            rigidBody.isKinematic = true;

            vehicleRider.OnRide += OnRide;
            vehicleRider.OnDismount += OnDismount;
        }

        private void OnRide()
        {
            rigidBody.isKinematic = false;
            balloonCamera.Priority = 1000;
        }

        private void OnDismount()
        {
            balloonCamera.Priority = 0;
        }

        private void FixedUpdate()
        {
            // プレイヤー乗っている間は風船を動かす
            if (vehicleRider.IsRiding)
            {
                MoveBalloon();
            }
        }

        private void MoveBalloon()
        {
            // 力を加える
            Vector2 verticalForce = Vector2.up * (forceMultiplier * scaler.CurrentStep);
            Vector2 horizontalForce = Vector2.right * moveSpeed;
            rigidBody.AddForce(verticalForce + horizontalForce);

            // 最大速度でクランプ
            Vector3 velocity = rigidBody.linearVelocity;
            velocity.x = Mathf.Clamp(velocity.x, -maxSpeed.x, maxSpeed.x);
            velocity.y = Mathf.Clamp(velocity.y, -maxSpeed.y, maxSpeed.y);
            rigidBody.linearVelocity = velocity;
        }
    }
}