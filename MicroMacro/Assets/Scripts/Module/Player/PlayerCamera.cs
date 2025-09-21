using UnityEngine;

namespace Module.Player
{
    /// <summary>
    /// プレイヤーのカメラを制御するクラス
    /// </summary>
    public class PlayerCamera : MonoBehaviour
    {
        [SerializeField, Header("追従対象(プレイヤー)")]
        private Transform target;
        
        [SerializeField,Header("X方向のオフセット")]
        private float offsetX;

        [SerializeField, Header("目標までの到達時間"), Min(0f)]
        private float smoothTime = 1f;

        [SerializeField, Header("上部のカメラ制限のしきい値"), Range(0f, 1f)]
        private float upSideDeadZone = 0.75f;

        [SerializeField, Header("下部のカメラ制限のしきい値"), Range(0f, 1f)]
        private float downSideDeadZone = 0.25f;

        [SerializeField, Header("カメラをロックするか")]
        private bool lockState;

        [SerializeField] private float offsetZ;

        private float targetY;
        private float velocityX;
        private float velocityZ;
        private Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main;
            targetY = transform.position.y;
            offsetZ = target.position.z - mainCamera.transform.position.z;
        }

        private void FixedUpdate()
        {
            if (lockState)
                return;
            
            ClampVerticalPosition();
            SmoothPosition();
        }

        private void ClampVerticalPosition()
        {
            Vector2 screenPoint = mainCamera.WorldToViewportPoint(target.position);

            if (screenPoint.y > upSideDeadZone)
            {
                // 上部のカメラ制限のしきい値を超えた場合
                targetY += (screenPoint.y - upSideDeadZone) * mainCamera.orthographicSize * 0.5f;
            }
            else if (screenPoint.y < downSideDeadZone)
            {
                // 下部のカメラ制限のしきい値を超えた場合
                targetY -= (downSideDeadZone - screenPoint.y) * mainCamera.orthographicSize * 0.5f;
            }
        }

        private void SmoothPosition()
        {
            // 現在の位置を取得
            Vector3 newPosition = transform.position;
            
            // X, Z座標のみを追従
            newPosition.x = Mathf.SmoothDamp(newPosition.x, target.position.x + offsetX, ref velocityX, smoothTime, Mathf.Infinity, Time.fixedDeltaTime);
            newPosition.z = Mathf.SmoothDamp(newPosition.z, target.position.z - offsetZ, ref velocityZ, smoothTime, Mathf.Infinity,
                Time.fixedDeltaTime);
            
            // 位置を更新
            transform.position = new Vector3(newPosition.x, targetY, newPosition.z);
        }

        public void SetLockState(bool lockState)
        {
            this.lockState = lockState;
        }
    }
}