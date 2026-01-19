using System;
using Constants;
using Cysharp.Threading.Tasks;
using Module.Management;
using Module.Player.Component;
using Module.Scaling;
using Module.UI;
using PropertyGenerator.Generated;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.VFX;

namespace Module.Gimmick
{
    public class SingleBalloon : MonoBehaviour
    {
        [SerializeField, Header("スケール量に対するY軸の力")]
        private float forceMultiplier;

        [SerializeField, Header("スケールを変更した瞬間に発生するY軸の力")]
        private float forceMultiplierOnScale;

        [SerializeField, Header("X軸方向の移動スピード")]
        private float moveSpeed;

        [SerializeField, Header("最大スピード")] private Vector2 maxSpeed;

        [SerializeField, Header("壁に当たったときに反発する力")]
        private float bouncePower;

        [SerializeField, Header("連続で衝突する間隔")] private float bounceInterval = 0.5f;

        [SerializeField] private VehicleRider vehicleRider;
        [SerializeField] private Scaler scaler;
        [SerializeField] private Collider scalerCollider;
        [SerializeField] private Collider bodyCollider;
        [SerializeField] private Collider bouncerCollider;
        [SerializeField] private Rigidbody rigidBody;
        [SerializeField] private CinemachineCamera balloonCamera;
        [SerializeField] private VisualEffect fluffSplash;
        [SerializeField] private GameObject[] fluffObjects;
        [SerializeField] private BalloonControllerWrapper balloonControllerWrapper;

        public int HealthPoint { get; private set; }
        public event Action OnReset;

        private Vector3 initialPosition;
        private float lastBounceTime;

        private void Start()
        {
            rigidBody.isKinematic = true;
            initialPosition = rigidBody.position;
            HealthPoint = fluffObjects.Length;

            vehicleRider.OnRide += OnRide;
            vehicleRider.OnDismount += OnDismount;
            scaler.OnScaleStarted += OnScale;
        }

        private void OnScale(ScaleEventArgs args)
        {
            if (HealthPoint <= 0)
                return;

            Vector2 verticalForce = Vector2.up * (forceMultiplierOnScale * (args.CurrentStep - args.PreviousStep));
            rigidBody.AddForce(verticalForce, ForceMode.VelocityChange);
        }

        private void OnRide()
        {
            rigidBody.isKinematic = false;
            balloonCamera.Priority = 1000;
            CinemachineCore.SoloCamera = null;

            SoundManager.instance.Play("風の音");
        }

        private void OnDismount()
        {
            balloonCamera.Priority = 0;
            SoundManager.instance.StopPlay("風の音");
        }

        private void FixedUpdate()
        {
            // プレイヤー乗っている間は風船を動かす
            if (vehicleRider.IsRiding && HealthPoint > 0)
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

        private bool CanDamage()
        {
            return vehicleRider.IsRiding &&
                   HealthPoint > 0 &&
                   Time.time - lastBounceTime >= bounceInterval;
        }

        private void OnCollisionEnter(Collision other)
        {
            if (CanDamage())
            {
                SoundManager.instance.Play("衝突音2");
                Damage(other);
                lastBounceTime = Time.time;
            }

            // if (other.gameObject.GetComponent<Thorn>() != null)
            // {
            //     KillBalloon().Forget();
            // }
        }

        private void Damage(Collision collision)
        {
            Vector3 normal = collision.GetContact(0).normal;
            Vector3 bounceVelocity = normal * bouncePower;

            rigidBody.linearVelocity = bounceVelocity;
            fluffSplash.Play();

            HealthPoint--;
            fluffObjects[HealthPoint].SetActive(false);

            if (HealthPoint == 0)
            {
                rigidBody.useGravity = true;
                scalerCollider.enabled = false;
            }
        }

        public async UniTaskVoid KillBalloon()
        {
            rigidBody.linearVelocity = Vector3.zero;
            vehicleRider.Dismount();
            vehicleRider.enabled = false;

            KillPlayer();

            await UniTask.Delay(TimeSpan.FromSeconds(2f), cancellationToken: destroyCancellationToken);

            ResetBalloon();
            vehicleRider.enabled = true;
        }

        public void BeFree()
        {
            scalerCollider.enabled = false;
            bodyCollider.enabled = false;
            bouncerCollider.enabled = false;

            vehicleRider.Dismount();
            vehicleRider.enabled = false;
        }

        private void KillPlayer()
        {
            GameObject playerObject = GameObject.FindWithTag(Tag.Player);
            if (playerObject == null)
                return;

            if (playerObject.TryGetComponent<HealthStatus>(out var playerStatus))
            {
                // 確実に死亡させる（RespawnSystemはPlayerStatus.OnDeathを購読している）
                int killDamage = Mathf.Max(1, playerStatus.CurrentHealth);
                playerStatus.Damage(killDamage);
            }
        }

        private void ResetBalloon()
        {
            // 風船操作停止（カメラも戻す）
            rigidBody.useGravity = false;
            balloonCamera.Priority = 0;

            // 位置・回転・スケールを初期値へ
            rigidBody.position = initialPosition;
            transform.position = initialPosition;

            // 速度リセット
            rigidBody.linearVelocity = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;
            rigidBody.isKinematic = true;

            // 綿毛を戻す
            HealthPoint = fluffObjects.Length;
            scalerCollider.enabled = true;
            foreach (GameObject fluffObject in fluffObjects)
            {
                fluffObject.SetActive(true);
            }

            // スケール段階も初期化（0段階へ）
            if (scaler != null)
            {
                scaler.ResetScale();
            }

            OnReset?.Invoke();
        }
    }
}