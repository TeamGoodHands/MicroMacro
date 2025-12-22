using System;
using System.Threading;
using Module.Management;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Playables;

namespace Module.Gimmick
{
    /// <summary>
    /// 木の実がWarmに触れたら、木の実を食べる → ドアの始点に移動 → 終点に向かってドアを食べる(BreakDoorモーション)　また、秒数を指定してドアが壊れるモーションと再生タイミングを揃える　　　　　　　
    /// </summary>
    public class DoorGimmick : MonoBehaviour
    {
        [Header("Targets")]
        [SerializeField] private GameObject nuts;
        [SerializeField] private GameObject nutsDestination; // ※今回は未使用のようですが維持
        [SerializeField] private GameObject startPoint;
        [SerializeField] private GameObject endPoint;
        
        [Header("Components")]
        [SerializeField] private Animator warmAnim;
        [SerializeField] private Rigidbody rb;
        [SerializeField] private PlayableDirector director;
        
        [Header("Settings")]
        [SerializeField] private float stopDistance = 0.1f; 
        [SerializeField] private float breakDuration = 2.0f; 
        [SerializeField] private float defaultMoveSpeed = 5f;
        [SerializeField] private float rotateDuration = 0.5f;
        [SerializeField] private Vector3 modelRotationOffset = new Vector3(90f, -70f, 0f);
        
        [Header("ギミック中切り替えるLayer & Z軸オフセット")]
        [SerializeField] private int ignoreLayerIndex = 2; 
        // 2Dゲームで手前に表示するためにZ軸をどれだけマイナスするか（-1f 〜 -5fくらい?）
        [SerializeField] private float frontOffsetZ = -2.0f;
        
        // キャンセル用トークンソース（連打されたときに前の処理を止めるため）
        private CancellationTokenSource cts;
        private bool isOpened;

        // 元の状態を保存するための変数
        private int originalLayer;
        private float originalZ;
        
        public event Action OnArrivalDoor;
      
        private void OnDestroy()
        {
            cts?.Cancel();
            cts?.Dispose();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isOpened)
                return;
            
            if (other.gameObject == nuts)
            {
                isOpened = true;
                SoundManager.instance.Play("スイッチ");
                EatNutsAndBreakDoorFlow().Forget();
            }
        }
        
        private async UniTaskVoid EatNutsAndBreakDoorFlow()
        {
            // 一連の処理が走っている間有効なトークンを作成
            cts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            CancellationToken token = cts.Token;
            
            if (!nuts.activeSelf)
                return;
            
            originalLayer = gameObject.layer;
            originalZ = transform.position.z;

            try
            {
                // レイヤー切り替えて衝突回避
                gameObject.layer = ignoreLayerIndex;
                
                // --- 木の実を食べる ---
                nuts.SetActive(false);
                warmAnim.SetTrigger("EatNuts");
                await WaitForAnimation(warmAnim, "EatNuts", 0, token);
                
              //  transform.localScale = new Vector3(1f, 1f, 1f); // スケールリセット
                
                // --- 回転してドアの始点へ移動 ---
                await TurnToTargetAsync(startPoint.transform.position, rotateDuration, token);
                // Z軸を手前にズラして手前に描画 
                Vector3 target = new Vector3(transform.position.x, transform.position.y, originalZ + frontOffsetZ);
                await MoveRoutine(target, 0.3f, token);
               
                await MoveToDoorAsync(startPoint.transform.position, token);
                
                // 奥行を元に戻す
                Vector3 currentPos = transform.position;
                transform.position = new Vector3(currentPos.x, currentPos.y, originalZ);
                
                // --- 回転とドア破壊 ---
                await TurnToTargetAsync(endPoint.transform.position, 0.5f, token);    
                await BreakDoorAsync(token);
            }
            catch (System.OperationCanceledException)
            {
                Debug.Log("ギミック処理キャンセル");
            }
            finally
            {
                // 後処理
                if (rb != null)
                 rb.linearVelocity = Vector3.zero;
                
                gameObject.layer = originalLayer;
            }
        }

        /// <summary>
        /// ドアの始点まで移動
        /// </summary>
        private async UniTask MoveToDoorAsync(Vector3 targetPosition, CancellationToken token)
        {
            warmAnim.SetTrigger("BreakDoor");
            // director.Play(); // カメラ移動開始
            
            // 時間指定なし(0f) = defaultMoveSpeedで移動
            await MoveRoutine(targetPosition, 0f, token);
        }
        
