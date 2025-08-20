using System;
using Module.Scaling;
using UnityEngine;

namespace Module.Enemy.Thwomp
{
    [Serializable]
    public class ThwompComponent
    {
        [SerializeField] private ThwompParameter parameter;
        [SerializeField] private ThwompCondition condition;
        [SerializeField] private Rigidbody rigidbody;
        [SerializeField] private Collider collider;
        [SerializeField] private TwoAxisScaler scaler;
        
        public ThwompParameter Parameter => parameter;
        public ThwompCondition Condition => condition;
        public Rigidbody Rigidbody => rigidbody;
        public Collider Collider => collider;
        public TwoAxisScaler Scaler => scaler;
    }
}