using UnityEngine;
using UnityEngine.Playables;

namespace Module.Enemy.Hose.SnakeHose.Timeline.Spline
{
    public class SnakeSplineMixerBehaviour : PlayableBehaviour
    {
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            SnakeController controller = playerData as SnakeController;
            if (controller == null) return;

            int inputCount = playable.GetInputCount();
            
            int activeSplineIndex = controller.targetSplineIndex;
            float finalDistance = 0f;
            float totalWeight = 0f;

            for (int i = 0; i < inputCount; i++)
            {
                float weight = playable.GetInputWeight(i);
                
                if (weight > 0.001f)
                {
                    ScriptPlayable<SnakeSplineBehaviour> inputPlayable = (ScriptPlayable<SnakeSplineBehaviour>)playable.GetInput(i);
                    SnakeSplineBehaviour input = inputPlayable.GetBehaviour();

                    if (weight > 0.5f)
                    {
                        activeSplineIndex = input.splineIndex;
                    }

                    // --- 時間計算 ---
                    double time = inputPlayable.GetTime();
                    double duration = inputPlayable.GetDuration();
                    float t = (float)(time / duration);

                    // --- イージング加工ロジック ---
                    float adjustedT = t;

                    // 共通カーブがある場合のみ計算
                    if (controller.commonEaseCurve != null && controller.commonEaseCurve.length > 0)
                    {
                        switch (input.easingMode)
                        {
                            case EasingMode.Default:
                                // そのまま (0.0 ～ 1.0)
                                adjustedT = controller.commonEaseCurve.Evaluate(t);
                                break;

                            case EasingMode.CutOut:
                                // 「前半 0.0～0.5」の部分を「0.0～1.0」に引き伸ばして使う
                                // 結果：加速して最高速になった状態で終わる（減速しない）
                                float halfT_Out = t * 0.5f; 
                                adjustedT = controller.commonEaseCurve.Evaluate(halfT_Out) * 2.0f; 
                                // ※カーブが(0.5, 0.5)を通る対称形であることを前提とした簡易計算
                                break;

                            case EasingMode.CutIn:
                                // 「後半 0.5～1.0」の部分を「0.0～1.0」に割り当てて使う
                                // 結果：最高速の状態から始まり、減速して止まる（加速しない）
                                float halfT_In = 0.5f + (t * 0.5f);
                                // 値も 0.5～1.0 の範囲で返ってくるので、0.0～1.0に補正
                                float val = controller.commonEaseCurve.Evaluate(halfT_In);
                                adjustedT = (val - 0.5f) * 2.0f;
                                break;

                            case EasingMode.Linear:
                                // カーブ無視（等速）
                                adjustedT = t;
                                break;
                        }
                    }

                    float calculatedDist = Mathf.Lerp(input.startDistance, input.endDistance, adjustedT);

                    finalDistance += calculatedDist * weight;
                    totalWeight += weight;
                }
            }

            if (totalWeight > 0.001f)
            {
                controller.targetSplineIndex = activeSplineIndex;
                controller.distanceTraveled = finalDistance;
            }
        }
    }
}