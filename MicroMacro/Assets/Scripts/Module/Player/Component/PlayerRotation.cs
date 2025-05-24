using CoreModule.Input;
using UnityEngine;

namespace Module.Player.Component
{
    /// <summary>
    /// プレイヤーの回転を制御するクラス
    /// </summary>j
    public class PlayerRotation
    {
        private readonly PlayerParameter parameter;
        private readonly PlayerCondition condition;

        public PlayerRotation(PlayerParameter parameter, PlayerCondition condition)
        {
            this.parameter = parameter;
            this.condition = condition;
        }

        public Vector2 GetDirection(Vector2 moveInput)
        {
            // ゼロ入力の場合は前回の入力を使用
            if (moveInput == Vector2.zero)
            {
                return condition.LastSideInput;
            }

            // 縦方向のデッドゾーンを超えている & 指定角度内に収まっている場合はその方向に向く
            if (moveInput.y > parameter.VerticalDeadZone && IsVerticalWithin(moveInput.normalized, Vector2.up))
            {
                return Vector2.up;
            }

            if (moveInput.y < -parameter.VerticalDeadZone && IsVerticalWithin(moveInput.normalized, Vector2.down))
            {
                return Vector2.down;
            }

            // 上下に向いていない場合は左右の入力を使用
            return moveInput.x > 0f ? Vector2.right : Vector2.left;
        }

        private bool IsVerticalWithin(Vector2 a, Vector2 b)
        {
            // 垂直方向の有効角度を計算
            return Vector2.Dot(a, b) >= Mathf.Cos(parameter.VerticalValidAngle * Mathf.Deg2Rad);
        }
    }
}