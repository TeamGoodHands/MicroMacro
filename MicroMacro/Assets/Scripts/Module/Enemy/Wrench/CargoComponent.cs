using System;
using Module.Enemy.Thwomp;
using Module.Scaling;
using UnityEngine;
using UnityEngine.VFX;

namespace Module.Enemy.Wrench
{
    [Serializable]
    public class WrenchComponent
    {
        [SerializeField] private WrenchParameter parameter;
        [SerializeField] private WrenchCondition condition;
        [SerializeField] private Rigidbody rigidbody;
        [SerializeField] private Collider collider;
        [SerializeField] private TwoAxisScaler scaler;
        [SerializeField] private EnemyStatus status;
        [SerializeField] private Transform moveParent;
        [SerializeField] private Renderer bodyRenderer;

        public WrenchParameter Parameter => parameter;
        public WrenchCondition Condition => condition;
        public Rigidbody Rigidbody => rigidbody;
        public Collider Collider => collider;
        public TwoAxisScaler Scaler => scaler;
        public EnemyStatus Status => status;
        public Transform MoveParent => moveParent;
        public Renderer Renderer => bodyRenderer;
    }
}