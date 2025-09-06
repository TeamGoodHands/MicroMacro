//------------------------------------
//          Asset Validator
//     Copyright© 2025 OmniShade     
//------------------------------------

using System.Linq;
using UnityEditor;
using UnityEngine;

/**
 * ProjectAssetValidatorManager is a validator manager that inspects all relevant Object instances (prefabs) 
 * in the Assets folder as well as executing validation using all AssetValidatorBase derived classes
 **/
namespace AssetValidator {

    internal sealed class ProjectAssetValidatorManager : InstanceValidatorManagerBase {
        const int ProcessCountPerFrame = 25;

        readonly string[] allPrefabGUIDs;
        int continueProgress;
        int projectProgress;
        int projectValidationProgress;
        readonly AssetValidatorCache assetValidatorCache;

        public ProjectAssetValidatorManager(LogCache logCache)
            : base(logCache) {
            var assetsFolder = new string[] { "Assets" };
            continueProgress = 0;
            allPrefabGUIDs = AssetDatabase.FindAssets("t:prefab t:scriptableobject", assetsFolder);
            assetValidatorCache = new AssetValidatorCache();
            for (int i = 0; i < assetValidatorCache.Count; i++)
                assetValidatorCache[i].LogCreated += OnLogCreated;
        }

        public override void Search() {
            // Get all prefab locations for instance validators
            for (int i = 0; i < allPrefabGUIDs.Length; i++)
                AddPrefabToValidate(i);

            // Map all project asset validators and Search
            for (int i = 0; i < assetValidatorCache.Count; i++)
                assetValidatorCache[i].Search();
        }

        public void ContinueSearch() {
            objectsToValidate.Clear();

            var nextStep = continueProgress + ProcessCountPerFrame >= allPrefabGUIDs.Length
                ? allPrefabGUIDs.Length : continueProgress + ProcessCountPerFrame;
            for (; continueProgress < nextStep; continueProgress++)
                AddPrefabToValidate(continueProgress);

            // Iterate one at a time through all project asset validators and validate
            nextStep = projectProgress + 1 >= assetValidatorCache.Count
                ? assetValidatorCache.Count : projectProgress + 1;
            for (; projectProgress < nextStep; projectProgress++)
                assetValidatorCache[projectProgress].Search();
        }

        void AddPrefabToValidate(int index) {
            string assetPath = AssetDatabase.GUIDToAssetPath(allPrefabGUIDs[index]);
            var prefabObject = AssetDatabase.LoadMainAssetAtPath(assetPath);
            var labels = AssetDatabase.GetLabels(prefabObject);
            if (labels.Contains(AssetValidatorBase.SkipLabel))
                return;

            objectsToValidate.Add(prefabObject);
            if (prefabObject.GetType() == typeof(GameObject)) {
                var prefabGO = prefabObject as GameObject;
                var components = prefabGO.GetComponentsInChildren<MonoBehaviour>(true);
                foreach (var comp in components) {
                    if (comp != null)
                        objectsToValidate.Add(comp);
                }
            }
        }

        public override void Validate() {
            base.Validate();
            for (int i = 0; i < assetValidatorCache.Count; i++)
                assetValidatorCache[i].ProcessObjects();
        }

        public void ContinueValidate() {
            base.Validate();

            // Iterate one at a time through all project asset validators and validate
            int nextStep = projectValidationProgress + 1 >= assetValidatorCache.Count
                ? assetValidatorCache.Count
                : projectValidationProgress + 1;

            for (; projectValidationProgress < nextStep; projectValidationProgress++)
                assetValidatorCache[projectValidationProgress].ProcessObjects();
        }

        public float GetProgress() {
            return (continueProgress + projectValidationProgress) /
                ((float)allPrefabGUIDs.Length + assetValidatorCache.Count + 1);
        }

        public bool IsComplete() {
            return continueProgress >= allPrefabGUIDs.Length &&
                projectProgress >= assetValidatorCache.Count &&
                progress >= objectsToValidate.Count;
        }

        protected override void OnLogCreated(ValidationLog validationLog) {
            validationLog.source = LogSource.Project;
            base.OnLogCreated(validationLog);
        }
    }
}
