using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Splines;

namespace Module.Enemy.Hose
{
    public class HoseExtender : MonoBehaviour
    {
        [SerializeField] private float extendSpeed = 1f;
        [SerializeField] private float bossLength = 0.1f;
        [SerializeField] private SplineExtrude splineExtrude;
        [SerializeField] private SplineAnimate splineAnimate;

        private void Start()
        {
            UpdateLength(0f);
            ExtendAsync().Forget();
        }

        private async UniTaskVoid ExtendAsync()
        {
            float currentLength = 0f;

            while (currentLength < 1f)
            {
                currentLength += extendSpeed * Time.deltaTime;
                UpdateLength(currentLength);
                await UniTask.Yield(cancellationToken: destroyCancellationToken);
            }
        }

        private void UpdateLength(float length)
        {
            length = Mathf.Clamp01(length);
            
            Vector2 range = splineExtrude.Range;
            range.y = length;
            splineExtrude.Range = range;
            splineExtrude.Rebuild();
            
            splineAnimate.NormalizedTime = length;
        }
    }
}