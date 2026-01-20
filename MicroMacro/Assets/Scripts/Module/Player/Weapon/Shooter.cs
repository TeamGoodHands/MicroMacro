using System;
using CoreModule.Input;
using CoreModule.ObjectPool;
using Module.Management;
using Module.Player.Component;
using Module.UI;
using PropertyGenerator.Generated;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Module.Player.Weapon
{
    public class Shooter : AbstractWeapon
    {
        [SerializeField] private float shootPower;
        [SerializeField] private float shootRadius;
        [SerializeField] private float shootInterval;
        [SerializeField] private float minSpeed;
        [SerializeField] private float maxAdditionalSpeed;
        [SerializeField] private int poolAmount;

        [SerializeField] private GameObject macroBulletPrefab;
        [SerializeField] private GameObject microBulletPrefab;

        private PlayerCondition condition;
        private HealthStatus healthStatus;
        private PlayerControllerWrapper animatorWrapper;
        private Rigidbody playerRigBody;
        private ObjectPool<GameObject> macroBulletPool;
        private ObjectPool<GameObject> microBulletPool;
        private InputEvent macroShootEvent;
        private InputEvent microShootEvent;

        private float lastShootTime;

        public override void Initialize(PlayerComponent component)
        {
            condition = component.Condition;
            healthStatus = component.HealthStatus;
            playerRigBody = component.Rigidbody;
            animatorWrapper = component.AnimatorWrapper;

            // 弾のObjectPoolの初期化
            macroBulletPool = new ObjectPool<GameObject>(() => OnBulletCreate(macroBulletPrefab), null, null, poolAmount);
            microBulletPool = new ObjectPool<GameObject>(() => OnBulletCreate(microBulletPrefab), null, null, poolAmount);

            // 入力イベントを取得
            macroShootEvent = InputProvider.CreateEvent(ActionGuid.Player.MacroShoot);
            microShootEvent = InputProvider.CreateEvent(ActionGuid.Player.MicroShoot);
        }

        public override void OnEnabled()
        {
            macroShootEvent.Started += OnMacroShoot;
            microShootEvent.Started += OnMicroShoot;
            gameObject.SetActive(true);
        }

        public override void OnDisabled()
        {
            macroShootEvent.Started -= OnMacroShoot;
            microShootEvent.Started -= OnMicroShoot;
            gameObject.SetActive(false);
        }

        private GameObject OnBulletCreate(GameObject prefab)
        {
            GameObject obj = Instantiate(prefab, ObjectPool.Root, true);
            obj.SetActive(false);
            return obj;
        }

        private void OnMacroShoot(InputAction.CallbackContext _)
        {
            if (condition.IsPlayerLocked || healthStatus.CurrentHealth <= 0f)
                return;

            Shoot(macroBulletPool, true);
        }

        private void OnMicroShoot(InputAction.CallbackContext _)
        {
            if (condition.IsPlayerLocked || healthStatus.CurrentHealth <= 0f)
                return;

            Shoot(microBulletPool, false);
        }

        private void Update()
        {
            // 銃の向きはカクカクで回転させる
            if (condition.Direction.x != 0f)
            {
                transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            }
            else if (condition.Direction.y != 0f)
            {
                transform.localRotation = Quaternion.Euler(0f, 0f, condition.Direction.y * 90f);
            }
        }

        private void Shoot(ObjectPool<GameObject> targetPool, bool isMacro)
        {
            // 発射間隔が空いていない場合は終了
            if (Time.time - lastShootTime < shootInterval)
                return;

            // プールが空の場合は終了
            if (!targetPool.TryGet(out GameObject bulletObj))
            {
                Debug.LogError("ObjectPoolが空になりました");
                return;
            }

            if (isMacro)
            {
                SoundManager.instance.Play("マクロ発射");
            }
            else
            {
                SoundManager.instance.Play("ミクロ発射");
            }

            bulletObj.SetActive(true);
            bulletObj.transform.forward = condition.Direction;

            // 弾の初期化
            Bullet bullet = bulletObj.GetComponent<Bullet>();
            bullet.OnHit += () =>
            {
                bulletObj.SetActive(false);
                targetPool.Return(bulletObj);
            };

            // とりあえずプレイヤーから離れた位置から発射
            bullet.transform.position = transform.position + (Vector3)condition.Direction * shootRadius;

            // プレイヤーの速度を足して発射
            Vector2 dirVelocity = GetDirectedVelocity(condition.Direction, playerRigBody.linearVelocity, maxAdditionalSpeed);
            Vector2 bulletForce = condition.Direction * shootPower + dirVelocity + condition.ExternalWeaponForce;

            if (Vector2.Dot(bulletForce, condition.Direction) < 0f || bulletForce.magnitude < minSpeed)
            {
                bulletForce = condition.Direction * minSpeed;
            }

            bullet.Shoot(bulletForce, condition.Direction.y != 0f);
            lastShootTime = Time.time;

            PlayShotAnimation();
        }

        private void PlayShotAnimation()
        {
            if (condition.Direction.y == 0f)
            {
                animatorWrapper.SetShotSideTrigger();
            }
            else if (condition.Direction.y > 0f)
            {
                animatorWrapper.SetShotUpTrigger();
            }
            else if (condition.Direction.y < 0f)
            {
                animatorWrapper.SetShotDownTrigger();
            }
        }

        /// <summary>
        /// directionと同じ方向の移動ベクトルを返します
        /// </summary>
        private Vector2 GetDirectedVelocity(Vector2 direction, Vector2 velocity, float maxSpeed)
        {
            if (direction == Vector2.right)
                return new Vector2(Mathf.Clamp(velocity.x, 0f, maxSpeed), 0f);
            if (direction == Vector2.left)
                return new Vector2(Mathf.Clamp(velocity.x, -maxSpeed, 0f), 0f);
            if (direction == Vector2.up)
                return new Vector2(0f, Mathf.Clamp(velocity.y, 0f, maxSpeed));
            if (direction == Vector2.down)
                return new Vector2(0f, Mathf.Clamp(velocity.y, -maxSpeed, 0f));

            Debug.LogWarning($"無効なdirectionが渡されました: {direction}");
            return Vector2.zero;
        }
    }
}