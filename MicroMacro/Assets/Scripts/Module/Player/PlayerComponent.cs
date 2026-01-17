using System;
using CoreModule.Utility;
using Module.Player.Component;
using Module.Gimmick;
using Module.UI;
using PropertyGenerator.Generated;
using UnityEngine;
using UnityEngine.VFX;

namespace Module.Player
{
    [Serializable]
    public class PlayerComponent
    {
        [SerializeField] private PlayerParameter parameter;
        [SerializeField] private PlayerCondition condition;
        [SerializeField] private Rigidbody rigidbody;
        [SerializeField] private Transform bodyTransform;
        [SerializeField] private PlayerControllerWrapper playerAnimatorController;
        [SerializeField] private SkinnedMeshRenderer meshRenderer;
        [SerializeField] private HealthStatus healthStatus;
        [SerializeField] private VisualEffect footEffect;
        [SerializeField] private VisualEffect jumpEffect;
        [SerializeField] private PlayerAnimationEventReceiver animationEventReceiver;
       
        public PlayerParameter Parameter => parameter;
        public PlayerCondition Condition => condition;
        public Rigidbody Rigidbody => rigidbody;
        public Transform Transform => rigidbody.transform;
        public Transform BodyTransform => bodyTransform;
        public HealthStatus HealthStatus => healthStatus;
        public SkinnedMeshRenderer MeshRenderer => meshRenderer;
        public VisualEffect FootEffect => footEffect;
        public VisualEffect JumpEffect => jumpEffect;
        public PlayerAnimationEventReceiver AnimationEventReceiver => animationEventReceiver;
        public PlayerControllerWrapper AnimatorWrapper => playerAnimatorController;
        public PlayerMovement PlayerMovement => playerMovement ??= new PlayerMovement(parameter, condition);
        public PlayerRotation PlayerRotation => playerRotation ??= new PlayerRotation(parameter, condition);
        public WeaponSwitcher WeaponSwitcher => weaponSwitcher ??= new WeaponSwitcher(this);

        private PlayerMovement playerMovement;
        private PlayerRotation playerRotation;
        private WeaponSwitcher weaponSwitcher;
    }
}