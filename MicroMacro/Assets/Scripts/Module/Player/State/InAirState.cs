using CoreModule.Input;
using Module.Player.Component;
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

        private readonly InputEvent jumpEvent;
        private readonly InputEvent moveEvent;

        private Vector2 moveInput;

        public InAirState(PlayerComponent component)
        {
            transform = component.Transform;
            rigidbody = component.Rigidbody;
            parameter = component.Parameter;
            condition = component.Condition;
            movement = component.PlayerMovement;

            // 入力イベントを取得
            moveEvent = InputProvider.CreateEvent(ActionGuid.Player.Move);
            jumpEvent = InputProvider.CreateEvent(ActionGuid.Player.Jump);
        }

        internal override void OnEnter()
        {
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
            Vector2 velocity = rigidbody.linearVelocity;
            Vector2 externalVelocity = condition.ExternalForce;

            velocity.y += parameter.Gravity; // 重力を加算

            movement.PerformMovement(moveInput.x, ref velocity); // 移動速度を適用
            movement.PerformDamping(false, ref velocity); // 速度減衰を適用
            movement.PerformExternalDamping(ref externalVelocity); // 外部力への減衰を適用

            // ジャンプ中は、空中で追加ジャンプ力を適用
            if (condition.IsJumping)
            {
                PerformAdditionalJump(ref velocity);
            }

            rigidbody.linearVelocity = velocity + externalVelocity;
            condition.ExternalForce = externalVelocity;

            // ジャンプから一定時間経過してから、着地状態を更新
            if (condition.JumpStartTime + parameter.GroundInterval < Time.time)
            {
                UpdateGroundState();
            }
        }

        private void UpdateGroundState()
        {
            // 着地状態を更新
            condition.IsGround = movement.IsGround(transform);

            // 着地した場合は、ジャンプ状態を解除
            if (condition.IsGround)
            {
                condition.IsJumping = false;
            }
        }

        private void PerformAdditionalJump(ref Vector2 velocity)
        {
            // ジャンプ中の追加の力を加える
            float jumpTime = Time.time - condition.JumpStartTime;
            float additionalPower = parameter.AdditionalJumpPower.Evaluate(jumpTime);
            velocity += new Vector2(0f, additionalPower);
        }
    }
}