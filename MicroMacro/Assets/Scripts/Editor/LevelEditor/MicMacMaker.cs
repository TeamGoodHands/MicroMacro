using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.LevelEditor
{
    /// <summary>
    /// レベルエディタ
    /// </summary>
    public class MicMacMaker : EditorWindow
    {
        private const string DictionaryPath = "Assets/Settings/LevelObjectDictionary.asset";
        private Dictionary<Tab, LevelObjectGroup> levelObjectGroups = new Dictionary<Tab, LevelObjectGroup>();
        private float createdTime;

        [MenuItem("Tools/MicMacMaker")]
        private static void CreateWindow()
        {
            var win = GetWindow<MicMacMaker>();
            win.titleContent = new GUIContent("MicMacMaker");
        }

        public void CreateGUI()
        {
            // Refreshボタン作成
            var refreshButton = CreateRefreshButton();
            rootVisualElement.Add(refreshButton);

            // タブビューの作成
            var tabView = new TabView();
            var categories = AssetDatabase.LoadAssetAtPath<MicMacMakerSettings>(DictionaryPath);
            
            foreach (MicMacMakerSettings.ObjectCategory category in categories.ObjectCategories)
            {
                LevelObjectGroup group = new LevelObjectGroup(category.Name);
                Tab tab = group.CreateTab(category.Prefabs);

                levelObjectGroups.Add(tab, group);
                tabView.Add(tab);
            }

            tabView.activeTabChanged += (before, after) =>
            {
                levelObjectGroups[before].ResetButtonGroup();
                levelObjectGroups[before].ResetSelectedButton();
            };

            rootVisualElement.Add(tabView);
        }



        private Button CreateRefreshButton()
        {
            createdTime = Time.realtimeSinceStartup;

            return new Button(() =>
            {
                if (createdTime + 1f > Time.realtimeSinceStartup)
                {
                    return;
                }

                rootVisualElement.Clear();
                OnDisable();
                CreateGUI();
            })
            {
                text = "Refresh",
                style =
                {
                    width = 80f,
                    height = 30f
                }
            };
        }

        private void OnDisable()
        {
            foreach (LevelObjectGroup group in levelObjectGroups.Values)
            {
                group.CleanUp();
            }

            levelObjectGroups.Clear();
        }
    }
}