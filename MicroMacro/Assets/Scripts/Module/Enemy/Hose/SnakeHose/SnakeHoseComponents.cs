using System;
using Module.Enemy.Hose.ChildSnake;
using Module.Scaling;
using Module.UI;
using PropertyGenerator.Generated;
using Unity.Cinemachine;
using UnityEngine;

namespace Module.Enemy.Hose.SnakeHose
{
    [Serializable]
    public class SnakeHoseComponents 
    {
        [SerializeField] private Transform headTransform;
        [SerializeField] private Transform bodyTransform;
        [SerializeField] private Transform splineTransform;
        [SerializeField] private Scaler scaler;
        [SerializeField] private HealthStatus healthStatus;
        [SerializeField] private LockOnEffect lockOnEffect;
        [SerializeField] private SnakeHoseController controller;
        [SerializeField] private CinemachineCamera beamAttackCamera;
        [SerializeField] private CinemachineCamera nearInCamera;
        [SerializeField] private CinemachineCamera waterBallAttackCamera;
        [SerializeField] private ClearPlayer clearPlayer;
        [SerializeField] private ChildSnakeBehaviour[] children;
        [SerializeField] private Animator animator;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Renderer bodyRenderer;
        [SerializeField] private SnakeGripControllerWrapper gripControllerWrapper;

        public Transform HeadTransform => headTransform;
        public Transform BodyTransform => bodyTransform;
        public Transform SplineTranform => splineTransform;
        public CinemachineCamera BeamAttackCamera => beamAttackCamera;
        public CinemachineCamera NearInCamera => nearInCamera;
        public CinemachineCamera WaterBallAttackCamera => waterBallAttackCamera;
        public ClearPlayer ClearPlayer => clearPlayer;
        public ChildSnakeBehaviour[] Children => children;
        public HealthStatus HealthStatus => healthStatus;
        public LockOnEffect LockOnEffect => lockOnEffect;
        public SnakeGripControllerWrapper GripControllerWrapper => gripControllerWrapper;
        public Scaler Scaler => scaler;
        public Animator Animator => animator;
        public Renderer BodyRenderer => bodyRenderer;
        public CanvasGroup HpBarCanvasGroup => canvasGroup;
        public SnakeHoseController Controller => controller;
    }
}