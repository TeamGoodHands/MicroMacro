using System.Threading;
using Constants;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Module.Scaling;
using PropertyGenerator.Generated;
using UnityEngine;

namespace Module.Enemy.Hose
{
    [RequireComponent(typeof(WaterPhysicsSystem))]
    [RequireComponent(typeof(WaterVisualSystem))]
    public class SnakeHoseController : MonoBehaviour, IWaterFlow
    {
        [Header("Settings")] [SerializeField] private WaterFlowParameter parameter;
        [SerializeField] private Scaler scaler;
        [SerializeField] private HoseControllerWrapper hoseControllerWrapper;

        [Header("State")] [SerializeField] private bool isRapids;

        public WaterFlowParameter Parameter => parameter;
        public WaterPhysicsSystem Physics => physicsSystem;
        public bool IsRapid => isRapids;
        public WaterFlow WaterFlow => null;

        private WaterPhysicsSystem physicsSystem;
        private WaterVisualSystem visualSystem;
        private Transform playerTransform;

        public float CurrentIntensity { get; private set; } = 0f;

        private void Awake()
        {
            physicsSystem = GetComponent<WaterPhysicsSystem>();
            visualSystem = GetComponent<WaterVisualSystem>();
        }

        private void Start()
        {
            var playerObj = GameObject.FindWithTag(Tag.Player);
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }

            visualSystem.Initialize(parameter, scaler);
            visualSystem.SetRapidsMode(isRapids);

            if (scaler != null)
            {
                scaler.OnScaleStarted += OnScaleStarted;
            }
        }

        private void OnDestroy()
        {
            if (scaler != null)
            {
                scaler.OnScaleStarted -= OnScaleStarted;
            }
        }

        private void Update()
        {
            visualSystem.UpdateVisuals(CurrentIntensity);
        }

        private void FixedUpdate()
        {
            physicsSystem.RunPhysics(CurrentIntensity, playerTransform);

            // 【変更点】
            // ここでの自動追従（parameter.LookAtPlayer チェック）は削除しました。
            // 必要な場合は LookAtPlayerSmoothAsync を呼んで制御します。
        }

        private void OnScaleStarted(ScaleEventArgs args)
        {
            int direction = (args.CurrentStep - args.PreviousStep) > 0 ? 1 : -1;
            if (direction > 0) hoseControllerWrapper.SetRotateRTrigger();
            else hoseControllerWrapper.SetRotateLTrigger();

            isRapids = args.CurrentStep > 0;
            visualSystem.SetRapidsMode(isRapids);
        }

        // --- Async Methods ---

        /// <summary>
        /// 指定時間、プレイヤーの方をゆっくり向き続けます
        /// の機能再現
        /// </summary>
        /// <param name="duration">追従する合計時間（秒）</param>
        /// <param name="smoothSpeed">振り向く速さ</param>
        public async UniTask LookAtPlayerSmoothAsync(float duration, float smoothSpeed)
        {
            // 自身のキャンセル・トークンを使用
            await LookAtPlayerSmoothAsync(duration, smoothSpeed, this.destroyCancellationToken);
        }

        public async UniTask LookAtPlayerSmoothAsync(float duration, float smoothSpeed, CancellationToken token)
        {
            float timeElapsed = 0f;

            // 指定時間が経過するまでループ
            while (timeElapsed < duration)
            {
                if (token.IsCancellationRequested) return;
                if (playerTransform == null) return;

                // 物理システムに「今の瞬間のターゲット角度」へ少しだけ回してもらう
                physicsSystem.LookAtTarget(playerTransform.position, smoothSpeed);

                timeElapsed += Time.fixedDeltaTime;

                // 物理演算に合わせて FixedUpdate のタイミングで待機
                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);
            }
        }

        public UniTask OnWater()
        {
            return OnWater(this.destroyCancellationToken);
        }

        public async UniTask OnWater(CancellationToken token)
        {
            float timer = 0f;
            while (timer < parameter.OnTime && !token.IsCancellationRequested)
            {
                CurrentIntensity += parameter.WaterSpeed * Time.deltaTime;
                CurrentIntensity = Mathf.Clamp01(CurrentIntensity);

                timer += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }

        public UniTask OffWater()
        {
            return OffWater(this.destroyCancellationToken);
        }

        public async UniTask OffWater(CancellationToken token)
        {
            float timer = 0f;
            while (timer < parameter.OffTime && !token.IsCancellationRequested)
            {
                CurrentIntensity -= parameter.WaterSpeed * Time.deltaTime;
                CurrentIntensity = Mathf.Clamp01(CurrentIntensity);

                timer += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            CurrentIntensity = 0f;
        }

        public void OffWaterImmediately()
        {
            CurrentIntensity = 0f;
        }

        public Tween ShakeBody(float duration)
        {
            return physicsSystem.ShakeBody(duration);
        }

        public Tween ResetAngle(float time)
        {
            return physicsSystem.ResetAngle(time)
                .OnComplete(() =>
                {
                    if (scaler != null) scaler.SetScale(0, true);
                });
        }
    }
}