using CoreModule.Input;
using Module.Player.Component;
using UnityEngine;
using UnityEngine.InputSystem;

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
        private readonly Rigidbody rigidbody;
        private readonly Transform transform;

        private readonly InputEvent moveEvent;
        private readonly InputEvent jumpEvent;

        private Vector2 moveInput;

        public GroundState(PlayerComponent component)
        {
            condition = component.Condition;
            parameter = component.Parameter;
            movement = component.PlayerMovement;
            rigidbody = component.Rigidbody;
            transform = component.Transform;

            // 入力イベントを取得
            moveEvent = InputProvider.CreateEvent(ActionGuid.Player.Move);
            jumpEvent = InputProvider.CreateEvent(ActionGuid.Player.Jump);
        }

        internal override void OnEnter()
        {
            // ジャンプイベントを登録
            jumpEvent.Started += OnJump;
        }

        internal override void OnExit()
        {
            // ジャンプイベントを解除
            jumpEvent.Started -= OnJump;
        }

        internal override void Update()
        {
            moveInput = moveEvent.ReadValue<Vector2>();
        }

        internal override void UpdatePhysics()
        {
            Vector2 velocity = rigidbody.linearVelocity;
            Vector2 externalVelocity = condition.ExternalForce;
            
            velocity.y += parameter.Gravity; // 重力を加算

            movement.PerformMovement(moveInput.x, ref velocity); // 移動速度を適用
            movement.PerformDamping(true, ref velocity); // 速度減衰を適用
            movement.PerformExternalDamping(ref externalVelocity); // 外部力への減衰を適用

            rigidbody.linearVelocity = velocity + externalVelocity;
            condition.ExternalForce = externalVelocity;

            // 着地状態を更新
            condition.IsGround = movement.IsGround(transform);
        }

        private void OnJump(InputAction.CallbackContext _)
        {
            // プレイヤーにかかった重力をリセット
            rigidbody.linearVelocity = new Vector2(rigidbody.linearVelocity.x, 0f);

            // 上方向に力を加える
            rigidbody.AddForce(new Vector2(0f, parameter.JumpPower), ForceMode.Impulse);
            condition.IsGround = false;
            condition.IsJumping = true;
            condition.JumpStartTime = Time.time;
        }
    }
}