using System;
using CoreModule.Input;
using UnityEngine;
using Module.Enemy;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;

namespace Module.Application.SceneSwitch
{
    
    public class TempBossBattleManager : MonoBehaviour
    {
        [SerializeField] private EnemyStatus enemyStatus;

        [SerializeField] private FadeAndSceneTransition sceneManager;
        private void Start()
        {
            enemyStatus.OnDeath += OnDeath;
        }

        private void OnDestroy()
        {
            enemyStatus.OnDeath -= OnDeath;
        }

        private void OnDeath()
        {
            SceneSwitchAsync().Forget();
        }

        private async UniTaskVoid SceneSwitchAsync()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(4f));

            if (sceneManager != null && sceneManager.nextSceneName == "Feedback")
            {
                Debug.Log("ボス戦をクリアしました");
                sceneManager.StartTransition();
            }
        }
    }
}