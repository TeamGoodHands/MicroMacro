using Module.Scaling;
using UnityEngine;

namespace Module.Enemy.Cargo
{
    public sealed class ProjectileObjectCache
    {
        public ProjectileObject Obj { get; }
        public Rigidbody Rb { get; }
        public Collider Col { get; }
        public FallObjectPlayerAttacker Attacker { get; }
        public Scaler Scaler { get; }
        public ScalerEffector Effector { get; }
        public Vector3 DefaultPos { get; }

        public ProjectileObjectCache(ProjectileObject obj)
        {
            Obj = obj;
            Rb = obj.GetComponent<Rigidbody>();
            Col = obj.GetComponent<Collider>();
            Attacker = obj.GetComponent<FallObjectPlayerAttacker>();
            Scaler = obj.GetComponent<Scaler>();
            Effector = obj.GetComponent<ScalerEffector>();
            DefaultPos = obj.transform.position;
        }
    }
}