using UnityEngine;
using System;
using UnityEngine.Serialization;

namespace Module.Application.Dialogue
{
    public class HelpDialogue : MonoBehaviour
    {
        [Header("セリフ表示を一度のみとするか")] [SerializeField] private bool onlyOnce;
        [Header("呼び出すセリフたち")] [SerializeField] private String[] dialogues;
        [Header("〇秒経過でお助けUIを表示")] [SerializeField] private float helpTriggerTime;
        [Header("ゲート(入口)のPivot")] [SerializeField] private GameObject entrancePivot;
        [Header("ゲート(出口)のPivot")] [SerializeField] private GameObject exitPivot;
        [SerializeField] private SectionGate entrance;
        [SerializeField] private SectionGate exit;
        [SerializeField] private GameObject player;
        [SerializeField] private DialogueManager dialogueManager;
        
        private float elapsedTime;
        private bool  isPlayerInside;
        private bool isEnqueued;
        
        private Vector2 entrancePos;
        private Vector2 exitPos;

        private void Awake()
        {
             entrancePos = entrancePivot.transform.position;
             exitPos = exitPivot.transform.position;
             
             entrance.OnPlayerExit += OnPlayerExit;
             exit.OnPlayerExit     += OnPlayerExit;
        }

        private void OnDestroy()
        {
            entrance.OnPlayerExit -= OnPlayerExit;
            exit.OnPlayerExit     -= OnPlayerExit;
        }

        private void OnPlayerExit()
        {
            Vector2 playerPos = player.transform.position;
            isPlayerInside = CheckPlayerIsInArea(playerPos, entrancePos, exitPos);
        }

        /// <summary>
        /// プレイヤーが指定された二点間のエリア内にいるか。
        /// </summary>
        private bool CheckPlayerIsInArea(Vector2 playerPos, Vector2 point1, Vector2 point2)
        {
            // 矩形の角となる座標を計算
            float minX = Mathf.Min(point1.x, point2.x);
            float maxX = Mathf.Max(point1.x, point2.x);
            float minY = Mathf.Min(point1.y, point2.y); 
            float maxY = Mathf.Max(point1.y, point2.y);
           
            // XとYそれぞれでエリアに収まっているか比較
            bool isInHorizontal = minX < playerPos.x && playerPos.x < maxX;
            bool isInVertical   = minY < playerPos.y && playerPos.y < maxY;
            
            return isInHorizontal && isInVertical;
        }
    
        // TODO: 常にUpdate呼ぶ必要ないから今後Unitask等に改善したい。
        private void Update()
        {
            if (isEnqueued && onlyOnce) // 一度表示済みで弾くことも可能に
                return;
            
            if (isPlayerInside)
            {
                elapsedTime += Time.deltaTime;
                
                if (elapsedTime >= helpTriggerTime)
                {
                    // セリフキューに追加
                    dialogueManager.EnqueueDialogues(dialogues);
                    isEnqueued = true;
                    
                    // 経過時間リセット 再表示しないなら必要なし。
                    isPlayerInside = false;
                }
            }
            else
            {
                elapsedTime = 0f;
            }
        }
        
#if UNITY_EDITOR
        /// <summary>
        /// SceneビューでGameObjectが選択された時に呼び出され、ギズモを描画。
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (entrancePivot == null || exitPivot == null)
            {
                return;
            }

            // Pivotの位置を取得
            Vector3 p1 = entrancePivot.transform.position;
            Vector3 p2 = exitPivot.transform.position;
        
            // 矩形の中心を計算
            Vector3 center = (p1 + p2) / 2f;
        
            // 矩形のサイズを計算 (XとYの差の絶対値)
            Vector3 size = new Vector3(
                Mathf.Abs(p1.x - p2.x),
                Mathf.Abs(p1.y - p2.y),
                0.1f // Zが0だとなにも見えない
            );
        
            // ギズモの色を設定 (黄色、半透明)
            Gizmos.color = new Color(1, 0.92f, 0.016f, 0.25f);
            Gizmos.DrawCube(center, size);
        }
#endif
    }
}