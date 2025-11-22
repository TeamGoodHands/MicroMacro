using NaughtyAttributes;
using UnityEngine;

namespace Module.Player.Component
{
    /// <summary>
    /// プレイヤーの動的なパラメータを保持するクラス
    /// </summary>
    public class PlayerCondition : MonoBehaviour
    {
        [SerializeField, ReadOnly] private float lastJumpTime;
        [SerializeField, ReadOnly] private Vector2 externalForce;
        [SerializeField, ReadOnly] private Vector2 externalWeaponForce;
        [SerializeField, ReadOnly] private bool isJumping;
        [SerializeField, ReadOnly] private bool isGround;
        [SerializeField, ReadOnly] private bool isRiding;
        [SerializeField, ReadOnly] private bool isPlayerLocked;
        [SerializeField, ReadOnly] private Vector2 direction = Vector2.right;
        [SerializeField, ReadOnly] private Vector2 lastSideInput = Vector2.right;

        public float LastJumpTime
        {
            get => lastJumpTime;
            set => lastJumpTime = value;
        }

        public Vector2 ExternalForce
        {
            get => externalForce;
            set => externalForce = value;
        }
        
        public Vector2 ExternalWeaponForce
        {
            get => externalForce;
            set => externalForce = value;
        }

        public bool IsJumping
        {
            get => isJumping;
            set => isJumping = value;
        }

        public bool IsPlayerLocked
        {
            get => isPlayerLocked;
            set => isPlayerLocked = value;
        }

        public bool IsGround
        {
            get => isGround;
            set => isGround = value;
        }
        
        public bool IsRiding
        {
            get => isRiding;
            set => isRiding = value;
        }

        public Vector2 Direction
        {
            get => direction;
            set => direction = value;
        }

        public Vector2 LastSideInput
        {
            get => lastSideInput;
            set => lastSideInput = value;
        }
    }
}