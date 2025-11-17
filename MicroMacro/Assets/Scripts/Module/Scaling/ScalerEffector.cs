using System;
using ChameleonOutline;
using DG.Tweening;
using Module.Management;
using PostProcessing.ChameleonOutline;
using PropertyGenerator.Generated;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Module.Scaling
{
    public class ScalerEffector : MonoBehaviour
    {
        [SerializeField] private Scaler scaler;
        [SerializeField] private Renderer bodyRenderer;
        [SerializeField] private ScaleFaceWrapper scaleFaceWrapper;
        [SerializeField] private Animator scaleAnimator;
        [SerializeField, Header("効果発動時のアウトライン幅")] private float outlineWidth = 0.01f;

        [Header("拡大縮小成功時ののフレネルとアウトラインの色")]
        [SerializeField, ColorUsage(true, true)] private Color macroFresnelColor;

        [SerializeField, ColorUsage(true, true)] private Color macroOutlineColor;
        [SerializeField, ColorUsage(true, true)] private Color microFresnelColor;
        [SerializeField, ColorUsage(true, true)] private Color microOutlineColor;

        [Header("拡大縮小失敗時のフレネルの色")]
        [SerializeField, ColorUsage(true, true)] private Color invalidFresnelColor;

        [SerializeField, Header("失敗時の震えるスピード")] private float invalidWaveSpeed = 10f;
        [SerializeField, Header("失敗時の震える力")] private float invalidWavePower = 0.6f;
        [SerializeField, Header("失敗時の震える時間")] private float invalidWaveTime = 0.6f;

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
            Color fresnelColor = isMacro ? macroFresnelColor : microFresnelColor;
            Color outlineColor = isMacro ? macroOutlineColor : microOutlineColor;

            scalerShaderWrapper.OutlineColor = outlineColor;

            const float tweenTime = 0.05f;
            const float disappearTime = 0.25f;
            const float disappearWaitTime = 1.2f;

            float progress = 0f;
            Sequence sequence = DOTween.Sequence();

            // 拡大中
            sequence.Append(DOTween.To(() => progress, value =>
            {
                scalerShaderWrapper.FresnelColor = Color.Lerp(Color.clear, fresnelColor, value);
                scalerShaderWrapper.OutlineWidth = Mathf.Lerp(0f, outlineWidth, value);
                progress = value;
            }, 1f, tweenTime));

            sequence.AppendCallback(() => progress = 0f);

            // 拡大終了まで待機
            sequence.AppendInterval(Mathf.Max(0f, scaleDuration - tweenTime));

            // 拡大縮小エフェクトが消えるのを少し遅延させる
            sequence.AppendInterval(disappearWaitTime);

            // 拡大エフェクトをだんだん消す
            sequence.Append(DOTween.To(() => progress, value =>
            {
                scalerShaderWrapper.FresnelColor = Color.Lerp(fresnelColor, Color.clear, value);
                scalerShaderWrapper.OutlineWidth = Mathf.Lerp(outlineWidth, 0f, value);
                progress = value;
            }, 1f, disappearTime)).SetEase(Ease.InSine);
            return sequence;
        }

        private Tween CreateInvalidScaleTween()
        {
            float progress = 0f;
            Sequence sequence = DOTween.Sequence();

            // 失敗エフェクトだんだん適用する
            sequence.Append(DOTween.To(() => progress, value =>
            {
                scalerShaderWrapper.WaveSpeed = Mathf.Lerp(defaultWaveSpeed, invalidWaveSpeed, value);
                scalerShaderWrapper.WavePower = Mathf.Lerp(defaultWavePower, invalidWavePower, value);
                scalerShaderWrapper.FresnelColor = Color.Lerp(Color.clear, invalidFresnelColor, value);
            }, 1f, invalidWaveTime / 2));

            sequence.AppendCallback(() => progress = 0f);

            // 失敗エフェクトだんだん消す
            sequence.Append(DOTween.To(() => progress, value =>
            {
                scalerShaderWrapper.WaveSpeed = Mathf.Lerp(invalidWaveSpeed, defaultWaveSpeed, value);
                scalerShaderWrapper.WavePower = Mathf.Lerp(invalidWavePower, defaultWavePower, value);
                scalerShaderWrapper.FresnelColor = Color.Lerp(invalidFresnelColor, Color.clear, value);
            }, 1f, invalidWaveTime / 2));

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