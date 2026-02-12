using System;
using System.Threading;
using Constants;
using CoreModule.AI.HSM;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Module.Application.Dialogue;
using Module.Management;
using Module.Scaling;
using NaughtyAttributes;
using PropertyGenerator.Generated;
using UGizmo;
using UnityEngine;

namespace Module.Enemy.Hose
{
    public class LastAttackHoseBehaviour : MonoBehaviour
    {
        [SerializeField] private Scaler scaler;
        [SerializeField] private Transform waterPivot;
        [SerializeField] private HoseControllerWrapper hoseControllerWrapper;
        [SerializeField] private SnakeHoseController snakeHoseController;
        [SerializeField] private WaterSplashScaler waterSplashScaler;
        [SerializeField] private WaterVisualSystem visualSystem;
        [SerializeField] private DialogueManager dialogueManager;
        [SerializeField] private Renderer screwRenderer;
        [SerializeField] private Renderer waterRenderer;

        [SerializeField] private float screwWidth = 0.3f;
        [SerializeField] private float waterSpeed = 0.3f;
        [SerializeField, Header("激流時の水の色")] private Color rapidsWaterColor;

        [Header("BGM Settings")] [SerializeField]
        private string bgmName;

        [SerializeField] private float fadeDuration = 2.0f;

        private Vector3 waterDefaultScale;
        private Vector3 scalerDefaultScale;

        public float CurrentIntensity { get; private set; } = 1f;

        private static readonly int WaterThresholdId = Shader.PropertyToID("_WaterThreshold");
        private static readonly int MainColor = Shader.PropertyToID("_MainColor");

        private void Awake()
        {
            scaler.OnScaleStarted += OnScaleStarted;

            // 初期情報の取得
            waterDefaultScale = waterPivot.localScale;
            scalerDefaultScale = scaler.transform.localScale;

            screwRenderer.material.DOFloat(screwWidth, WaterThresholdId, 1f);
            waterRenderer.material.SetColor(MainColor, rapidsWaterColor);
        }

        private void OnScaleStarted(ScaleEventArgs args)
        {
            int direction = (args.CurrentStep - args.PreviousStep) > 0 ? 1 : -1;
            if (direction > 0)
            {
                hoseControllerWrapper.SetRotateRTrigger();
            }
            else
            {
                hoseControllerWrapper.SetRotateLTrigger();
            }
        }

        public async UniTask OnWater(CancellationToken token, float maxMultiplier)
        {
            CurrentIntensity = 0f;
            waterSplashScaler.UpdateWaterSplashState(WaterState.Pushing);

            while (!token.IsCancellationRequested)
            {
                CurrentIntensity += waterSpeed * Time.deltaTime;
                CurrentIntensity = Mathf.Clamp(CurrentIntensity, 0, 1f); // maxMultiplierによる制限はBoxCast後に行うためここでは1f上限

                // maxMultiplier制限
                if (CurrentIntensity > maxMultiplier)
                {
                    CurrentIntensity = maxMultiplier;
                    snakeHoseController.CurrentIntensity = CurrentIntensity;
                    break;
                }

                snakeHoseController.CurrentIntensity = CurrentIntensity;

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }

        public async UniTask OffWater(CancellationToken token)
        {
            waterSplashScaler.UpdateWaterSplashState(WaterState.Ending);

            float customMultiplier = 3f;

            while (!token.IsCancellationRequested)
            {
                CurrentIntensity -= waterSpeed * customMultiplier * Time.deltaTime;
                CurrentIntensity = Mathf.Clamp(CurrentIntensity, 0f, 1f);

                if (CurrentIntensity <= 0f)
                {
                    CurrentIntensity = 0f;
                    snakeHoseController.CurrentIntensity = CurrentIntensity;
                    break;
                }

                snakeHoseController.CurrentIntensity = CurrentIntensity;

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            Debug.Log("OffWater Ended", this);
            waterSplashScaler.UpdateWaterSplashState(WaterState.End);
        }

        public void ShowDialogue(int index)
        {
            switch (index)
            {
                case 0:
                    dialogueManager.Enqueue("Boss2-3");
                    dialogueManager.Enqueue("Boss2-4");
                    break;
                case 1:
                    dialogueManager.Enqueue("Boss2-5");
                    break;
            }
        }

        public void ManualAttack(float multiplier)
        {
            if (multiplier == 0f)
            {
                OffWater(destroyCancellationToken).Forget();
                return;
            }

            OnWater(destroyCancellationToken, multiplier).Forget();
        }

        [Button]
        public void PlayBGMWithFade()
        {
            if (string.IsNullOrEmpty(bgmName)) return;

            var source = SoundManager.instance.Play(bgmName, 0f);
            if (source != null)
            {
                float targetVolume = 0.1f;
                source.volume = 0f; // Start from 0
                source.DOFade(targetVolume, fadeDuration);
            }
        }
    }
}