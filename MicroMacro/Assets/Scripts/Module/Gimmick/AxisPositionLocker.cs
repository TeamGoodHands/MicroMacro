using UnityEngine;

namespace Module.Gimmick
{
    public class AxisPositionLocker : MonoBehaviour
    {
        [Header("固定したい軸にチェックを入れる")]
        [SerializeField] private bool lockX = true;
        [SerializeField] private bool lockY = true;
        [SerializeField] private bool lockZ = true;

        // 初期位置を保存する変数
        private Vector3 initialPosition;

        void Start()
        {
            // ゲーム開始時の位置を記憶
            initialPosition = transform.position;
        }

        void LateUpdate()
        {
            // 現在の位置を取得
            Vector3 currentPos = transform.position;

            // チェックが入っている軸は「初期位置」を、入っていない軸は「現在の位置」を採用
            float x = lockX ? initialPosition.x : currentPos.x;
            float y = lockY ? initialPosition.y : currentPos.y;
            float z = lockZ ? initialPosition.z : currentPos.z;

            // 位置を強制的に適用
            transform.position = new Vector3(x, y, z);
        }
    }
}