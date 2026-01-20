using System;
using Constants;
using CoreModule.AI.HSM;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Module.Management;
using PropertyGenerator.Generated;
using Unity.Cinemachine;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Module.Enemy.Cargo
{
    /// <summary>
    /// ボスが後退しながら攻撃を行うステート
    /// </summary>
    public sealed class BackAttackState : HierarchicalStateMachine.State
    {
        private readonly CargoComponent component;
        private readonly Rigidbody player;
        private readonly CinemachineBasicMultiChannelPerlin perlin;
        private readonly FallSequencerSwitcher sequencerSwitcher;
        private readonly CargoShaderWrapper cargoShaderWrapper;

        private bool isMovingBack;
        private Vector3 targetPosition;
        private Tween rotationTween;
        private Sequence damageColorSequence;

        public BackAttackState(CargoComponent component)
        {
            this.component = component;

            player = GameObject.FindWithTag(Tag.Player).GetComponent<Rigidbody>();
            sequencerSwitcher = Object.FindAnyObjectByType<FallSequencerSwitcher>();
            cargoShaderWrapper = new CargoShaderWrapper(component.Renderer.material);
            perlin = component.CineMachinePerlin;
        }

        internal override void OnEnter()
        {
            component.Condition.IsStart = !component.Condition.IsStart;

            sequencerSwitcher.Switch();
            BackAttackAsync().Forget();

            component.Status.OnDamage += HandleDamage;
            component.Status.OnDeath += HandleDeath;
        }


        internal override void OnExit()
        {
            rotationTween?.Kill();
            component.Status.OnDamage -= HandleDamage;
            component.Status.OnDeath -= HandleDeath;
        }

        internal override void Update()
        {
        }

        internal override void UpdatePhysics()
        {
            if (!isMovingBack)
                return;

            MoveBack();

            if (HasReachedTarget(targetPosition))
            {
                isMovingBack = false;
            }
        }

        internal override void Dispose()
        {
        }

        private async UniTaskVoid BackAttackAsync()
        {
            UpdateAnimatorForAttack();

            await UniTask.Delay(TimeSpan.FromSeconds(component.Parameter.AttackDelay), cancellationToken: CancellationToken);

            targetPosition = component.Transform.position + Vector3.forward * component.Parameter.BackAttackDistanceZ;
            isMovingBack = true;

            // 後退が終わるまで待機
            await UniTask.WaitUntil(() => !isMovingBack, cancellationToken: CancellationToken);

            await PerformImpactAsync();

            UpdateAnimatorForIdle();

            await sequencerSwitcher.Current.PlayAsync();

            component.Condition.SwitchState(CargoCondition.State.PrepareMove);
        }

        private void MoveBack()
        {
            var velocity = Vector3.forward * component.Parameter.AttackMoveSpeed;

            component.MoveParent.position += velocity;
            player.MovePosition(player.position + velocity);
            
            component.Condition.MoveDelta = velocity;
        }

        private bool HasReachedTarget(Vector3 target)
        {
            float remainZ = Mathf.Abs(target.z - component.Transform.position.z);
            return remainZ <= component.Parameter.AttackMoveSpeed * 0.5f;
        }

        private void UpdateAnimatorForAttack()
        {
            component.AnimatorWrapper.DirectionX = 0;
            component.AnimatorWrapper.DirectionY = -1;
        }

        private void UpdateAnimatorForIdle()
        {
            component.AnimatorWrapper.DirectionX = 0;
            component.AnimatorWrapper.DirectionY = 0;
        }

        private async UniTask PerformImpactAsync()
        {
            perlin.NoiseProfile = component.BackAttackNoise;
            perlin.AmplitudeGain = component.Parameter.AmplitudeGainOnImpact;
            perlin.FrequencyGain = component.Parameter.FrequencyGainOnImpact;
            perlin.enabled = true;

            SoundManager.instance.Play("打撃6");

            await DOVirtual.Float(1f, 0f, 0.5f, t =>
            {
                perlin.AmplitudeGain = component.Parameter.AmplitudeGainOnImpact * t;
                perlin.FrequencyGain = component.Parameter.FrequencyGainOnImpact * t;
            }).SetEase(Ease.InCirc, 3f);

            perlin.enabled = false;
        }

        private void HandleDeath()
        {
            component.Condition.SwitchState(CargoCondition.State.Death);
        }

        private void HandleDamage(int _)
        {
            if (component.Status.CurrentHealth > 0)
            {
                component.AnimatorWrapper.SetDamageTrigger();
            }

            Color color = component.Parameter.DamageAdditionalColor;
            damageColorSequence?.Kill();
            damageColorSequence = DOTween.Sequence();
            damageColorSequence.Append(DOTween.To(() => Color.black, c => cargoShaderWrapper.AdditionalColor = c, color, 0.1f));
            damageColorSequence.Append(DOTween.To(() => color, c => cargoShaderWrapper.AdditionalColor = c, Color.black, 0.1f));
            damageColorSequence.Play();
        }
    }
}