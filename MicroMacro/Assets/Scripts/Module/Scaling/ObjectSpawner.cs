using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private float interval;
    [SerializeField] private float firstDelay;

    private async void Start()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(firstDelay), cancellationToken: destroyCancellationToken);

        while (!destroyCancellationToken.IsCancellationRequested)
        {
            Instantiate(prefab, transform);
            await UniTask.Delay(TimeSpan.FromSeconds(interval), cancellationToken: destroyCancellationToken);
        }
    }
}