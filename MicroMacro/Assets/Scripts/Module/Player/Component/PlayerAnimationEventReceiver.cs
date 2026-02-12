using System;
using UnityEngine;

namespace Module.Player.Component
{
    public class PlayerAnimationEventReceiver : MonoBehaviour
    {
        public event Action OnWalk;
        public event Action OnAnimatorMoveEvent;
        private bool isWalked;

        private void WalkEvent()
        {
            // 歩行イベントが連続で呼ばれないように止める
            if (!isWalked)
            {
                OnWalk?.Invoke();
                isWalked = true;
            }
        }

        private void OnAnimatorMove()
        {
            isWalked = false;
            OnAnimatorMoveEvent?.Invoke();
        }
    }
}