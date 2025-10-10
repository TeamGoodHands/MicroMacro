using System;
using System.Collections.Generic;
using Constants;
using Cysharp.Threading.Tasks;
using Module.Player;
using Module.Player.Component;
using UnityEngine;

namespace Module.Application.Respawn
{
    public class RespawnSystem : MonoBehaviour
    {
        [SerializeField, Header("リスポーン時間")]
        private float respawnTime = 1f;

        [SerializeField, Header("ステージ上のチェックポイント(ゲーム開始時に自動で取得)")]
        private CheckPoint[] checkPoints;

        public event Action<Vector3> OnPlayerDespawn;
        public event Action<Vector3> OnPlayerRespawn;

        private Vector3 spawnPosition;
        private PlayerBehaviour playerBehaviour;
        private PlayerStatus playerStatus;

        private void Start()
        {
            GameObject playerObject = GameObject.FindWithTag(Tag.Player);

            // 初めの座標を登録
            playerBehaviour = playerObject.GetComponent<PlayerBehaviour>();
            spawnPosition = playerBehaviour.transform.position;

            // 死亡時のリスポーンイベントを登録
            playerStatus = playerObject.GetComponent<PlayerStatus>();
            playerStatus.OnDeath += Respawn;

            // チェックポイントを登録
            checkPoints = FindObjectsByType<CheckPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (CheckPoint checkPoint in checkPoints)
            {
                checkPoint.OnPlayerArrived += () => { spawnPosition = checkPoint.transform.position; };
            }
        }

        private async void Respawn()
        {
            OnPlayerDespawn?.Invoke(playerBehaviour.transform.position);

            // リスポーンまで少し待機
            await UniTask.Delay(TimeSpan.FromSeconds(respawnTime), cancellationToken: destroyCancellationToken);

            // 座標とHPをリセット
            playerBehaviour.transform.position = spawnPosition;
            playerStatus.Reset();

            await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: destroyCancellationToken);

            OnPlayerRespawn?.Invoke(playerBehaviour.transform.position);
        }
    }
}