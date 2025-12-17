using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LevelEditor.Editor;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.LevelEditor
{
    /// <summary>
    /// レベルエディタ
    /// </summary>
    public class MicMacMaker : EditorWindow
    {
        private static readonly int backAlpha = Shader.PropertyToID("_BackAlpha");
        private const string dictionaryPath = "Assets/Settings/LevelObjectDictionary.asset";
        private Dictionary<Tab, Category> categoryGroups = new Dictionary<Tab, Category>();
        private float createdTime;
        private Category selectedCategory;
        private ObjectPlacer objectPlacer;
        private GameObject lastSelectedObject;
        private Button eraseButton;
        private Material groundMaterial;

        public event Action<bool> OnEnableStateChanged;

        [MenuItem("Tools/MicMacMaker")]
        private static void CreateWindow()
        {
            var win = GetWindow<MicMacMaker>();
            win.titleContent = new GUIContent("MicMacMaker");
        }

        public void OnSceneGUI()
        {
            objectPlacer?.DrawMousePosition();
        }

        private void LoadGroundMaterial()
        {
            if (groundMaterial != null)
                return;

            var settings = AssetDatabase.LoadAssetAtPath<MicMacMakerSettings>(dictionaryPath);
            groundMaterial = settings.GroundMaterial;
        }

        public void CreateGUI()
        {
            // 即座に実行せず、delayCallに入れる
            EditorApplication.delayCall += ReCreate;
        }

        private void ReCreate()
        {
            OnDisable();
            LoadGroundMaterial();

            // EditorToolがアクティブでない場合は、Windowを無効化する
            if (!IsToolActive())
            {
                SetEnable(false);
            }

            objectPlacer = new ObjectPlacer();
            objectPlacer.OnSequenceCanceled += ResetCategory;

            windowFocusChanged += OnWindowFocusChanged;

            EditorApplication.playModeStateChanged -= OnStateChanged;
            EditorApplication.playModeStateChanged += OnStateChanged;

            // 上部のタブバーの作成
            var buttonGroup = CreateButtonGroup();
            rootVisualElement.Add(buttonGroup);

            // タブビューの作成
            var tabView = new TabView();
            var settings = AssetDatabase.LoadAssetAtPath<MicMacMakerSettings>(dictionaryPath);

            foreach (MicMacMakerSettings.ObjectCategory category in settings.ObjectCategories)
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

        public void SetEnable(bool isEnable)
        {
            if (isEnable)
            {
                rootVisualElement.SetEnabled(true);
                rootVisualElement.style.opacity = 1f;
                groundMaterial.SetFloat(backAlpha, 0.7f);
                objectPlacer?.Enable();

                OnEnableStateChanged?.Invoke(true);
            }
            else
            {
                rootVisualElement.SetEnabled(false);
                rootVisualElement.style.opacity = 0.5f;
                groundMaterial.SetFloat(backAlpha, 0f);
                objectPlacer?.Disable();

                OnEnableStateChanged?.Invoke(false);
            }
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

            // 更新ボタン作成
            buttonElements.Add(CreateRefreshButton());

            // 全て破壊ボタン作成
            buttonElements.Add(CreateDestroyButton());

            // 重複確認ボタン作成
            buttonElements.Add(CreateCheckOverlapButton());

            // 再生時に地面を表示するチェックボックス作成
            buttonElements.Add(CreateShowGroundToggle());

            return buttonElements;
        }

        private Toggle CreateShowGroundToggle()
        {
            Toggle toggle = new Toggle()
            {
                text = " <b>再生時に地面を表示する</b>",
                style =
                {
                    overflow = Overflow.Visible,
                    width = 150f,
                    marginLeft = 10f
                },
                value = objectPlacer.GetShowGround()
            };

            OnEnableStateChanged += isEnable =>
            {
                if (isEnable)
                {
                    toggle.value = objectPlacer.GetShowGround();
                }
                else
                {
                    objectPlacer.UpdateShowGround(toggle.value);
                }
            };

            return toggle;
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
                ReCreate();
                objectPlacer.UpdateParentObject();
            })
            {
                text = "<b>更新</b>",
                enableRichText = true,
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

        private Button CreateDestroyButton()
        {
            return new Button(() => { objectPlacer.DestroyAll(); })
            {
                text = "<b>全て破壊</b>",
                enableRichText = true,
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

        private Button CreateCheckOverlapButton()
        {
            return new Button(() => { objectPlacer.Postprocess(); })
            {
                text = "<b>重複削除</b>",
                enableRichText = true,
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

        private void OnStateChanged(PlayModeStateChange state)
        {
            // EditorToolがアクティブでない場合は、何もしない
            if (!IsToolActive())
                return;

            if (state == PlayModeStateChange.ExitingEditMode)
            {
                // EditModeを抜けるとき、オブジェクト配置を停止する
                SetEnable(false);
                objectPlacer.StopPlaceSequence();
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                // EditModeに入るとき、オブジェクト配置を再開する
                ReCreate();
                SetEnable(true);
                if (lastSelectedObject != null)
                {
                    objectPlacer.StartPlaceSequence(lastSelectedObject);
                }
            }
        }

        private void OnWindowFocusChanged()
        {
            // シーンビュー以外がフォーカスされた場合、配置をキャンセルする
            if (!IsSceneViewFocused())
            {
                OnPlaceCanceled();
                ResetCategory();
            }
        }

        private void OnObjectChanged(GameObject prefab)
        {
            objectPlacer.StopPlaceSequence();
            objectPlacer.StartPlaceSequence(prefab);
            lastSelectedObject = prefab;
        }

        private void OnPlaceCanceled()
        {
            objectPlacer.StopPlaceSequence();
            lastSelectedObject = null;
        }

        private void ResetCategory()
        {
            selectedCategory?.ResetCategoryGroup();
            selectedCategory?.ResetSelectedButton();
        }

        private bool IsSceneViewFocused()
        {
            var sceneView = SceneView.lastActiveSceneView;
            return sceneView != null && focusedWindow == sceneView;
        }

        private bool IsToolActive()
        {
            return ToolManager.activeToolType == typeof(MicMacTool);
        }

        private void OnDisable()
        {
            if (objectPlacer != null)
            {
                objectPlacer.Disable();
                objectPlacer = null;
            }

            foreach (Category group in categoryGroups.Values)
            {
                group.CleanUp();
            }

            categoryGroups.Clear();
            rootVisualElement.Clear();
        }
    }
}