//------------------------------------
//          Asset Validator
//     Copyright© 2025 OmniShade     
//------------------------------------

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/**
 * AssetValidatorRunner is a utility class meant to help aid in running 
 * multiple types of validators in sequence for a session.
 **/
namespace AssetValidator {

    internal sealed class AssetValidatorRunner {
        // The current mode for the validation session.
        enum Mode {
            ActiveScene,
            ProjectAssets
        }

        Mode mode;
        int sceneProgress;
        string currentScenePath;
        string originalScenePath;
        public bool IsRunning => isRunning;
        bool isRunning;
        public readonly double runningTime;

        readonly IReadOnlyList<string> scenePaths;
        readonly LogCache logCache;
        ProjectAssetValidatorManager projectAssetValidatorManager;
        readonly ActiveSceneValidatorManager activeSceneValidatorManager;

        // Constructor that accepts the LogCache that logs will be stored in and the 
        // SceneValidationMode that scenes will be validated in (if any).
        public AssetValidatorRunner(LogCache _logCache) {
            scenePaths = ProjectTools.GetScenePaths();
            originalScenePath = SceneManager.GetActiveScene().path;
            activeSceneValidatorManager = new ActiveSceneValidatorManager(_logCache);
            logCache = _logCache;
            isRunning = true;
            runningTime = EditorApplication.timeSinceStartup;
            projectAssetValidatorManager = new ProjectAssetValidatorManager(logCache);
        }

        // Returns true if the validation run has completed, otherwise false.
        public bool IsComplete() {
            return (scenePaths == null || sceneProgress >= scenePaths.Count) &&
                (projectAssetValidatorManager == null || projectAssetValidatorManager.IsComplete());
        }

        // Returns a scalar value [0-1] representing the progress of the current validation area.
        public float GetProgress() {
            switch (mode) {
                case Mode.ActiveScene:
                    return (float)sceneProgress / scenePaths.Count; ;
                case Mode.ProjectAssets:
                    return projectAssetValidatorManager != null
                        ? projectAssetValidatorManager.GetProgress() : 1f;
                default:
                    throw new ArgumentOutOfRangeException(Enum.GetName(typeof(Mode), mode));
            }
        }

        // Returns a message describing the progress of the current validation area.
        public string GetProgressMessage() {
            return string.Format("Running for {0:F} seconds...",
                EditorApplication.timeSinceStartup - runningTime);
        }

        // Synchronously runs validation on all of the configured areas.
        public void Run() {
            if (projectAssetValidatorManager != null) {
                projectAssetValidatorManager.Search();
                projectAssetValidatorManager.Validate();
            }
            while (HasScenesToSearch())
                ContinueSceneValidation();
            RestoreOriginalScene();
            isRunning = false;
        }

        // Runs a synchronous step of the current validation area and marks the runner as complete if all
        // areas have been completed.
        public void ContinueRunning() {
            ContinueProjectAssetValidation();
            ContinueSceneValidation();
            isRunning = !IsComplete();
            if (!isRunning)
                RestoreOriginalScene();
        }

        // Runs a synchronous step of project asset validation.
        void ContinueProjectAssetValidation() {
            if (projectAssetValidatorManager != null && !projectAssetValidatorManager.IsComplete()) {
                mode = Mode.ProjectAssets;
                projectAssetValidatorManager.ContinueSearch();
                projectAssetValidatorManager.ContinueValidate();
            }
        }

        // Runs a synchronous step of scene validation (cross and/or active if enabled).
        void ContinueSceneValidation() {
            if (HasScenesToSearch()) {
                mode = Mode.ActiveScene;
                currentScenePath = scenePaths[sceneProgress];

                // Only load the next scene if we are not already in it
                if (SceneManager.GetActiveScene().path != currentScenePath) {
                    try {
                        EditorSceneManager.OpenScene(currentScenePath);
                    }
                    catch {
                        // Skip deleted scenes
                    }
                }

                activeSceneValidatorManager.Search();
                activeSceneValidatorManager.Validate();
                ++sceneProgress;
            }
        }

        // Returns true whether or not there are scenes to search AND whether we have searched them already.
        bool HasScenesToSearch() {
            return scenePaths != null && sceneProgress < scenePaths.Count;
        }

        void RestoreOriginalScene() {
            if (!string.IsNullOrEmpty(originalScenePath)) {
                try {
                    EditorSceneManager.OpenScene(originalScenePath);
                }
                catch {
                    originalScenePath = string.Empty;
                }
            }
        }
    }
}
