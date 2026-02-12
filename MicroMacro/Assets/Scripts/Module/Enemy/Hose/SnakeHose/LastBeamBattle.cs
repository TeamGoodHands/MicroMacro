using System;
using CoreModule.Input;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Module.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

namespace Module.Enemy.Hose.SnakeHose
{
    public class LastBeamBattle : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private Transform enemy;

        [SerializeField] private Transform enemyPivot;
        [SerializeField] private Transform enemyHead;
        [SerializeField] private Transform ally;
        [SerializeField] private Transform allyPivot;
        [SerializeField] private Transform allyHead;
        [SerializeField] private VisualEffect effectPivot;
        [SerializeField] private ClearPlayer clearPlayer;
        [SerializeField] private GameObject disableOnForceWin;
        [SerializeField] private Module.Enemy.Hose.STG.STGPlayer stgPlayer;
        [SerializeField] private Module.Application.SceneSwitch.FadeAndSceneTransition fadeTransition;
        [SerializeField] private Module.Enemy.Hose.STG.ShootingGame shootingGame;

        [Header("Settings")] [SerializeField] private float scaleMultiplier = 1f;

// ... (existing code omitted) ...

        [SerializeField] private float headScaleBackOffset = 0f;
        [SerializeField] private float wholeScale = 10f;
        [SerializeField] private float playerPushStrength = 0.05f;
        [SerializeField] private float enemyPushStrength = 0.05f;
        [SerializeField] private float enemyPushInterval = 0.1f;
        [SerializeField] private float forceFillSpeed = 0.5f; // Speed for force win/loss

        [Header("Difficulty")]
        [SerializeField] 
        private AnimationCurve resistanceCurve = new AnimationCurve(new Keyframe(0f, 0.5f), new Keyframe(0.5f, 1f), new Keyframe(1f, 2f));
        // X: CurrentScale (0=Lose, 1=Win), Y: Enemy Push Multiplier

        [Header("Status")] [SerializeField] private float currentScale;
        [SerializeField] private float targetScale;
        [SerializeField] private float smoothSpeed = 5f;
        [SerializeField] private float minThicknessMultiplier = 0.5f;
        [SerializeField] private float maxThicknessMultiplier = 2.0f;
        [SerializeField] private float knockbackAmount = 2.0f;
        [SerializeField] private float introDuration = 1.0f;

        [Header("Shake Settings")] [SerializeField]
        private float shakeStrength = 0.2f;

        [SerializeField] private int shakeVibrato = 20;

        public enum BattleState
        {
            Idle,
            Intro,
            Battle,
            Finished // Optional: preventing input after result?
        }

        private BattleState state = BattleState.Idle;
        public BattleState State => state;
        public bool IsForceKilled => isForceKilled;

        private bool isForceWinning = false;
        private bool isForceLosing = false;
        private bool isForceKilled = false;

        public void ForceKill()
        {
            state = BattleState.Finished;
            isForceKilled = true;
        }

        private InputEvent macroShootEvent;
        private InputEvent microShootEvent;
        private float enemyPushTimer;
        private float introTimer;
        private float growthFactor; // 0 to 1, used for intro animation

        private Vector3 initialAllyLocalPos;
        private Vector3 initialEnemyLocalPos;
        private Vector3 initialAllyScale;
        private Vector3 initialEnemyScale;
        private Vector3 initialAllyHeadScale;
        private Vector3 initialEnemyHeadScale;

        // Tweens
        private Tweener allyPivotShake;
        private Tweener enemyPivotShake;
        private Tweener allyHeadShake;
        private Tweener enemyHeadShake;

        private void Start()
        {
            currentScale = 0.5f;
            targetScale = 0.5f;
            growthFactor = 0f;
            state = BattleState.Idle;

            initialAllyScale = allyPivot.localScale;
            initialEnemyScale = enemyPivot.localScale;
            if (allyHead != null) initialAllyHeadScale = allyHead.localScale;
            if (enemyHead != null) initialEnemyHeadScale = enemyHead.localScale;

            // Capture initial positions relative to THIS transform
            if (ally != null) initialAllyLocalPos = transform.InverseTransformPoint(ally.position);
            if (enemy != null) initialEnemyLocalPos = transform.InverseTransformPoint(enemy.position);

            // Input setup
            macroShootEvent = InputProvider.CreateEvent(ActionGuid.Player.MacroShoot);
            microShootEvent = InputProvider.CreateEvent(ActionGuid.Player.MicroShoot);

            macroShootEvent.Started += OnShoot;
            microShootEvent.Started += OnShoot;

            // Subscribe to Player Reset
            if (stgPlayer != null && stgPlayer.HealthStatus != null)
            {
                stgPlayer.HealthStatus.OnReset += ResetBattle;
            }

            // Initialize visuals to 0
            UpdateVisuals();
        }

        private void OnDestroy()
        {
            if (macroShootEvent != null) macroShootEvent.Started -= OnShoot;
            if (microShootEvent != null) microShootEvent.Started -= OnShoot;
            
            if (stgPlayer != null && stgPlayer.HealthStatus != null)
            {
                stgPlayer.HealthStatus.OnReset -= ResetBattle;
            }
            
            StopShake();
        }

