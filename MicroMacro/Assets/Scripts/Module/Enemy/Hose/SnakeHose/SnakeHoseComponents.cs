using System;
using Module.Scaling;
using Module.UI;
using Unity.Cinemachine;
using UnityEngine;

namespace Module.Enemy.Hose.SnakeHose
{
    [Serializable]
    public class SnakeHoseComponents 
    {
        [SerializeField] private Transform headTransform;
        [SerializeField] private Transform neckTransform;
        [SerializeField] private Transform bodyTransform;
        [SerializeField] private SnakeController controller;
        [SerializeField] private SnakeHoseController snakeHoseController;
        [SerializeField] private Scaler scaler;
        [SerializeField] private LockOnEffect lockOnEffect;
        [SerializeField] private HealthStatus healthStatus;
        [SerializeField] private CinemachineCamera beamAttackCamera;
        [SerializeField] private Animator animator;

        public Transform HeadTransform => headTransform;
        public Transform NeckTransform => neckTransform;
        public Transform BodyTransform => bodyTransform;
        public SnakeController Controller => controller;
        public SnakeHoseController SnakeHoseController => snakeHoseController;
        public CinemachineCamera BeamAttackCamera => beamAttackCamera;
        public HealthStatus HealthStatus => healthStatus;
        public Scaler Scaler => scaler;
        public LockOnEffect LockOnEffect => lockOnEffect;
        public Animator Animator => animator;
    }
}