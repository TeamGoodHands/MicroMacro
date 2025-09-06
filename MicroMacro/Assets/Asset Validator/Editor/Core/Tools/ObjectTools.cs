//------------------------------------
//          Asset Validator
//     Copyright© 2025 OmniShade     
//------------------------------------

using System.Linq;
using UnityEditor;
using UnityEngine;

/**
 * Helper methods for dealing with Object instances.
 **/
namespace AssetValidator {

    public static class ObjectTools {
        public static string GetObjectPath(Object obj) {
            // if this is an asset in the project, return the relative asset path.
            if (AssetDatabase.GetAssetPath(obj) != string.Empty)
                return AssetDatabase.GetAssetPath(obj);

            // Otherwise if this is a Component or GameObject in a scene, return
            // the hierarchical scene path.
            var objComponent = obj as Component;
            if (objComponent != null)
                return objComponent.transform.GetPath();

            var objGameObject = obj as GameObject;
            if (objGameObject != null)
                return objGameObject.transform.GetPath();

            return string.Empty;
        }

        // Returns true if the Object is null, otherwise returns false.
        public static bool IsNullReference(object obj) {
            return obj == null || (obj is Object && (obj as Object).ToString() == "null");
        }

        // Returns true if the Object represents a non-empty string
        public static bool IsNonEmptyString(object obj) {
            return obj != null && obj.GetType() == typeof(string) && !string.IsNullOrEmpty(obj.ToString());
        }

        // Returns true if the Object is tagged with label
        public static bool HasLabel(Object obj, string label) {
            var labels = AssetDatabase.GetLabels(obj);
            return labels.Contains(label);
        }
    }
}
