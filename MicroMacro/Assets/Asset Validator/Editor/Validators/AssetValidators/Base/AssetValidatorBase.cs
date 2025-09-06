//------------------------------------
//          Asset Validator
//     Copyright© 2025 OmniShade     
//------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Object = UnityEngine.Object;
using UnityEditor;

/**
 * AssetValidatorBase is fired once per project asset validation run (one search and validate).
 **/
namespace AssetValidator {

    public abstract class AssetValidatorBase {
        public const string SkipLabel = "SkipValidation";
        public const string ReadableLabel = "Readable";

        // Override this filter with the asset search type
        protected string filter = "t:prefab";

        // Invoked when this validator implementation has created a new ValidationLog instance.
        internal event Action<ValidationLog> LogCreated;

        protected readonly List<Object> objectsToValidate = new();

        string typeName;

        // The human-readable name of the type.
        public string TypeName {
            get {
                if (string.IsNullOrEmpty(typeName))
                    typeName = GetType().Name;
                return typeName;
            }
        }

        // Search the current Scene and add the information gathered to a cache so that
        // we can validate it in aggregate after the target Scenes have been searched.
        public void Search() {
            var assetsFolder = new string[] { "Assets" };
            var GUIDs = AssetDatabase.FindAssets(filter, assetsFolder);
            foreach (string GUID in GUIDs) {
                string path = AssetDatabase.GUIDToAssetPath(GUID);
                var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                var labels = AssetDatabase.GetLabels(obj);
                if (!labels.Contains(SkipLabel))
                    objectsToValidate.Add(obj);
            }
        }

        // After all scenes have been searched, validate using the aggregated results.
        public bool ProcessObjects() {
            bool isValid = true;
            foreach (var obj in objectsToValidate)
                isValid &= Validate(obj);
            return isValid;
        }

        protected abstract bool Validate(Object obj);

        // Dispatches a ValidationLog instance via LogCreated.
        protected void LogError(Object obj, string message) {
            string objName = obj == null ? string.Empty : "[" + obj.name + "] ";
            LogCreated?.Invoke(new ValidationLog {
                source = LogSource.Project,
                validatorName = TypeName,
                message = objName + message,
                objectPath = ObjectTools.GetObjectPath(obj)
            });
        }
    }
}
