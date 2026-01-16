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

        private static int deathCount;

        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            deathCount = 0;
        }

        private void Start()
        {
            playerStatus = GameObject.FindWithTag(Tag.Player).GetComponent<HealthStatus>();
            playerStatus.OnDeath += OnPlayerDeath;
        }

        private void OnPlayerDeath()
        {
            deathCount++;

            if (deathCount == 1)
            {
                // 今はとりあえずシーンを読み込み直す
                sceneTransition.StartTransitionSame();
            }
            else
            {
                deathCount = 0;
                sceneTransition.StartTransition("Feedback");
            }
        }
    }
}