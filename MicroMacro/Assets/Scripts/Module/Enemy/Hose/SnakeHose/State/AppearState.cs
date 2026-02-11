using System;
using Constants;
using CoreModule.AI.HSM;
using CoreModule.Utility;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Module.Management;
using Module.Player.Component;
using Unity.Cinemachine;
using UnityEngine;

namespace Module.Enemy.Hose.SnakeHose.State
{
    public class AppearState : HierarchicalStateMachine.State
    {
        private readonly Animator animator;
        private readonly CinemachineCamera nearInCamera;
        private readonly CinemachineCamera waterBallAttackCamera;
        private readonly SnakeHoseParameter parameter;
        private readonly SnakeHoseCondition condition;
        private readonly RadialBlurFeature blurFeature;
        private readonly SnakeHoseComponents components;
        
        private PlayerCondition playerCondition;

        public AppearState(SnakeHoseComponents components, SnakeHoseParameter parameter, SnakeHoseCondition condition)
        {
            this.animator = components.Animator;
            nearInCamera = components.NearInCamera;
            this.components = components;
            waterBallAttackCamera = components.WaterBallAttackCamera;
            this.parameter = parameter;
            this.condition = condition;
            
            playerCondition = GameObject.FindWithTag(Tag.Player).GetComponent<PlayerCondition>();

            if (!RendererFeatures.TryGetRendererFeature(out blurFeature))
            {
                Debug.LogError("RadialBlurFeatureが存在しません!");
            }
            
            components.HpBarCanvasGroup.alpha = 0f;
        }

        internal override void OnEnter()
        {
            condition.CurrentState = SnakeHoseCondition.State.WaterBallAttack;  
            return;
            WaitToNextStateAsync();
        }

        internal override void OnExit()
        {
            animator.enabled = false;
        }

        private async void WaitToNextStateAsync()
        {
            playerCondition.IsPlayerLocked = true;
            
            await UniTask.Delay(System.TimeSpan.FromSeconds(parameter.AppearDuration), cancellationToken: CancellationToken);

            nearInCamera.Priority = 1000;

            await UniTask.Delay(System.TimeSpan.FromSeconds(1f), cancellationToken: CancellationToken);

            SoundManager.instance.Play("目を開く");

            RadialBlurParams blurParams = blurFeature.GetParams();

            blurParams.Intensity = 0.3f;

            // 目を開いた瞬間にラディアルブラー
            _ = DOTween.To(() => blurParams.Intensity, x => blurParams.Intensity = x, 0f, 0.3f);

            await UniTask.Delay(TimeSpan.FromSeconds(1.2f));

            nearInCamera.Priority = 100;
            waterBallAttackCamera.Priority = 1000;
            _ = components.HpBarCanvasGroup.DOFade(1f, 2f).SetLink(components.HpBarCanvasGroup.gameObject);
            
            playerCondition.IsPlayerLocked = false;

            condition.CurrentState = SnakeHoseCondition.State.WaterBallAttack;  
        }

        internal override void Update()
        {
        }

        internal override void UpdatePhysics()
        {
        }

        internal override void Dispose()
        {
            RadialBlurParams parameter = blurFeature.GetParams();
            parameter.Intensity = 0f;
        }
    }
}