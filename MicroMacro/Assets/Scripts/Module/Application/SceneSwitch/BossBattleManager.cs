using System;
using Constants;
using CoreModule.Input;
using UnityEngine;
using Module.Enemy;
using Cysharp.Threading.Tasks;
using Module.Player.Component;
using Module.UI;
using UnityEngine.InputSystem;

namespace Module.Application.SceneSwitch
{
    public class BossBattleManager : MonoBehaviour
    {
        [SerializeField] private HealthStatus enemyStatus;
        [SerializeField] private BossGoalPlayer bossGoalPlayer;
        
        private PlayerCondition playerCondition;

        private void Start()
        {
            playerCondition = GameObject.FindWithTag(Tag.Player).GetComponent<PlayerCondition>();
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
            playerCondition.IsPlayerLocked = true;

            await UniTask.Delay(TimeSpan.FromSeconds(4f));

            bossGoalPlayer.Play().Forget();
        }
    }
}