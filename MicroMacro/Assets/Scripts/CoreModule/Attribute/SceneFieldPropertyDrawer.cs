#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace CoreModule.Attribute
{
    [CustomPropertyDrawer(typeof(SceneField))]
    public class SceneFieldPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // シーン名プロパティの取得
            SerializedProperty sceneNameProperty = property.FindPropertyRelative("sceneName");

            // ビルド設定に含まれるシーン名の配列を取得
            string[] scenes = GetSceneNames();

            // 現在のシーン名を取得
            string currentSceneName = sceneNameProperty.stringValue;

            // 現在のシーン名のインデックスを取得
            int currentIndex = -1;
            for (int i = 0; i < scenes.Length; i++)
            {
                if (scenes[i] == currentSceneName)
                {
                    currentIndex = i;
                    break;
                }
            }

            // プルダウンメニューを表示
            int selectedIndex = EditorGUI.Popup(position, label.text, currentIndex, scenes);

            // 選択されたシーン名を更新
            if (selectedIndex >= 0 && selectedIndex < scenes.Length)
            {
                sceneNameProperty.stringValue = scenes[selectedIndex];
            }
        }

        private string[] GetSceneNames()
        {
            var sceneList = new System.Collections.Generic.List<string>();
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (!scene.enabled) continue;
                string scenePath = scene.path;
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                sceneList.Add(sceneName);
            }

            return sceneList.ToArray();
        }
    }
}
#endif