using System;
using Module.Scaling;
using UnityEngine;
using UnityEngine.VFX;

namespace Module.Enemy.Thwomp
{
    [Serializable]
    public class ThwompComponent
    {
        [SerializeField] private ThwompParameter parameter;
        [SerializeField] private ThwompCondition condition;
        [SerializeField] private Rigidbody rigidbody;
        [SerializeField] private Collider collider;
        [SerializeField] private VisualEffect crackEffect;
        [SerializeField] private TwoAxisScaler scaler;
        [SerializeField] private EnemyStatus status;
        [SerializeField] private Renderer bodyRenderer;
        
        public ThwompParameter Parameter => parameter;
        public ThwompCondition Condition => condition;
        public Rigidbody Rigidbody => rigidbody;
        public Collider Collider => collider;
        public VisualEffect CrackEffect => crackEffect;
        public TwoAxisScaler Scaler => scaler;
        public EnemyStatus Status => status;
        public Renderer Renderer => bodyRenderer;
    }
}