        public void ResetBattle()
        {
            state = BattleState.Idle;
            isForceWinning = false;
            isForceLosing = false;
            isForceKilled = false;
            
            currentScale = 0.5f;
            targetScale = 0.5f;
            growthFactor = 0f;
            introTimer = 0f;
            enemyPushTimer = 0f;

            if (effectPivot != null)
            {
                effectPivot.Stop();
                effectPivot.gameObject.SetActive(false);
            }
            
            if (disableOnForceWin != null)
            {
                disableOnForceWin.SetActive(true);
            }

            StopShake();
            UpdateVisuals();
        }

        public void Play()
        {
            if (state != BattleState.Idle) return;

            state = BattleState.Intro;
            introTimer = 0f;
            growthFactor = 0f;

            // Start VFX
            if (effectPivot != null)
            {
                effectPivot.gameObject.SetActive(true);
                effectPivot.Stop();
                effectPivot.Play();
            }
        }

        public async void ForcePlayerWin()
        {
            isForceWinning = true;
            isForceLosing = false;
            
            await UniTask.Delay(TimeSpan.FromSeconds(3f), cancellationToken: this.GetCancellationTokenOnDestroy());
            
            clearPlayer.ClearEffect().Forget();
            
            await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: this.GetCancellationTokenOnDestroy());
            
