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
        private const string dictionaryPath = "Assets/Settings/LevelObjectDictionary.asset";
        private Dictionary<Tab, Category> categoryGroups = new Dictionary<Tab, Category>();
        private float createdTime;
        private Category selectedCategory;
        private ObjectPlacer objectPlacer;

        [MenuItem("Tools/MicMacMaker")]
        private static void CreateWindow()
        {
            var win = GetWindow<MicMacMaker>();
            win.titleContent = new GUIContent("MicMacMaker");
        }

        public void CreateGUI()
        {
            objectPlacer = new ObjectPlacer();
            objectPlacer.OnSequenceCanceled += ResetCategory;

            windowFocusChanged += () =>
            {
                if (!IsSceneViewFocused() && objectPlacer.IsPlacing)
                {
                    objectPlacer.StopPlaceSequence();
                    ResetCategory();
                }
            };

            // 上部のタブバーの作成
            var buttonGroup = CreateButtonGroup();
            rootVisualElement.Add(buttonGroup);

            // タブビューの作成
            var tabView = new TabView();
            var categories = AssetDatabase.LoadAssetAtPath<MicMacMakerSettings>(dictionaryPath);

            foreach (MicMacMakerSettings.ObjectCategory category in categories.ObjectCategories)
            {
                // カテゴリタブの作成
                var group = new Category(category.Name);
                var tab = group.CreateTab(category.Prefabs);
                categoryGroups.Add(tab, group);

                group.OnObjectChanged += OnObjectChanged;
                group.OnPlaceCanceled += OnPlaceCanceled;

                tabView.Add(tab);
            }

            // カテゴリタブが切り替わったときはカテゴリグループをリセットする
            tabView.activeTabChanged += (before, after) =>
            {
                categoryGroups[before].ResetCategoryGroup();
                categoryGroups[before].ResetSelectedButton();

                selectedCategory = categoryGroups[after];
            };

            rootVisualElement.Add(tabView);
            selectedCategory = categoryGroups.First().Value;
        }


        private VisualElement CreateButtonGroup()
        {
            var buttonElements = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row
                }
            };

            // Refreshボタン作成
            buttonElements.Add(CreateRefreshButton());

            // Eraseボタン作成
            buttonElements.Add(CreateEraseButton());

            return buttonElements;
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
                    height = 30f,
                    marginBottom = 4f,
                    marginTop = 4f,
                    marginLeft = 0f,
                    marginRight = 0f,
                }
            };
        }

        private Button CreateEraseButton()
        {
            return new Button(ResetCategory)
            {
                text = "Erase",
                style =
                {
                    width = 80f,
                    height = 30f,
                    marginBottom = 4f,
                    marginTop = 4f,
                    marginLeft = 0f,
                    marginRight = 0f,
                }
            };
        }

        private void OnObjectChanged(GameObject prefab)
        {
            objectPlacer.StopPlaceSequence();
            objectPlacer.StartPlaceSequence(prefab);
        }

        private void OnPlaceCanceled()
        {
            objectPlacer.StopPlaceSequence();
        }

        private void ResetCategory()
        {
            selectedCategory.ResetCategoryGroup();
            selectedCategory.ResetSelectedButton();
        }

        private bool IsSceneViewFocused()
        {
            var sceneView = SceneView.lastActiveSceneView;
            return sceneView != null && focusedWindow == sceneView;
        }

        private void OnDisable()
        {
            foreach (Category group in categoryGroups.Values)
            {
                group.CleanUp();
            }

            categoryGroups.Clear();
        }
    }
}