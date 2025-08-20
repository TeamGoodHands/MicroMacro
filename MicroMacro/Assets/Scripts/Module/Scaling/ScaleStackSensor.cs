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
        [SerializeField, Header("このオブジェクトのコライダー")] private Collider trigger;
        // RayLengthはそれぞれコライダーの半径＋プレイヤーの直径
        struct RayLength
        {
            public float x;
            public float y;
        }

        struct PlayerSize
        {
            public float x; 
            public float y;
        }
        
        private PlayerSize playerSize;
        private Collider playerCol;

        private Ray ray;
        private RayLength rayLength;
        private readonly RaycastHit[] hitInfo = new RaycastHit[5];   // RayCastで得た情報を格納する配列
        private int hitCount;
        private Vector2 rayDirection;                                // Rayを飛ばす方向

        private Vector2 baseTriggerSize;
        private bool isStack = false;

        private void Awake()
        {
            refScaler.OnScaleStarted += OnScaleStarted;
            scaler.OnScaleCompleted  += OnScaleCompleted;
        }

        private void Start()
        {
            GameObject player = GameObject.FindWithTag("Player");

            if (player == null)
            {
                Debug.LogError("Playerが見つかりません。");
                enabled = false;
                return;
            }
            playerCol = player.GetComponent<Collider>();
            
            // プレイヤーのコライダーの直径を取得 (colliderの種類変更にも対応)
            playerSize.x = playerCol.bounds.size.x;
            playerSize.y = playerCol.bounds.size.y;
            
            // スケール前の基準サイズ保持しておく
            // bounds.extents：中心から各面までの距離(Scaleも考慮されるのでRadiusより適している)
            baseTriggerSize = trigger.bounds.extents;
            
            CalcRayLength();
        }
        private void OnDestroy()
        {
            refScaler.OnScaleStarted -= OnScaleStarted;
            scaler.OnScaleCompleted -= OnScaleCompleted;
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

        /// <summary>
        /// 飛ばすRayの長さを計算するクラス。Triggerの中心から半径＋プレイヤーの幅 
        /// </summary>
        private void CalcRayLength()
        {
            // 毎回bounds.extentsを取得するとRayの長さがずれるので、元サイズにlossyScale（ワールド座標）を掛けてスケール後の面の位置を再計算
            // なぜこうなるのかあんまり納得いってない
            Vector2 worldSize = Vector2.Scale(baseTriggerSize, transform.lossyScale);
            
            // プレイヤーサイズ分足す
            rayLength.x = worldSize.x + playerSize.x;
            rayLength.y = worldSize.y + playerSize.y;
        }
        
        /// <summary>
        /// プレイヤーの方向を4方向で求める->Ray飛ばす->stack判断
        /// </summary>
        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                // トリガーからプレイヤーへの方向ベクトル
                Vector2 trig2Player = (playerCol.transform.position - transform.position).normalized;
                
                rayDirection = Normalize4Direction(trig2Player);
                hitCount = ShootRay(rayDirection);
                StackJudge(hitCount);
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
        const float HorizontalThreshold = 0.3f; // 閾値調整(小さいほど上下の判定エリアが狭くなるイメージ), プレイヤーサイズから計算してもいい。 
        private Vector2 Normalize4Direction(Vector2 direction)
        {
            if (direction == Vector2.zero)
            {
                Debug.LogWarning("ゼロベクトルが渡されました");
                return Vector2.zero;
            }
            
            float absX = Mathf.Abs(direction.x);
            float absY = Mathf.Abs(direction.y);
            
            
            // Y成分の方が大きい、かつx成分が閾値に収まっているとき(ほぼ真上か真下)
            if (absY > absX && absX < HorizontalThreshold)   
            {
                // 上下方向 (Mathf.Sign: 符号を返す 正:1, 負-1, ゼロ:0)
                return new Vector2(0, Mathf.Sign(direction.y));
            }
            else
            {
                // それ以外すべて左右
                // absX == absY の場合も左右方向として判定
                return new Vector2(Mathf.Sign(direction.x), 0);
            }
        }
        
        int layerMask  = 1 << 0; // RaycastはDefaultLayerのみを対象に 
        private int ShootRay(Vector2 direction)
        {
            float _rayLength = 0f;
            if (rayDirection == Vector2.up || rayDirection == Vector2.down)
            {
                _rayLength = rayLength.y; 
            }
            else if (rayDirection == Vector2.left || rayDirection == Vector2.right)
            {
                _rayLength = rayLength.x;
            }
            
            // RayCastNonAlloc: ヒット結果を既存配列に格納する。
            // 毎回配列生成する必要ないからGC回避ができるが、配列サイズを超えないよう注意。
            ray = new Ray(transform.position, direction);
            return Physics.RaycastNonAlloc(ray, hitInfo, _rayLength, layerMask);
        }

        private void StackJudge(int hitCount)
        {
            if (hitCount > 0)
            {
                // 衝突したオブジェクトがUntagged(壁など)ならスタック状態
                for (int i = 0; i < hitCount; i++)
                {
                    var hit = hitInfo[i];
                    
                    if (hit.collider == null) continue;
                    
                    if (hit.collider.CompareTag("Untagged"))
                    {
                        isStack = true;
                    }
                }
            }
            else
            {
                isStack = false;    // ヒットなし
            }
        }
    }
}