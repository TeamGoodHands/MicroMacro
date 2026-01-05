using UnityEngine;
using UnityEngine.Playables;

namespace Module.Enemy.Hose.SnakeHose.Timeline.Spline
{
    public class SnakeSplineMixerBehaviour : PlayableBehaviour
    {
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            // バインドされた SnakeController を取得
            SnakeController controller = playerData as SnakeController;
            if (controller == null) return;

            int inputCount = playable.GetInputCount();
            
            // ブレンド計算用
            float totalWeight = 0f;
            int activeSplineIndex = controller.targetSplineIndex;
            float finalDistance = 0f;

            for (int i = 0; i < inputCount; i++)
            {
                float weight = playable.GetInputWeight(i);
                
                if (weight > 0.001f)
                {
                    ScriptPlayable<SnakeSplineBehaviour> inputPlayable = (ScriptPlayable<SnakeSplineBehaviour>)playable.GetInput(i);
                    SnakeSplineBehaviour input = inputPlayable.GetBehaviour();

                    // 1. スプラインIndexの適用（支配的なクリップに従う）
                    if (weight > 0.5f)
                    {
                        activeSplineIndex = input.splineIndex;
                    }

                    // 2. 進行度(0~1)の計算
                    double time = inputPlayable.GetTime();
                    double duration = inputPlayable.GetDuration();
                    float t = (float)(time / duration);

                    // 3. 【共通イージングの適用】
                    // Controller側で設定した共通カーブを使って t を変換する
                    float easedT = t;
                    if (controller.commonEaseCurve != null && controller.commonEaseCurve.length > 0)
                    {
                        easedT = controller.commonEaseCurve.Evaluate(t);
                    }

                    // 4. 開始地点と終了地点の間を補間
                    float calculatedDist = Mathf.Lerp(input.startDistance, input.endDistance, easedT);

                    // ブレンド加算
                    finalDistance += calculatedDist * weight;
                    totalWeight += weight;
                }
            }

            // 値を適用
            if (totalWeight > 0.001f)
            {
                controller.targetSplineIndex = activeSplineIndex;
                controller.distanceTraveled = finalDistance;
            }
        }
    }
}