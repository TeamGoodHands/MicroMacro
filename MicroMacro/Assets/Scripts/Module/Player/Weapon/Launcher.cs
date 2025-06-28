using System;
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
        
        [SerializeField] private GameObject macroGrenadePrefab;
        [SerializeField] private GameObject microGrenadePrefab;
        [SerializeField] private GameObject muzzle;
        
        [SerializeField] private BallSimulator ballSimulator;
        
        private PlayerCondition condition;
        private Rigidbody playerRigBody;
        private ObjectPool<GameObject> macroGrenadePool;
        private ObjectPool<GameObject> microGrenadePool;
        private InputEvent macroShootEvent;
        private InputEvent microShootEvent;

        private Vector3 defaultPosition;
        private float lastShootTime;
        
        private Vector2 velocity;
        public override void Initialize(PlayerComponent component)
        {
            condition = component.Condition;
            playerRigBody = component.Rigidbody;
            defaultPosition = transform.localPosition;

            // 弾のObjectPoolの初期化
            macroGrenadePool = new ObjectPool<GameObject>(() => OnGrenadeCreate(macroGrenadePrefab), null, poolAmount);
            microGrenadePool = new ObjectPool<GameObject>(() => OnGrenadeCreate(microGrenadePrefab), null, poolAmount);

            // 入力イベントを取得
            macroShootEvent = InputProvider.CreateEvent(ActionGuid.Player.MacroShoot);
            microShootEvent = InputProvider.CreateEvent(ActionGuid.Player.MicroShoot);
        }
    
        public override void OnEnabled()
        {
            macroShootEvent.Started += OnMacroShoot;
            microShootEvent.Started += OnMicroShoot;
            
            // 武器と弾道予測の有効化
            gameObject.SetActive(true);
            ballSimulator.IsSimulate = true;
        }

        public override void OnDisabled()
        {
            macroShootEvent.Started -= OnMacroShoot;
            microShootEvent.Started -= OnMicroShoot;
            
            gameObject.SetActive(false);
            ballSimulator.IsSimulate = false;
        }

        private GameObject OnGrenadeCreate(GameObject prefab)
        {
            GameObject obj = Instantiate(prefab, ObjectPool.Root, true);
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

            SwitchLauncherPosition(condition.Direction);
            
            CalcVelocity();
            
            ballSimulator.Simulate(velocity);
        }
        
        /// <summary>
        /// 弾道予測のため速度計算は毎フレーム行う
        /// </summary>
        private void CalcVelocity()
        {
            // プレイヤーの速度を足して弾に加える力を計算　
            Vector2 playerVelocity = ClampVelocity(playerRigBody.linearVelocity, maxAdditionalSpeed);
            Vector2 dir = muzzle.transform.right;
            velocity = dir * shootPower + playerVelocity;
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
            
            bullet.AddForce(velocity);
            lastShootTime = Time.time;
        }

        /// <summary>
        /// プレイヤーの速度を足す際、極端に強い力が足されない用制限したVelocityを返す関数
        /// </summary>
        private Vector2 ClampVelocity(Vector2 velocity, float maxSpeed)
        {
            float x = 0f, y = 0f;
            
            if (velocity.x >= 0)
                x = Mathf.Clamp(velocity.x, 0f, maxSpeed);
            else if (velocity.x < 0)
                x = Mathf.Clamp(velocity.x, -maxSpeed, 0f);
            
            if (velocity.y >= 0)
                y = Mathf.Clamp(velocity.y, 0f, maxSpeed);
            else if (velocity.y < 0)
                y = Mathf.Clamp(velocity.y, -maxSpeed, 0f);
            
            return new Vector2(x, y);
        }
        
        /// <summary>
        /// 銃口の向きに応じてLauncherの位置を切り替える
        /// TODO: アニメーション実装時要修正
        /// </summary>
        private void SwitchLauncherPosition(Vector2 direction)
        {
           if (direction == Vector2.up)
           {
               transform.localPosition = new Vector3(0f, 1f, 0f);
               ballSimulator.IsSimulate = false;    // 上下の時は弾道予測off
           }
           else if (direction == Vector2.down)
           {
               transform.localPosition = new Vector3(0f, -1.5f, 0f);
               ballSimulator.IsSimulate = false;
           }
           else if (direction == Vector2.right || direction == Vector2.left)
           {
               if (transform.localPosition != defaultPosition)
                   transform.localPosition = defaultPosition;
               
               ballSimulator.IsSimulate = true;
           }
           else
           {
               Debug.LogWarning($"無効なdirectionが渡されました: {direction}");
           }
        }
    }
}