using CoreModule.Helper;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Timeline;

namespace Module.Enemy.Hose.SnakeHose
{
    public class SnakeHoseParameter : MonoBehaviour
    {
        [SerializeField] private float appearDuration = 3f;
        [SerializeField] private Vector2[] attackHeight;
        [SerializeField] private Transform[] raptures;

        [Header("Water Ball Attack Parameters")] [SerializeField]
        private float waterBallShootPower = 20f;

        [SerializeField] private float hoseMovementBaseSpeed = 12f;
        [SerializeField] private float hoseMovementSpeedMultiplier = 1f;
        [SerializeField] private float hoseMovementScaleOffset = 0.1f;
        [SerializeField] private float hoseShootInterval = 1f;
        [SerializeField] private float hoseShootIntervalOffset = 0.03f;
        [SerializeField] private float damageScaleDuration = 0.8f;
        [SerializeField] private GameObject waterBallPrefab;
        [SerializeField] private Transform shootPivot;

        public float WaterBallShootPower => waterBallShootPower;
        public float HoseMovementBaseSpeed => hoseMovementBaseSpeed;
        public float HoseMovementSpeedMultiplier => hoseMovementSpeedMultiplier;
        public float HoseMovementScaleOffset => hoseMovementScaleOffset;
        public float HoseShootInterval => hoseShootInterval;
        public float HoseShootIntervalOffset => hoseShootIntervalOffset;
        public GameObject WaterBallPrefab => waterBallPrefab;
        public Transform ShootPivot => shootPivot;
        public Vector2[] AttackHeight => attackHeight;
        public Transform[] Raptures => raptures;
        public float AppearDuration => appearDuration;
        public float DamageScaleDuration => damageScaleDuration;
    }
}