        /// <summary>
        /// ドア破壊モーションと移動は同期させる
        /// </summary>
        private async UniTask BreakDoorAsync(CancellationToken token)
        {
           // warmAnim.SetTrigger("BreakDoor");
           // これ受信させてドア側モーション再生
           OnArrivalDoor?.Invoke();
           await MoveRoutine(endPoint.transform.position, breakDuration, token);
           
        }
        
        /// <summary>
        ///  移動の共通処理
        ///  durationが0fの場合はdefaultMoveSpeedで移動
        /// </summary>
        private async UniTask MoveRoutine(Vector3 target, float duration,  CancellationToken token)
        {
            Debug.Log($"移動開始 Target:{target} Duration:{duration}");
            
            while(true)
            {
                
                Vector3 flatTarget = new Vector3(target.x, target.y, transform.position.z);
                
                if (Vector3.Distance(transform.position, flatTarget) <= stopDistance)
                {
                    break;
                }
                
                if (duration > 0f && duration <= -0.01f)
                {
                    // 時間指定モードで時間が過ぎていたら強制終了
                    Debug.LogWarning("移動が強制終了されました。");
                    break;
                }
                
                Vector3 direction = (flatTarget - transform.position).normalized;
                float currentDistance = Vector3.Distance(transform.position, flatTarget);
                
                if (duration > 0f)
                {
                    // --- 時間指定モード --- 
                    // ゼロ除算対策
                    float remainingTime = Mathf.Max(duration, Time.fixedDeltaTime);
                    
                    // 残り距離 / 残り時間 で速度を出す
                    float speed = currentDistance / remainingTime;
                    
                    // AddForceだと加速し続けてタイミングが合わなくなる     
                    rb.linearVelocity = direction * speed;        
                    
                    // タイマーを減算
                    duration -= Time.fixedDeltaTime;
                }
                else
                {
                    // --- 通常移動モード ---
                    rb.AddForce(direction * defaultMoveSpeed);
                    
                    // 速度（velocity）の大きさ（magnitude）が、制限速度を超えていたら
                    if (rb.linearVelocity.magnitude > defaultMoveSpeed)
                    {
                        // 方向はそのまま、制限速度（defaultMoveSpeed）に抑える
                        rb.linearVelocity = rb.linearVelocity.normalized * defaultMoveSpeed;
                    }
                }

                await UniTask.WaitForFixedUpdate(cancellationToken: token);
            }
            
            rb.linearVelocity = Vector3.zero;
            Debug.Log("移動完了");
        }
        
        private async UniTask TurnToTargetAsync(Vector3 targetPosition, float duration, CancellationToken token)
        {
            // 2DだからZはこのオブジェクトと同じにする
            Vector3 flatTarget = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
            Vector3 direction = (flatTarget - transform.position).normalized;
            
            if (direction == Vector3.zero)
                return;

            // 目標となる回転角度（Y軸回転）
            // 2Dゲーム上のキャラが横を向く動きなら LookRotationでOK
            Quaternion startRot = transform.rotation;
            Quaternion endRot = Quaternion.LookRotation(direction) * Quaternion.Euler(modelRotationOffset);

            // 指定時間で回転
            float time = 0;
            while (time < duration)
            {
                // Slerp（球状線形補間）で滑らかに回す
                transform.rotation = Quaternion.Slerp(startRot, endRot, time / duration);
                
                time += Time.deltaTime;
                await UniTask.WaitForFixedUpdate(cancellationToken: token);
            }

            // 最後にズレがないように確定させる
            transform.rotation = endRot;
        }
        
        /// <summary>
        /// 指定アニメーションが終了するまで待機
        /// layerIndexはAnimationを重ね掛けする場合変えるものだから、0でOK
        /// </summary>
        private async UniTask WaitForAnimation(Animator anim, string stateName, int layerIndex, CancellationToken token)
        {
            // ステート切り替えの反映まで1フレ待機
            await UniTask.Yield(token);
            
            // 指定のステートになるまで待機
            await UniTask.WaitUntil(() => anim.GetCurrentAnimatorStateInfo(layerIndex).IsName(stateName), cancellationToken: token);

            // 再生終了まで待機(normalizedTime >= 1.0f)
            await UniTask.WaitUntil(() =>
            {
                // ステート情報を確認し続けて指定モーションが終了したらtrueを返す
                var info = anim.GetCurrentAnimatorStateInfo(layerIndex);
                return info.IsName(stateName) && info.normalizedTime >= 1.0f;
            }, cancellationToken: token);
        }
    }
}