using UnityEngine.Playables;

namespace Module.Enemy.Hose.SnakeHose.Timeline.Spline
{
    public class SnakeSplineBehaviour : PlayableBehaviour
    {
        public int splineIndex;
        public float startDistance;
        public float endDistance;
        public EasingMode easingMode; // 追加
    }
}