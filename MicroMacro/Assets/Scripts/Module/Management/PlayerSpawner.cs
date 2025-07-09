using Constants;
using Module.Player.Component;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Module.Management
{
    /// <summary>
    /// プレイヤーをスポーンさせるクラス
    /// </summary>
    public class PlayerSpawner : MonoBehaviour
    {
        private PlayerStatus playerStatus;

        private void Start()
        {
            playerStatus = GameObject.FindWithTag(Tag.Player).GetComponent<PlayerStatus>();
            playerStatus.OnDeath += OnPlayerDeath;
        }

        private void OnPlayerDeath()
        {
            // 今はとりあえずシーンを読み込み直す
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
