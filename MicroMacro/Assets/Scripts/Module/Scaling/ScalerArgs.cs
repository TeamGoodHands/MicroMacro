
using UnityEngine;

namespace Module.Scaling
{
    public struct ScalerArgs
    {
        public Vector3 TargetScale; // 目標スケール値
        public Vector3 PositionOffset; // 前の地点からの座標の差分
        public float Duration; // スケール時間
    }
}