using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Module.Enemy.Hose.SnakeHose.Timeline.Spline
{
    // イージングの種類を定義
    public enum EasingMode
    {
        [Tooltip("設定どおり（S字など）。単発移動用")]
        Default,
        
        [Tooltip("加速のみ（後半の減速をカット）。次のクリップへ繋ぐときに使う")]
        CutOut, 
        
        [Tooltip("減速のみ（前半の加速をカット）。前のクリップから繋がるときに使う")]
        CutIn,

        [Tooltip("等速（加速も減速もしない）。連続移動の中間用")]
        Linear
    }

    [System.Serializable]
    public class SnakeSplineClip : PlayableAsset, ITimelineClipAsset
    {
        public int splineIndex = 0;

        [Header("Transition Settings")]
        [Tooltip("前後のクリップとの繋がり方")]
        public EasingMode easingMode = EasingMode.Default;

        [Header("Range")]
        public float startDistance = 0f;
        public float endDistance = 10f;

        public ClipCaps clipCaps => ClipCaps.Blending;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<SnakeSplineBehaviour>.Create(graph);
            var behaviour = playable.GetBehaviour();
            
            behaviour.splineIndex = splineIndex;
            behaviour.startDistance = startDistance;
            behaviour.endDistance = endDistance;
            behaviour.easingMode = easingMode; // 追加データを渡す

            return playable;
        }
    }
}