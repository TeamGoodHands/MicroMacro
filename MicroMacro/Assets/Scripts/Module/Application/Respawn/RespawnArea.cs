using System;
using Constants;
using Cysharp.Threading.Tasks;
using Module.Player.Component;
using UnityEngine;

namespace Module.Application.Respawn
{
    public class RespawnArea : MonoBehaviour
    {
        [SerializeField] private float respawnTime = 1f;
        private BoxCollider boxCollider;
        private Transform playerTransform;
        private Vector3 spawnPosition;

        private void Start()
        {
            playerTransform = GameObject.FindWithTag(Tag.Player).transform;
            spawnPosition = playerTransform.localPosition;
        }

        private void OnValidate()
        {
            boxCollider = GetComponent<BoxCollider>();
        }

        private void OnTriggerEnter(Collider other)
        {
            // ダメージを与える
            SendDamage(other.gameObject);
        }

        private void SendDamage(GameObject obj)
        {
            if (obj.CompareTag(Tag.Player) &&
                obj.TryGetComponent(out PlayerStatus player))
            {
                player.Damage(1);
                Respawn().Forget();
            }
        }

        private async UniTaskVoid Respawn()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(respawnTime), cancellationToken: destroyCancellationToken);
            playerTransform.localPosition = spawnPosition;
        }

        private void OnDrawGizmos()
        {
            if (boxCollider == null)
            {
                boxCollider = GetComponent<BoxCollider>();
            }

            Gizmos.color = new Color(1f, 0.06f, 0.1f, 0.35f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(boxCollider.center, boxCollider.size);
        }
    }
}