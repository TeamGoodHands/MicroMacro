using UnityEngine.Playables;

namespace Module.Enemy.Hose.SnakeHose.Timeline.Spline
{
    // データを運ぶだけのクラス
    public class SnakeSplineBehaviour : PlayableBehaviour
    {
        public int splineIndex;
        public float startDistance;
        public float endDistance;
    }
}