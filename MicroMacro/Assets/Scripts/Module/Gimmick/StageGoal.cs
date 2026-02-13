using System;
using Constants;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Module.Application.SceneSwitch;
using Module.Management;
using Module.Player;
using Module.Player.Component;
using Module.Scaling;
using PropertyGenerator.Generated;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.VFX;
using Module.Application.Data;
using Module.Application.Dialogue;
using UnityEngine.Playables;


namespace Module.Gimmick
{
    public class StageGoal : MonoBehaviour
    {
        [SerializeField] private Scaler scaler;
        [SerializeField] private VisualEffect splashEffect;
        [SerializeField] private Transform bodyTransform;
        [SerializeField] private GoalAnimatorWrapper animatorWrapper;
        [SerializeField] private CinemachineCamera goalCamera;
        [SerializeField] private FadeAndSceneTransition fadeAndSceneTransition;
        [SerializeField] private GameObject avoidAreaCamera;
        [SerializeField] private GameObject activationObject;
        [SerializeField] private DialogueManager dialogueManager;
        [SerializeField] private PlayableDirector goalPlayableDirector;
        [Header("このステージのID (例: 1-1)")]
        [SerializeField] private string currentStageId;
        

        private PlayerControllerWrapper playerController;
        private PlayerCondition playerCondition;
        private Transform playerBody;
        private PlayBGM playBGM;

        private void Start()
        {
            scaler.OnScaleStarted += OnScaleStarted;

            GameObject playerObject = GameObject.FindWithTag(Tag.Player);
            PlayerBehaviour playerBehaviour = playerObject.GetComponent<PlayerBehaviour>();
            playerController = playerBehaviour.Component.AnimatorWrapper;
            playerBody = playerBehaviour.Component.BodyTransform;
            playerCondition = playerBehaviour.Component.Condition;
        }

        private void OnScaleStarted(ScaleEventArgs args)
        {
            if (args.CurrentStep == args.PreviousStep)
                return;

            int step = args.CurrentStep - args.PreviousStep;

            animatorWrapper.Scale += step;

            if (step > 0)
            {
                SoundManager.instance.Play("ボス拡大");
            }

            if (animatorWrapper.Scale == 3)
            {
                DestroyGoal().Forget();
            }
        }

        private async UniTaskVoid DestroyGoal()
        {
            dialogueManager.AbortDialogue(false);
            if (activationObject != null)
            {
                activationObject.SetActive(true);
            }
            
            SoundManager.instance.Play("ボスカタカタ");
            await bodyTransform.DOShakePosition(3f, strength: 0.003f, vibrato: 40).WithCancellation(this.GetCancellationTokenOnDestroy());

            Time.timeScale = 0.5f;
            SoundManager.instance.Play("ボス爆発");
            splashEffect.Play();

            await transform.DOScale(Vector3.zero, 0.4f).SetEase(Ease.InBack).SetUpdate(true).WithCancellation(this.GetCancellationTokenOnDestroy());

            await UniTask.Delay(TimeSpan.FromSeconds(1.6f), cancellationToken: this.GetCancellationTokenOnDestroy(), ignoreTimeScale: true);

            CinemachineCore.SoloCamera = null;
            goalCamera.Priority = 10000;
            playerCondition.IsPlayerLocked = true;

            if (avoidAreaCamera != null)
            {
                avoidAreaCamera.SetActive(false);
            }

            playBGM = FindAnyObjectByType<PlayBGM>();
            playBGM.BGMSource.DOFade(0f, 0.5f);
            await UniTask.Delay(TimeSpan.FromSeconds(1.6f), cancellationToken: this.GetCancellationTokenOnDestroy(), ignoreTimeScale: true);

            Time.timeScale = 1f;

            SoundManager.instance.Play("クリア短め");

            playerCondition.transform.rotation = Quaternion.identity;
            playerBody.localScale = Vector3.one;
            goalPlayableDirector.Play();

            await UniTask.Delay(TimeSpan.FromSeconds(6f), cancellationToken: this.GetCancellationTokenOnDestroy());

            BackToStageSelect();
        }
        
        private void BackToStageSelect()
        {
            if (SaveManager.Instance != null && !string.IsNullOrEmpty(currentStageId))
            {
                SaveManager.Instance.SetStageCleared(currentStageId);
            }
            else
            {
                Debug.LogWarning("SaveManagerが無いか、StageIDが空です");
            }

            // ステージセレクト画面へ戻る
            if (fadeAndSceneTransition != null)
             fadeAndSceneTransition.StartPageFlipTransition("StageSelect");
        }
    }
}