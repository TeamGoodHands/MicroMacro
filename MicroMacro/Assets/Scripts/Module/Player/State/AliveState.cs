using CoreModule.Input;
using Module.Player.Component;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Module.Player.State
{
    /// <summary>
    /// 生存状態を表すステート
    /// </summary>
    public class AliveState : HierarchicalStateMachine.State
    {
        private readonly Transform transform;
        private readonly PlayerParameter parameter;
        private readonly PlayerCondition condition;
        private readonly PlayerRotation rotation;
        private readonly WeaponSwitcher weaponSwitcher;

        private readonly InputEvent moveEvent;
        private readonly InputEvent switchEvent;

        public AliveState(PlayerComponent component)
        {
            parameter = component.Parameter;
            transform = component.Transform;
            condition = component.Condition;
            rotation = component.PlayerRotation;
            weaponSwitcher = component.WeaponSwitcher;

            // 武器の初期化
            weaponSwitcher.Initialize();

            // 入力イベントの初期化
            moveEvent = InputProvider.CreateEvent(ActionGuid.Player.Move);
            switchEvent = InputProvider.CreateEvent(ActionGuid.Player.SwitchWeapon);
        }

        internal override void OnEnter()
        {
            // はじめは右を向いているとする
            condition.Direction = Vector2.right;
            condition.LastSideInput = Vector2.right;

            moveEvent.Started += UpdateDirection;
            moveEvent.Performed += UpdateDirection;
            moveEvent.Canceled += UpdateDirection;
            switchEvent.Started += OnSwitchWeapon;
        }

        internal override void OnExit()
        {
            moveEvent.Started -= UpdateDirection;
            moveEvent.Performed -= UpdateDirection;
            moveEvent.Canceled -= UpdateDirection;
            switchEvent.Started -= OnSwitchWeapon;
        }

        private void UpdateDirection(InputAction.CallbackContext ctx)
        {
            Vector2 moveInput = ctx.ReadValue<Vector2>();
            Vector2 direction = rotation.GetDirection(moveInput);

            condition.Direction = direction;

            // 左右の入力の場合は更新
            if (direction.x != 0)
            {
                condition.LastSideInput = direction;
            }
        }

        private void OnSwitchWeapon(InputAction.CallbackContext _)
        {
            weaponSwitcher.Switch();
        }

        internal override void Update()
        {
            // プレイヤーの向きを更新
            float angle = condition.LastSideInput.x > 0f ? 0f : -180f;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, angle, 0f), parameter.RotationSpeed * Time.deltaTime);
        }

        internal override void UpdatePhysics()
        {
        }
    }
}