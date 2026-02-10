using System;
using UnityEngine;
using UnityEngine.Serialization;
using Module.Application.Data; 

namespace Module.Application.SceneSwitch
{
    public class GoalGate : MonoBehaviour
    {
        [SerializeField] private FadeAndSceneTransition sceneManager;
        
        [Header("このステージのID (例: 1-1)")]
        [SerializeField] private string currentStageId;

        // ステージセレクトのシーン名
        private const string STAGE_SELECT_SCENE = "StageSelect";

        private void Start()
        {
            GetComponent<MeshRenderer>().enabled = false;
        }

        private void OnTriggerEnter(Collider other)
        {
             if (!other.CompareTag("Player")) return;

            // クリアフラグを保存
            if (SaveManager.Instance != null && !string.IsNullOrEmpty(currentStageId))
            {
                SaveManager.Instance.SetStageCleared(currentStageId);
            }
            else
            {
                Debug.LogWarning("SaveManagerが無いか、StageIDが空です");
            }

            // ステージセレクト画面へ戻る
            if (sceneManager != null)
            {
                // インスペクタで指定されていればそれを使うが、
                // 基本的にクリア後はステージセレクトに戻るならここで指定しても良い
                sceneManager.StartPageFlipTransition(STAGE_SELECT_SCENE);
            }
        }
    }
}