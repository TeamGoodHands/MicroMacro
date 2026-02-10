using System;
using Constants;
using Cysharp.Threading.Tasks;
using Module.Application.SceneSwitch;
using Module.Management;
using Module.Player.Component;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;
using Module.Application.Data; 


namespace Module.UI
{
    public class BossGoalPlayer : MonoBehaviour
    {
        [SerializeField] private PlayableDirector playableDirector;
        [SerializeField] private FadeAndSceneTransition fadeAndSceneTransition;
        [SerializeField] private CinemachineCamera clearCamera;
        [SerializeField] private bool lockPlayer;
        
        [Header("このステージのID (例: 1-1)")]
        [SerializeField] private string currentStageId;
        [Header("移動先シーン")]
        [SerializeField] private string nextSceneName = "StageSelect";


        private void Start()
        {
        }

        public async UniTaskVoid Play()
        {
            clearCamera.Priority = 10000;

            await UniTask.Delay(TimeSpan.FromSeconds(0.8f), cancellationToken: destroyCancellationToken);

            SoundManager.instance.Play("クリア長め");

            playableDirector.Play();

            await UniTask.Delay(TimeSpan.FromSeconds(9f), cancellationToken: destroyCancellationToken);

            SceneMove();
        }

        private void SceneMove()
        {
            fadeAndSceneTransition.StartTransition();
            
            if (SaveManager.Instance != null && !string.IsNullOrEmpty(currentStageId))
            {
                SaveManager.Instance.SetStageCleared(currentStageId);
            }
            else
            {
                Debug.LogWarning("SaveManagerが無いか、StageIDが空です");
            }

            // ステージセレクト画面へ戻る
            if (fadeAndSceneTransition != null)
                fadeAndSceneTransition.StartPageFlipTransition(nextSceneName);

        }
    }
}