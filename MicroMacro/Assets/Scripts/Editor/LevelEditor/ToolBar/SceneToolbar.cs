using System;
using LevelEditor;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.SceneManagement;
using UnityEditor.SceneTemplate;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Editor.LevelEditor
{
    /// <summary>
    /// ステージ作成ボタン
    /// </summary>
    [EditorToolbarElement(nameof(SceneCreatorButton), typeof(SceneView))]
    internal class SceneCreatorButton : EditorToolbarButton
    {
        public static event Action<Scene> OnLevelCreated;

        public SceneCreatorButton()
        {
            //ステージ名入力欄の登録
            TextField levelNameField = new TextField
            {
                style =
                {
                    minWidth = 80f
                }
            };
            Add(levelNameField);

            text = "Create Level";
            clicked += () => CreateSceneFields(levelNameField);
        }

        private void CreateSceneFields(TextField levelNameField)
        {
            Scene currentLevel = SceneManager.GetActiveScene();

            if (!ValidateLevelName(levelNameField.text))
            {
                return;
            }

            //テンプレートからシーンを生成
            SceneTemplateAsset template = AssetDatabase.LoadAssetAtPath<SceneTemplateAsset>(LevelEditorUtil.SceneTemplatePath);

            if (template == null)
            {
                Debug.LogError($"シーンテンプレートが見つかりません。" +
                               $"\n{LevelEditorUtil.SceneTemplatePath}にテンプレートを作成してください。");
                return;
            }
            
            string savePath = LevelEditorUtil.GetSceneAssetPath(levelNameField.text);
            InstantiationResult result = SceneTemplateService.Instantiate(template, false, savePath);

            if (currentLevel.IsValid())
            {
                EditorSceneManager.CloseScene(currentLevel, true);
            }

            //入力欄の初期化
            levelNameField.SetValueWithoutNotify(null);

            Scene newScene = result.scene;
            OnLevelCreated?.Invoke(newScene);
        }

        private bool ValidateLevelName(string name)
        {
            if (name == string.Empty)
            {
                Debug.LogError("Level名が設定されていないため、作成に失敗しました。");
                return false;
            }

            string[] names = name.Split('/');

            if (names.Length != 2)
            {
                Debug.LogError("不正なステージ名です。");
                return false;
            }

            return true;
        }
    }


    [Overlay(typeof(SceneView), "LevelToolbar")]
    public class SceneToolbar : ToolbarOverlay
    {
        private SceneToolbar() : base(
            nameof(SceneCreatorButton),
            nameof(SceneSelectorDropdown)
        )
        {
        }
    }
}