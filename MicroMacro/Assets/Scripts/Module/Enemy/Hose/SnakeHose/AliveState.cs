using CoreModule.AI.HSM;
using UnityEngine;
using UnityEngine.Playables;

namespace Module.Enemy.Hose.SnakeHose
{
    public class AliveState : HierarchicalStateMachine.State
    {
        private readonly PlayableDirector director;
        private readonly SnakeHoseParameter parameter;
        private int index;

        public AliveState(PlayableDirector director, SnakeHoseParameter parameter)
        {
            this.director = director;
            this.parameter = parameter;
        }

        private void PlayNext()
        {
            Debug.Log($"Play {parameter.TimelineAssets[index].name}");
            director.Play(parameter.TimelineAssets[index]);
            index++;

            if (index == 3)
            {
                index = 1;
            }
        }

        internal override void OnEnter() { }
        internal override void OnExit() { }

        internal override void Update()
        {
            if (director.state == PlayState.Paused)
            {
                PlayNext();
            }
        }

        internal override void UpdatePhysics() { }
        internal override void Dispose() { }
    }
}