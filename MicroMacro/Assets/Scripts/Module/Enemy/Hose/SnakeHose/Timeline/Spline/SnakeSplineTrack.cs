using Module.Enemy.Hose.SnakeHose.Timeline.Spline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Module.Enemy.Hose.SnakeHose.Timeline
{
    [TrackColor(0.0f, 0.8f, 0.4f)]
    [TrackClipType(typeof(SnakeSplineClip))]
    [TrackBindingType(typeof(SnakeController))]
    public class SnakeSplineTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<SnakeSplineMixerBehaviour>.Create(graph, inputCount);
        }
    }
}