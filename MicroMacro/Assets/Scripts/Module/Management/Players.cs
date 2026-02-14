using Constants;
using Module.Application.SceneSwitch;
using Module.Player.Component;
using Module.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Module.Management
{
    /// <summary>
    /// プレイヤーをスポーンさせるクラス
    /// </summary>
    public class Players : MonoBehaviour
    {
        private HealthStatus playerStatus;
        [SerializeField] private FadeAndSceneTransition sceneTransition;

        private void Start()
        {
            playerStatus = GameObject.FindWithTag(Tag.Player).GetComponent<HealthStatus>();
            playerStatus.OnDeath += OnPlayerDeath;
        }

        private void OnPlayerDeath()
        {
            sceneTransition.StartTransitionSame();
        }
    }
}