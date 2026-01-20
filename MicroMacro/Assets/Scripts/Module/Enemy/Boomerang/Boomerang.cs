using System;
using System.Threading;
using CoreModule.Utility;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Module.Management;
using Module.Scaling;
using PropertyGenerator.Generated;
using UnityEngine;

namespace Module.Enemy.Boomerang
{
    public class Boomerang : MonoBehaviour
    {
        // =============================
        // ▼ フライトパラメータ
        // =============================

        [Header("フライト設定")] [Min(0.001f)] [SerializeField]
        private float oneWayDistance = 10f;
        // → ブーメランの片道飛距離（D）

        [Min(0.001f)] [SerializeField] private float oneWayTime = 1.0f;
        // → ブーメランの片道所要時間（T）

        [SerializeField] [Min(0.0001f)] private float exponentialSharpness = 4.0f;
        // → 加速・減速のカーブの鋭さ（指数イージングのk）

        // =============================
        // ▼ 投擲タイミング・発射調整
        // =============================

        [Header("投擲タイミング")] [SerializeField] private float throwInterval = 1f;
        // → 「戻った後、次を投げるまでの待ち時間」

        [Header("発射位置の調整")] [SerializeField] private float shootHeightOffset = -0.1f;

        [Header("発射方向の調整")] [SerializeField] private float shootDirectionOffset = -0.1f;
        // → キャラの向きから斜めに飛ばしたい場合の微調整

        [SerializeField, Header("最初の投擲までの遅延時間")]
        private float firstThrowDelay = 0f;

        // =============================
        // ▼ 揺れ（被ダメージ時）
        // =============================

        [Header("揺れ設定（被ダメージ時）")] [SerializeField]
        private float damageShakeDuration = 0.35f;

        [SerializeField] private float damageShakeStrength = 0.2f;
        [SerializeField] private int damageShakeVibrato = 30;
        [SerializeField] private float damageShakeRandomness = 90f;

        // =============================
        // ▼ 参照
        // =============================

        [Header("ブーメラン参照")] [SerializeField] private EnemyStatus status;
        [SerializeField] private Scaler boomerang;
        [SerializeField] private Rigidbody boomerangRig;
        [SerializeField] private Collider boomerangCapCollider;
        [SerializeField] private TransformSyncer capTransformSyncer;
        [SerializeField] private Transform headBoneTransform;
        [SerializeField] private Transform capTransform;
        [SerializeField] private BoomerangWrapper animatorWrapper;
        [SerializeField] private Animator capAnimator;

        // =============================
        // ▼ キャップ回転アニメ演出
        // =============================

        [Header("回転設定")] [SerializeField] private float capSpinSpeedDegreesPerSecond = 1080f;
        [SerializeField] private float capFixedEulerX = 0f;
        [SerializeField] private float capFixedEulerZ = 60f;

        // =============================
        // ▼ 内部状態
        // =============================

        private static readonly int boomerangRotationHash = Animator.StringToHash("BoomerangRotation");

        private const float SmallPositiveValue = 0.0001f;
        private const float FinishMarginSeconds = 0.01f;

        private bool isActive = false;

        // ---- フライト計算用 ----
        private float elapsedTimeSeconds = 0f;
        private float initialSpeed;
        private float acceleration; // （現状未使用だが後で戻す可能性があるので保持）
        private Vector3 throwDirection;
        private Vector3 throwOriginPosition;

        // =============================
        // ▼ Unity Event
        // =============================

        private void Start()
        {
            // 敵から飛んでくるイベント登録
            status.OnDeath += OnDeath;
            status.OnDamage += OnDamage;

            // 投げ続けるループ（AIの簡易代替）
            LoopThrow().Forget();
        }

        private void OnDestroy()
        {
            // イベント解除しないとデッドイベントが溜まる
            if (status != null)
            {
                status.OnDeath -= OnDeath;
                status.OnDamage -= OnDamage;
            }
        }

        // =============================
        // ▼ ループ（投げモーション）
        // =============================

        private async UniTaskVoid LoopThrow()
        {
            CancellationToken token = destroyCancellationToken;

            // 最初の1回だけの遅延
            if (firstThrowDelay > 0f)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(firstThrowDelay), cancellationToken: token);
            }

