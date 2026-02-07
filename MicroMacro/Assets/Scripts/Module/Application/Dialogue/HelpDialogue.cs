using UnityEngine;
using System;
using Module.Player.Component;
using Module.UI;

namespace Module.Application.Dialogue
{
    /// <summary>
    /// 特定エリアに一定時間滞在した際にお助けセリフを表示するクラス
    /// </summary>
    public class HelpDialogue : MonoBehaviour
    {
        [Header("セリフ表示を一度のみとするか")] [SerializeField] private bool onlyOnce;
        [Header("呼び出すセリフたちの登録名(DialogueDatabaseのKey)")] [SerializeField] private String[] dialogues;
        [Header("〇秒経過でお助けUIを表示")] [SerializeField] private float helpTriggerTime;
        
        [Header("判定エリア設定")]
        [Header("ゲート(入口)のPivot")] [SerializeField] private GameObject entrancePivot;
        [Header("ゲート(出口)のPivot")] [SerializeField] private GameObject exitPivot;

        [Header("References")]
        [SerializeField] private SectionGate entrance;
        [SerializeField] private SectionGate exit;
        [SerializeField] private GameObject player;
        [SerializeField] private HealthStatus healthStatus;
        [SerializeField] private DialogueManager dialogueManager;
        
        private float elapsedTime;
        private bool  isPlayerInside;
        private bool  isEnqueued;
        
        private Vector2 entrancePos;
        private Vector2 exitPos;

        private void Awake()
        {
             if (entrancePivot == null || exitPivot == null)
             {
                 Debug.LogError($"[HelpDialogue] {gameObject.name}: Pivotが設定されていません。");
                 return;
             }

             entrancePos = entrancePivot.transform.position;
             exitPos = exitPivot.transform.position;

             if (healthStatus == null)
             {
                 Debug.LogError($"[HelpDialogue] {gameObject.name}: HealthStatusが設定されていません。");
                 return;
             }

             // イベント登録
             healthStatus.OnDeath  += OnDeath;
             entrance.OnPlayerExit += OnPlayerExit;
             exit.OnPlayerExit     += OnPlayerExit;
        }

        private void OnDestroy()
        {
            // イベント解除
            if (entrance != null) entrance.OnPlayerExit -= OnPlayerExit;
            if (exit != null) exit.OnPlayerExit -= OnPlayerExit;
            if (healthStatus != null) healthStatus.OnDeath -= OnDeath;
        }
        
        /// <summary>
        /// プレイヤー死亡時の処理
        /// セリフ表示フラグをリセットし、復活後に再度見られるようにする
        /// </summary>
        private void OnDeath()
        {
            // 死亡時にキューをクリア（DialogueManager側でも行っているが念のため）
            dialogueManager.ClearQueue();
            
            // 内部フラグのリセット
            isEnqueued = false; 
            elapsedTime = 0f;
            isPlayerInside = false; // 復活地点がエリア外であることを想定
        }

        /// <summary>
        /// ゲート通過（エリア内外判定の更新）
        /// </summary>
        private void OnPlayerExit()
        {
            if (player == null) return;

            Vector2 playerPos = player.transform.position;
            bool wasInside = isPlayerInside;
            isPlayerInside = CheckPlayerIsInArea(playerPos, entrancePos, exitPos);

            // ループ設定（onlyOnce = false）の場合
            // エリアから外に出たタイミングで、次回の滞在カウントを許可する
            if (wasInside && !isPlayerInside && !onlyOnce)
            {
                isEnqueued = false;
                elapsedTime = 0f;
            }
        }

        /// <summary>
        /// プレイヤーが指定された二点間のエリア内にいるか判定
        /// </summary>
        private bool CheckPlayerIsInArea(Vector2 playerPos, Vector2 point1, Vector2 point2)
        {
            float minX = Mathf.Min(point1.x, point2.x);
            float maxX = Mathf.Max(point1.x, point2.x);
            float minY = Mathf.Min(point1.y, point2.y); 
            float maxY = Mathf.Max(point1.y, point2.y);
           
            return playerPos.x > minX && playerPos.x < maxX && 
                   playerPos.y > minY && playerPos.y < maxY;
        }
    
        private void Update()
        {
            // すでにキュー追加済み かつ 「一度きり」設定なら何もしない
            if (isEnqueued && onlyOnce)
                return;
            
            if (isPlayerInside)
            {
                // まだ今回の滞在でセリフを投げていない場合のみカウント
                if (!isEnqueued)
                {
                    elapsedTime += Time.deltaTime;
                    
                    if (elapsedTime >= helpTriggerTime)
                    {
                        // セリフキューに追加
                        dialogueManager.EnqueueDialogues(dialogues);
                        isEnqueued = true;
                        
                        // ループ設定でないならこのまま isEnqueued=true でUpdateが止まる
                        // ループ設定なら、一度エリアを出るまで待機
                    }
                }
            }
            else
            {
                // エリア外にいる間は常にカウントをリセット
                elapsedTime = 0f;
            }
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (entrancePivot == null || exitPivot == null) return;

            Vector3 p1 = entrancePivot.transform.position;
            Vector3 p2 = exitPivot.transform.position;
            Vector3 center = (p1 + p2) / 2f;
            Vector3 size = new Vector3(Mathf.Abs(p1.x - p2.x), Mathf.Abs(p1.y - p2.y), 0.1f);
        
            Gizmos.color = new Color(1, 0.92f, 0.016f, 0.25f);
            Gizmos.DrawCube(center, size);
        }
#endif
    }
}