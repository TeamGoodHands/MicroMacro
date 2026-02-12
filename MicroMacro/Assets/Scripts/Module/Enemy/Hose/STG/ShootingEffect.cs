using DG.Tweening;
using Module.Enemy.Hose.SnakeHose;
using PropertyGenerator.Generated;
using UnityEngine;

namespace Module.Enemy.Hose.STG
{
    /// <summary>
    /// 射撃中にマテリアルのプロパティ（エフェクト）を有効にするコンポーネント。
    /// ShootingGame の状態を参照して実行する。
    /// </summary>
    public class ShootingEffect : MonoBehaviour
    {
        [SerializeField] private Renderer bodyRenderer;

        [Header("Effect Settings")]
        [SerializeField] private Color shootingColor = new Color(0f, 1f, 1f, 1f); // Cyan default
        [SerializeField] private float fadeInDuration = 0.02f;
        [SerializeField] private float fadeOutDuration = 0.2f;
        [SerializeField] private float waveSpeed = 5f;
        [SerializeField] private float wavePower = 0.5f;
        [SerializeField] private float shootingOutlineWidth = 2f;
        [SerializeField] private Color shootingOutlineColor = new Color(0f, 1f, 1f, 1f); // Cyan default

        private ScalerShaderWrapper shaderWrapper;
        private ShootingGame shootingGame;
        private LastBeamBattle lastBeamBattle;
        private EffectState currentState;
        private Sequence currentSequence;
        
        private enum EffectState
        {
            Default,
            Active,
            Disabled
        }
        
        // 元のマテリアル設定を保持
        private float defaultWaveSpeed;
        private float defaultWavePower;
        private Color defaultFresnelColor;
        private float defaultOutlineWidth;
        private Color defaultOutlineColor;

        private void Start()
        {
            if (bodyRenderer == null)
            {
                Debug.LogError("Body Renderer is not assigned!", this);
                enabled = false;
                return;
            }

            // ShootingGameの参照を取得
            shootingGame = GetComponentInParent<ShootingGame>();
            if (shootingGame == null)
            {
                // 親に見つからない場合はシーン全体から検索
                shootingGame = FindFirstObjectByType<ShootingGame>();
            }

            if (shootingGame == null)
            {
                Debug.LogError("ShootingGame not found!", this);
                enabled = false;
                return;
            }

            // LastBeamBattleの参照を取得（あれば）
            lastBeamBattle = FindFirstObjectByType<LastBeamBattle>();

            // ShaderWrapperの初期化（プロパティIDへのアクセス用）
            shaderWrapper = new ScalerShaderWrapper(bodyRenderer.material);

            // 初期値を保存
            defaultWaveSpeed = shaderWrapper.WaveSpeed;
            defaultWavePower = shaderWrapper.WavePower;
            defaultFresnelColor = shaderWrapper.FresnelColor;
            defaultOutlineWidth = shaderWrapper.OutlineWidth;
            defaultOutlineColor = shaderWrapper.OutlineColor;
        }

        private void OnDestroy()
        {
            currentSequence?.Kill();
        }

        private void Update()
        {
            EffectState targetState = EffectState.Default;

            // 優先度高: ビームバトルでForceKillされた場合は無効化（透明にする）
            if (lastBeamBattle != null && lastBeamBattle.IsForceKilled)
            {
                targetState = EffectState.Disabled;
            }
            // 次点: シューティングでDestroyAllされた場合（射撃不可）はデフォルトに戻す
            else if (shootingGame != null && !shootingGame.CanPlayerShoot)
            {
                targetState = EffectState.Default;
            }
            else
            {
                // エフェクト有効化条件
                // 1. ボタンが押されていること (ShootingGame.IsShooting)
                // 2. 以下のいずれかの状態であること
                //    a. STGモードで射撃可能 (ShootingGame.CanPlayerShoot) -> 上のifでチェック済み
                //    b. ビームバトル中 (LastBeamBattle.State == Battle)

                bool isInputting = shootingGame != null && shootingGame.IsShooting;
                bool isBeamBattle = lastBeamBattle != null && lastBeamBattle.State == LastBeamBattle.BattleState.Battle;
                
                if (isInputting && (isBeamBattle || (shootingGame != null && shootingGame.CanPlayerShoot)))
                {
                    targetState = EffectState.Active;
                }
            }

            // 状態が変化した場合のみエフェクトを更新
            if (targetState != currentState)
            {
                currentState = targetState;
                PlayEffect(currentState);
            }
        }

