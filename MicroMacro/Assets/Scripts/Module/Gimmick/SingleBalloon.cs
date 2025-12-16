using System;
using Constants;
using Cysharp.Threading.Tasks;
using Module.Player.Component;
using Module.Scaling;
using Unity.Cinemachine;
using UnityEngine;

namespace Module.Gimmick
{
    public class SingleBalloon : MonoBehaviour
    {
        [SerializeField, Header("スケール量に対するY軸の力")] private float forceMultiplier;

        [SerializeField, Header("スケールを変更した瞬間に発生するY軸の力")] private float forceMultiplierOnScale;

        [SerializeField, Header("X軸方向の移動スピード")] private float moveSpeed;

        [SerializeField, Header("最大スピード")] private Vector2 maxSpeed;

        [SerializeField] private VehicleRider vehicleRider;
        [SerializeField] private Scaler scaler;
        [SerializeField] private Rigidbody rigidBody;
        [SerializeField] private CinemachineCamera balloonCamera;

        private Vector3 initialPosition;

        private void Start()
        {
            rigidBody.isKinematic = true;
            initialPosition = rigidBody.position;

            vehicleRider.OnRide += OnRide;
            vehicleRider.OnDismount += OnDismount;
            scaler.OnScaleStarted += OnScale;
        }

        private void OnScale(ScaleEventArgs args)
        {
            Vector2 verticalForce = Vector2.up * (forceMultiplierOnScale * (args.CurrentStep - args.PreviousStep));
            rigidBody.AddForce(verticalForce, ForceMode.VelocityChange);
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


        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.GetComponent<Thorn>() != null)
            {
                KillBalloon().Forget();
            }
        }

        private async UniTaskVoid KillBalloon()
        {
            rigidBody.linearVelocity = Vector3.zero;
            vehicleRider.Dismount();
            vehicleRider.enabled = false;
            KillPlayer();

            await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: destroyCancellationToken);

            ResetBalloon();
            vehicleRider.enabled = true;
        }

        private void KillPlayer()
        {
            GameObject playerObject = GameObject.FindWithTag(Tag.Player);
            if (playerObject == null)
                return;

            if (playerObject.TryGetComponent<PlayerStatus>(out var playerStatus))
            {
                // 確実に死亡させる（RespawnSystemはPlayerStatus.OnDeathを購読している）
                int killDamage = Mathf.Max(1, playerStatus.CurrentHealth);
                playerStatus.Damage(killDamage);
            }
        }

        private void ResetBalloon()
        {
            // 風船操作停止（カメラも戻す）
            rigidBody.isKinematic = true;
            balloonCamera.Priority = 0;

            // 位置・回転・スケールを初期値へ
            rigidBody.position = initialPosition;

            // 速度リセット
            rigidBody.linearVelocity = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;

            // スケール段階も初期化（0段階へ）
            if (scaler != null)
            {
                scaler.ResetScale();
            }
        }
    }
}