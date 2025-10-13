using System;
using DG.Tweening;
using Module.Player.Component;
using UnityEngine;
using UnityEngine.UI;

namespace Module.UI
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private GameObject baseObject;
        [SerializeField] private PlayerStatus playerStatus;
        private Image[] healthImage;

        private void Start()
        {
            playerStatus.OnDamage += OnDamage;
            healthImage = new Image[playerStatus.MaxHealth];
            healthImage[0] = baseObject.GetComponent<Image>();

            for (int i = 1; i < playerStatus.MaxHealth; i++)
            {
                GameObject obj = Instantiate(baseObject, transform);
                healthImage[i] = obj.GetComponent<Image>();
            }
        }

        private void OnDamage(int damage)
        {
            Time.timeScale = 0f;
            var rectTransform = healthImage[playerStatus.CurrentHealth].rectTransform;
            healthImage[playerStatus.CurrentHealth].DOFade(0f, 0.1f).SetLoops(5, LoopType.Yoyo).OnComplete(() =>
            {
                healthImage[playerStatus.CurrentHealth].color = Color.clear;
                Time.timeScale = 1f;
            }).SetUpdate(true);
        }
    }
}