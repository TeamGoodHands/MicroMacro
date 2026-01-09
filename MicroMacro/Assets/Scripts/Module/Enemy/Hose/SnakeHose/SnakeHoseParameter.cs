using CoreModule.Helper;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Timeline;

namespace Module.Enemy.Hose.SnakeHose
{
    public class SnakeHoseParameter : MonoBehaviour
    {
        [SerializeField] private float shakeTime = 2.0f;
        [SerializeField] private float attackDuration = 2.0f;
        [SerializeField] private float timeToFacePlayer = 0.5f;
        [SerializeField] private float timeToResetAngle = 0.3f;
        [SerializeField] private float waterBallShootPower = 20f;
        [SerializeField] private float hoseMovementBaseSpeed = 12f;
        [SerializeField] private float hoseMovementSpeedMultiplier = 1f;
        [SerializeField] private float hoseMovementScaleOffset = 0.1f;
        [SerializeField] private float hoseShootInterval = 1f;
        [SerializeField] private float hoseShootIntervalOffset = 0.03f;
        [SerializeField] private TimelineAsset[] timelineAssets;
        [SerializeField] private GameObject waterBallPrefab;
        [SerializeField] private Transform shootPivot;
        [SerializeField] private Vector2[] attackHeight;

        public float ShakeTime => shakeTime;
        public float AttackDuration => attackDuration;
        public float TimeToFacePlayer => timeToFacePlayer;
        public float TimeToResetAngle => timeToResetAngle;
        public float WaterBallShootPower => waterBallShootPower;
        public float HoseMovementBaseSpeed => hoseMovementBaseSpeed;
        public float HoseMovementSpeedMultiplier => hoseMovementSpeedMultiplier;
        public float HoseMovementScaleOffset => hoseMovementScaleOffset;
        public float HoseShootInterval => hoseShootInterval;
        public float HoseShootIntervalOffset => hoseShootIntervalOffset;
        public TimelineAsset[] TimelineAssets => timelineAssets;
        public GameObject WaterBallPrefab => waterBallPrefab;
        public Transform ShootPivot => shootPivot;
        public Vector2[] AttackHeight => attackHeight;
    }
}