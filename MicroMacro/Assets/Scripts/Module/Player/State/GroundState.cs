using CoreModule.AI.HSM;
using CoreModule.Input;
using Module.Management;
using Module.Player.Component;
using PropertyGenerator.Generated;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

namespace Module.Player.State
{
    /// <summary>
    /// 地面にいる状態を表すステート
    /// </summary>
    public class GroundState : HierarchicalStateMachine.State
    {
        private readonly PlayerCondition condition;
        private readonly PlayerParameter parameter;
        private readonly PlayerMovement movement;
        private readonly PlayerControllerWrapper animatorWrapper;
        private readonly Rigidbody rigidbody;
        private readonly Transform transform;
        private readonly VisualEffect footEffect;
        private readonly VisualEffect jumpEffect;
        private readonly PlayerAnimationEventReceiver animationEventReceiver;

        private readonly InputEvent moveEvent;
        private readonly InputEvent jumpEvent;
        
        private static readonly int footDirectionId = Shader.PropertyToID("Direction");

        private Vector2 moveInput;
        private float footStepTimer;
        private bool isPlayingFootstep;

        public GroundState(PlayerComponent component)
        {
            condition = component.Condition;
            parameter = component.Parameter;
            movement = component.PlayerMovement;
            rigidbody = component.Rigidbody;
            transform = component.Transform;
            footEffect = component.FootEffect;
            jumpEffect = component.JumpEffect;

            animatorWrapper = component.AnimatorWrapper;
            animationEventReceiver = component.AnimationEventReceiver;

            // 入力イベントを取得
            moveEvent = InputProvider.CreateEvent(ActionGuid.Player.Move);
            jumpEvent = InputProvider.CreateEvent(ActionGuid.Player.Jump);
        }

        internal override void OnEnter()
        {
            // ジャンプイベントを登録
            jumpEvent.Started += OnJump;

            footStepTimer = 0f;
            animationEventReceiver.OnWalk += PlayFootEffect;
        }


        internal override void OnExit()
        {
            // ジャンプイベントを解除
            jumpEvent.Started -= OnJump;
            animationEventReceiver.OnWalk -= PlayFootEffect;
        }

        internal override void Update()
        {
            moveInput = moveEvent.ReadValue<Vector2>();
            PlayFootSound();
        }

        internal override void UpdatePhysics()
        {
            Vector2 velocity = rigidbody.linearVelocity;
            Vector2 externalVelocity = condition.ExternalForce;

            velocity.y += parameter.GravityOnDown; // 重力を加算

            movement.PerformMovement(moveInput.x, true, ref velocity); // 移動速度を適用
            movement.PerformDamping(ref velocity); // 速度減衰を適用
            movement.PerformExternalDamping(ref externalVelocity); // 外部力への減衰を適用

            // RigidBodyに適用
            rigidbody.linearVelocity = velocity + externalVelocity;
            condition.ExternalForce = externalVelocity;

            // 着地状態を更新
            condition.IsGround = movement.IsGround(transform);
            animatorWrapper.IsGround = condition.IsGround;

            float normalizedSpeed = CalculateNormalizedSpeed();
            animatorWrapper.Speed = normalizedSpeed;
        }

        internal override void Dispose()
        {
        }

        private void PlayFootSound()
        {
            float speed = Mathf.Abs(rigidbody.linearVelocity.x);

            // スピードが極端に小さい場合は無視
            if (speed <= 0.1f)
                return;

            // タイマーを減らす
            footStepTimer -= Time.deltaTime;

            if (footStepTimer <= 0f)
            {
                // 鳴らす間隔を計算する
                float interval = parameter.FootstepSoundInterval * (parameter.FootstepSoundSpeed / speed);
                interval = Mathf.Min(interval, parameter.FootstepMaxInterval);

                SoundManager.instance.Play("足音");
                footStepTimer = interval;
            }
        }

        private void PlayFootEffect()
        {
            int direction = (int)Mathf.Sign(rigidbody.linearVelocity.x) >= 0 ? 0 : 1;
            float speed = Mathf.Abs(rigidbody.linearVelocity.x);

            if (speed <= 0.1f)
                return;

            footEffect.SetInt(footDirectionId, direction);

            footEffect.Play();
        }

        private void PlayJumpEffect()
        {
            jumpEffect.Play();
        }

        /// <summary>
        /// 最大速度で正規化した現在の速度を返します。
        /// </summary>
        private float CalculateNormalizedSpeed()
        {
            float xVelocity = Mathf.Abs(rigidbody.linearVelocity.x);
            float maxSpeed = parameter.MaxSpeedX;

            return Mathf.Clamp01(xVelocity / maxSpeed);
        }

        private void OnJump(InputAction.CallbackContext _)
        {
            // プレイヤーにかかった重力をリセット
            rigidbody.linearVelocity = new Vector2(rigidbody.linearVelocity.x, 0f);

            // 上方向に力を加える
            rigidbody.AddForce(new Vector2(0f, parameter.JumpPower), ForceMode.Impulse);
            condition.IsGround = false;
            animatorWrapper.IsGround = false;
            condition.IsJumping = true;
            condition.LastJumpTime = Time.time;

            animatorWrapper.IsJumping = true;
            PlayJumpEffect();
            SoundManager.instance.Play("ジャンプ");
        }
    }
}