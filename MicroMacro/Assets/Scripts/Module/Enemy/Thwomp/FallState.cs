using Constants;
using CoreModule.AI.HSM;
using DG.Tweening;
using UnityEngine;

namespace Module.Enemy.Thwomp
{
    public class FallState : HierarchicalStateMachine.State
    {
        private readonly ThwompParameter parameter;
        private readonly ThwompCondition condition;
        private readonly Rigidbody rigidbody;
        private readonly Collider collider;
        private float startTime;

        public FallState(ThwompComponent component)
        {
            parameter = component.Parameter;
            condition = component.Condition;
            rigidbody = component.Rigidbody;
            collider = component.Collider;
        }

        internal override void OnEnter()
        {
            startTime = Time.time;

            // 落ちる前に少し揺らす
            rigidbody.transform.DOShakePosition(parameter.FallDelay, 0.1f, 30, fadeOut: false).SetDelay(0.5f);
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
                condition.CurrentState = ThwompCondition.State.Idle;
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