using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Module.UI
{
    public class HpBar : MonoBehaviour
    {
        [SerializeField] private Image hpBar;
        [SerializeField] private Image hpBarBackground;
        [SerializeField] private float hpBarTweenDuration = 0.2f;
        [SerializeField] private float hpBarBackgroundTweenDuration = 0.3f;

        private Tween currentTween;

        public void ApplyHp(float currentHp, float maxHp)
        {
            Debug.Assert(maxHp > 0f, "maxHp must be greater than 0");

            currentTween?.Kill();

            float ratio = currentHp / maxHp;

            Sequence sequence = DOTween.Sequence();
            sequence.Append(hpBar.DOFillAmount(ratio, hpBarTweenDuration));
            sequence.Append(hpBarBackground.DOFillAmount(ratio, hpBarBackgroundTweenDuration));
            currentTween = sequence;
        }
    }
}