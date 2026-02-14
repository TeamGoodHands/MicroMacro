using Constants;
using CoreModule.AI.HSM;
using CoreModule.Input;
using CoreModule.Utility;
using Module.Player.Component;
using PropertyGenerator.Generated;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Module.Player.State
{
    /// <summary>
    /// 空中にいる状態を表すステート
    /// </summary>
    public class InAirState : HierarchicalStateMachine.State
    {
        private readonly Transform transform;
        private readonly Rigidbody rigidbody;
        private readonly PlayerParameter parameter;
        private readonly PlayerCondition condition;
        private readonly PlayerMovement movement;
        private readonly PlayerControllerWrapper animatorWrapper;

        private readonly InputEvent jumpEvent;
        private readonly InputEvent moveEvent;

        private Vector2 moveInput;
        private bool isTopStop;
        private int topStopFrameCount;

        public InAirState(PlayerComponent component)
        {
            transform = component.Transform;
            rigidbody = component.Rigidbody;
            parameter = component.Parameter;
            condition = component.Condition;
            movement = component.PlayerMovement;
            animatorWrapper = component.AnimatorWrapper;

            // 入力イベントを取得
            moveEvent = InputProvider.CreateEvent(ActionGuid.Player.Move);
            jumpEvent = InputProvider.CreateEvent(ActionGuid.Player.Jump);
        }

        internal override void OnEnter()
        {
            isTopStop = false;
            topStopFrameCount = condition.Direction.y < 0f ? parameter.TopStopFrameCount : 0; // 下入力がある場合は頂点停止を無効化

            jumpEvent.Canceled += CancelJump;
        }

        internal override void OnExit()
        {
            jumpEvent.Canceled -= CancelJump;
        }

        internal override void Update()
        {
            moveInput = moveEvent.ReadValue<Vector2>();
        }

        private void CancelJump(InputAction.CallbackContext _)
        {
            // ジャンプ入力無くなった場合はキャンセル
            condition.IsJumping = false;
        }

        internal override void UpdatePhysics()
        {
            if (condition.IsPlayerLocked)
                return;
            
            Vector2 velocity = rigidbody.linearVelocity;
            Vector2 externalVelocity = condition.ExternalForce;

            // 重力を適用
            ApplyGravity(ref velocity);

            movement.PerformMovement(moveInput.x, false, ref velocity); // 移動速度を適用
            movement.PerformDamping(ref velocity); // 速度減衰を適用
            movement.PerformExternalDamping(ref externalVelocity); // 外部力への減衰を適用

            // ジャンプ中は、空中で追加ジャンプ力を適用
            if (condition.IsJumping)
            {
                PerformAdditionalJump(ref velocity);
            }

            Vector2 linearVelocity = velocity + externalVelocity;
            ClampJumpPower(ref linearVelocity);

            rigidbody.linearVelocity = linearVelocity;
            condition.ExternalForce = externalVelocity;

            // ジャンプから一定時間経過してから、着地状態を更新
            if (condition.LastJumpTime + parameter.GroundInterval < Time.time)
            {
                UpdateGroundState();
            }
        }

        private void ClampJumpPower(ref Vector2 velocity)
        {
            velocity.y = Mathf.Min(velocity.y, parameter.MaxSpeedY);
        }

        private void ApplyGravity(ref Vector2 velocity)
        {
            // 上昇から下降状態に来た時、空中停止フラグを有効にする
            if (!isTopStop && velocity.y < 0f)
            {
                isTopStop = true;
            }

            // ジャンプの頂点に来た時、数フレームだけ重力を無くす
            if (isTopStop && topStopFrameCount < parameter.TopStopFrameCount)
            {
                topStopFrameCount++;
                velocity.y = Mathf.Max(velocity.y, 0f); // 下向き速度をゼロ以上にクランプ
                return;
            }

            velocity.y += velocity.y < 0f ? parameter.GravityOnDown : parameter.GravityOnUp;
        }

        internal override void Dispose()
        {
        }

        private void UpdateGroundState()
        {
            // 着地状態を更新
            condition.IsGround = movement.IsGround(transform);
            animatorWrapper.IsGround = condition.IsGround;

            // 着地した場合は、ジャンプ状態を解除
            if (condition.IsGround)
            {
                condition.IsJumping = false;
                animatorWrapper.IsJumping = false;
            }
        }

        private void PerformAdditionalJump(ref Vector2 velocity)
        {
            // ジャンプ中の追加の力を加える
            float jumpTime = Time.time - condition.LastJumpTime;
            float additionalPower = parameter.AdditionalJumpPower.Evaluate(jumpTime);
            velocity += new Vector2(0f, additionalPower);
        }
    }
}