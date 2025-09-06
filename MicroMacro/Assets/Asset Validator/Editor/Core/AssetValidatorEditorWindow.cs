//------------------------------------
//          Asset Validator
//     Copyright© 2025 OmniShade     
//------------------------------------

using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/**
 * Asset Validator editor window GUI.
 **/
namespace AssetValidator {
    
    internal sealed class AssetValidatorEditorWindow : EditorWindow {
        const string ToolName = "Asset Validator";

        LogCache logCache;
        AssetValidatorRunner runner;
        ValidationLogTreeView validationLogTreeView;
        Rect logRect;
        bool hasRun = false;
        double lastCompleteTime = 0;

        [MenuItem("Window/Asset Validator %&v")]
        public static void LaunchWindow() {
            var window = GetWindow<AssetValidatorEditorWindow>();
            window.Show();
        }

        void OnEnable() {
            titleContent.text = ToolName;
            logCache = new LogCache();
            validationLogTreeView = new ValidationLogTreeView(new TreeViewState());
            EditorSceneManager.sceneOpened += OnSceneLoaded;
        }

        void OnDisable() {
            EditorSceneManager.sceneOpened -= OnSceneLoaded;
        }

        void OnGUI() {
            // Info window
            EditorGUILayout.HelpBox(
                "This tool validates all the assets and scenes in the project.\n" +
                "Tag assets with label '" + AssetValidatorBase.SkipLabel + "' to skip validation if needed.", 
                MessageType.Info);
            EditorGUILayout.Separator();

            // Run Validation Button
            EditorGUI.BeginDisabledGroup(runner != null && runner.IsRunning);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Validate assets and scenes", GUILayout.Height(30), GUILayout.Width(260))) {
                hasRun = true;
                OnValidateSelectionClick();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();

            // If we are running validators, show progress
            if (runner != null && runner.IsRunning) {
                var progressRect = EditorGUILayout.BeginVertical();
                EditorGUI.ProgressBar(progressRect, runner.GetProgress(), runner.GetProgressMessage());
                GUILayout.Space(16);
                EditorGUILayout.EndVertical();
            }

            if (hasRun) {
                EditorGUILayout.Separator();

                // Get rect size
                var groupByRect = EditorGUILayout.BeginHorizontal();
                EditorGUILayout.EndHorizontal();
                groupByRect.y += EditorGUIUtility.standardVerticalSpacing * 2f;
                logRect = new Rect(groupByRect.xMin, groupByRect.yMax, groupByRect.width,
                    position.height - groupByRect.yMin - 25f);

                if (!HasLogs())
                    EditorGUILayout.LabelField("All assets passed validation.");
                else if (!IsRunning() && HasLogs())
                    OnLogsGUI();

                if (lastCompleteTime > 0) {
                    var statusRect = new Rect(groupByRect.xMin + 8, position.height - 25, groupByRect.width, 25);
                    EditorGUI.LabelField(statusRect, string.Format("Completed in {0:F} seconds.", lastCompleteTime));
                }
            }
        }

        void Update() {
            if (runner == null || !runner.IsRunning)
                return;

            runner.ContinueRunning();
            ForceEditorWindowUpdate();
            if (!runner.IsComplete())
                return;

            //EditorSceneManager.OpenScene(currentScenePath)
            lastCompleteTime = EditorApplication.timeSinceStartup - runner.runningTime;
            ForceEditorWindowUpdate();
        }
        
        bool IsRunning() {
            return runner != null && runner.IsRunning;
        }

        bool HasLogs() {
            return logCache != null && logCache.HasLogs();
        }

        void ForceEditorWindowUpdate() {
            Repaint();
        }

        void OnValidateSelectionClick() {
            // Only perform validation across multiple scenes if the current scene is saved, or
            // barring that if you don't care if it gets unloaded and lose changes
            var canExecute = true;
            var currentScene = SceneManager.GetActiveScene();
            if (currentScene.isDirty) {
                var didSave = EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo() && !currentScene.isDirty;
                if (!didSave) {
                    canExecute = EditorUtility.DisplayDialog(ToolName,
                        "Continuing with validation may unload the current scene. Do you wish to continue?",
                        "Continue Validation",
                        "Cancel");
                }
            }
            if (!canExecute)
                return;

            // Clear out any remaining logs and queue up an AssetValidationRunner with
            // the desired config
            logCache.ClearLogs();
            runner = new AssetValidatorRunner(logCache);
        }

        void OnSceneLoaded(Scene scene, OpenSceneMode mode) {
            validationLogTreeView.TryReload();
        }

        void OnLogsGUI() {
            validationLogTreeView.SetLogData(logCache);
            validationLogTreeView.Reload();
            validationLogTreeView.OnGUI(logRect);
        }
    }
}
