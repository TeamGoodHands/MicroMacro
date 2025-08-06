using System;
using UnityEngine;

namespace Module.Scaling
{
    /// <summary>
    /// オブジェクトを拡大した時、プレイヤーが挟まって動けなくなる場合はキャンセル
    /// </summary>
    public class ScaleStackSensor : MonoBehaviour
    {
        
        [SerializeField, Header("基のオブジェクトのスケーラー")] private Scaler refScaler;
        [SerializeField, Header("このオブジェクトのスケーラー")] private Scaler scaler;
        // RayLengthはそれぞれコライダーの半径＋プレイヤーの直径
        struct RayLength
        {
            public float x;
            public float y;
        }

        struct PlayerRadius
        {
            public float x; 
            public float y;
        }

        private RayLength rayLength;
        private PlayerRadius playerSize;
        private readonly RaycastHit[] hitInfo = new RaycastHit[5];   // RayCastで得た情報を格納する配列
        private Vector2 rayDirection;                                // Rayを飛ばす方向
        
        private Collider playerCol;
        private Collider trigger;

        private bool isStack = false;

        private void Awake()
        {
            refScaler.OnScaleStarted += OnScaleStarted;
            scaler.OnScaleCompleted  += OnScaleCompleted;
        }

        private void Start()
        {
            playerCol = GameObject.FindWithTag("Player").GetComponent<Collider>();
            trigger = gameObject.GetComponent<Collider>();
            
            // プレイヤーのコライダーの直径を取得 (colliderの種類変更にも対応)
            playerSize.x = playerCol.bounds.size.x;
            playerSize.y = playerCol.bounds.size.y;
            
            CalcRayLength();
        }
        private void OnScaleStarted(ScaleEventArgs args)
        {
            if (isStack)
            { 
                // 拡大の時のみスケール中止
                int def = args.CurrentStep - args.PreviousStep;
                if (def > 0)
                {
                    Debug.Log("スケールがキャンセルされました。");
                    refScaler.CancelScale();
                }
            }
        }
        
        private void OnScaleCompleted(ScaleEventArgs args)
        {
            // サイズ変動したらRayLengthを再計算
            CalcRayLength();
        }
        
        private void OnDestroy()
        {
            refScaler.OnScaleStarted -= OnScaleStarted;
            scaler.OnScaleCompleted -= OnScaleCompleted;
        }

        private void Update()
        {
            // rayの長さ検証用
            /*Debug.DrawRay(transform.position, Vector2.up * rayLength.y, Color.red);
            Debug.DrawRay(transform.position, Vector2.down * rayLength.y, Color.red);
            Debug.DrawRay(transform.position, Vector2.right * rayLength.x, Color.red);
            Debug.DrawRay(transform.position, Vector2.left * rayLength.x, Color.red);*/
            
            // rayの方向と長さ検証用
            if (_rayLength > 0f && rayDirection != Vector2.zero)
            {
                Debug.DrawRay(transform.position, rayDirection * _rayLength, Color.red);
            }
        }

        /// <summary>
        /// 飛ばすRayの長さを計算するクラス。Triggerの中心から半径＋プレイヤーの幅 ← 半径じゃなく直径だった
        /// </summary>
        private void CalcRayLength()
        {
            // bounds.extents：中心から各面までの距離(Scaleも考慮されるのでRadiusより適している)
            rayLength.x = trigger.bounds.size.x;
            rayLength.y = trigger.bounds.size.y;
            
            // RayLengthを更新
            rayLength.x += playerSize.x;
            rayLength.y += playerSize.y;
        }
        
        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                // トリガーからプレイヤーへの方向ベクトル
                Vector2 trig2Player = (playerCol.transform.position - transform.position).normalized;
                
                rayDirection = Normalize4Direction(trig2Player);
                ShootRay(rayDirection);
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            // リセット
            if (other.CompareTag("Player"))
            {
                isStack = false;
                rayDirection = Vector2.zero; 
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

            if (absX < absY)    // TODO: 上下は明らかに上乗っているときか真下にいるときだけreturnしたい。
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
  
        float _rayLength = 0f;
        int   layerMask  = 1 << 0; // ignoreRaycastとPlayerLayerを除外(ignoreRaycastはデフォルトで入ってそう)
        private void ShootRay(Vector2 direction)
        {
             _rayLength = 0f;
            if (rayDirection == Vector2.up || rayDirection == Vector2.down)
            {
                _rayLength = rayLength.y; 
            }
            else if (rayDirection == Vector2.left || rayDirection == Vector2.right)
            {
                _rayLength = rayLength.x;
            }
            
            Ray ray = new Ray(transform.position, direction);
            if (Physics.RaycastNonAlloc(ray, hitInfo, _rayLength, layerMask) > 0)
            {
                // 衝突したオブジェクトの名前を表示
                foreach (RaycastHit hit in hitInfo)
                {
                    if (hit.collider == null) continue;
                    Debug.Log($"衝突したオブジェクト: {hit.collider.gameObject.name}");
                    
                    if (hit.collider.CompareTag("Untagged"))
                    {
                        Debug.Log("isStackをtrueに");
                        isStack = true;
                    }
                    else
                    {
                        isStack = false;
                    }
                }
            }
        }
    }
}