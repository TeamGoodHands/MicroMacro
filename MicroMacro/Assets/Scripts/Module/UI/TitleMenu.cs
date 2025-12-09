using System;
using CoreModule.Attribute;
using CoreModule.Input;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Module.UI
{
    public class TitleMenu : MonoBehaviour
    {
        [SerializeField] private Button titleButton;
        [SerializeField] private SceneField stageSelectScene;

        private void Start()
        {
            titleButton.onClick.AddListener(OnTitleButtonClicked);
            SelectTitleButton().Forget();
        }

        private void Update()
        {
            // 現在何もUIが選択されていない状態になったら
            if (EventSystem.current.currentSelectedGameObject == null)
            {
                // もう一度タイトルボタンを選択状態する
                titleButton.Select();
            }
        }

        private async UniTaskVoid SelectTitleButton()
        {
            // Buttonが読み込まれるまで1フレーム待つ
            await UniTask.Yield();
            titleButton.Select();
        }

        private async void OnTitleButtonClicked()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
            SceneManager.LoadScene(stageSelectScene);
        }

        private void OnDestroy()
        {
            titleButton.onClick.RemoveListener(OnTitleButtonClicked);
        }
    }
}