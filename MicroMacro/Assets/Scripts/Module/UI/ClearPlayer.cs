using System;
using Constants;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Module.Application.SceneSwitch;
using Module.Management;
using Module.Player;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Module.UI
{
    public class ClearPlayer : MonoBehaviour
    {
        [SerializeField] private string videoName;
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private Image clearImage;
        [SerializeField] private PlayableDirector playableDirector;
        [SerializeField] private FadeAndSceneTransition fadeAndSceneTransition;
        [SerializeField] private CinemachineCamera clearCamera;
        [SerializeField] private GameObject[] disableObjects;

        private Vector3 spawnPosition;
        private Transform playerTransform;
        private Transform animatorTransform;

        private void Start()
        {
            videoPlayer.url = System.IO.Path.Combine(UnityEngine.Application.streamingAssetsPath, videoName);
            videoPlayer.Prepare();

            GameObject playerObject = GameObject.FindWithTag(Tag.Player);
            animatorTransform = playerObject.GetComponent<PlayerBehaviour>().Component.BodyTransform;
            playerTransform = playerObject.transform;
            spawnPosition = playerTransform.localPosition;
        }

        public async UniTask ClearEffect()
        {
            await clearImage.DOFade(1f, 1f);

            videoPlayer.Play();
            clearImage.color = Color.clear;

            foreach (GameObject disableObject in disableObjects)
            {
                disableObject.SetActive(false);
            }

            playerTransform.localPosition = spawnPosition;
            playerTransform.localRotation = Quaternion.identity;
            animatorTransform.localEulerAngles = new Vector3(0f, 180f, 0f);
            animatorTransform.localScale = Vector3.one;

            await UniTask.WaitWhile(IsPlaying, cancellationToken: destroyCancellationToken);

            await DOTween
                .To(() => videoPlayer.targetCameraAlpha, x => videoPlayer.targetCameraAlpha = x, 0f, 1f)
                .SetLink(videoPlayer.gameObject);

            clearCamera.Priority = 10000;

            await UniTask.Delay(TimeSpan.FromSeconds(0.8f), cancellationToken: destroyCancellationToken);

            SoundManager.instance.Play("クリア長め");

            playableDirector.Play();

            await UniTask.Delay(TimeSpan.FromSeconds(9f), cancellationToken: destroyCancellationToken);

            fadeAndSceneTransition.StartTransition();
        }

        private bool IsPlaying()
        {
            return videoPlayer.frame < ((long)videoPlayer.frameCount - 1);
        }
    }
}