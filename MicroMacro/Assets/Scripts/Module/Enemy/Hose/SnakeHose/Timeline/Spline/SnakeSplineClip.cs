using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Module.Enemy.Hose.SnakeHose.Timeline.Spline
{
    [System.Serializable]
    public class SnakeSplineClip : PlayableAsset, ITimelineClipAsset
    {
        [Tooltip("この期間中に適用するスプラインのインデックス")]
        public int splineIndex = 0;

        [Header("Range")]
        [Tooltip("開始時の距離 (m)")]
        public float startDistance = 0f;

        [Tooltip("終了時の距離 (m)")]
        public float endDistance = 10f;

        // クリップをブレンド可能にする
        public ClipCaps clipCaps => ClipCaps.Blending;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<SnakeSplineBehaviour>.Create(graph);
            var behaviour = playable.GetBehaviour();
            
            behaviour.splineIndex = splineIndex;
            behaviour.startDistance = startDistance;
            behaviour.endDistance = endDistance;

            return playable;
        }
    }
}