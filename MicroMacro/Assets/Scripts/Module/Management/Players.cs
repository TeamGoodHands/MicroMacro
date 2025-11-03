using Constants;
using Module.Application.SceneSwitch;
using Module.Player.Component;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Module.Management
{
    /// <summary>
    /// プレイヤーをスポーンさせるクラス
    /// </summary>
    public class Players : MonoBehaviour
    {
        private PlayerStatus playerStatus;
        [SerializeField] private FadeAndSceneTransition sceneTransition;

        private void Start()
        {
            playerStatus = GameObject.FindWithTag(Tag.Player).GetComponent<PlayerStatus>();
            playerStatus.OnDeath += OnPlayerDeath;
        }

        private void OnPlayerDeath()
        {
            // 今はとりあえずシーンを読み込み直す
            sceneTransition.StartTransition();
        }
    }
}