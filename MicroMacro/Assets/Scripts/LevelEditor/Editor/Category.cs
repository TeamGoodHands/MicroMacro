using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.LevelEditor
{
    public class Category
    {
        private string name;
        private PreviewTextureCreator previewTextureCreator;
        private List<Button> buttonGroup = new List<Button>();
        private Button selectedButton;

        private static readonly Color backgroundColor = Color.clear;
        private static readonly Color selectedColor = new Color(0.13f, 0.2f, 0.14f);
        private static readonly float buttonSize = 64;
        private static readonly float rowCount = 3;

        public event Action<GameObject> OnObjectChanged; 
        public event Action OnPlaceCanceled; 

        public Category(string name)
        {
            this.name = name;
            previewTextureCreator = new PreviewTextureCreator();
        }

        public Tab CreateTab(GameObject[] prefabs)
        {
            var tab = new Tab(name);
            var elementsView = new ScrollView(ScrollViewMode.Horizontal)
            {
                contentContainer =
                {
                    style =
                    {
                        flexDirection = FlexDirection.Column,
                        flexWrap = Wrap.Wrap,
                        maxHeight = buttonSize * rowCount
                    }
                }
            };

            var renderTextures = previewTextureCreator.CreatePreviewTextures(prefabs);

            foreach (RenderTexture renderTexture in renderTextures)
            {
                var button = CreateSelectionElement(renderTexture);
                elementsView.Add(button);
                buttonGroup.Add(button);
            }

            for (var i = 0; i < buttonGroup.Count; i++)
            {
                var button = buttonGroup[i];
                var prefab = prefabs[i];

                button.focusable = false;
                button.clicked += () =>
                {
                    ResetCategoryGroup();

                    if (selectedButton == button)
                    {
                        ResetSelectedButton();
                        OnPlaceCanceled?.Invoke();
                    }
                    else
                    {
                        selectedButton = button;
                        button.style.backgroundColor = selectedColor;
                        OnObjectChanged?.Invoke(prefab);
                    }
                };
            }

            tab.Add(elementsView);

            return tab;
        }

        private Button CreateSelectionElement(RenderTexture image)
        {
            Button button = new Button
            {
                style =
                {
                    width = buttonSize,
                    height = buttonSize,
                    marginBottom = 0f,
                    marginTop = 0f,
                    marginLeft = 0f,
                    marginRight = 0f,
                    backgroundColor = backgroundColor,
                    backgroundImage = Background.FromRenderTexture(image)
                },
            };

            return button;
        }

        public void ResetCategoryGroup()
        {
            foreach (Button button in buttonGroup)
            {
                button.style.backgroundColor = backgroundColor;
            }
        }

        public void ResetSelectedButton()
        {
            selectedButton = null;
        }

        public void CleanUp()
        {
            previewTextureCreator.Dispose();
        }
    }
}