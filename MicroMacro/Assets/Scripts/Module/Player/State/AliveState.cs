using CoreModule.AI.HSM;
using CoreModule.Input;
using Module.Player.Component;
using PropertyGenerator.Generated;
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
        private readonly Rigidbody rigidbody;
        private readonly Transform bodyTransform;
        private readonly PlayerParameter parameter;
        private readonly PlayerCondition condition;
        private readonly PlayerRotation rotation;
        private readonly PlayerStatus status;
        private readonly WeaponSwitcher weaponSwitcher;
        private readonly PlayerControllerWrapper animatorWrapper;

        private readonly InputEvent moveEvent;
        private readonly InputEvent switchEvent;

        public AliveState(PlayerComponent component)
        {
            parameter = component.Parameter;
            transform = component.Transform;
            rigidbody = component.Rigidbody;
            bodyTransform = component.BodyTransform;
            condition = component.Condition;
            status = component.PlayerStatus;
            rotation = component.PlayerRotation;
            weaponSwitcher = component.WeaponSwitcher;
            animatorWrapper = component.AnimatorWrapper;

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
            UpdateAnimatorDirection(condition.Direction.x);

            switchEvent.Started += OnSwitchWeapon;
            status.OnDamage += OnDamaged;
        }

        internal override void OnExit()
        {
            switchEvent.Started -= OnSwitchWeapon;
            status.OnDamage -= OnDamaged;
        }

        private void OnDamaged(int damage)
        {
            if (status.CurrentHealth == 0)
                return;

            animatorWrapper.SetDamagedTrigger();
        }

        private void UpdateAnimatorDirection(float directionX)
        {
            Vector3 localScale = bodyTransform.localScale;
            float zScale = Mathf.Abs(localScale.z);
            bodyTransform.localScale = new Vector3(localScale.x, localScale.y, directionX > 0 ? zScale : -zScale);
        }

        private void OnSwitchWeapon(InputAction.CallbackContext _)
        {
            weaponSwitcher.Switch();
        }

        internal override void Update()
        {
            // プレイヤーの向きを更新
            UpdateDirectionInput();

            UpdateAnimatorParameter();
        }

        private void UpdateDirectionInput()
        {
            Vector2 moveInput = moveEvent.ReadValue<Vector2>();
            Vector2 direction = rotation.GetDirection(moveInput);

            condition.Direction = direction;

            // 左右の入力の場合は更新
            if (direction.x != 0)
            {
                condition.LastSideInput = direction;
                UpdateAnimatorDirection(direction.x);
            }
        }

        private void UpdateRotation()
        {
            float angle = condition.LastSideInput.x > 0f ? -180f : 0f;
            rigidbody.rotation = Quaternion.Slerp(rigidbody.rotation, Quaternion.Euler(0f, angle, 0f), parameter.RotationSpeed * Time.fixedDeltaTime);
        }

        private void UpdateAnimatorParameter()
        {
            // Animatorに適用
            float paramDir = animatorWrapper.DirectionY;

            if (condition.Direction == Vector2.up)
            {
                paramDir = Mathf.Lerp(paramDir, 1, Time.deltaTime * parameter.VerticalLookSpeed);
            }
            else if (condition.Direction == Vector2.down)
            {
                paramDir = Mathf.Lerp(paramDir, -1, Time.deltaTime * parameter.VerticalLookSpeed);
            }
            else
            {
                paramDir = Mathf.Lerp(paramDir, 0, Time.deltaTime * parameter.VerticalLookSpeed);
            }

            animatorWrapper.DirectionY = paramDir;
        }

        internal override void UpdatePhysics()
        {
            UpdateRotation();
        }

        internal override void Dispose()
        {
            weaponSwitcher.Destroy();
        }
    }
}