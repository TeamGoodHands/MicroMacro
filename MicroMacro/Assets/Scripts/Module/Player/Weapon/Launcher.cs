using CoreModule.Input;
using CoreModule.ObjectPool;
using Module.Player.Component;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Module.Player.Weapon
{
    public class Launcher : AbstractWeapon
    {
        [SerializeField] private float shootPower;
        [SerializeField] private float shootRadius;
        [SerializeField] private float shootInterval;
        [SerializeField] private float maxAdditionalSpeed;
        [SerializeField, Range(0f, 90f)] float launcherAngle = 45f;
        [SerializeField] private int poolAmount;
        [SerializeField] private Transform poolParent;
        
        [SerializeField] private GameObject macroGrenadePrefab;
        [SerializeField] private GameObject microGrenadePrefab;
        [SerializeField] private GameObject muzzle;
        
        
        private PlayerCondition condition;
        private Rigidbody playerRigBody;
        private ObjectPool<GameObject> macroGrenadePool;
        private ObjectPool<GameObject> microGrenadePool;
        private InputEvent macroShootEvent;
        private InputEvent microShootEvent;

        private float lastShootTime;
        public override void Initialize(PlayerComponent component)
        {
            condition = component.Condition;
            playerRigBody = component.Rigidbody;

            // 弾のObjectPoolの初期化
            macroGrenadePool = new ObjectPool<GameObject>(() => OnBulletCreate(macroGrenadePrefab), null, poolAmount);
            microGrenadePool = new ObjectPool<GameObject>(() => OnBulletCreate(microGrenadePrefab), null, poolAmount);

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
            GameObject obj = Instantiate(prefab, poolParent, true);
            obj.SetActive(false);
            return obj;
        }
        
        private void OnMacroShoot(InputAction.CallbackContext _)
        {
            Shoot(macroGrenadePool);
        }

        private void OnMicroShoot(InputAction.CallbackContext _)
        {
            Shoot(microGrenadePool);
        }
        private void Update()
        {
            // 銃の向きはカクカクで回転させる
            if (condition.Direction.x != 0f)
            {
                transform.localRotation = Quaternion.Euler(0f, 0f, launcherAngle);
            }
            else if (condition.Direction.y != 0f)
            {
                transform.localRotation = Quaternion.Euler(0f, 0f, condition.Direction.y * 90f);
            }
        }

        private void Shoot(ObjectPool<GameObject> targetPool)
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

            bulletObj.SetActive(true);

            // 弾の初期化
            Bullet bullet = bulletObj.GetComponent<Bullet>();
            bullet.OnHit += () =>
            {
                bulletObj.SetActive(false);
                targetPool.Return(bulletObj);
            };

            // 砲口に移動
            bullet.transform.position = muzzle.transform.position;
           
            // WARNING: ちゃんと速度乗ってなさそう 
            // プレイヤーの速度を足して発射　
            Vector2 dirVelocity = GetDirectedVelocity(condition.Direction, playerRigBody.linearVelocity, maxAdditionalSpeed);

            // condition.Directionに角度足す感じで出来そうな気もする。
            Vector2 dir = (muzzle.transform.position - transform.position).normalized;
            bullet.AddForce(dir * shootPower + dirVelocity);
            lastShootTime = Time.time;
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