            disableOnForceWin.SetActive(false);
        }

        public void ForceEnemyWin()
        {
            isForceWinning = false;
            isForceLosing = true;
        }

        private void Update()
        {
            if (state == BattleState.Idle)
            {
                return;
            }

            if (state == BattleState.Intro)
            {
                introTimer += Time.deltaTime;
                growthFactor = Mathf.Clamp01(introTimer / introDuration);

                if (introTimer >= introDuration)
                {
                    state = BattleState.Battle;
                    growthFactor = 1f;
                    StartShake();
                }
            }
            else if (state == BattleState.Battle)
            {
                if (isForceWinning)
                {
                    float speed = forceFillSpeed;
                    if (targetScale > 1.0f) speed *= 5.0f; // Speed up when piercing

                    targetScale += speed * Time.deltaTime;
                    // Allow overshooting for "piercing" effect (e.g. up to 5x)
                    if (targetScale > 5.0f) targetScale = 5.0f;
                }
                else if (isForceLosing)
                {
                    float speed = forceFillSpeed;
                    if (targetScale < 0.0f) speed *= 5.0f; // Speed up when piercing

                    targetScale -= speed * Time.deltaTime;
                    // Allow overshooting for "piercing" effect (e.g. down to -4x)
                    if (targetScale < -4.0f) targetScale = -4.0f;
                }
                else
                {
                    // Regular Battle Logic
                    enemyPushTimer += Time.deltaTime;
                    if (enemyPushTimer >= enemyPushInterval)
                    {
                        enemyPushTimer = 0f;
                        PushEnemy();
                    }
                }

                // Smoothly interpolate currentScale
                currentScale = Mathf.Lerp(currentScale, targetScale, Time.deltaTime * smoothSpeed);

                // Lose Condition
                if (currentScale <= 0f && !isForceWinning && !isForceLosing && !isForceKilled)
                {
                    ForceEnemyWin();
                    KillPlayerSequence().Forget();
                }
            }

            // Update Visuals
            UpdateVisuals();
        }

        private async UniTaskVoid KillPlayerSequence()
        {
            // Disable auto-reset in ShootingGame so we can control timing
            if (shootingGame != null)
            {
                shootingGame.SetAutoReset(false);
            }

            // Wait for visual pierce
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: this.GetCancellationTokenOnDestroy());
            
            // Kill Player (Visuals only, no reset yet)
            if (stgPlayer != null)
            {
                stgPlayer.TakeDamage(9999);
            }

            // Wait for death animation / lingering moment
            await UniTask.Delay(TimeSpan.FromSeconds(1.5f), cancellationToken: this.GetCancellationTokenOnDestroy());

            // Start Fade Out
            if (fadeTransition != null)
            {
                await fadeTransition.FadeOut(false);
            }
            
            // Manual Reset
            if (shootingGame != null)
            {
                shootingGame.ResetGame();
                shootingGame.SetAutoReset(true); // Re-enable for future
            }

            // Wait a moment for reset to process
            await UniTask.Yield(this.GetCancellationTokenOnDestroy());

            // Start Fade In
            if (fadeTransition != null)
            {
                await fadeTransition.FadeIn(false);
            }
        }

        private void StartShake()
        {
            StopShake();

            if (allyPivot != null)
                allyPivotShake = allyPivot.DOShakePosition(1f, shakeStrength, shakeVibrato, fadeOut: false).SetLoops(-1, LoopType.Restart);

            if (enemyPivot != null)
                enemyPivotShake = enemyPivot.DOShakePosition(1f, shakeStrength, shakeVibrato, fadeOut: false).SetLoops(-1, LoopType.Restart);

            if (allyHead != null)
                allyHeadShake = allyHead.DOShakePosition(1f, shakeStrength, shakeVibrato, fadeOut: false).SetLoops(-1, LoopType.Restart);

            if (enemyHead != null)
                enemyHeadShake = enemyHead.DOShakePosition(1f, shakeStrength, shakeVibrato, fadeOut: false).SetLoops(-1, LoopType.Restart);
        }

        private void StopShake()
        {
            allyPivotShake?.Kill();
            enemyPivotShake?.Kill();
            allyHeadShake?.Kill();
            enemyHeadShake?.Kill();

            // Reset local positions if needed, but DOShakePosition usually stays near original if looping properly or killed completely?
            // Usually DOShakePosition might leave offset if killed mid-way. 
            // Ideally we assume the Pivots/Heads localPosition was (0,0,0) or static properly.
            // Since we didn't cache their initial local positions for reset, let's rely on DOTween's snap back or relative shake.
            // DOShakePosition is relative.
        }

        private void OnShoot(InputAction.CallbackContext context)
        {
            if (state != BattleState.Battle) return;
            if (isForceWinning || isForceLosing) return; // Disable input during forced outcome

            targetScale += playerPushStrength;
            // Allow slightly over 1 to ensure win condition triggers easily
            targetScale = Mathf.Clamp(targetScale, -0.5f, 1.5f);
        }

        private void PushEnemy()
        {
            // Evaluate resistance based on current scale
            // If currentScale is near 1 (Player Winning), multiplier should be high.
            // If currentScale is near 0 (Enemy Winning), multiplier should be low.
            float multiplier = resistanceCurve.Evaluate(currentScale);
            
            targetScale -= enemyPushStrength * multiplier;
            // Allow slightly under 0 to ensure lose condition triggers easily
            targetScale = Mathf.Clamp(targetScale, -0.5f, 1.5f);
        }

        private void UpdateVisuals()
        {
            // Intro Animation: Scale everything by growthFactor

            // X Scale Calculation
            float allyScaleX = currentScale * wholeScale * growthFactor;
            float enemyScaleX = (wholeScale - (currentScale * wholeScale)) * growthFactor;
            
            // Prevent negative visual scales if piercing
            if (allyScaleX < 0) allyScaleX = 0;
            if (enemyScaleX < 0) enemyScaleX = 0;

            // YZ Scale Calculation (Thickness)
            // Clamp t value for Lerp to ensure thickness doesn't go wild beyond 0-1 range
            float t = Mathf.Clamp01(currentScale);
            float allyThicknessMult = Mathf.Lerp(minThicknessMultiplier, maxThicknessMultiplier, t);
            float enemyThicknessMult = Mathf.Lerp(minThicknessMultiplier, maxThicknessMultiplier, 1f - t);

            // Apply to Ally Pivot
            Vector3 allyScale = initialAllyScale;
            allyScale.x = allyScaleX;
            allyScale.y *= allyThicknessMult * growthFactor;
            allyScale.z *= allyThicknessMult * growthFactor;
            allyPivot.localScale = allyScale;

            // Apply to Enemy Pivot
            Vector3 enemyScale = initialEnemyScale;
            enemyScale.x = enemyScaleX;
            enemyScale.y *= enemyThicknessMult * growthFactor;
            enemyScale.z *= enemyThicknessMult * growthFactor;
            enemyPivot.localScale = enemyScale;

            // Apply to Heads (Keep constant during Intro)
            if (allyHead != null)
            {
                if (state == BattleState.Battle)
                {
                    allyHead.localScale = initialAllyHeadScale * allyThicknessMult;
                }
                else
                {
                    allyHead.localScale = initialAllyHeadScale;
                }
            }

            if (enemyHead != null)
            {
                if (state == BattleState.Battle)
                {
                    enemyHead.localScale = initialEnemyHeadScale * enemyThicknessMult;
                }
                else
                {
                    enemyHead.localScale = initialEnemyHeadScale;
                }
            }

            // Apply Position Offset (Knockback)
            // Ally moves +Z (relative to THIS) when small
            if (ally != null)
            {
                float allyLossRatio = Mathf.Clamp01((0.5f - currentScale) / 0.5f);
                Vector3 allyOffset = Vector3.forward * (allyLossRatio * knockbackAmount);
                // Convert (Initial Local + Offset) -> World -> Set Position
                ally.position = transform.TransformPoint(initialAllyLocalPos + allyOffset);
            }

            // Enemy moves -Z (relative to THIS) when small
            if (enemy != null)
            {
                float enemyLossRatio = Mathf.Clamp01((currentScale - 0.5f) / 0.5f);
                Vector3 enemyOffset = Vector3.back * (enemyLossRatio * knockbackAmount);
                // Convert (Initial Local + Offset) -> World -> Set Position
                enemy.position = transform.TransformPoint(initialEnemyLocalPos + enemyOffset);
            }

            // Effect Position: Z = currentScale * wholeScale
            Vector3 effectPos = effectPivot.transform.localPosition;
            effectPos.z = currentScale * wholeScale * growthFactor;
            effectPivot.transform.localPosition = effectPos;
        }
    }
}