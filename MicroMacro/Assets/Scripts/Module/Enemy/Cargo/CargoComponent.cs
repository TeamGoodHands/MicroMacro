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
        [SerializeField] private Rigidbody rigidbody;
        [SerializeField] private Transform transform;
        [SerializeField] private Collider collider;
        [SerializeField] private TwoAxisScaler scaler;
        [SerializeField] private EnemyStatus status;
        [SerializeField] private Rigidbody moveParent;
        [SerializeField] private Renderer bodyRenderer;
        [SerializeField] private CargoControllerWrapper animatorWrapper;
        [SerializeField] private CinemachineCamera cinemachineCamera;
        [SerializeField] private Transform start;
        [SerializeField] private Transform goal;

        public CargoParameter Parameter => parameter;
        public CargoCondition Condition => condition;
        public Rigidbody Rigidbody => rigidbody;
        public Transform Transform => transform;
        public Collider Collider => collider;
        public TwoAxisScaler Scaler => scaler;
        public EnemyStatus Status => status;
        public Rigidbody MoveParent => moveParent;
        public Renderer Renderer => bodyRenderer;
        public CargoControllerWrapper AnimatorWrapper => animatorWrapper;
        public CinemachineCamera CinemachineCamera => cinemachineCamera;
        public Transform Start => start;
        public Transform Goal => goal;
    }
}