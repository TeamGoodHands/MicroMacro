using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace CoreModule.Editor
{
    public class BuildSettingScenesUpdater : AssetPostprocessor
    {
        private static readonly string sceneDirLevel;
        private static readonly string sceneDirTitle;
        private static readonly string initialLoadScene;

        static BuildSettingScenesUpdater()
        {
            //プラットフォーム毎に異なるpathを設定
#if UNITY_EDITOR_WIN
            sceneDirLevel = @"Scenes\Level\Main";
            sceneDirTitle = @"Scenes\Level\OutGame";
            initialLoadScene = @"Scenes\Level\Main\Root.unity";
#elif UNITY_EDITOR_OSX
            sceneDirLevel = "Scenes/Level/Main";
            sceneDirTitle = "Scenes/Level/OutGame";
            initialLoadScene = "Scenes/Level/Main/Root.unity";
#endif
        }

        public static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths
        )
        {
            if (!CheckExists())
                return;

            var assets = importedAssets
                .Union(deletedAssets)
                .Union(movedAssets)
                .Union(movedFromAssetPaths);

            if (CheckInSceneDir(assets))
            {
                Create();
            }
        }

        // Sceneディレクトリ以下のアセットが編集されたか
        private static bool CheckInSceneDir(IEnumerable<string> assets)
        {
            return assets.Any(asset =>
            {
                string directoryName = Path.GetDirectoryName(asset);
                return directoryName == GetAssetsPath(sceneDirLevel);
            });
        }

        // 存在チェック
        private static bool CheckExists()
        {
            string sceneDirFullPath = GetFullPath(sceneDirLevel);
            string initialLoadSceneFullPath = GetFullPath(initialLoadScene);

            if (!Directory.Exists(sceneDirFullPath))
            {
                Debug.LogError("Not Found Dir :" + sceneDirFullPath);
                return false;
            }

            if (!File.Exists(initialLoadSceneFullPath))
            {
                Debug.LogError("Not Found Initial Load Scene : " + initialLoadSceneFullPath);
                return false;
            }

            return true;
        }

        private static void Create()
        {
            string initialLoadSceneAssetsPath = GetAssetsPath(initialLoadScene);
            string initialLoadSceneAssetsUnityPath = ToUnityPath(initialLoadSceneAssetsPath);

            var scenes = AssetDatabase.FindAssets("t:Scene", new string[] { GetAssetsPath(sceneDirLevel), GetAssetsPath(sceneDirTitle) })
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .OrderBy(path => path)
                .Where(path => path != initialLoadSceneAssetsUnityPath)
                .Select(path => new EditorBuildSettingsScene(path, true))
                .ToList();

            // 初回に呼び込まれて欲しいシーンを先頭に配置する
            scenes.Insert(0, new EditorBuildSettingsScene(initialLoadSceneAssetsPath, true));

            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();

            Debug.Log("Created BuildSettings.");
        }

        private static string GetFullPath(string path)
        {
#if UNITY_EDITOR_WIN
            return Application.dataPath + "\\" + path;
#elif UNITY_EDITOR_OSX
            return Application.dataPath + "/" + path;
#endif
        }

        private static string GetAssetsPath(string path)
        {
#if UNITY_EDITOR_WIN
            return "Assets\\" + path;
#elif UNITY_EDITOR_OSX
            return "Assets/" + path;
#endif
        }

        private static string ToUnityPath(string path)
        {
            
#if UNITY_EDITOR_WIN
            const char separator = '\\';
#elif UNITY_EDITOR_OSX
            const char separator = '/';
#endif

            string[] splitPath = path.Split(separator);
            StringBuilder result = new StringBuilder();
            foreach (string dir in splitPath)
            {
                result.Append(dir);
                result.Append('/');
            }
            
            result.Remove(result.Length - 1, 1);
            
            return result.ToString();
        }
    }
}