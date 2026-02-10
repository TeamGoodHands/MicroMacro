using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Module.Application.SceneSwitch;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Module.UI
{
    /// <summary>
    /// 仮のステージセレクトメニュー
    /// </summary>
    public class StageSelectMenu : MonoBehaviour
    {
        [SerializeField] private GameObject buttonPrefab;
        [SerializeField] private FadeAndSceneTransition sceneTransition;
        [SerializeField] private List<SceneButton> sceneButtons;
        [SerializeField] private int currentIndex;

        private async void Start()
        {
            RegisterSelector();

            await UniTask.Yield();

            // 現在のインデックスで選択する
            sceneButtons[currentIndex].Button.Select();
        }

        private void Update()
        {
            // 現在何もUIが選択されていない状態になったら
            if (EventSystem.current.currentSelectedGameObject == null &&
                sceneButtons.Count > 0)
            {
                // もう一度ボタンを選択状態する
                sceneButtons[currentIndex].Button.Select();
            }
        }

        private void RegisterSelector()
        {
            int index = 0;

            foreach (SceneButton sceneButton in sceneButtons)
            {
                // ボタンのイベント登録
                int buttonIndex = index;

                // ボタンがクリックされたらシーンをロードする
                sceneButton.Button.onClick.AddListener(async () =>
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: destroyCancellationToken);
                    ;

                    currentIndex = buttonIndex;
                    sceneTransition.StartPageFlipTransition(sceneButton.SceneName);
                });


                // ボタンが選択されたらインデックスを更新する
                var selector = sceneButton.GetComponentInChildren<ButtonSelector>();
                selector.OnSelectStateChanged += isSelected =>
                {
                    if (isSelected)
                    {
                        currentIndex = buttonIndex;
                    }
                };

                index++;
            }
        }
    }
}