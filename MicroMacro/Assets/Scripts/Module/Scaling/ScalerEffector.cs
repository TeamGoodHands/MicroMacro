using System;
using ChameleonOutline;
using DG.Tweening;
using Module.Management;
using PostProcessing.ChameleonOutline;
using PropertyGenerator.Generated;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.VFX;

namespace Module.Scaling
{
    public class ScalerEffector : MonoBehaviour
    {
        [SerializeField] private Scaler scaler;
        [SerializeField] private Renderer bodyRenderer;
        [SerializeField] private ScaleFaceWrapper scaleFaceWrapper;
        [SerializeField] private Animator scaleAnimator;
        [SerializeField] private ScaleEffectProfile profile;
        [SerializeField] private VisualEffect sparkEffect;

        private ScalerShaderWrapper scalerShaderWrapper;
        private Tween currentTween;
        private float defaultWaveSpeed;
        private float defaultWavePower;
        private int outlineStyleHandle;

        private void Start()
        {
            try
            {
                scaler.OnScaleStarted += OnScaleStarted;
                scaler.OnScaleResumed += Resume;
                scaler.OnScalePaused += Pause;
                scalerShaderWrapper = new ScalerShaderWrapper(bodyRenderer.material);

                // 初期値を登録
                defaultWaveSpeed = scalerShaderWrapper.WaveSpeed;
                defaultWavePower = scalerShaderWrapper.WavePower;
                
                scalerShaderWrapper.OutlineStencilComp = (float)CompareFunction.Never;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                Debug.Log(gameObject.name, this);
                throw;
            }
        }

        private void OnScaleStarted(ScaleEventArgs args)
        {
            bool isValid = args.PreviousStep != args.CurrentStep;
            bool isMacro = isValid && args.CurrentStep - args.PreviousStep > 0;

            Effect(isValid, isMacro, args.Duration);
        }

        private void Pause()
        {
            currentTween?.Pause();
        }

        private void Resume(bool isForwards, bool isMacro)
        {
            if (isForwards)
            {
                currentTween?.Play();
            }
            else
            {
                float progressDuration = currentTween.Duration() - currentTween.Elapsed();
                Effect(true, isMacro, progressDuration);
            }
        }

        private void Effect(bool isValid, bool isMacro, float duration)
        {
            // 実行中のTweenとパラメータをリセット
            currentTween?.Kill();
            ResetMaterial();

            if (isValid)
            {
                // 拡大縮小に成功した
                currentTween = CreateScaleTween(isMacro, duration);
                scaleFaceWrapper.SetScaleTrigger();

                scaleAnimator.Play("Scale");
                // sparkEffect.Play();
            }
            else
            {
                // 拡大縮小に失敗した
                currentTween = CreateInvalidScaleTween();
            }

            if (enabled)
            {
                // SEの再生
                PlaySound();
            }
        }

        private Tween CreateScaleTween(bool isMacro, float scaleDuration)
        {
            Color fresnelColor = isMacro ? profile.MacroFresnelColor : profile.MicroFresnelColor;
            Color outlineColor = isMacro ? profile.MacroOutlineColor : profile.MicroOutlineColor;

            scalerShaderWrapper.OutlineColor = outlineColor;

            float progress = 0f;
            Sequence sequence = DOTween.Sequence();

            sequence.AppendCallback(() => { scalerShaderWrapper.OutlineStencilComp = (float)CompareFunction.Always; });

            // 拡大中
            sequence.Append(DOTween.To(() => progress, value =>
            {
                scalerShaderWrapper.FresnelColor = Color.Lerp(Color.clear, fresnelColor, value);
                scalerShaderWrapper.OutlineWidth = Mathf.Lerp(0f, profile.OutlineWidth, value);
                progress = value;
            }, 1f, profile.OutlineTweenTime));

            sequence.AppendCallback(() => progress = 0f);

            // 拡大終了まで待機
            sequence.AppendInterval(Mathf.Max(0f, scaleDuration - profile.OutlineTweenTime));

            // 拡大縮小エフェクトが消えるのを少し遅延させる
            sequence.AppendInterval(profile.OutlineDisappearWaitTime);

            // 拡大エフェクトをだんだん消す
            sequence.Append(DOTween.To(() => progress, value =>
                {
                    scalerShaderWrapper.FresnelColor = Color.Lerp(fresnelColor, Color.clear, value);
                    scalerShaderWrapper.OutlineWidth = Mathf.Lerp(profile.OutlineWidth, 0f, value);
                    progress = value;
                }, 1f, profile.OutlineDisappearTime))
                .SetEase(Ease.InSine);
            
            sequence.AppendCallback(() => { scalerShaderWrapper.OutlineStencilComp = (float)CompareFunction.Never; });
            
            return sequence;
        }

        private Tween CreateInvalidScaleTween()
        {
            float progress = 0f;
            Sequence sequence = DOTween.Sequence();

            // 失敗エフェクトだんだん適用する
            sequence.Append(DOTween.To(() => progress, value =>
            {
                scalerShaderWrapper.WaveSpeed = Mathf.Lerp(defaultWaveSpeed, profile.InvalidWaveSpeed, value);
                scalerShaderWrapper.WavePower = Mathf.Lerp(defaultWavePower, profile.InvalidWavePower, value);
                scalerShaderWrapper.FresnelColor = Color.Lerp(Color.clear, profile.InvalidFresnelColor, value);
            }, 1f, profile.InvalidWaveTime / 2f));

            sequence.AppendCallback(() => progress = 0f);

            // 失敗エフェクトだんだん消す
            sequence.Append(DOTween.To(() => progress, value =>
            {
                scalerShaderWrapper.WaveSpeed = Mathf.Lerp(profile.InvalidWaveSpeed, defaultWaveSpeed, value);
                scalerShaderWrapper.WavePower = Mathf.Lerp(profile.InvalidWavePower, defaultWavePower, value);
                scalerShaderWrapper.FresnelColor = Color.Lerp(profile.InvalidFresnelColor, Color.clear, value);
            }, 1f, profile.InvalidWaveTime / 2f));

            return sequence;
        }

        private void ResetMaterial()
        {
            scalerShaderWrapper.OutlineWidth = 0f;
            scalerShaderWrapper.FresnelColor = Color.clear;
            scalerShaderWrapper.WaveSpeed = defaultWaveSpeed;
            scalerShaderWrapper.WavePower = defaultWavePower;
        }

        private void PlaySound()
        {
            bool isUpScaling = scaler.CurrentStep > scaler.PreviousStep;
            bool isDownScaling = scaler.CurrentStep < scaler.PreviousStep;

            if (isUpScaling)
            {
                SoundManager.instance.Play("拡大");
            }
            else if (isDownScaling)
            {
                SoundManager.instance.Play("縮小");
            }
            else
            {
                SoundManager.instance.Play("拡縮失敗");
            }
        }
    }
}