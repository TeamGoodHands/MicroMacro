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
        [SerializeField] private GameObject nuts;
        
        [SerializeField] private GameObject nutsDestination;
        [SerializeField] private GameObject startPoint;
        [SerializeField] private GameObject endPoint;
        
        [SerializeField] private Animator warmAnim;
        [SerializeField] private Rigidbody rb;
        [SerializeField] private PlayableDirector director;
        
        [SerializeField] private float stopDistance = 0.5f;
        [SerializeField] private float breakDuration;

        private AnimationState stateInfo;
        // キャンセル用トークンソース（連打されたときに前の処理を止めるため）
        private CancellationTokenSource moveCts;
        private float speed = 10f; // 秒数指定なければ
        private bool isOpened; 

        private void Awake()
        {
            if (startPoint == null || endPoint == null)
            {
                Debug.LogError("移動先が指定されていません");
            }
        }
        
        private void OnCollisionEnter(Collision other)
        {
            if (isOpened)
                return;
            
            if (other.gameObject == nuts)
            {
                SoundManager.instance.Play("スイッチ");
                EatNutsAndBreakDoor().Forget();
            }
        }
        
        private async UniTaskVoid EatNutsAndBreakDoor()
        {
            if (!nuts.activeSelf)
                return;
            
            nuts.SetActive(false);
            warmAnim.SetTrigger("EatNuts");
            // TODO EatNutsモーションが終わったらDoorまで移動させたい
            MoveToDoor(startPoint.transform.position);
            // TODO Doorまで移動が完了したらBreakDoor再生したい
        }

        /// <summary>
        /// カメラの移動とWarmの移動
        /// </summary>
        /// <param name="targetPosition"></param>
        private void MoveToDoor(Vector3 targetPosition)
        {
            moveCts?.Cancel();
            moveCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
           
            warmAnim.SetTrigger("MoveToDoor");
            director.Play(); // カメラの移動
            
            MoveRoutine(targetPosition, 0f, moveCts.Token).Forget();
        }

        
        private async UniTaskVoid MoveRoutine(Vector3 target, float Duration,  CancellationToken token)
        {
            Debug.Log("移動開始");

            try
            {
                // 移動先は固定だから毎回方向再計算する必要はなさそう
                Vector3 direction = (target - transform.position).normalized;
                
                // 移動にかかる時間指定ない場合はデフォルト(10f)のまま
                if (Duration != 0f)
                    speed = Vector3.Distance(transform.position, target) / Duration;
                
                while (Vector3.Distance(transform.position, target) > stopDistance)
                {
                   rb.AddForce(direction * speed);

                   // 次のFixedUpdateまで待機
                   await UniTask.WaitForFixedUpdate(cancellationToken: token);
                }
                
                // 到着後の処理
                rb.linearVelocity = Vector3.zero; // 慣性を止める
                Debug.Log("到着: ループ終了");
            }
            catch (System.OperationCanceledException)
            {
                Debug.Log("移動キャンセル");
            }
        }
        
        /// <summary>
        /// ドア破壊はドア側のモーション再生時間に揃える
        /// </summary>
        private void BreakDoor()
        {
            moveCts?.Cancel();
            moveCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());

            // 扉の始点から終点にかけてBreakDoorモーション再生しつつ移動
            warmAnim.SetTrigger("BreakDoor");
            MoveRoutine(endPoint.transform.position, breakDuration, moveCts.Token).Forget();
        }
    }
}