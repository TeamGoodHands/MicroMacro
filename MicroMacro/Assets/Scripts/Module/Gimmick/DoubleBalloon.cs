using System;
using Constants;
using Cysharp.Threading.Tasks;
using Module.Management;
using Module.Scaling;
using Module.UI;
using PropertyGenerator.Generated;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.VFX;

namespace Module.Gimmick
{
    [Serializable]
    public struct FluffObject
    {
        public GameObject R;
        public GameObject L;
    }

    public class DoubleBalloon : MonoBehaviour
    {
        [SerializeField, Header("スケール量に対する力の倍率")]
        private Vector2 forceMultiplier;

        [SerializeField, Header("Y軸方向の基本移動スピード")]
        private float verticalSpeed;

        [SerializeField, Header("スケールを変更した瞬間に発生するX軸の力")]
        private float forceMultiplierOnScale;

        [SerializeField, Header("最大スピード")] private Vector2 maxSpeed;

        [SerializeField, Header("連続で衝突する間隔")] private float bounceInterval = 0.5f;

        [SerializeField, Header("壁に当たったときに反発する力")]
        private float bouncePower;

        [SerializeField] private Scaler balloonScalerR;
        [SerializeField] private Scaler balloonScalerL;

        [SerializeField] private VehicleRider vehicleRider;
        [SerializeField] private Collider scalerColliderR;
        [SerializeField] private Collider scalerColliderL;
        [SerializeField] private Collider bodyCollider;
        [SerializeField] private Collider bouncerCollider;
        [SerializeField] private Rigidbody rigidBody;
        [SerializeField] private CinemachineCamera balloonCamera;
        [SerializeField] private VisualEffect fluffSplashR;
        [SerializeField] private VisualEffect fluffSplashL;
        [SerializeField] private FluffObject[] fluffObjects;
        [SerializeField] private BalloonControllerWrapper balloonControllerWrapper;

        public int HealthPoint { get; private set; }
        public event Action OnReset;

        private Vector3 initialPosition;
        private float lastBounceTime;
        private bool isDead;

        private void Start()
        {
            rigidBody.isKinematic = true;
            initialPosition = rigidBody.position;
            HealthPoint = fluffObjects.Length;

            vehicleRider.OnRide += OnRide;
            vehicleRider.OnDismount += OnDismount;
            balloonScalerL.OnScaleStarted += OnScaleLeft;
            balloonScalerR.OnScaleStarted += OnScaleRight;
        }

        private void OnScaleLeft(ScaleEventArgs args)
        {
            if (HealthPoint <= 0)
                return;

            Vector2 verticalForce = -Vector2.right * (forceMultiplierOnScale * (args.CurrentStep - args.PreviousStep));
            rigidBody.AddForce(verticalForce, ForceMode.VelocityChange);
        }

        private void OnScaleRight(ScaleEventArgs args)
        {
            if (HealthPoint <= 0)
                return;

            Vector2 verticalForce = Vector2.right * (forceMultiplierOnScale * (args.CurrentStep - args.PreviousStep));
            rigidBody.AddForce(verticalForce, ForceMode.VelocityChange);
        }

        private void OnRide()
        {
            rigidBody.isKinematic = false;
            balloonCamera.Priority = 1000;

            // Cinemachineの他のカメラを無効化
            CinemachineCore.SoloCamera = null;

            SoundManager.instance.Play("風の音");
        }

        private void OnDismount()
        {
            if (!isDead)
            {
                balloonCamera.Priority = 0;
            }
        }

        private void FixedUpdate()
        {
            // プレイヤー乗っている間は風船を動かす
            if (vehicleRider.IsRiding && HealthPoint > 0)
            {
                MoveBalloon();
            }
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
            fluffSplashR.Play();
            fluffSplashL.Play();

            HealthPoint--;
            fluffObjects[HealthPoint].R.SetActive(false);
            fluffObjects[HealthPoint].L.SetActive(false);

            if (HealthPoint == 0)
            {
                rigidBody.useGravity = true;
                scalerColliderR.enabled = false;
                scalerColliderL.enabled = false;
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

        public async UniTaskVoid KillBalloon()
        {
            isDead = true;
            rigidBody.linearVelocity = Vector3.zero;
            vehicleRider.Dismount();
            vehicleRider.enabled = false;

            KillPlayer();

            await UniTask.Delay(TimeSpan.FromSeconds(2f), cancellationToken: destroyCancellationToken);

            ResetBalloon();
            vehicleRider.enabled = true;
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
            scalerColliderR.enabled = true;
            scalerColliderL.enabled = true;

            foreach (FluffObject fluffObject in fluffObjects)
            {
                fluffObject.R.SetActive(true);
                fluffObject.L.SetActive(true);
            }

            // スケール段階も初期化（0段階へ）
            balloonScalerL.ResetScale();
            balloonScalerR.ResetScale();

            isDead = false;
            OnReset?.Invoke();
        }

        public void BeFree()
        {
            scalerColliderR.enabled = false;
            scalerColliderL.enabled = false;
            bodyCollider.enabled = false;
            bouncerCollider.enabled = false;

            vehicleRider.Dismount();
            vehicleRider.enabled = false;
        }
    }
}