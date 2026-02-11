using System;
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
        [SerializeField] private SnakeGripControllerWrapper gripControllerWrapper;
        [SerializeField] private Transform waterPivot;
        [SerializeField] private Transform rotatePivot;

        [Header("State")] [SerializeField] private bool isRapids;

        public WaterFlowParameter Parameter => parameter;
        public WaterPhysicsSystem Physics => physicsSystem;
        public bool IsRapid => isRapids;
        public WaterFlow WaterFlow { get; private set; } 

        private WaterPhysicsSystem physicsSystem;
        private WaterVisualSystem visualSystem;
        private Transform playerTransform;

        public float CurrentIntensity { get; set; } = 0f;
        public event Action<WaterState> OnWaterStateChanged;
        private WaterState waterState;

        private void Awake()
        {
            physicsSystem = GetComponent<WaterPhysicsSystem>();
            visualSystem = GetComponent<WaterVisualSystem>();
            
            WaterFlow = new WaterFlow(scaler, waterPivot, rotatePivot, parameter);
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
            physicsSystem.RunPhysics(CurrentIntensity, playerTransform);
        }

        private void OnScaleStarted(ScaleEventArgs args)
        {
            // isRapids = args.CurrentStep > 0;
            // visualSystem.SetRapidsMode(isRapids);
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

        public async UniTask OnWater(CancellationToken token)
        {
            SetWaterState(WaterState.Pushing);

            gripControllerWrapper.SetPushTrigger();
            gripControllerWrapper.IsLock = true;

            float timer = 0f;
            while (timer < parameter.OnTime && !token.IsCancellationRequested)
            {
                CurrentIntensity += parameter.WaterSpeed * Time.deltaTime;
                CurrentIntensity = Mathf.Clamp01(CurrentIntensity);

                if (CurrentIntensity == 1f)
                {
                    SetWaterState(WaterState.Pushed);
                }

                timer += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            SetWaterState(WaterState.Pushed);
        }

        public async UniTask OffWater(CancellationToken token)
        {
            OnWaterStateChanged?.Invoke(WaterState.Ending);

            gripControllerWrapper.IsLock = false;

            float timer = 0f;
            while (timer < parameter.OffTime && !token.IsCancellationRequested)
            {
                CurrentIntensity -= parameter.WaterSpeed * Time.deltaTime;
                CurrentIntensity = Mathf.Clamp01(CurrentIntensity);

                if (CurrentIntensity == 0f)
                {
                    SetWaterState(WaterState.End);
                }

                timer += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            CurrentIntensity = 0f;
            SetWaterState(WaterState.End);
        }

        public void OffWaterImmediately()
        {
            gripControllerWrapper.IsLock = false;
            OnWaterStateChanged?.Invoke(WaterState.Ending);
            OnWaterStateChanged?.Invoke(WaterState.End);
            CurrentIntensity = 0f;
        }

        private void SetWaterState(WaterState state)
        {
            if (waterState != state)
            {
                waterState = state;
                OnWaterStateChanged?.Invoke(state);
            }
        }

        public Tween ShakeBody(float duration)
        {
            return physicsSystem.ShakeBody(duration);
        }

        public Tween ResetAngle(float time)
        {
            return physicsSystem.ResetAngle(time);
        }
    }
}