using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.LevelEditor
{
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
            var refreshButton = CreateRefreshButton();
            rootVisualElement.Add(refreshButton);

            var tabView = new TabView();
            var categories = AssetDatabase.LoadAssetAtPath<LevelObjectCategories>(DictionaryPath);

            foreach (LevelObjectCategories.ObjectCategory category in categories.ObjectCategories)
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

        private void StartPlaceSequence()
        {
            SceneView.duringSceneGui += UpdateSceneGUI;
        }

        private void UpdateSceneGUI(SceneView sceneView)
        {
            Event e = Event.current;
            switch (e.type)
            {
                case EventType.MouseMove:
                    Debug.Log("Mouse Move");

                    break;
                case EventType.MouseDown:
                    if (e.shift)
                    {
                    }

                    Debug.Log("Mouse Down");
                    break;
                case EventType.MouseLeaveWindow:
                    Debug.Log("Mouse Leave Window");
                    SceneView.duringSceneGui -= UpdateSceneGUI;
                    break;
                case EventType.MouseEnterWindow:
                    Debug.Log("Mouse Enter Window");
                    break;
                case EventType.ScrollWheel:
                    Debug.Log("Scroll Wheel");
                    break;
            }
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