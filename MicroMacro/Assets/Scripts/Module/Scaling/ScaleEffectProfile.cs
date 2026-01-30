using UnityEngine;

namespace Module.Scaling
{
    [CreateAssetMenu(fileName = "ScaleEffectProfile", menuName = "ScriptableObjects/ScaleEffectProfile", order = 0)]
    public class ScaleEffectProfile : ScriptableObject
    {
        [SerializeField, Header("デフォルト時のアウトライン幅")] private float defaultOutlineWidth = 0.06f;
        [SerializeField, Header("効果発動時のアウトライン幅")] private float outlineWidth = 0.01f;

        [Header("拡大縮小成功時ののフレネルとアウトラインの色")]
        [SerializeField, ColorUsage(true, true)]
        private Color macroFresnelColor;

        [SerializeField, ColorUsage(true, true)] private Color defaultOutlineColor;
        [SerializeField, ColorUsage(true, true)] private Color macroOutlineColor;
        [SerializeField, ColorUsage(true, true)] private Color microFresnelColor;
        [SerializeField, ColorUsage(true, true)] private Color microOutlineColor;
        
        [SerializeField, Header("アウトラインが出現する時間")] private float outlineTweenTime = 0.05f;
        [SerializeField, Header("アウトラインが消滅する時間")] private float outlineDisappearTime = 0.25f;
        [SerializeField, Header("アウトラインが消滅するまで待機する時間")] private float outlineDisappearWaitTime = 1.1f;

        [Header("拡大縮小失敗時のフレネルの色")]
        [SerializeField, ColorUsage(true, true)]
        private Color invalidFresnelColor;

        [SerializeField, Header("失敗時の震えるスピード")] private float invalidWaveSpeed = 10f;
        [SerializeField, Header("失敗時の震える力")] private float invalidWavePower = 0.6f;
        [SerializeField, Header("失敗時の震える時間")] private float invalidWaveTime = 0.6f;

        public float DefaultOutlineWidth => defaultOutlineWidth;
        public float OutlineWidth => outlineWidth;

        public Color DefaultOutlineColor => defaultOutlineColor;
        public Color MacroFresnelColor => macroFresnelColor;
        public Color MacroOutlineColor => macroOutlineColor;
        public Color MicroFresnelColor => microFresnelColor;
        public Color MicroOutlineColor => microOutlineColor;
        public Color InvalidFresnelColor => invalidFresnelColor;

        public float OutlineTweenTime => outlineTweenTime;
        public float OutlineDisappearTime => outlineDisappearTime;
        public float OutlineDisappearWaitTime => outlineDisappearWaitTime;
        public float InvalidWaveSpeed => invalidWaveSpeed;
        public float InvalidWavePower => invalidWavePower;
        public float InvalidWaveTime => invalidWaveTime;
    }
}