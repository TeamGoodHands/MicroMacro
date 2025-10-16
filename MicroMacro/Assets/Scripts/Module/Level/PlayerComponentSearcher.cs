using Constants;
using Unity.Cinemachine;
using UnityEngine;

namespace Module.Level
{
    [ExecuteInEditMode]
    public class PlayerComponentSearcher : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera cinemachineCamera;


        private void OnValidate()
        {
            // すでにターゲットが設定されている場合は何もしない
            if (cinemachineCamera.Target.TrackingTarget != null)
                return;
                
            GameObject player = GameObject.FindWithTag(Tag.Player);
            if (player == null)
                return;

            cinemachineCamera.Target.TrackingTarget = player.transform;
        }
    }
}