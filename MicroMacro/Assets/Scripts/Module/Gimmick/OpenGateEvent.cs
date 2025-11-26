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
        [SerializeField] private PlayableDirector director;
        [Header("ステージ上に配置する木の実")][SerializeField] private GameObject fallNuts;
        [Header("虫に付随し食べられる木の実")][SerializeField] private GameObject nutsForEat;

        private PlayerBehaviour playerBehaviour;
        private bool isTriggered = false;

        private void Start()
        {
            playerBehaviour = GameObject.FindWithTag(Tag.Player).GetComponent<PlayerBehaviour>();
            if (nutsForEat != null && nutsForEat.activeSelf)
            {
                nutsForEat.SetActive(false);
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            if (!gate.activeSelf || isTriggered)
                return;
            
            if (other.gameObject.CompareTag(Tag.Handle.Untagged))
            {
                EatNuts().Forget();
                isTriggered = true;
                SoundManager.instance.Play("スイッチ");
            }
        }

        private async UniTaskVoid EatNuts()
        {
            if (nutsForEat == null)
                return;
            
            playerBehaviour.Component.Condition.IsPlayerLocked = true;
            ChangeActiveNuts();
            // TODO : 木の実を食べるアニメーション再生
            await UniTask.Delay(TimeSpan.FromSeconds(3));  
            
            nutsForEat.SetActive(false);
            DoorBreak().Forget();
        }

        private async UniTaskVoid DoorBreak()
        {
            director.Play();
            await UniTask.Delay(TimeSpan.FromSeconds(3));
            // TODO: ドア破壊アニメーション再生
            gate.SetActive(false);
            
            await UniTask.Delay(TimeSpan.FromSeconds(2));
            playerBehaviour.Component.Condition.IsPlayerLocked = false;
        }

        private void ChangeActiveNuts()
        {
            fallNuts.SetActive(false);
            nutsForEat.SetActive(true);
        }
    }
}