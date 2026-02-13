using System;
using Constants;
using CoreModule.AI.HSM;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Module.Management;
using Module.Player.Component;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Module.Enemy.Hose.SnakeHose.State
{
    public class SmashAttackState : HierarchicalStateMachine.State
    {
        private readonly SnakeHoseComponents components;
        private readonly SnakeHoseParameter parameter;
        private readonly SnakeHoseCondition condition;

        private PlayerCondition playerCondition;
        private Rigidbody playerRigidbody;
        private PlayBGM playBgm;

        public SmashAttackState(SnakeHoseComponents components, SnakeHoseParameter parameter, SnakeHoseCondition condition)
        {
            this.components = components;
            this.parameter = parameter;
            this.condition = condition;

            GameObject player = GameObject.FindWithTag(Tag.Player);
            playerRigidbody = player.GetComponent<Rigidbody>();
            playerCondition = player.GetComponent<PlayerCondition>();
        }

        internal override void OnEnter()
        {
            PlayClearEffect().Forget();
        }

        private async UniTaskVoid PlayClearEffect()
        {
            // 仮
            components.RespawnArea.Disable();
            playerCondition.IsPlayerLocked = true;

            await UniTask.Yield(PlayerLoopTiming.LastFixedUpdate, CancellationToken);

            components.Effector.Disable();
            components.LastAttackDirector.Play();

            components.Scaler.SetScaleImmediate(0, true);

            playerCondition.transform.position = parameter.LastAttackPosition;
            playerCondition.transform.rotation = Quaternion.identity;
            playerRigidbody.rotation = Quaternion.identity;
            playerCondition.transform.GetChild(0).localScale = new Vector3(1f, 1f, 1f);

            foreach (AreaSoundManager soundManager in components.AreaSoundManager)
            {
                soundManager.Stop();
                soundManager.enabled = false;
            }

            await UniTask.Delay(TimeSpan.FromSeconds(2f), cancellationToken: CancellationToken);

            if (CancellationToken.IsCancellationRequested)
                return;

            playBgm = Object.FindAnyObjectByType<PlayBGM>();
            playBgm.BGMSource.DOFade(0f, 1f).SetUpdate(true);
        }

        public async void FirstBeam()
        {
            Time.timeScale = 0.5f;

            await components.LastAttackHose.OnWater(CancellationToken, 0.12f);

            Time.timeScale = 1f;
        }

        public void EndFirstBeam()
        {
            components.LastAttackHose.OffWater(CancellationToken).Forget();
        }

        internal override void OnExit()
        {
        }

        internal override void Update()
        {
        }

        internal override void UpdatePhysics()
        {
        }

        internal override void Dispose()
        {
        }
    }
}