        private void PlayEffect(EffectState state)
        {
            currentSequence?.Kill();
            currentSequence = DOTween.Sequence();

            switch (state)
            {
                case EffectState.Active:
                    // エフェクト有効化（即座にフェードイン）
                    currentSequence.Join(DOTween.To(() => shaderWrapper.FresnelColor, x => shaderWrapper.FresnelColor = x, shootingColor, fadeInDuration));
                    currentSequence.Join(DOTween.To(() => shaderWrapper.WaveSpeed, x => shaderWrapper.WaveSpeed = x, waveSpeed, fadeInDuration));
                    currentSequence.Join(DOTween.To(() => shaderWrapper.WavePower, x => shaderWrapper.WavePower = x, wavePower, fadeInDuration));
                    currentSequence.Join(DOTween.To(() => shaderWrapper.OutlineWidth, x => shaderWrapper.OutlineWidth = x, shootingOutlineWidth, fadeInDuration));
                    currentSequence.Join(DOTween.To(() => shaderWrapper.OutlineColor, x => shaderWrapper.OutlineColor = x, shootingOutlineColor, fadeInDuration));
                    break;

                case EffectState.Disabled:
                    // エフェクト完全無効化（透明・０にする）
                    currentSequence.Join(DOTween.To(() => shaderWrapper.FresnelColor, x => shaderWrapper.FresnelColor = x, Color.clear, fadeOutDuration));
                    currentSequence.Join(DOTween.To(() => shaderWrapper.OutlineWidth, x => shaderWrapper.OutlineWidth = x, 0f, fadeOutDuration));
                    // Waveなどはデフォルトに戻しておくのが無難か、あるいは影響ないので放置でも良いが統一感のためデフォルトへ
                    currentSequence.Join(DOTween.To(() => shaderWrapper.WaveSpeed, x => shaderWrapper.WaveSpeed = x, defaultWaveSpeed, fadeOutDuration));
                    currentSequence.Join(DOTween.To(() => shaderWrapper.WavePower, x => shaderWrapper.WavePower = x, defaultWavePower, fadeOutDuration));
                    currentSequence.Join(DOTween.To(() => shaderWrapper.OutlineColor, x => shaderWrapper.OutlineColor = x, defaultOutlineColor, fadeOutDuration));
                    break;

                case EffectState.Default:
                default:
                    // デフォルトへ戻す
                    currentSequence.Join(DOTween.To(() => shaderWrapper.FresnelColor, x => shaderWrapper.FresnelColor = x, defaultFresnelColor, fadeOutDuration));
                    currentSequence.Join(DOTween.To(() => shaderWrapper.WaveSpeed, x => shaderWrapper.WaveSpeed = x, defaultWaveSpeed, fadeOutDuration));
                    currentSequence.Join(DOTween.To(() => shaderWrapper.WavePower, x => shaderWrapper.WavePower = x, defaultWavePower, fadeOutDuration));
                    currentSequence.Join(DOTween.To(() => shaderWrapper.OutlineWidth, x => shaderWrapper.OutlineWidth = x, defaultOutlineWidth, fadeOutDuration));
                    currentSequence.Join(DOTween.To(() => shaderWrapper.OutlineColor, x => shaderWrapper.OutlineColor = x, defaultOutlineColor, fadeOutDuration));
                    break;
            }
        }
    }
}
