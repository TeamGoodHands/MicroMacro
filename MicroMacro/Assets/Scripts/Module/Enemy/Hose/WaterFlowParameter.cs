using UnityEngine;

namespace Module.Enemy.Hose
{
    public class WaterFlowParameter : MonoBehaviour
    {
        [SerializeField, Header("水流の力")] private Vector2 waterPower;
        [SerializeField, Header("左右に弾くときの倍率")] private float sideForceMultiplier = 0.5f;

        [SerializeField, Header("スケールによって水流を強くするか")]
        private float scaleMultiplier = 1.5f;

        [SerializeField, Header("スケールによってどれだけ水を長くするか")]
        private float lengthMultiplier = 1.5f;

        [SerializeField, Header("プレイヤーには何倍力を強くするか")]
        private float playerMultiplier = 2f;

        [Space] [SerializeField, Header("一定の間隔で発射するか")]
        private bool isLooping;


        [SerializeField, Header("起動時の遅延")] private float firstDelay;

        [SerializeField, Header("オン状態の時間")] private float onTime;

        [SerializeField, Header("オフ状態の時間")] private float offTime;

        [SerializeField, Header("オンになるスピード")] private float waterSpeed;

        [Space] [SerializeField, Header("プレイヤーの方向を向くか")]
        private bool lookAtPlayer;


        [SerializeField, Header("激流時のスクリューの太さ")]
        private float screwWidth;

        [SerializeField, Header("通常時の水の色")] private Color defaultWaterColor;
        [SerializeField, Header("激流時の水の色")] private Color rapidsWaterColor;

        public Vector2 WaterPower => waterPower;
        public float LengthMultiplier => lengthMultiplier;
        public float ScaleMultiplier => scaleMultiplier;
        public float SideForceMultiplier => sideForceMultiplier;
        public float PlayerMultiplier => playerMultiplier;
        public bool IsLooping => isLooping;
        public float FirstDelay => firstDelay;
        public float OnTime => onTime;
        public float OffTime => offTime;
        public float WaterSpeed => waterSpeed;
        public bool LookAtPlayer => lookAtPlayer;
        public float ScrewWidth => screwWidth;
        public Color DefaultWaterColor => defaultWaterColor;
        public Color RapidsWaterColor => rapidsWaterColor;
    }
}