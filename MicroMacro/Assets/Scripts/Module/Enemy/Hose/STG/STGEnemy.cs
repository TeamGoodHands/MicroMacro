using System.Threading;
using CoreModule.ObjectPool;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Module.Management;
using UnityEngine;
using UnityEngine.VFX;
using System.Linq;

namespace Module.Enemy.Hose.STG
{
    /// <summary>
    /// STG用の敵。画面外から入り、回転しながら4方向に弾を撃ち、画面外へ抜ける。
    /// </summary>
    public class STGEnemy : MonoBehaviour
    {
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private Transform body;
        [SerializeField] private float bulletSpeed = 8f;
        [SerializeField] private float shootInterval = 0.15f;
        [SerializeField] private float rotateSpeed = 45f;
        [SerializeField] private int wayCount = 8;
        [SerializeField] private int bulletPoolAmount = 64;
        [SerializeField] private int hp = 5;
        [SerializeField] private VisualEffect deathVfx;
        [SerializeField] private float vfxDuration = 0.5f;

        private ObjectPool<GameObject> bulletPool;
        private int currentHp;
        private Transform bulletParent;

        /// <summary>
        /// 無効化時に呼ばれる（プールへ返却用）
        /// </summary>
        public event System.Action OnReturn;

        // 移動パラメータ
        private Vector3 moveVelocity;
        private float lifeTime;

        /// <summary>
        /// 初期化（Activate の前に呼ぶ）
        /// </summary>
        public void Initialize(Vector3 velocity, float lifeTime)
        {
            this.moveVelocity = velocity;
            this.lifeTime = lifeTime;
        }

        /// <summary>
        /// 敵を起動する
        /// </summary>
        public async UniTaskVoid Activate(Transform bulletParent, CancellationToken token)
        {
            this.bulletParent = bulletParent;
            currentHp = hp;

            // 弾プールの初期化（初回のみ）
            bulletPool ??= new ObjectPool<GameObject>(CreateBullet, null, null, bulletPoolAmount);

            // 状態リセット（Renderer/Collider復帰）
            if (deathVfx) 
            {
                deathVfx.Stop();
                deathVfx.gameObject.SetActive(true);
            }
            foreach (var r in GetComponentsInChildren<Renderer>(true)) r.enabled = true;
            foreach (var c in GetComponentsInChildren<Collider>(true)) c.enabled = true;

            float elapsed = 0f;
            float shootTimer = 0f;

            while (!token.IsCancellationRequested && currentHp > 0 && elapsed < lifeTime && gameObject.activeSelf)
            {
                float dt = Time.fixedDeltaTime;
                elapsed += dt;

                // ── 移動（等速直線） ──
                transform.position += moveVelocity * dt;

                // ── 回転 ──
                transform.Rotate(0f, rotateSpeed * dt, 0f);

                // ── 射撃 ──
                shootTimer += dt;
                if (shootTimer >= shootInterval)
                {
                    shootTimer = 0f;
                    ShootNWay();
                }

                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);
            }

            // パス終了 or HP0 → 無効化してプールへ返却
            // 死亡時VFX処理
            if (currentHp <= 0 && deathVfx != null)
            {
                 // Disable Colliders / Renderers (Excluding VFX)
                 var renderers = GetComponentsInChildren<Renderer>();
                 var colliders = GetComponentsInChildren<Collider>();
                 
                 foreach (var c in colliders) c.enabled = false;
                 foreach (var r in renderers)
                 {
                     if (r.gameObject != deathVfx.gameObject && !r.transform.IsChildOf(deathVfx.transform))
                     {
                         r.enabled = false;
                     }
                 }

                 deathVfx.enabled = true;
                 deathVfx.Reinit();
                 deathVfx.Play();
                 
                 await UniTask.Delay(System.TimeSpan.FromSeconds(vfxDuration), cancellationToken: token);
            }

            Deactivate();
        }

        /// <summary>
        /// ダメージを受ける
        /// </summary>
        public void TakeDamage(int damage)
        {
            SoundManager.instance.Play("打撃1");
            body.DOShakePosition(0.35f, 0.1f, 20, 90f, false, true);
            
            currentHp -= damage;
            if (currentHp <= 0)
            {
                currentHp = 0;
            }
        }

        public void ForceDeath()
        {
            if (currentHp > 0)
            {
                currentHp = 0;
            }
        }

        public void Deactivate()
        {
            if (!gameObject.activeSelf)
                return;

            gameObject.SetActive(false);
            OnReturn?.Invoke();
            OnReturn = null;
        }

        /// <summary>
        /// N方向に弾を発射する（wayCount等分）
        /// </summary>
        private void ShootNWay()
        {
            if (wayCount <= 0) return;

            float angleStep = 360f / wayCount;
            for (int i = 0; i < wayCount; i++)
            {
                float angle = angleStep * i;
                // 現在の回転に加算
                Quaternion rot = Quaternion.Euler(0f, angle, 0f);
                Vector3 dir = transform.rotation * rot * Vector3.forward;

                // スクロール進行方向（Z > 0）には撃たない
                if (dir.z <= 0f)
                {
                    ShootBullet(dir);
                }
            }
        }

        private void ShootBullet(Vector3 direction)
        {
            GameObject bulletObj = bulletPool.GetOrCreate();
            bulletObj.transform.position = transform.position;
            bulletObj.transform.rotation = Quaternion.LookRotation(direction);
            bulletObj.SetActive(true);

            if (bulletObj.TryGetComponent(out STGBullet bullet))
            {
                bullet.OnReturn += () =>
                {
                    if (bulletPool != null)
                        bulletPool.Return(bulletObj);
                };
                Vector3 velocity = (direction * bulletSpeed) + moveVelocity;
                // Debug.Log($"[STGEnemy] Shoot: Speed={bulletSpeed}, Dir={direction}, Velocity={velocity} (BaseVel={moveVelocity})");
                bullet.Shoot(velocity, gameObject);
            }
        }

        private GameObject CreateBullet()
        {
            GameObject obj = Instantiate(bulletPrefab, bulletParent);
            obj.SetActive(false);
            return obj;
        }
    }
}