using System;
using UnityEngine;

namespace Module.Scaling
{
    public class ScaleStackSensor : MonoBehaviour
    {
        
        [SerializeField, Header("Rayの長さ")] private float rayLength = 10f;
        private GameObject player;
        private Vector2 trig2Player;
        
        private void Start()
        {
          
        }

        private void Update()
        {
            // Rayを発射して、衝突したオブジェクトの情報を取得
            Ray ray = new Ray(transform.position, transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, rayLength))
            {
                // 衝突したオブジェクトの名前を表示
                Debug.Log($"衝突したオブジェクト: {hitInfo.collider.gameObject.name}");
            }
            else
            {
                Debug.Log("何も衝突しませんでした。");
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
          
            if (other.CompareTag("Player"))
            {
                player = other.gameObject;
                trig2Player = (player.transform.position - transform.position).normalized;
            }
        
            Debug.Log($"プレイヤーとトリガーの距離: {trig2Player.magnitude}");
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
    }
}