using CoreModule.Helper;
using UnityEngine;
using UnityEngine.Timeline;

namespace Module.Enemy.Hose.SnakeHose
{
    public class SnakeHoseParameter : MonoBehaviour
    {
        [SerializeField] private float shakeTime = 2.0f;
        [SerializeField] private float attackDuration = 2.0f;
        [SerializeField] private float timeToFacePlayer = 0.5f;
        [SerializeField] private float timeToResetAngle = 0.3f;
        [SerializeField] private TimelineAsset[] timelineAssets;

        public float ShakeTime => shakeTime;
        public float AttackDuration => attackDuration;
        public float TimeToFacePlayer => timeToFacePlayer;
        public float TimeToResetAngle => timeToResetAngle;
        public TimelineAsset[] TimelineAssets => timelineAssets;
    }
}