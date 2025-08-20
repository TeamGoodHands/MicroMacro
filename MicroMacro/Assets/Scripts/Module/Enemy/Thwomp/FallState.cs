using Constants;
using CoreModule.AI.HSM;
using DG.Tweening;
using Module.Scaling;
using UnityEngine;
using UnityEngine.VFX;

namespace Module.Enemy.Thwomp
{
    public class FallState : HierarchicalStateMachine.State
    {
        private readonly ThwompParameter parameter;
        private readonly ThwompCondition condition;
        private readonly TwoAxisScaler scaler;
        private readonly Rigidbody rigidbody;
        private readonly Collider collider;
        private readonly VisualEffect crackEffect;
        private readonly EnemyStatus status;
        private float startTime;

        public FallState(ThwompComponent component)
        {
            parameter = component.Parameter;
            condition = component.Condition;
            rigidbody = component.Rigidbody;
            collider = component.Collider;
            scaler = component.Scaler;
            crackEffect = component.CrackEffect;
            status = component.Status;
        }

        internal override void OnEnter()
        {
            startTime = Time.time;

            // 落ちる前に少し揺らす
            rigidbody.transform.DOShakePosition(parameter.FallDelay, 0.1f, 30, fadeOut: false).SetDelay(0.5f);
            scaler.enabled = true;
        }

        internal override void OnExit()
        {
        }

        internal override void Update()
        {
        }

        internal override void UpdatePhysics()
        {
            if (startTime + parameter.FallDelay > Time.time)
                return;

            PerformFall();

            // 地面の設置判定
            if (IsGround())
            {
                condition.LastAttackTime = Time.time;
                scaler.enabled = false;

                if (scaler.CurrentStep > 0)
                {
                    status.Damage(scaler.CurrentStep);
                    crackEffect.Play();
                }

                condition.CurrentState = status.CurrentHealth > 0 ? ThwompCondition.State.Idle : ThwompCondition.State.Death;
            }
        }

        private bool IsGround()
        {
            // 判定に用いる距離
            const float checkDistance = 0.6f;

            // BoxCastのパラメータ
            Vector3 center = rigidbody.transform.position;
            Vector3 halfExtents = collider.bounds.extents * 0.95f; // 少し縮小して誤検知を防ぐ
            Quaternion rotation = Quaternion.identity;
            Vector3 direction = Vector3.down;
            const int layer = Layer.Mask.Player | Layer.Mask.Default;

            // 真下にBoxCast実行してコライダーを判定する
            if (Physics.BoxCast(center, halfExtents, direction, out var hit, rotation, checkDistance, layer, QueryTriggerInteraction.Ignore))
            {
                // 自分自身でなければ接地
                return hit.collider != collider;
            }

            return false;
        }

        private void PerformFall()
        {
            Vector3 pos = rigidbody.position;
            pos.y -= parameter.FallSpeed * Time.deltaTime;
            rigidbody.position = pos;
        }

        internal override void Dispose()
        {
        }
    }
}