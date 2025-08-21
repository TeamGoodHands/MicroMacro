using System;
using Module.Player.Component;
using Module.Gimmick;
using PropertyGenerator.Generated;
using UnityEngine;

namespace Module.Player
{
    [Serializable]
    public class PlayerComponent
    {
        [SerializeField] private PlayerParameter parameter;
        [SerializeField] private PlayerCondition condition;
        [SerializeField] private Rigidbody rigidbody;
        [SerializeField] private PlayerControllerWrapper playerAnimatorController;
        [SerializeField] private PlayerStatus playerStatus;
       
        public PlayerParameter Parameter => parameter;
        public PlayerCondition Condition => condition;
        public Rigidbody Rigidbody => rigidbody;
        public Transform Transform => rigidbody.transform;
        public PlayerStatus PlayerStatus => playerStatus;
        public PlayerControllerWrapper AnimatorWrapper => playerAnimatorController;
        public PlayerMovement PlayerMovement => playerMovement ??= new PlayerMovement(parameter, condition);
        public PlayerRotation PlayerRotation => playerRotation ??= new PlayerRotation(parameter, condition);
        public WeaponSwitcher WeaponSwitcher => weaponSwitcher ??= new WeaponSwitcher(this);

        private PlayerMovement playerMovement;
        private PlayerRotation playerRotation;
        private PlayerAnimator playerAnimator;
        private WeaponSwitcher weaponSwitcher;
    }
}