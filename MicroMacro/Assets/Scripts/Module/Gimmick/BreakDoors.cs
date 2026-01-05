using System;
using System.Threading;
using Module.Management;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Module.Gimmick
{
    public class BreakDoors : MonoBehaviour
    {
        [SerializeField] private DoorGimmick doorGimmick;
        [SerializeField] private Animator[] doorAnimators;
        
        private CancellationTokenSource cts;

        private void Start()
        {
            doorGimmick.OnArrivalDoor += BreakDoorRapper;
        }
        private void OnDestroy()
        {
            cts?.Cancel();
            cts?.Dispose();
            doorGimmick.OnArrivalDoor -= BreakDoorRapper;
        }
        
        private void BreakDoorRapper()
        {
            BreakDoorsAsync().Forget();
        }

        private async UniTaskVoid BreakDoorsAsync()
        {
            cts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            CancellationToken token = cts.Token;
            
            // 順番にドア破壊
            foreach (var anim in doorAnimators)
            {
                Debug.Log("再生");
                anim.SetTrigger("BreakDoor");
                await WaitForAnimation(anim, "BreakDoor", 0, token);
            }
        }
        
        private async UniTask WaitForAnimation(Animator anim, string stateName, int layerIndex, CancellationToken token)
        {
            // ステート切り替えの反映まで1フレ待機
            await UniTask.Yield(token);
            
            // 指定のステートになるまで待機
            await UniTask.WaitUntil(() => anim.GetCurrentAnimatorStateInfo(layerIndex).IsName(stateName), cancellationToken: token);

            // 再生終了まで待機(normalizedTime >= 1.0f)
            await UniTask.WaitUntil(() =>
            {
                // ステート情報を確認し続けて指定モーションが終了したらtrueを返す
                var info = anim.GetCurrentAnimatorStateInfo(layerIndex);
                return info.IsName(stateName) && info.normalizedTime >= 1.0f;
            }, cancellationToken: token);
        }
    }
}