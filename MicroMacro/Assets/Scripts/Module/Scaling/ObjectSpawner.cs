using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private float interval;

    private async void Start()
    {
        while (!destroyCancellationToken.IsCancellationRequested)
        {
            Instantiate(prefab, transform);
            await UniTask.Delay(TimeSpan.FromSeconds(interval));
        }
    }
}