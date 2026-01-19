using System;
using Constants;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Module.Application.SceneSwitch;
using Module.Management;
using Module.Player.Component;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;

namespace Module.UI
{
    public class BossGoalPlayer : MonoBehaviour
    {
        [SerializeField] private PlayableDirector playableDirector;
        [SerializeField] private FadeAndSceneTransition fadeAndSceneTransition;
        [SerializeField] private CinemachineCamera clearCamera;
        [SerializeField] private bool lockPlayer;

        private PlayerCondition playerCondition;

        private void Start()
        {
            playerCondition = GameObject.FindWithTag(Tag.Player).GetComponent<PlayerCondition>();
        }

        public async UniTaskVoid Play()
        {
            if (lockPlayer)
            {
                playerCondition.IsPlayerLocked = true;
            }

            clearCamera.Priority = 10000;

            await UniTask.Delay(TimeSpan.FromSeconds(0.8f), cancellationToken: destroyCancellationToken);

            SoundManager.instance.Play("クリア長め");

            playableDirector.Play();

            await UniTask.Delay(TimeSpan.FromSeconds(9f), cancellationToken: destroyCancellationToken);

            fadeAndSceneTransition.StartTransition();
        }
    }
}