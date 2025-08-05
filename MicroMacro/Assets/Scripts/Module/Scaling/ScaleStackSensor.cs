using System;
using UnityEngine;

namespace Module.Scaling
{
    public class ScaleStackSensor : MonoBehaviour
    {
        // RayLengthはそれぞれコライダーの半径＋プレイヤーの直径
        struct RayLength
        {
            public float x;
            public float y;
        }

        private RayLength rayLength;
        private readonly RaycastHit[] hitInfo = new RaycastHit[5];   // RayCastで得た情報を格納する配列
        private Vector2 rayDirection;                                // Rayを飛ばす方向
        
        private Collider playerCol;
        private Collider trigger;

        private bool isStack = true;

        private void Start()
        {
            playerCol = GameObject.FindWithTag("Player").GetComponent<Collider>();
            trigger = gameObject.GetComponent<Collider>();
            CalcRayLength();
        }
        
        /// <summary>
        /// 飛ばすRayの長さを計算するクラス。Triggerの中心から半径＋プレイヤーの幅
        /// </summary>
        private void CalcRayLength()
        {
            // bounds.extents 中心から各面までの距離(Scaleも考慮されるのでRadiusより適している)
            rayLength.x = trigger.bounds.extents.x;
            rayLength.y = trigger.bounds.extents.y;
            
            // プレイヤーのコライダーの半径を取得
            float playerRadius = playerCol.bounds.extents.x; // x軸方向の半径を使用
            
            // RayLengthを更新
            rayLength.x += playerRadius;
            rayLength.y += playerRadius;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                // トリガーからプレイヤーへの方向ベクトル
                Vector2 trig2Player = (playerCol.transform.position - transform.position).normalized;
                
                rayDirection = Normalize4Direction(trig2Player);
            }
        }

        /// <summary>
        /// ベクトルを絶対値で比較し上下左右で一番近いdirectionを返す
        /// 例: (0.5, 1) -> (0, 1)    
        /// </summary>
        /// <param name="direction"></param>
        private Vector2 Normalize4Direction(Vector2 direction)
        {
            float absX = Mathf.Abs(direction.x);
            float absY = Mathf.Abs(direction.y);
            if (absX > absY)
            {
                if (direction.x > 0)
                    return Vector2.right; // 右
                else
                    return Vector2.left; // 左
            }

            if (absX < absY)
            {
                if (direction.y > 0)
                    return Vector2.up; // 上
                else
                    return Vector2.down; // 下
            }

            // 角の時どうするか要検討
            Debug.LogWarning($"無効なdirectionが渡されました: {direction}");
            return Vector2.zero;
        }

        private void ShootRay(Vector2 direction)
        {
            float _rayLength = 0f;
            if (rayDirection == Vector2.up || rayDirection == Vector2.down)
            {
                _rayLength = rayLength.y; 
            }
            else
            {
                _rayLength = rayLength.x;
            }

            
            Ray ray = new Ray(transform.position, direction);
            if (Physics.RaycastNonAlloc(ray, hitInfo, _rayLength) > 0)
            {
                // 衝突したオブジェクトの名前を表示
               

                foreach (RaycastHit hit in hitInfo)
                {
                    if (hit.collider.CompareTag("Untagged"))
                    {
                        Debug.Log($"衝突したオブジェクト: {hit.collider.gameObject.name}");
                    }
                }
            }
        }
    }
}