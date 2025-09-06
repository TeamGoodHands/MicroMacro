//------------------------------------
//          Asset Validator
//     Copyright© 2025 OmniShade     
//------------------------------------

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/**
 * Helper methods for dealing with Unity project assets and folder.
 **/
namespace AssetValidator {

    public static class ProjectTools {
        // Returns a read-only list of scene paths
        public static IReadOnlyList<string> GetScenePaths() {
            var finalScenes = GetAllScenePathsInAssetBundles();
            finalScenes.AddRange(GetAllScenePathsInBuildSettings());
            return finalScenes;
        }

        // Returns a list of scene paths in relative path format from the Unity Assets folder for all
        // SceneAssets included in AssetBundles.
        public static List<string> GetAllScenePathsInAssetBundles() {
            var sceneNames = new List<string>();
            var allAssetBundleNames = AssetDatabase.GetAllAssetBundleNames();
            foreach (string bundleName in allAssetBundleNames) {
                var assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);
                foreach (var assetName in assetNames) {
                    if (assetName.Contains(".unity"))
                        sceneNames.Add(assetName);
                }
            }
            return sceneNames;
        }

        // Returns a list of scene paths in relative path format from the Unity Assets folder for all
        // SceneAssets included in the build settings.
        public static List<string> GetAllScenePathsInBuildSettings() {
            return EditorBuildSettings.scenes.Select(x => x.path).ToList();
        }

        // Strips the "Assets/" prefix from a path
        public static string StripAssetsPrefix(string path) {
            const string AssetsPrefix = "Assets/";
            if (path.StartsWith(AssetsPrefix))
                return path[AssetsPrefix.Length..];
            else
                return path;
        }

        // Attempts to ping an UnityEngine.Object instance in the current scene or project window
        // based on the passed ValidationLog
        internal static void TryPingObject(ValidationLog log) {
            Object obj = null;
            if (log.source == LogSource.Scene)
                obj = GameObject.Find(log.objectPath);
            else if (log.source == LogSource.Project)
                obj = AssetDatabase.LoadAssetAtPath(log.objectPath, typeof(Object));

            if (obj != null)
                EditorGUIUtility.PingObject(obj);
            else
                Debug.LogWarningFormat("Could not find object at path of [{0}]", log.objectPath);
        }
    }
}
