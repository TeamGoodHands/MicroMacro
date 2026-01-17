using System;
using Constants;
using Cysharp.Threading.Tasks;
using Module.Player.Component;
using Module.UI;
using UnityEngine;

namespace Module.Application.Respawn
{
    public class RespawnArea : MonoBehaviour
    {
        [SerializeField] private float respawnTime = 1f;
        private Transform playerTransform;
        private HealthStatus healthStatus;
        private PlayerCondition playerCondition;
        private Vector3 spawnPosition;
        private bool isWaitingRespawn;

        private void Start()
        {
            GameObject playerObject = GameObject.FindWithTag(Tag.Player);
            playerTransform = playerObject.transform;
            healthStatus = playerObject.GetComponent<HealthStatus>();
            playerCondition = playerObject.GetComponent<PlayerCondition>();
            spawnPosition = playerTransform.localPosition;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isWaitingRespawn)
                return;

            // ダメージを与える
            SendDamage(other.gameObject);
        }
        
        private void OnCollisionEnter(Collision other)
        {
            if (isWaitingRespawn)
                return;

            // ダメージを与える
            SendDamage(other.gameObject);
        }

        private void SendDamage(GameObject obj)
        {
            if (obj.CompareTag(Tag.Player))
            {
                healthStatus.Damage(1);
                playerCondition.IsPlayerLocked = true;
                Respawn().Forget();
            }
        }

        private async UniTaskVoid Respawn()
        {
            isWaitingRespawn = true;

            await UniTask.Delay(TimeSpan.FromSeconds(respawnTime), cancellationToken: destroyCancellationToken);

            playerCondition.IsPlayerLocked = false;
            playerTransform.localPosition = spawnPosition;
            isWaitingRespawn = false;
        }
    }
}