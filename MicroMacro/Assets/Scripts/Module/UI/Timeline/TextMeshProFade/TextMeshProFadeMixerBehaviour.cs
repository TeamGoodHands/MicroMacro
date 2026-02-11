using TMPro;
using UnityEngine;
using UnityEngine.Playables;

public class TextMeshProFadeMixerBehaviour : PlayableBehaviour
{
    private Color m_DefaultColor;
    private float m_DefaultAlpha;
    private TMP_Text m_TrackBinding;
    private bool m_FirstFrameHappened;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        m_TrackBinding = playerData as TMP_Text;

        if (m_TrackBinding == null)
            return;

        if (!m_FirstFrameHappened)
        {
            m_DefaultColor = m_TrackBinding.color;
            m_DefaultAlpha = m_TrackBinding.alpha;
            m_FirstFrameHappened = true;
        }

        int inputCount = playable.GetInputCount();

        Color mixedColor = Color.clear;
        float finalAlpha = 0f;
        float totalWeight = 0f;

        for (int i = 0; i < inputCount; i++)
        {
            float inputWeight = playable.GetInputWeight(i);
            ScriptPlayable<TextMeshProFadeBehaviour> inputPlayable = (ScriptPlayable<TextMeshProFadeBehaviour>)playable.GetInput(i);
            TextMeshProFadeBehaviour input = inputPlayable.GetBehaviour();
            
            totalWeight += inputWeight;

            // Alpha blending
            finalAlpha += input.alpha * inputWeight;

            // Color blending
            if (input.overrideColor)
            {
                mixedColor += input.color * inputWeight;
            }
            else
            {
                // If not overriding, contribute default color proportional to weight
                mixedColor += m_DefaultColor * inputWeight;
            }
        }
        
        // Handle remaining weight (empty timeline space)
        float remainingWeight = 1f - totalWeight;
        if (remainingWeight > 0)
        {
             // finalAlpha += m_DefaultAlpha * remainingWeight; // REMOVED: Force Alpha 0 when no clip
             
             // Keep mixing the default RGB color so we don't darken the color unnecessarily
             mixedColor += m_DefaultColor * remainingWeight;
        }

        // Apply
        mixedColor.a = finalAlpha;
        m_TrackBinding.color = mixedColor;
    }

    public override void OnPlayableDestroy(Playable playable)
    {
        m_FirstFrameHappened = false;
        if (m_TrackBinding != null)
        {
            // Restore? Optional. Usually Timeline does not restore automatically unless configured.
        }
    }
}
