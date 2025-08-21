using UnityEngine;

namespace CoreModule.AI.BT
{
    public class DelayNode : Node
    {
        private readonly float delayTime;
        private float startTime;

        public DelayNode(float delayTime)
        {
            this.delayTime = delayTime;
        }

        protected override void OnStart()
        {
            startTime = Time.time;
        }

        protected override Status OnUpdate()
        {
            if (Time.time - startTime >= delayTime)
            {
                return Status.Success;
            }
            
            return Status.Running;
        }
    }
}