            while (!token.IsCancellationRequested)
            {
                // 攻撃アニメ開始
                animatorWrapper.IsAttacking = true;

                // ブーメランが1往復するまで待つ（T × 2）
                float roundTrip = GetClampedOneWayTime() * 2f;
                await UniTask.Delay(TimeSpan.FromSeconds(roundTrip), cancellationToken: token);

                // 攻撃アニメ終了
                animatorWrapper.IsAttacking = false;

                // 次の投擲までのインターバル
                await UniTask.Delay(TimeSpan.FromSeconds(throwInterval), cancellationToken: token);
            }
        }


        // =============================
        // ▼ 受ダメ演出
        // =============================

        private void OnDamage(int damage)
        {
            SoundManager.instance.Play("打撃1");

            // 敵本体を揺らす
            transform.DOShakePosition(
                damageShakeDuration,
                damageShakeStrength,
                damageShakeVibrato,
                damageShakeRandomness,
                false, false
            );
        }

        // =============================
        // ▼ 死亡
        // =============================

        private void OnDeath()
        {
            animatorWrapper.IsDeath = true;
            Destroy(gameObject, 1f);
        }

        // =============================
        // ▼ Catch（帰還処理）
        // =============================

        public void Catch()
        {
            if (destroyCancellationToken.IsCancellationRequested)
                return;

            CheckBoomerangHit();

            // 回転アニメ停止
            capAnimator.enabled = false;

            // ブーメランを即時縮小
            boomerang.SetScaleImmediate(0, true);

            // 頭の位置へロック
            SetCapLockState(true);
        }

        private void SetCapLockState(bool isLocked)
        {
            // ロック中は見た目アニメ無効化
            capAnimator.gameObject.SetActive(!isLocked);

            // TransformSyncer による位置同期のON/OFF
            capTransformSyncer.syncPosition = isLocked;

            // 一度「現在の位置」を強制同期してズレを防止
            capTransformSyncer.UpdateManual();

            // 衝突判定の有無も切り替える
            boomerangCapCollider.enabled = !isLocked;
        }

        // =============================
        // ▼ 物理挙動（往復運動）
        // =============================

        private void FixedUpdate()
        {
            if (!isActive)
            {
                return;
            }

            UpdateFlight(Time.fixedDeltaTime);
        }

        private void UpdateFlight(float deltaTime)
        {
            // 経過時間
            elapsedTimeSeconds += deltaTime;

            float T = GetClampedOneWayTime();
            float D = GetClampedOneWayDistance();

            // 正規化時間 u = t / T
            float normalizedTime = elapsedTimeSeconds / T;

            // 指数イージング（往復）速度
            float velocityScale = EvalExpPingPongVelocityPerSec(normalizedTime, exponentialSharpness, T);

            // 実 Velocity = 方向ベクトル × (距離 × ds/dt)
            boomerangRig.linearVelocity = throwDirection * (D * velocityScale);

            // キャップを回転させる
            UpdateCapRotation(deltaTime);

            // 2T 経過で飛行終了
            float finishTime = (2f * T) - FinishMarginSeconds;
            if (elapsedTimeSeconds >= finishTime)
            {
                FinishFlight();
            }
        }

        private void UpdateCapRotation(float deltaTime)
        {
            // 回転のオイラー角を毎フレーム更新
            Vector3 angle = capTransform.localEulerAngles;
            angle.x = capFixedEulerX;
            angle.y += capSpinSpeedDegreesPerSecond * deltaTime;
            angle.z = capFixedEulerZ;

            capTransform.localEulerAngles = angle;
        }

        // =============================
        // ▼ 投擲処理
        // =============================

        public async UniTaskVoid Throw()
        {
            if (destroyCancellationToken.IsCancellationRequested)
                return;

            // TransformSyncer の位置をまず確定させる
            capTransformSyncer.UpdateManual();

            // 次フレームまで待つ → Unity の Transform 更新完了を待つ
            await UniTask.Yield(destroyCancellationToken);

            PrepareBeforeThrow();
            InitializeFlightParameters();
            StartFlight();
        }

        private void PrepareBeforeThrow()
        {
            if (boomerang == null)
                return;

            // 縮小状態に戻す
            boomerang.SetScaleImmediate(0, true);

            // キャップのロック解除（キャラの頭から自由に動ける状態）
            SetCapLockState(false);
        }

        private void InitializeFlightParameters()
        {
            float T = GetClampedOneWayTime();
            float D = GetClampedOneWayDistance();

            // 速度計算式：初速 = 2D/T
            initialSpeed = 2f * D / T;

            // 加速度（指数イージングでは使わないが一応保持）
            acceleration = 2f * D / (T * T);

            // 発射方向 = キャラの右方向 + 少し上方向
            throwDirection = (transform.right + Vector3.up * shootDirectionOffset).normalized;

            // 発射位置 = ボーン位置 + 高さオフセット
            throwOriginPosition = headBoneTransform.position + Vector3.up * shootHeightOffset;

            // 初期化
            elapsedTimeSeconds = 0f;
            isActive = true;
        }

        private void StartFlight()
        {
            // 物理で動かす
            boomerangRig.isKinematic = false;

            // 初期位置
            boomerangRig.position = throwOriginPosition;

            // 初速
            boomerangRig.linearVelocity = throwDirection * initialSpeed;
            boomerangRig.angularVelocity = Vector3.zero;

            // 回転アニメ開始
            capAnimator.enabled = true;
            capAnimator.Play(boomerangRotationHash);
        }

        private void FinishFlight()
        {
            isActive = false;

            // 完全停止
            boomerangRig.linearVelocity = Vector3.zero;
            boomerangRig.angularVelocity = Vector3.zero;
            boomerangRig.isKinematic = true;

            // 向きを戻す（キャッチしやすい向きに）
            capTransform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }

        // =============================
        // ▼ 当たり判定（帰還時）
        // =============================

        private void CheckBoomerangHit()
        {
            // CurrentStep が0より大きい＝何かに当たっている
            if (boomerang.CurrentStep > 0)
            {
                status.Damage(1);
            }
        }

        // =============================
        // ▼ ヘルパー
        // =============================

        private float GetClampedOneWayTime()
        {
            return Mathf.Max(SmallPositiveValue, oneWayTime);
        }

        private float GetClampedOneWayDistance()
        {
            return Mathf.Max(SmallPositiveValue, oneWayDistance);
        }

        /// <summary>
        /// 指数イージング（ピンポン）: ds/dt を返す
        /// u: 0〜2 の範囲で進行（1で折り返し）
        /// k: シャープネス（大きいほどピーキー）
        /// </summary>
        private float EvalExpPingPongVelocityPerSec(float u, float k, float T)
        {
            float kk = Mathf.Max(SmallPositiveValue, k);
            float denom = 1f - Mathf.Exp(-kk);

            // 行き（0〜1）
            if (u <= 1f)
            {
                float x = Mathf.Clamp01(u);
                return (kk * Mathf.Exp(-kk * x)) / (denom * T);
            }
            // 帰り（1〜2）
            else
            {
                float x = Mathf.Clamp01(2f - u);
                return -(kk * Mathf.Exp(-kk * x)) / (denom * T);
            }
        }
    }
}