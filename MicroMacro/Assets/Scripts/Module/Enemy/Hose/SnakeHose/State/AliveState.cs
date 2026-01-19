using CoreModule.AI.HSM;
using DG.Tweening;
using PropertyGenerator.Generated;
using UnityEngine;
using UnityEngine.Playables;

namespace Module.Enemy.Hose.SnakeHose
{
    public class AliveState : HierarchicalStateMachine.State
    {
        private Sequence damageColorSequence;
        private readonly SnakeHoseComponents components;
        private readonly SnakeHoseParameter parameter;
        private ScalerShaderWrapper scalerShaderWrapper;

        public AliveState(SnakeHoseComponents components, SnakeHoseParameter parameter)
        {
            this.components = components;
            this.parameter = parameter;

            scalerShaderWrapper = new ScalerShaderWrapper(components.BodyRenderer.material);
        }

        internal override void OnEnter()
        {
            components.HealthStatus.OnDamage += _ => Damage();
        }

        internal override void OnExit()
        {
        }

        internal override void Update()
        {
        }

        private void Damage()
        {
            Color color = parameter.DamageAdditionalColor;
            damageColorSequence?.Kill();
            damageColorSequence = DOTween.Sequence();
            damageColorSequence.Append(DOTween.To(() => Color.white, c => scalerShaderWrapper.BaseColor = c, color, 0.1f));
            damageColorSequence.Append(DOTween.To(() => color, c => scalerShaderWrapper.BaseColor = c, Color.white, 0.1f));
            damageColorSequence.Play();
        }

        internal override void UpdatePhysics()
        {
        }

        internal override void Dispose()
        {
        }
    }
}