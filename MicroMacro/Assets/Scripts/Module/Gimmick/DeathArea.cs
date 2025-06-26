using System;
using Constants;
using Module.Player.Component;
using UnityEngine;

namespace Module.Gimmick
{
    /// <summary>
    /// 入ると死亡するエリアのコンポーネント
    /// </summary>
    public class DeathArea : MonoBehaviour
    {
        private const int maxDamage = 99999999;
        private BoxCollider boxCollider;

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
                player.Damage(maxDamage);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.06f, 0.1f, 0.35f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(boxCollider.center, boxCollider.size);
        }
    }
}