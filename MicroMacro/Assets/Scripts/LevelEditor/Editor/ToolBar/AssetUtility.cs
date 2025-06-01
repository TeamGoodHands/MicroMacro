using UnityEditor;
using UnityEngine;

namespace Editor.LevelEditor
{
    public static class AssetUtility
    {
        public static GameObject[] LoadAllPrefabs(string optionalPath = "")
        {
            string[] GUIDs;
            if (optionalPath != "")
            {
                if (optionalPath.EndsWith("/"))
                {
                    optionalPath = optionalPath.TrimEnd('/');
                }

                GUIDs = AssetDatabase.FindAssets("t:Prefab", new string[] { optionalPath });
            }
            else
            {
                GUIDs = AssetDatabase.FindAssets("t:Prefab");
            }

            GameObject[] objectList = new GameObject[GUIDs.Length];

            for (int index = 0; index < GUIDs.Length; index++)
            {
                string guid = GUIDs[index];
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                GameObject asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject;
                objectList[index] = asset;
            }

            return objectList;
        }
    }
}