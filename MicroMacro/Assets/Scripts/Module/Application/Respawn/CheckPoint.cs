using System;
using Constants;
using UnityEngine;

namespace Module.Application.Respawn
{
    public class CheckPoint : MonoBehaviour
    {
        [SerializeField] private bool isChecked;
        public event Action OnPlayerArrived;

        private void OnTriggerEnter(Collider other)
        {
            // チェック済みであればイベントを送信しない
            if (isChecked)
                return;
            
            if (other.CompareTag(Tag.Handle.Player))
            {
                OnPlayerArrived?.Invoke();
                isChecked = true;
            }
        }
    }
}
