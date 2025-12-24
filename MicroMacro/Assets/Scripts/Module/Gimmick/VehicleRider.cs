using System;
using Constants;
using CoreModule.Input;
using Module.Player.Component;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Module.Gimmick
{
    /// <summary>
    /// 乗り物の乗り降りを管理するクラス
    /// </summary>
    public class VehicleRider : MonoBehaviour
    {
        [SerializeField, Header("乗り物を降りるときのオフセット")] private float dismountOffset;
        [SerializeField] private Transform ridePivot;
        [SerializeField] private Rigidbody rigidBody;

        public event Action OnRide;
        public event Action OnDismount;
        public bool IsRiding => isRiding;

        private Rigidbody playerRigidbody;
        private PlayerCondition playerCondition;
        private InputEvent jumpEvent;
        private bool isRiding;

        private void Start()
        {
            // ジャンプボタン押したら降りる
            jumpEvent = InputProvider.CreateEvent(ActionGuid.Player.Jump);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!enabled)
                return; 
            
            // プレイヤーがトリガーに触れたら乗る
            if (other.CompareTag(Tag.Handle.Player) && other.TryGetComponent(out PlayerCondition condition))
            {
                playerCondition = condition;
                HandleRide();
            }
        }

        public void Ride()
        {
            isRiding = true;
            playerRigidbody = playerCondition.GetComponent<Rigidbody>();
            playerCondition.IsRiding = true;

            // ジャンプボタンのイベントを登録
            jumpEvent.Started += HandleDismount;

            OnRide?.Invoke();
        }

        private void HandleRide()
        {
            Ride();
        }

        public void Dismount()
        {
            isRiding = false;
            playerCondition.IsRiding = false;

            // 乗り物からは少しズレた位置に下ろす
            playerRigidbody.position += Vector3.right * dismountOffset;

            // ジャンプボタンのイベントを解除
            jumpEvent.Started -= HandleDismount;

            OnDismount?.Invoke();
        }

        private void HandleDismount(InputAction.CallbackContext _)
        {
            Dismount();
        }

        private void FixedUpdate()
        {
            // 乗ってる最中はridePivotに強制的に座標を合わせる
            if (isRiding)
            {
                playerRigidbody.position = ridePivot.position;
                playerCondition.ExternalWeaponForce = rigidBody.linearVelocity;
            }
        }

        private void OnDestroy()
        {
            jumpEvent?.Clear();
        }
    }
}