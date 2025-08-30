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
    public class BackAttackState : HierarchicalStateMachine.State
    {
        private readonly CargoComponent component;
        private readonly Rigidbody player;
        private readonly CinemachineBasicMultiChannelPerlin perlin;
        private readonly FallSequencerSwitcher sequencerSwitcher;
        private bool doMoveBack;
        private Vector3 targetPosition;
        private Quaternion defaultRotation;
        private Tween rotationTween;

        public BackAttackState(CargoComponent component)
        {
            this.component = component;
            defaultRotation = component.BodyTransform.localRotation;

            player = GameObject.FindWithTag(Tag.Player).GetComponent<Rigidbody>();
            sequencerSwitcher = Object.FindAnyObjectByType<FallSequencerSwitcher>();
            perlin = component.CinemachineCamera
                .GetCinemachineComponent(CinemachineCore.Stage.Noise)
                .GetComponent<CinemachineBasicMultiChannelPerlin>();
        }

        internal override void OnEnter()
        {
            sequencerSwitcher.Switch();
            BackAttack().Forget();
            component.Status.OnDamage += OnDamage;
        }

        private async UniTaskVoid BackAttack()
        {
            float attackDelay = component.Parameter.AttackDelay;

            await UniTask.Delay(TimeSpan.FromSeconds(attackDelay), cancellationToken: CancellationToken);

            UpdateDirection();

            doMoveBack = true;
            targetPosition = component.Transform.position + new Vector3(0, 0, component.Parameter.BackAttackDistanceZ);

            await UniTask.WaitUntil(() => doMoveBack == false, cancellationToken: CancellationToken);

            await PerformImpact();

            await sequencerSwitcher.Current.DoSequence();

            component.Condition.CurrentState = CargoCondition.State.PrepareMove;
        }

        internal override void OnExit()
        {
            component.Status.OnDamage -= OnDamage;
        }

        internal override void Update()
        {
        }

        internal override void UpdatePhysics()
        {
            if (!doMoveBack)
                return;

            MoveBack();

            if (IsTargetReached(targetPosition))
            {
                doMoveBack = false;
            }
        }

        private void MoveBack()
        {
            Rigidbody moveParent = component.MoveParent;
            Vector3 velocity = new Vector3(0, 0, component.Parameter.AttackMoveSpeed);

            Vector3 position = moveParent.position;
            position += velocity;
            moveParent.position = position;

            // プレイヤーのRigidBodyも更新する
            player.MovePosition(player.position + velocity);
        }

        private bool IsTargetReached(Vector3 targetPosition)
        {
            Vector3 position = component.Transform.position;
            Vector3 diff = targetPosition - position;
            float distance = Mathf.Abs(diff.z);

            return distance <= component.Parameter.AttackMoveSpeed * 0.5f;
        }

        private void UpdateDirection()
        {
            component.AnimatorWrapper.Direction = 0f;
        }

        private void OnDamage(int damage)
        {
            component.AnimatorWrapper.SetDamageTrigger();

            // 仮ダメージアニメーション
            rotationTween?.Complete();
            rotationTween?.Kill();
            rotationTween = component.BodyTransform.DOShakeRotation(0.5f, new Vector3(5f, 0f, 0f), 35).OnComplete(() =>
            {
                component.BodyTransform.localRotation = defaultRotation;
            });
        }

        private async UniTask PerformImpact()
        {
            perlin.AmplitudeGain = component.Parameter.AmplitudeGainOnImpact;
            perlin.FrequencyGain = component.Parameter.FrequencyGainOnImpact;
            perlin.enabled = true;

            SoundManager.instance.Play("打撃6");
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: CancellationToken);

            perlin.enabled = false;
        }

        internal override void Dispose()
        {
        }
    }
}