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
        [SerializeField] private FadeAndSceneTransition fadeAndSceneTransition;
        [SerializeField] private GameObject[] disableObjects;

        private void Start()
        {
            videoPlayer.url = System.IO.Path.Combine(UnityEngine.Application.streamingAssetsPath, videoName);
            videoPlayer.Prepare();
        }

        public async UniTask ClearEffect()
        {
            await clearImage.DOFade(1f, 1f);
            
            await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: destroyCancellationToken);
            
            videoPlayer.Play();
            
            await clearImage.DOFade(0f, 1f);
            
            await UniTask.WaitWhile(IsPlaying, cancellationToken: destroyCancellationToken);

            fadeAndSceneTransition.StartNormalTransition();
        }

        private bool IsPlaying()
        {
            return videoPlayer.frame < ((long)videoPlayer.frameCount - 1);
        }
    }
}