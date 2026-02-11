using System;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]
public class TextMeshProFadeBehaviour : PlayableBehaviour
{
    [Range(0f, 1f)]
    public float alpha = 1f;

    public Color color = Color.white;
    public bool overrideColor = false;
}
