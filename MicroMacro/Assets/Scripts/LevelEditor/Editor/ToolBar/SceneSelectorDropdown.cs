using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LevelEditor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Editor.LevelEditor
{
    /// <summary>
    /// ステージ選択欄
    /// </summary>
    [EditorToolbarElement(nameof(SceneSelectorDropdown), typeof(SceneView))]
    internal class SceneSelectorDropdown : EditorToolbarDropdown
    {
        public static event Action<Scene> OnSceneChanged;

        public SceneSelectorDropdown()
        {
            text = SceneManager.GetActiveScene().name;
            SceneCreatorButton.OnLevelCreated += scene => text = scene.name;

            clicked += ShowDropdown;

            EditorSceneManager.sceneOpened += (scene, mode) =>
            {
                if (mode == OpenSceneMode.Single)
                {
                    text = scene.name;
                }
            };
        }

        private void ShowDropdown()
        {
            var menu = new GenericMenu();

            Scene currentScene = SceneManager.GetActiveScene();

            //全てのシーンアセットを取得
            foreach (string rootPath in Directory.GetDirectories(LevelEditorUtil.SceneSavePath))
            {
                List<SceneAsset> assets = LevelEditorUtil.LoadAllAsset<SceneAsset>(rootPath);

                //ディレクトリ名を取得して、それをサブメニューとする
                string rootName = Path.GetFileName(rootPath);

                foreach (SceneAsset scene in assets)
                {
                    bool enableItem = scene.name == currentScene.name;
                    string fileName = $"{rootName}/{scene.name}";
                    menu.AddItem(new GUIContent(fileName), enableItem, () => OnDropdownItemSelected(fileName));
                }
            }

            menu.ShowAsContext();
        }

        private void OnDropdownItemSelected(string itemName)
        {
            Scene activeScene = SceneManager.GetActiveScene();
            string sceneName = activeScene.name;
            string sceneAssetPath = FindScenePath(sceneName);

            if (activeScene.isDirty && EditorUtility.DisplayDialog("SceneToolbar", "現在編集中のシーンをセーブしますか？", "はい", "いいえ"))
            {
                //現在のシーンをセーブ
                EditorSceneManager.SaveScene(activeScene, sceneAssetPath);
            }

            //シーン名からシーンをロード
            string assetPath = LevelEditorUtil.GetSceneAssetPath(itemName);
            Scene scene = EditorSceneManager.OpenScene(assetPath, OpenSceneMode.Single);
            text = scene.name;
            OnSceneChanged?.Invoke(scene);
        }

        private string FindScenePath(string sceneName)
        {
            //シーン名からシーンのパスを取得

            string sceneAssetPath = AssetDatabase.FindAssets("t:Scene", new string[] { LevelEditorUtil.SceneSavePath })
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .FirstOrDefault(path =>
                {
                    string fileName = Path.GetFileName(path);

                    //.unityを削除する
                    var slicedName = fileName.Substring(0, fileName.Length - 6);

                    //シーン名とアセット名が等しかったらそのパスを返す
                    return slicedName == sceneName;
                });

            // シーンアセットが見つからなかった場合は、ルートパスを使用
            if (string.IsNullOrEmpty(sceneAssetPath))
            {
                sceneAssetPath = $"{LevelEditorUtil.SceneSavePath}/{sceneName}.unity";
            }

            return sceneAssetPath;
        }
    }
}