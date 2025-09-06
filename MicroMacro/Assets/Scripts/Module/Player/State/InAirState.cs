using Constants;
using CoreModule.AI.HSM;
using CoreModule.Input;
using CoreModule.Utility;
using Module.Player.Component;
using PropertyGenerator.Generated;
using Unity.VisualScripting;
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
        private readonly AnimationClip landingClip;

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
            landingClip = component.LandingClip;

            // 入力イベントを取得
            moveEvent = InputProvider.CreateEvent(ActionGuid.Player.Move);
            jumpEvent = InputProvider.CreateEvent(ActionGuid.Player.Jump);
        }

        internal override void OnEnter()
        {
            animatorWrapper.IsLanding = false;
            animatorWrapper.IsJumping = true;
            isTopStop = false;
            topStopFrameCount = 0;

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
            animatorWrapper.IsJumping = false;
        }

        internal override void UpdatePhysics()
        {
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

            rigidbody.linearVelocity = velocity + externalVelocity;
            condition.ExternalForce = externalVelocity;

            float landingTime = landingClip.length + parameter.LandingTimeOffset;

            if (CanGroundingAgain(landingTime))
            {
                animatorWrapper.IsLanding = true;
            }

            // ジャンプから一定時間経過してから、着地状態を更新
            if (condition.LastJumpTime + parameter.GroundInterval < Time.time)
            {
                UpdateGroundState();
            }
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

            // 着地した場合は、ジャンプ状態を解除
            if (condition.IsGround)
            {
                condition.IsJumping = false;
                animatorWrapper.IsJumping = false;
                animatorWrapper.IsLanding = true;
            }
        }

        private bool CanGroundingAgain(float landingTime)
        {
            // 前のジャンプから一定時間が経過していたらチェック開始
            bool canGrounding = condition.LastJumpTime + parameter.GroundInterval <= Time.time;
            if (!canGrounding)
            {
                return false;
            }

            float yVelocity = Mathf.Min(rigidbody.linearVelocity.y, 0f);
            float g = parameter.GravityOnDown;

            // 着地モーションが間に合う距離を算出
            float detectDistance = -yVelocity * landingTime + 0.5f * g * landingTime * landingTime;

            // 着地モーションが間に合う距離に入ったら着地確定とする
            bool isHit = Physics.Raycast(transform.position, Vector3.down, detectDistance, Layer.Mask.Default);

            return isHit;
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