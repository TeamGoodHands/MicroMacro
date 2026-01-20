using System;
using DG.Tweening;
using Module.Player.Component;
using UnityEngine;
using UnityEngine.UI;

namespace Module.UI
{
    public class BossHealthView : MonoBehaviour
    {
        [SerializeField] private GameObject baseObject;
        [SerializeField] private HealthStatus healthStatus;
        [SerializeField] private Image[] healthImage;

        private void Awake()
        {
            healthStatus.OnDamage += OnDamage;
            healthStatus.OnReset += Reset;

            healthImage = new Image[healthStatus.MaxHealth];

            // HP分インスタンス生成
            GameObject obj = null;
            for (int i = 0; i < healthStatus.MaxHealth; i++)
            {
                // 左側
                if (i % 2 == 0)
                {
                    obj = Instantiate(baseObject, transform);
                    Transform leftHeart = obj.transform.GetChild(0);

                    healthImage[i] = leftHeart.GetComponent<Image>();
                }
                // 右側
                else
                {
                    Transform rightHeart = obj.transform.GetChild(1);

                    healthImage[i] = rightHeart.GetComponent<Image>();
                    healthImage[i].enabled = true;
                }
            }

            // オリジナルのオブジェクトは削除する
            Destroy(baseObject);
        }

        private void OnDamage(int damage)
        {
            int delta = healthStatus.PreviousHealth - healthStatus.CurrentHealth;
            for (int i = 0; i < delta; i++)
            {
                int index = healthStatus.CurrentHealth + i;

                // とりあえず仮で点滅させる
                healthImage[index].DOFade(0f, 0.1f).SetLink(gameObject).SetLoops(5, LoopType.Yoyo).OnComplete(() => { healthImage[index].color = Color.clear; }).SetUpdate(true);
            }
        }

        private void Reset()
        {
            foreach (Image image in healthImage)
            {
                image.color = Color.white;
            }
        }
    }
}