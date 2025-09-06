//------------------------------------
//          Asset Validator
//     Copyright© 2025 OmniShade     
//------------------------------------

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

/**
 * A TreeView representing one or more groups of ValidationLogTreeViewItems
 **/
namespace AssetValidator {

    internal sealed class ValidationLogTreeView : TreeView {
        IReadOnlyList<ValidationLog> logs;

        public ValidationLogTreeView(TreeViewState state)
            : base(state) {
            showAlternatingRowBackgrounds = true;
            showBorder = true;
        }

        // Creates the full tree of ValidationLogTreeView and returns the root element.
        protected override TreeViewItem BuildRoot() {
            var root = ValidationLogTreeViewTools.CreateTreeGroupedByValidatorType(logs);
            SetupDepthsFromParentsAndChildren(root);
            return root;
        }

        // Renders the row GUI of an individual ValidationLogTreeViewItem
        protected override void RowGUI(RowGUIArgs args) {
            float contentIndent = GetContentIndent(args.item);

            // Background for header and body
            var bgRect = args.rowRect;
            bgRect.x = contentIndent;
            bgRect.width = bgRect.width - contentIndent - 5f;
            bgRect.yMin += 2f;
            bgRect.yMax -= 2f;

            var headerRect = new Rect(bgRect);
            headerRect.xMin += 5f;
            if (args.item is not ValidationLogTreeViewItem vLogTreeItem)  // Draw Header Content
                OnHeaderGUI(headerRect, args.label, args.item);
            else {  // Draw Body Content
                var controlsRect = new Rect(headerRect);
                controlsRect.xMin += 16f;
                OnControlsGUI(controlsRect, vLogTreeItem);
            }
        }

        // Renders the header GUI of an individual ValidationLogTreeViewItem or
        // ValidationLogTreeViewHeader based on where we are currently rendering in the ValidationLogTreeView.
        static void OnHeaderGUI(Rect headerRect, string label, TreeViewItem treeViewItem) {
            var style = GraphicsTools.GetLogHeaderStyle();

            headerRect.y += 1f;

            // Do toggle
            var toggleRect = headerRect;
            toggleRect.width = 16f;

            var labelRect = new Rect(headerRect);
            labelRect.xMin += toggleRect.width + 2f;

            // If this header is for a grouping of logs, render the header showing the total count of
            // each type of log present.
            if (treeViewItem is ValidationLogTreeViewHeader vLogTreeViewHeader) {
                labelRect.width -= 50f;
                GUI.Label(labelRect, label, style);
                var fieldRect = new Rect(labelRect) {
                    xMin = labelRect.xMin + labelRect.width,
                    width = 50f
                };

                // Now that we have drawn our title, lets draw the aggregate logs if possible
                GUI.Label(fieldRect, new GUIContent(vLogTreeViewHeader.ErrorCount.ToString(), 
                    GraphicsTools.ErrorIconSmall));
            }
            else
                GUI.Label(labelRect, label, style);
        }

        // Renders the controls for an individual ValidationLogTreeViewItem.
        static void OnControlsGUI(Rect controlsRect, ValidationLogTreeViewItem viewItem) {
            var rect = controlsRect;
            rect.y += 3f;
            rect.height = EditorGUIUtility.singleLineHeight;

            var prefixRect = new Rect(rect) { width = 64f };
            var fieldRect = new Rect(rect) {
                width = rect.width - prefixRect.width,
                x = rect.x + prefixRect.width
            };

            if (viewItem.Log.Source != LogSource.Project) {
                EditorGUI.LabelField(prefixRect, "Scene:   ", EditorStyles.boldLabel);
                string scenePath = ProjectTools.StripAssetsPrefix(viewItem.Log.GetSourceDescription());
                EditorGUI.LabelField(fieldRect, scenePath);

                prefixRect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
                fieldRect.y = prefixRect.y;
                fieldRect.height = EditorGUIUtility.singleLineHeight;
            }

            EditorGUI.LabelField(prefixRect, "Object:   ", EditorStyles.boldLabel);
            string objectPath = ProjectTools.StripAssetsPrefix(viewItem.Log.objectPath);
            if (viewItem.Log.CanPingObject()) {
                if (GUI.Button(fieldRect, objectPath, EditorStyles.textField))
                    ProjectTools.TryPingObject(viewItem.Log);
            }
            else
                EditorGUI.LabelField(fieldRect, objectPath);


            prefixRect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
            fieldRect.y = prefixRect.y;

            int charactersPerRow = GraphicsTools.GetCharactersPerRow(fieldRect, viewItem.Log.message);
            string multiLineMessage = GraphicsTools.GetMultilineString(viewItem.Log.message, charactersPerRow);

            fieldRect.height = 1.7f * EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(prefixRect, "Message:   ", EditorStyles.boldLabel);
            var messageStyle = EditorStyles.label;
            messageStyle.alignment = TextAnchor.UpperLeft;
            EditorGUI.TextArea(fieldRect, multiLineMessage, messageStyle);
        }

        // Returns the height that the row rect should.
        protected override float GetCustomRowHeight(int row, TreeViewItem item) {
            float height = 24f;

            if (item is ValidationLogTreeViewItem vLogTreeViewItem) {
                height = 56;

                // Add space if showing scene
                if (vLogTreeViewItem.Log.Source != LogSource.Project)
                    height += 20;
            }

            return height;
        }

        // Sets the collection of ValidationLog that this will render.
        public void SetLogData(IReadOnlyList<ValidationLog> _logs) {
            logs = _logs;
        }

        // Attempts to reload the data and render again.
        public void TryReload() {
            if (logs != null && logs.Count > 0)
                Reload();
        }
    }
}
