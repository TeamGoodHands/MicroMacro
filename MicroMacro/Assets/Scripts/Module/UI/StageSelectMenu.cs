using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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
        [SerializeField] private List<Button> createdButtons;
        [SerializeField] private int currentIndex;

        private async void Start()
        {
            // シーン遷移ボタンを生成
            List<string> sceneNames = GetSceneNames();

            if (sceneNames.Count == 0)
            {
                Debug.LogWarning("No scenes found for stage selection.");
                return;
            }

            CreateSceneButtons(sceneNames);

            await UniTask.Yield();

            // 現在のインデックスで選択する
            createdButtons[currentIndex].Select();
        }

        private void Update()
        {
            // 現在何もUIが選択されていない状態になったら
            if (EventSystem.current.currentSelectedGameObject == null &&
                createdButtons.Count > 0)
            {
                // もう一度ボタンを選択状態する
                createdButtons[currentIndex].Select();
            }
        }

        private List<string> GetSceneNames()
        {
            const string sceneSavePath = "Assets/Scenes/Level/Main";
            const string sceneSavePathTest = "Assets/Scenes/Level/Test";
            const string sceneSavePathOutGame = "Assets/Scenes/Level/OutGame";
            const string rootSceneName = "Root";
            const string stage1_1 = "1-1Ver3";
            const string stage1_2 = "1-2Ver3";
            const string bossStage = "Boss2";
            const string feedback = "Feedback";
            const string title = "Title";

            var sceneNames = new List<string>();

            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                // シーンパスを取得
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);

                // 指定したパスに含まれていなかったらスキップ
                if (!scenePath.Contains(sceneSavePath) && !scenePath.Contains(sceneSavePathTest) && !scenePath.Contains(sceneSavePathOutGame))
                    continue;

                // 拡張子を除いたファイル名(シーン名)を取得
                string sceneName = Path.GetFileNameWithoutExtension(scenePath);

                // ルートシーンは省く
                if (sceneName == rootSceneName)
                    continue;

                // 一旦ボスステージとメインステージ、feedback意外省く
                if (sceneName != stage1_1 && sceneName != stage1_2 && sceneName != bossStage && sceneName != title && sceneName != feedback)
                    continue;

                sceneNames.Add(sceneName);
            }

            return sceneNames;
        }

        private void CreateSceneButtons(List<string> sceneNames)
        {
            int index = 0;

            foreach (string sceneName in sceneNames)
            {
                // ボタンのオブジェクト生成
                GameObject buttonObject = Instantiate(buttonPrefab, transform);

                // シーン名を設定
                var textMesh = buttonObject.GetComponentInChildren<TextMeshProUGUI>();
                textMesh.text = sceneName;

                // ボタンのイベント登録
                var button = buttonObject.GetComponentInChildren<Button>();
                int buttonIndex = index;

                // ボタンがクリックされたらシーンをロードする
                button.onClick.AddListener(async () =>
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
                    
                    currentIndex = buttonIndex;
                    SceneManager.LoadScene(sceneName);
                });


                // ボタンが選択されたらインデックスを更新する
                var selector = buttonObject.GetComponentInChildren<ButtonSelector>();
                selector.OnSelectStateChanged += isSelected =>
                {
                    if (isSelected)
                    {
                        currentIndex = buttonIndex;
                    }
                };

                createdButtons.Add(button);
                index++;
            }
        }
    }
}