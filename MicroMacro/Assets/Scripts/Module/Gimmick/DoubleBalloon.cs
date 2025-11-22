using Module.Scaling;
using Unity.Cinemachine;
using UnityEngine;

namespace Module.Gimmick
{
    public class DoubleBalloon : MonoBehaviour
    {
        [SerializeField, Header("スケール量に対する力の倍率")] private Vector2 forceMultiplier;
        [SerializeField, Header("Y軸方向の基本移動スピード")] private float verticalSpeed;
        [SerializeField, Header("最大スピード")] private Vector2 maxSpeed;

        [SerializeField] private Scaler balloonScalerR;
        [SerializeField] private Transform balloonPivotR;
        [SerializeField] private Scaler balloonScalerL;
        [SerializeField] private Transform balloonPivotL;

        [SerializeField] private VehicleRider vehicleRider;
        [SerializeField] private Rigidbody rigidBody;
        [SerializeField] private CinemachineCamera balloonCamera;

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
            float stepL = balloonScalerL.CurrentStep;
            float stepR = balloonScalerR.CurrentStep;

            // 基礎スピード × (max(0, 左風船の大きさ + 右風船の大きさ) × Y軸の速度倍率)
            Vector2 verticalForce = Vector2.up * (verticalSpeed * (Mathf.Max(0, stepL + stepR) * forceMultiplier.y));

            Vector2 horizontalForceL = -Vector2.right * (forceMultiplier.x * balloonScalerL.CurrentStep);
            Vector2 horizontalForceR = Vector2.right * (forceMultiplier.x * balloonScalerR.CurrentStep);
            Vector2 horizontalForce = horizontalForceL + horizontalForceR;

            rigidBody.AddForce(verticalForce + horizontalForce);

            // 最大速度でクランプ
            Vector3 velocity = rigidBody.linearVelocity;
            velocity.x = Mathf.Clamp(velocity.x, -maxSpeed.x, maxSpeed.x);
            velocity.y = Mathf.Clamp(velocity.y, -maxSpeed.y, maxSpeed.y);
            rigidBody.linearVelocity = velocity;
        }
    }
}