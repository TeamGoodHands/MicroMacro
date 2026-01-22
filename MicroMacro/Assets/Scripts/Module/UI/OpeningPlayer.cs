using System;
using System.IO;
using CoreModule.Input;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Module.Application.SceneSwitch;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Module.UI
{
    public class OpeningPlayer : MonoBehaviour
    {
        [SerializeField, Header("スキップ文字を表示するまでの時間")]
        private float showSkipDelay = 5f;

        [SerializeField, Header("スキップ文字を表示するフェード時間")]
        private float showSkipFadeDuration = 2f;

        [SerializeField, Header("スキップ長押し速度")] private float skipHoldSpeed = 2f;
        [SerializeField, Header("長押しの巻き戻し速度")] private float skipHoldBackwardsSpeed = 1f;

        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private CanvasGroup skipGroup;
        [SerializeField] private Image skipSlider;
        [SerializeField] private FadeAndSceneTransition sceneTransition;
        [SerializeField] private string videoFileName;

        private InputEvent submitEvent;
        private bool isSkipping;
        private float skipProgress;

        private void Start()
        {
            // URL指定
            videoPlayer.source = VideoSource.Url;

            // StreamingAssetsフォルダ配下のパスの動画をURLとして指定する
            videoPlayer.url = Path.Combine(UnityEngine.Application.streamingAssetsPath, videoFileName);

            // Videoプレイヤーの準備完了を待つ
            videoPlayer.prepareCompleted += OnPrepareCompleted;
            videoPlayer.Prepare();

            submitEvent = InputProvider.CreateEvent(ActionGuid.UI.Submit);
            submitEvent.Started += OnCancelStart;
            submitEvent.Canceled += OnCancelStop;

            ShowSkip().Forget();
        }

        private void OnPrepareCompleted(VideoPlayer source)
        {
            // 準備完了したら再生する
            PlayMovie().Forget();
        }

        private async UniTaskVoid PlayMovie()
        {
            // 動画再生
            videoPlayer.Play();

            await UniTask.WaitWhile(IsPlaying, cancellationToken: destroyCancellationToken);

            Unbind();

            // 再生が終わったら次のシーンへ行く
            sceneTransition.StartTransition();
        }

        private async UniTaskVoid ShowSkip()
        {
            // 初めは透明
            skipGroup.alpha = 0f;

            UniTask delayTask = UniTask.Delay(TimeSpan.FromSeconds(showSkipDelay), cancellationToken: destroyCancellationToken);
            UniTask doCancelWaitTask = UniTask.WaitUntil(() => isSkipping, cancellationToken: destroyCancellationToken);

            // n秒経過 or スキップボタンが押されるまで待つ
            await UniTask.WhenAny(delayTask, doCancelWaitTask);

            _ = skipGroup.DOFade(1f, showSkipFadeDuration);
        }

        private bool IsPlaying()
        {
            return videoPlayer.frame < ((long)videoPlayer.frameCount - 1);
        }

        private void Update()
        {
            if (!IsPlaying())
                return;

            float speed = isSkipping ? skipHoldSpeed : skipHoldBackwardsSpeed;

            skipProgress += Time.deltaTime * speed;
            skipProgress = Mathf.Clamp01(skipProgress);
            skipSlider.fillAmount = skipProgress;

            if (skipProgress >= 1f)
            {
                videoPlayer.Stop();
            }
        }

        private void OnCancelStart(InputAction.CallbackContext _)
        {
            isSkipping = true;
        }

        private void OnCancelStop(InputAction.CallbackContext _)
        {
            isSkipping = false;
        }

        private void Unbind()
        {
            if (submitEvent != null)
            {
                submitEvent.Started -= OnCancelStart;
                submitEvent.Canceled -= OnCancelStop;
            }
        }
    }
}