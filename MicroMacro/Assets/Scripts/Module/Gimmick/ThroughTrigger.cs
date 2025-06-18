using System;
using UnityEngine;
using Constants;
using Module.Player.Component;

namespace Module.Gimmick
{
    public class ThroughTrigger : MonoBehaviour
    {
        public event Action<GameObject> OnTriggerChanged;
        public bool IsTriggered { get; private set; }
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(Tag.Handle.Player))
            {
                // 片方のトリガーでのみプレイヤーのコンディションを取得
                IsTriggered = true;
                OnTriggerChanged?.Invoke(other.gameObject);	
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(Tag.Handle.Player))
            {
                Debug.Log("Trigger Exit");
                IsTriggered = false;
                OnTriggerChanged?.Invoke(other.gameObject);
            }
        }
    }
}