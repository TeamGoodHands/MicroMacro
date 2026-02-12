using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.1f, 0.5f, 0.8f)]
[TrackClipType(typeof(TextMeshProFadeClip))]
[TrackBindingType(typeof(TMP_Text))]
public class TextMeshProFadeTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        return ScriptPlayable<TextMeshProFadeMixerBehaviour>.Create(graph, inputCount);
    }
}
