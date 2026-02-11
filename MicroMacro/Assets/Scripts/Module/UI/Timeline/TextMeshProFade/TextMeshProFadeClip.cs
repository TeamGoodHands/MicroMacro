using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class TextMeshProFadeClip : PlayableAsset, ITimelineClipAsset
{
    public TextMeshProFadeBehaviour template = new TextMeshProFadeBehaviour();

    public ClipCaps clipCaps
    {
        get { return ClipCaps.Blending; }
    }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<TextMeshProFadeBehaviour>.Create(graph, template);
        return playable;
    }
}
