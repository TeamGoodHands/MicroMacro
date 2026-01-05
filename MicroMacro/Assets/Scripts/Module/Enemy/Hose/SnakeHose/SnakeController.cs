using UnityEngine;
using UnityEngine.Splines;

namespace Module.Enemy.Hose.SnakeHose
{
    [ExecuteAlways]
    public class SnakeController : MonoBehaviour
    {
        [Header("Spline Settings")]
        public SplineContainer splineContainer;
        
        [Tooltip("Timelineで制御されます")]
        public int targetSplineIndex = 0;

        [Header("Runtime / Timeline")]
        [Tooltip("実際の移動距離")]
        public float distanceTraveled = 0f;

        [Header("Movement Config")]
        [Tooltip("すべてのTimelineクリップに適用される共通のイージング設定。\n直線(Linear)なら等速、S字なら加減速します。")]
        public AnimationCurve commonEaseCurve = AnimationCurve.Linear(0, 0, 1, 1); // ★追加: 共通設定

        public float baseSpeed = 5.0f;
        public float SpeedMultiplier = 1.0f;
        public bool loopMovement = true;

        [Header("Shape Data")]
        public float snakeLength = 10.0f;
        public float baseRadius = 0.5f;
        public AnimationCurve widthCurve = AnimationCurve.Linear(0, 1, 1, 1);

        public float CurrentSplineLength { get; private set; }

        private void Update()
        {
            if (splineContainer == null) return;
            
            // 安全対策
            if (splineContainer.Splines.Count > 0)
            {
                targetSplineIndex = Mathf.Clamp(targetSplineIndex, 0, splineContainer.Splines.Count - 1);
                CurrentSplineLength = splineContainer.Splines[targetSplineIndex].GetLength();
            }
            if (CurrentSplineLength <= 0.001f) CurrentSplineLength = 1f;
        }

        // GetSampleAtOffset は変更なしのため省略（そのまま使ってください）
        public void GetSampleAtOffset(float offsetFromHead, out Vector3 position, out Quaternion rotation, out float radiusScale)
        {
            if (splineContainer == null || splineContainer.Splines.Count == 0)
            {
                position = transform.position; rotation = transform.rotation; radiusScale = 1.0f; return;
            }

            var targetSpline = splineContainer.Splines[Mathf.Clamp(targetSplineIndex, 0, splineContainer.Splines.Count - 1)];
            float splineLen = targetSpline.GetLength();
            if (splineLen <= 0.001f) splineLen = 1f;

            float distOnSnake = snakeLength - offsetFromHead;
            float targetDist = distanceTraveled + distOnSnake;

            float sampleDist;
            if (loopMovement) sampleDist = Mathf.Repeat(targetDist, splineLen);
            else sampleDist = Mathf.Clamp(targetDist, 0, splineLen);

            float t = sampleDist / splineLen;
            Vector3 localPos = targetSpline.EvaluatePosition(t);
            position = splineContainer.transform.TransformPoint(localPos);
            
            Vector3 localTangent = Vector3.Normalize(targetSpline.EvaluateTangent(t));
            Vector3 localUp = Vector3.Normalize(targetSpline.EvaluateUpVector(t));
            if (localTangent == Vector3.zero) localTangent = Vector3.forward;
            if (localUp == Vector3.zero) localUp = Vector3.up;

            Vector3 worldTangent = splineContainer.transform.TransformDirection(localTangent);
            Vector3 worldUp = splineContainer.transform.TransformDirection(localUp);
            rotation = Quaternion.LookRotation(worldTangent, worldUp);

            float rate = Mathf.Clamp01(distOnSnake / snakeLength);
            radiusScale = widthCurve.Evaluate(rate);
        }
    }
}