using UnityEngine;

namespace Module.UI
{
    public class Billboard : MonoBehaviour
    {
        private Camera mainCamera;
        void Start()
        {
            mainCamera = Camera.main;
        }

        void LateUpdate()
        {
            // 常にカメラの方向を向く
            transform.forward = mainCamera.transform.forward;
        
            // 【補足】もし「縦軸（Y軸）だけ回転させたい（看板みたいに）」場合は上記を消して以下を使う
            // transform.rotation = Quaternion.Euler(0, mainCamera.transform.rotation.eulerAngles.y, 0);
        }
    }
}