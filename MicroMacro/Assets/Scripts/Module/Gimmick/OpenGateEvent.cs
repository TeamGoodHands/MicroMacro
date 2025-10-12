using System;
using Constants;
using Cysharp.Threading.Tasks;
using Module.Management;
using Module.Player;
using UnityEngine;
using UnityEngine.Playables;

namespace Module.Gimmick
{
    public class OpenGateEvent : MonoBehaviour
    {
        [SerializeField] private GameObject gate;
        [SerializeField] private PlayerCamera playerCamera;
        [SerializeField] private PlayableDirector director;

        private PlayerBehaviour playerBehaviour;
        private bool isTriggered = false;

        private void Start()
        {
            playerBehaviour = GameObject.FindWithTag(Tag.Player).GetComponent<PlayerBehaviour>();
        }

        private void OnCollisionEnter(Collision other)
        {
            if (!gate.activeSelf || isTriggered)
                return;
            
            if (other.gameObject.CompareTag(Tag.Handle.Untagged))
            {
                OpenGate().Forget();
                isTriggered = true;
                SoundManager.instance.Play("スイッチ");
            }
        }

        private async UniTaskVoid OpenGate()
        {
            playerBehaviour.Component.Condition.IsPlayerLocked = true;
            playerCamera.SetLockState(true);
            director.Play();
            
            await UniTask.Delay(TimeSpan.FromSeconds(3));
            
            gate.SetActive(false);
            
            await UniTask.Delay(TimeSpan.FromSeconds(2));
            
            playerBehaviour.Component.Condition.IsPlayerLocked = false;
            playerCamera.SetLockState(false);
        }
    }
}