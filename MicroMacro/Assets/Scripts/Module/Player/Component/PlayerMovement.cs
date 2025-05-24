using Constants;
using UnityEngine;

namespace Module.Player.Component
{
    /// <summary>
    /// プレイヤーの移動を制御するクラス
    /// </summary>
    public class PlayerMovement
    {
        private readonly PlayerParameter parameter;
        private readonly PlayerCondition condition;

        public PlayerMovement(PlayerParameter parameter, PlayerCondition condition)
        {
            this.parameter = parameter;
            this.condition = condition;
        }

        /// <summary>
        /// velocityに対して移動速度を適用します
        /// </summary>
        public void PerformMovement(float inputX, ref Vector2 velocity)
        {
            // 上下方向を向いている場合は移動しない
            if (condition.Direction.y != 0f)
                return;

            // x方向に力を与える
            float vx = inputX * parameter.MoveAccel;
            velocity.x += vx;

            // Rigidbodyの速度を設定する
            velocity.x = Mathf.Clamp(velocity.x, -parameter.MaxSpeed, parameter.MaxSpeed);
        }

        /// <summary>
        /// velocityに対して速度減衰を適用します
        /// </summary>
        public void PerformDamping(bool isGround, ref Vector2 velocity)
        {
            // 空中状態の抵抗力を適用
            float airControl = isGround ? 1f : parameter.AirControl;

            velocity *= parameter.Damping * airControl;
        }

        /// <summary>
        /// 外力に対して速度減衰を適用します
        /// </summary>
        public void PerformExternalDamping(ref Vector2 externalForce)
        {
            externalForce *= parameter.ExternalDamping;

            if (externalForce.sqrMagnitude < 0.01f)
            {
                externalForce = Vector2.zero;
            }
        }

        /// <summary>
        /// 着地判定を行います
        /// </summary>
        public bool IsGround(Transform transform)
        {
            // プレイヤーは地面に含めない
            const int mask = ~Layer.Mask.Player;

            // 下方向にボックスキャストを行い着地判定
            return Physics.BoxCast(
                transform.position,
                transform.localScale * 0.5f,
                -transform.up,
                Quaternion.identity,
                parameter.CheckGroundDistance,
                mask);
        }

        /// <summary>
        /// 外部からの力を加えます
        /// </summary>
        public void AddExternalForce(Vector2 externalForce)
        {
            condition.ExternalForce += externalForce;
        }
    }
}