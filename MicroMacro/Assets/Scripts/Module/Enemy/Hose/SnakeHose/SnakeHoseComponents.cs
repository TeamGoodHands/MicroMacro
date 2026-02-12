using System;
using Module.Enemy.Hose.ChildSnake;
using Module.Management;
using Module.Scaling;
using Module.UI;
using PropertyGenerator.Generated;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;

namespace Module.Enemy.Hose.SnakeHose
{
    [Serializable]
    public class SnakeHoseComponents 
    {
        [SerializeField] private Transform headTransform;
        [SerializeField] private Transform snakeHeadTransform;
        [SerializeField] private Transform bodyTransform;
        [SerializeField] private Transform splineTransform;
        [SerializeField] private Scaler scaler;
        [SerializeField] private ScalerEffector effector;
        [SerializeField] private HealthStatus healthStatus;
        [SerializeField] private LockOnEffect lockOnEffect;
        [SerializeField] private SnakeHoseController controller;
        [SerializeField] private LastAttackHoseBehaviour lastAttackHose;
        [SerializeField] private PlayableDirector lastAttackDirector;
        [SerializeField] private CinemachineCamera beamAttackCamera;
        [SerializeField] private CinemachineCamera nearInCamera;
        [SerializeField] private CinemachineCamera waterBallAttackCamera;
        [SerializeField] private CinemachineCamera lastAttackCamera;
        [SerializeField] private AreaSoundManager[] areaSoundManager;
        [SerializeField] private ClearPlayer clearPlayer;
        [SerializeField] private ChildSnakeBehaviour[] children;
        [SerializeField] private Animator animator;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Renderer bodyRenderer;
        [SerializeField] private SnakeGripControllerWrapper gripControllerWrapper;

        public Transform HeadTransform => headTransform;
        public Transform SnakeHeadTransform => snakeHeadTransform;
        public Transform BodyTransform => bodyTransform;
        public Transform SplineTranform => splineTransform;
        public CinemachineCamera BeamAttackCamera => beamAttackCamera;
        public CinemachineCamera NearInCamera => nearInCamera;
        public CinemachineCamera WaterBallAttackCamera => waterBallAttackCamera;
        public CinemachineCamera LastAttackCamera => lastAttackCamera;
        public ClearPlayer ClearPlayer => clearPlayer;
        public AreaSoundManager[] AreaSoundManager => areaSoundManager;
        public ChildSnakeBehaviour[] Children => children;
        public HealthStatus HealthStatus => healthStatus;
        public LockOnEffect LockOnEffect => lockOnEffect;
        public SnakeGripControllerWrapper GripControllerWrapper => gripControllerWrapper;
        public Scaler Scaler => scaler;
        public ScalerEffector Effector => effector;
        public Animator Animator => animator;
        public Renderer BodyRenderer => bodyRenderer;
        public CanvasGroup HpBarCanvasGroup => canvasGroup;
        public SnakeHoseController Controller => controller;
        public LastAttackHoseBehaviour LastAttackHose => lastAttackHose;
        public PlayableDirector LastAttackDirector => lastAttackDirector;
    }
}