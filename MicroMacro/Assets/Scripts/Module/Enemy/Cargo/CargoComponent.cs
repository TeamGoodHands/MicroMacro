using System;
using Module.Scaling;
using PropertyGenerator.Generated;
using Unity.Cinemachine;
using UnityEngine;

namespace Module.Enemy.Cargo
{
    [Serializable]
    public class CargoComponent
    {
        [SerializeField] private CargoParameter parameter;
        [SerializeField] private CargoCondition condition;
        [SerializeField] private Transform transform;
        [SerializeField] private Transform bodyTransform;
        [SerializeField] private EnemyStatus status;
        [SerializeField] private Rigidbody moveParent;
        [SerializeField] private CargoControllerWrapper animatorWrapper;
        [SerializeField] private CinemachineCamera cinemachineCamera;
        [SerializeField] private Transform start;
        [SerializeField] private Transform goal;
        [SerializeField] private Transform[] eyes;

        public CargoParameter Parameter => parameter;
        public CargoCondition Condition => condition;
        public Transform Transform => transform;
        public Transform BodyTransform => bodyTransform;
        public EnemyStatus Status => status;
        public Rigidbody MoveParent => moveParent;
        public CargoControllerWrapper AnimatorWrapper => animatorWrapper;
        public CinemachineCamera CinemachineCamera => cinemachineCamera;
        public Transform Start => start;
        public Transform Goal => goal;
        public Transform[] Eyes => eyes;
    }
}