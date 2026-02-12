using Module.Management;
using Module.UI;
using UnityEngine;

namespace Module.Enemy.Hose.STG
{
    /// <summary>
    /// STGのプレイヤー（自機）のダメージ処理を行うクラス。
    /// HealthStatusコンポーネントと連携して動作する。
    /// </summary>
    public class STGPlayer : MonoBehaviour
    {
        [SerializeField] private float invincibilityTime = 1.0f; // ダメージ後の無敵時間
        [SerializeField] private Renderer bodyRenderer; // 点滅用
        [SerializeField] private HealthStatus healthStatus;

        public HealthStatus HealthStatus => healthStatus;
        
        private float lastDamageTime;
        private bool isInvincible;

        // 外部に通知するイベント（HealthStatusのイベントを中継しても良いが、ここではSTG固有の処理のみ担当）

        private void Start()
        {
            if (healthStatus != null)
            {
                // イベント購読
                healthStatus.OnDamage += OnDamage;
                healthStatus.OnDeath += OnDeath;
                healthStatus.OnReset += OnReset;
            }
            else
            {
                Debug.LogError("HealthStatus not found for STGPlayer!", this);
            }

            if (bodyRenderer == null)
            {
                bodyRenderer = GetComponentInChildren<Renderer>();
            }
        }

        private void OnDestroy()
        {
            if (healthStatus != null)
            {
                healthStatus.OnDamage -= OnDamage;
                healthStatus.OnDeath -= OnDeath;
                healthStatus.OnReset -= OnReset;
            }
        }

        public void TakeDamage(int damage)
        {
            if (healthStatus == null) return;

            // 無敵時間チェック
            if (Time.time - lastDamageTime < invincibilityTime)
                return;

            lastDamageTime = Time.time;

            // HealthStatusにダメージを与える（これによりOnDamageイベントが発火する）
            healthStatus.Damage(damage);
        }

        /// <summary>
        /// HealthStatusのOnDamageイベントハンドラ
        /// </summary>
        private void OnDamage(int currentHealth)
        {
            // ダメージ演出一時削除要望によりコメントアウト
            /*
            SoundManager.instance.Play("打撃1");
            transform.DOShakePosition(0.5f, 0.5f, 20, 90f, false, true);

            // 無敵点滅
            FlashInvincible().Forget();
            */
        }

        private void OnDeath()
        {
            // 死亡時の処理（ログ出力やエフェクトなど）
            Debug.Log("Player Dead (STG)");
        }

        private void OnReset()
        {
            if (bodyRenderer != null)
            {
                bodyRenderer.enabled = true;
            }
        }

        private async Cysharp.Threading.Tasks.UniTaskVoid FlashInvincible()
        {
            if (bodyRenderer == null) return;

            float elapsed = 0f;
            float interval = 0.1f;
            while (elapsed < invincibilityTime)
            {
                bodyRenderer.enabled = !bodyRenderer.enabled;
                await Cysharp.Threading.Tasks.UniTask.Delay(System.TimeSpan.FromSeconds(interval));
                elapsed += interval;
            }

            bodyRenderer.enabled = true;
        }

        public void ResetHp()
        {
            if (healthStatus != null)
            {
                healthStatus.Reset();
            }
        }
    }
}