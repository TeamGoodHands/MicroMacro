using System;
using UnityEngine;
using UnityEngine.VFX;
using Cysharp.Threading.Tasks;
using System.Linq;

namespace Module.Enemy.Hose.STG
{
    /// <summary>
    /// STG用の弾。Rigidbodyで直進し、一定時間または衝突で無効化される（プーリング対応）。
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class STGBullet : MonoBehaviour
    {
        [SerializeField] private float lifeTime = 5f;
        [SerializeField] private int damage = 1;
        [SerializeField] private int maxDurability = 1; // 耐久値（何回当たったら消えるか）
        [SerializeField] private bool playVfxOnDestroy = true; // 破壊時にVFXを再生するか
        [SerializeField] private VisualEffect deathVfx;
        [SerializeField] private float vfxDuration = 0.5f;

        private Rigidbody rb;
        private Collider[] cols;
        [SerializeField] private Renderer[] rens;
        private float spawnTime;
        private GameObject owner;
        private bool isReturning;
        private int currentDurability;

        /// <summary>
        /// 弾が無効化されるときに呼ばれる（プールへ返却用）
        /// </summary>
        public event Action OnReturn;

        public GameObject Owner => owner;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            cols = GetComponentsInChildren<Collider>();

            // VFXに関連するRendererを除外して取得（VFX自体を消さないため）
            var allRenderers = GetComponentsInChildren<Renderer>();
            if (deathVfx != null)
            {
                rens = allRenderers.Where(r => r.gameObject != deathVfx.gameObject && !r.transform.IsChildOf(deathVfx.transform)).ToArray();
            }
            else
            {
                rens = allRenderers;
            }

            rb.useGravity = false;
        }

        /// <summary>
        /// 弾を指定方向に発射する
        /// </summary>
        public void Shoot(Vector3 velocity, GameObject owner = null)
        {
            this.owner = owner;
            isReturning = false;
            currentDurability = maxDurability;

            // 状態リセット
            foreach (var c in cols) c.enabled = true;
            foreach (var r in rens) r.enabled = true;
            if (deathVfx) deathVfx.Stop();

            rb.isKinematic = false;
            rb.linearVelocity = velocity;
            spawnTime = Time.time;
        }

        private void Update()
        {
            // 寿命チェック
            if (Time.time - spawnTime >= lifeTime)
            {
                Deactivate(playVfx: false);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // 敵に当たった場合は即効で消える
            if (TryCauseDamage(other.gameObject))
            {
                currentDurability = 0;
                Deactivate(playVfx: true);
                return;
            }


            // 敵弾などの場合は耐久値を減らす
            currentDurability--;
            if (playVfxOnDestroy)
            {
                Debug.Log(currentDurability);
            }

            if (currentDurability <= 0)
            {
                Deactivate(playVfx: true);
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            if (TryCauseDamage(other.gameObject))
            {
                currentDurability = 0;
                Deactivate(playVfx: true);
                return;
            }

            currentDurability--;
            if (currentDurability <= 0)
            {
                Deactivate(playVfx: true);
            }
        }

        private bool TryCauseDamage(GameObject target)
        {
            // 敵に当たった場合
            if (target.TryGetComponent(out STGEnemy enemy))
            {
                enemy.TakeDamage(damage);
                return true;
            }

            return false;
        }

        private void Deactivate(bool playVfx = false)
        {
            if (isReturning || !gameObject.activeSelf)
                return;

            if (playVfx && playVfxOnDestroy && deathVfx != null)
            {
                isReturning = true;

                // 物理・見た目を無効化してVFXのみ再生
                rb.linearVelocity = Vector3.zero;
                rb.isKinematic = true;
                foreach (var c in cols) c.enabled = false;
                foreach (var r in rens)
                {
                    r.enabled = false;
                }

                deathVfx.Play();
                WaitAndReturn(vfxDuration).Forget();
                return;
            }

            ReturnToPool();
        }

        private async UniTaskVoid WaitAndReturn(float duration)
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(duration), cancellationToken: this.GetCancellationTokenOnDestroy());
            ReturnToPool();
        }

        private void ReturnToPool()
        {
            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
            }

            gameObject.SetActive(false);
            OnReturn?.Invoke();
            OnReturn = null;
        }
    }
}