using System;
using Constants;
using CoreModule.AI.HSM;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Module.Management;
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

        private bool isMovingBack;
        private Vector3 targetPosition;
        private Quaternion defaultBodyRotation;
        private Tween rotationTween;

        public BackAttackState(CargoComponent component)
        {
            this.component = component;
            defaultBodyRotation = component.BodyTransform.localRotation;

            player = GameObject.FindWithTag(Tag.Player).GetComponent<Rigidbody>();
            sequencerSwitcher = Object.FindAnyObjectByType<FallSequencerSwitcher>();
            perlin = component.CineMachinePerlin;
        }

        internal override void OnEnter()
        {
            sequencerSwitcher.Switch();
            BackAttackAsync().Forget();

            component.Status.OnDamage += HandleDamage;
        }

        internal override void OnExit()
        {
            rotationTween?.Kill();
            component.Status.OnDamage -= HandleDamage;
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
            await UniTask.Delay(TimeSpan.FromSeconds(component.Parameter.AttackDelay), cancellationToken: CancellationToken);

            UpdateAnimatorForAttack();

            targetPosition = component.Transform.position + Vector3.forward * component.Parameter.BackAttackDistanceZ;
            isMovingBack = true;

            // 後退が終わるまで待機
            await UniTask.WaitUntil(() => !isMovingBack, cancellationToken: CancellationToken);

            await PerformImpactAsync();

            await sequencerSwitcher.Current.PlayAsync();

            component.Condition.CurrentState = CargoCondition.State.PrepareMove;
        }

        private void MoveBack()
        {
            var velocity = Vector3.forward * component.Parameter.AttackMoveSpeed;

            component.MoveParent.position += velocity;
            player.MovePosition(player.position + velocity);
        }

        private bool HasReachedTarget(Vector3 target)
        {
            float remainZ = Mathf.Abs(target.z - component.Transform.position.z);
            return remainZ <= component.Parameter.AttackMoveSpeed * 0.5f;
        }

        private void UpdateAnimatorForAttack()
        {
            component.AnimatorWrapper.Direction = 0f;
        }

        private async UniTask PerformImpactAsync()
        {
            perlin.AmplitudeGain = component.Parameter.AmplitudeGainOnImpact;
            perlin.FrequencyGain = component.Parameter.FrequencyGainOnImpact;
            perlin.enabled = true;

            SoundManager.instance.Play("打撃6");

            var cameraShakeDuration = TimeSpan.FromSeconds(0.5f);
            await UniTask.Delay(cameraShakeDuration, cancellationToken: CancellationToken);

            perlin.enabled = false;
        }

        private void HandleDamage(int _)
        {
            component.AnimatorWrapper.SetDamageTrigger();
            PlayDamageTween();
        }

        private void PlayDamageTween()
        {
            rotationTween?.Kill();
            rotationTween = component.BodyTransform
                .DOShakeRotation(0.5f, new Vector3(5f, 0f, 0f), 35)
                .OnComplete(() => component.BodyTransform.localRotation = defaultBodyRotation);
        }
    }
}