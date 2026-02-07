using UnityEngine;
using System;
using System.Threading;
using Cysharp.Threading.Tasks; // UniTask
using Module.Player.Component;
using Module.UI;

namespace Module.Application.Dialogue
{
    public class HelpDialogue : MonoBehaviour
    {
        [Header("セリフ表示を一度のみとするか")] [SerializeField] private bool onlyOnce;
        [Header("呼び出すセリフたちの登録名")] [SerializeField] private String[] dialogues;
        [Header("〇秒経過でお助けUIを表示")] [SerializeField] private float helpTriggerTime;
        
        [Header("条件オブジェクト (Noneの場合は常に復活・再生する)")]
        [SerializeField] private GameObject targetEnemy;
        
        [Header("死亡後の判定ロック時間(秒)")] 
        [Tooltip("死亡演出中に判定が進まないようにするための待機時間。")]
        [SerializeField] private float deathLockDuration = 2.0f;

        [Header("判定エリア設定")]
        [Header("ゲート(入口)のPivot")] [SerializeField] private GameObject entrancePivot;
        [Header("ゲート(出口)のPivot")] [SerializeField] private GameObject exitPivot;

        [SerializeField] private SectionGate entrance;
        [SerializeField] private SectionGate exit;
        [SerializeField] private GameObject player;
        [SerializeField] private HealthStatus healthStatus;
        [SerializeField] private DialogueManager dialogueManager;
        
        private bool isEnqueued;
        private bool hasTargetEnemy; // 最初から敵が設定されていたかのフラグ
        
        private CancellationTokenSource timerCts;
        private CancellationTokenSource respawnCts;

        private Vector2 entrancePos;
        private Vector2 exitPos;

        private void Awake()
        {
             // 敵がアサインされているかを確認
             // (targetEnemyがnullでないなら true)
             hasTargetEnemy = targetEnemy != null;

             if (entrancePivot != null) entrancePos = entrancePivot.transform.position;
             else if (entrance != null) entrancePos = entrance.transform.position;

             if (exitPivot != null) exitPos = exitPivot.transform.position;
             else if (exit != null) exitPos = exit.transform.position;

             if (healthStatus != null) healthStatus.OnDeath += OnDeath;
             
             if (entrance != null) entrance.OnPlayerExit += CheckAndControlTimer;
             if (exit != null) exit.OnPlayerExit     += CheckAndControlTimer;
        }

        private void OnDestroy()
        {
            CancelTimer();
            CancelRespawnWait();

            if (entrance != null) entrance.OnPlayerExit -= CheckAndControlTimer;
            if (exit != null) exit.OnPlayerExit -= CheckAndControlTimer;
            if (healthStatus != null) healthStatus.OnDeath -= OnDeath;
        }
        
        private void OnDeath()
        {
            CancelTimer();
            dialogueManager.ClearQueue();

            // 「敵設定なし」または「敵がまだ生きている」場合のみ復活（リセット）させる
            // 敵が設定されており、かつ死んでいる(Deleteされた)場合は false が返るのでリセットされない
            if (CanPlayDialogue())
            {
                isEnqueued = false;
                // リスポーン後にエリア判定を行う予約
                WaitAndCheckRespawnAsync().Forget();
            }
        }

        /// <summary>
        /// リスポーン待機コルーチン
        /// </summary>
        private async UniTaskVoid WaitAndCheckRespawnAsync()
        {
            CancelRespawnWait();
            respawnCts = new CancellationTokenSource();

            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(deathLockDuration), cancellationToken: respawnCts.Token);
                
                // 待機完了後、エリア内にいるかチェック
                CheckAndControlTimer();
            }
            catch (OperationCanceledException)
            {
                // キャンセル時は何もしない
            }
        }

        /// <summary>
        /// 現在の状態を確認し、タイマーの起動/停止を行う
        /// </summary>
        private void CheckAndControlTimer()
        {
            if (player == null) return;
            
            // そもそも再生条件（敵生存など）を満たしていないなら何もしない
            if (!CanPlayDialogue()) return;

            bool isInside = CheckPlayerIsInArea(player.transform.position, entrancePos, exitPos);

            if (isInside)
            {
                // エリア内 かつ まだ再生してない かつ タイマーが未起動なら開始
                if (!isEnqueued && timerCts == null)
                {
                    StartDialogueTimerAsync().Forget();
                }
            }
            else
            {
                // エリア外に出た -> タイマーキャンセル
                CancelTimer();

                // ループ設定ならフラグをリセットして次回再生を許可
                if (!onlyOnce)
                {
                    isEnqueued = false;
                }
            }
        }

        private async UniTaskVoid StartDialogueTimerAsync()
        {
            timerCts = new CancellationTokenSource();
            
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(helpTriggerTime), cancellationToken: timerCts.Token);

                dialogueManager.EnqueueDialogues(dialogues);
                isEnqueued = true;
                
                CancelTimer(); 
            }
            catch (OperationCanceledException)
            {
                // 待機中にキャンセルされた場合
            }
        }

        private void CancelTimer()
        {
            if (timerCts != null)
            {
                timerCts.Cancel();
                timerCts.Dispose();
                timerCts = null;
            }
        }
        
        private void CancelRespawnWait()
        {
            if (respawnCts != null)
            {
                respawnCts.Cancel();
                respawnCts.Dispose();
                respawnCts = null;
            }
        }

        /// <summary>
        /// セリフを再生（または復活）できる状態か判定する
        /// </summary>
        private bool CanPlayDialogue()
        {
            // 1. 最初から敵が設定されていない -> 無条件でOK (true)
            if (!hasTargetEnemy) return true;

            // 2. 敵が設定されていた -> 今存在しているか？ (Destroyされていればnullになる)
            // 存在していればOK (true)、死んでいればNG (false)
            return targetEnemy != null; 
        }

        private bool CheckPlayerIsInArea(Vector2 playerPos, Vector2 point1, Vector2 point2)
        {
            float minX = Mathf.Min(point1.x, point2.x);
            float maxX = Mathf.Max(point1.x, point2.x);
            float minY = Mathf.Min(point1.y, point2.y); 
            float maxY = Mathf.Max(point1.y, point2.y);
           
            return playerPos.x > minX && playerPos.x < maxX && 
                   playerPos.y > minY && playerPos.y < maxY;
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Vector3 p1 = entrancePivot != null ? entrancePivot.transform.position : (entrance != null ? entrance.transform.position : Vector3.zero);
            Vector3 p2 = exitPivot != null ? exitPivot.transform.position : (exit != null ? exit.transform.position : Vector3.zero);

            if (p1 == Vector3.zero && p2 == Vector3.zero) return;

            Vector3 center = (p1 + p2) / 2f;
            Vector3 size = new Vector3(Mathf.Abs(p1.x - p2.x), Mathf.Abs(p1.y - p2.y), 0.1f);
            
            Gizmos.color = new Color(1, 0.92f, 0.016f, 0.25f);
            Gizmos.DrawCube(center, size);
        }
#endif
    }
}