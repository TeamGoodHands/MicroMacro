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

        [SerializeField, Header("追従速度"), Min(0f)]
        private float followSpeed = 5f;

        [SerializeField, Header("上部のカメラ制限のしきい値"), Range(0f, 1f)]
        private float upSideDeadZone = 0.75f;

        [SerializeField, Header("下部のカメラ制限のしきい値"), Range(0f, 1f)]
        private float downSideDeadZone = 0.25f;

        private float targetY;
        private Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main;
            targetY = transform.position.y;
        }

        private void FixedUpdate()
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

            // 現在の位置を取得
            Vector3 newPosition = transform.position;
            // X座標のみを追従
            newPosition.x = Mathf.Lerp(newPosition.x, target.position.x, followSpeed);
            // 位置を更新
            transform.position = new Vector3(newPosition.x, targetY, newPosition.z);
        }
    }
}