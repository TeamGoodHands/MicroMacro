using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Module.Management
{
    public class CollisionSoundManager : MonoBehaviour
    {
        [SerializeField] private string soundName;

        [SerializeField] private float velocityThreshold = 2.0f;

        [SerializeField] private float cooldownSeconds = 0.1f;

        private bool isCoolingDown;

        private void OnCollisionEnter(Collision collision)
        {
            if (isCoolingDown)
                return;

            if (collision.relativeVelocity.magnitude < velocityThreshold)
                return;

            PlayCollisionSound();
            StartCooldownAsync().Forget();
        }

        private void PlayCollisionSound()
        {
            SoundManager.instance.Play(soundName);
        }

        private async UniTaskVoid StartCooldownAsync()
        {
            isCoolingDown = true;

            try
            {
                TimeSpan delayTimeSpan = TimeSpan.FromSeconds(cooldownSeconds);
                await UniTask.Delay(delayTimeSpan, cancellationToken: this.GetCancellationTokenOnDestroy());
            }
            catch (OperationCanceledException)
            {
                // オブジェクト破棄時は速やかに終了します。
            }
            finally
            {
                isCoolingDown = false;
            }
        }
    }
}