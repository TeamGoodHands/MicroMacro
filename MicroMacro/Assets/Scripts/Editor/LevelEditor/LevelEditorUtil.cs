using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LevelEditor
{
    public static class LevelEditorUtil
    {
        public const string SceneSavePath = "Assets/Scenes/Level";
        public const string SceneTemplatePath = "Assets/Scenes/Template/LevelTemplate.scenetemplate";

        public static string GetSceneAssetPath(string assetName)
        {
            return $"{SceneSavePath}/{assetName}.unity";
        }

        public static List<T> LoadAllAsset<T>(string directoryPath) where T : Object
        {
            List<T> assetList = new List<T>();

            string[] filePathArray = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);

            foreach (string filePath in filePathArray)
            {
                T asset = AssetDatabase.LoadAssetAtPath<T>(filePath);
                if (asset != null)
                {
                    assetList.Add(asset);
                }
            }

            return assetList;
        }
    }
}