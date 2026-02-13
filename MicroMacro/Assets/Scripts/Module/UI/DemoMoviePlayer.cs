using System;
using System.IO;
using CoreModule.Input;
using DG.Tweening;
using Module.Management;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Video;

namespace Module.UI
{
    [RequireComponent(typeof(VideoPlayer))]
    public class DemoMoviePlayer : MonoBehaviour
    {
        [Header("設定")] [SerializeField] private string videoFileName = "Demo.mp4"; // 拡張子を含めたファイル名
        [SerializeField] private float noInputStartTime = 30f;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private ForeverSelect foreverSelect;
        [SerializeField] private bool isLooping = true;

        private VideoPlayer videoPlayer;
        private float lastInputTime;
        private InputEvent anyKeyAction;
        private bool isPlaying = false;
        private Tween fadeTween;

        private void Awake()
        {
            videoPlayer = GetComponent<VideoPlayer>();
            anyKeyAction = InputProvider.CreateEvent(ActionGuid.UI.AnyButton);
            anyKeyAction.Started += OnAnyKeyAction;
            lastInputTime = Time.time;
        }

        private void OnDestroy()
        {
            if (anyKeyAction != null)
            {
                anyKeyAction.Started -= OnAnyKeyAction;
            }
            fadeTween?.Kill();
        }

        private void OnAnyKeyAction(InputAction.CallbackContext _)
        {
            lastInputTime = Time.time;

            if (isPlaying)
            {
                isPlaying = false;
                fadeTween?.Kill();

                canvasGroup.gameObject.SetActive(true);
                fadeTween = canvasGroup.DOFade(1f, 0.3f).OnComplete(() =>
                {
                    foreverSelect.enabled = true;
                    canvasGroup.interactable = true;
                    videoPlayer.Stop();
                    SoundManager.instance.Play("Title");
                });
            }
        }

        private void Update()
        {
            if (!isPlaying && Time.time - lastInputTime > noInputStartTime)
            {
                Debug.Log("Play Demo Movie");
                PlayMovie();
                isPlaying = true;
            }

            if (EventSystem.current != null && !foreverSelect.enabled)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        public void PlayMovie()
        {
            fadeTween?.Kill();
            canvasGroup.interactable = false;
            foreverSelect.enabled = false;
            
            // SoundManager.StopPlay は typo の可能性が高いですが、元のコードに従います。
            // もしコンパイルエラーになる場合は修正してください。
            SoundManager.instance.StopPlay("Title");

            if (videoPlayer.isPrepared)
            {
                videoPlayer.Play();
                fadeTween = canvasGroup.DOFade(0f, 0.3f).OnComplete(() => canvasGroup.gameObject.SetActive(false));
                return;
            }

            // StreamingAssets内のパスを取得
            string filePath = Path.Combine(UnityEngine.Application.streamingAssetsPath, videoFileName);

            // VideoPlayerの設定
            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = filePath;
            videoPlayer.isLooping = isLooping;

            // 再生準備ができたら再生を開始する（準備が必要な場合）
            videoPlayer.prepareCompleted += OnPrepareCompleted;
            videoPlayer.Prepare();
        }

        private void OnPrepareCompleted(VideoPlayer source)
        {
            source.prepareCompleted -= OnPrepareCompleted;
            
            if (!isPlaying) return;

            source.Play();
            fadeTween?.Kill();
            fadeTween = canvasGroup.DOFade(0f, 0.3f).OnComplete(() => canvasGroup.gameObject.SetActive(false));
            Debug.Log($"再生開始: {source.url}");
        }
    }
}