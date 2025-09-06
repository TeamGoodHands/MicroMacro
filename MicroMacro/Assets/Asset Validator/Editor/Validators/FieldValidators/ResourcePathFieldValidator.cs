//------------------------------------
//          Asset Validator
//     Copyright© 2025 OmniShade     
//------------------------------------

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/**
 * ResourcePathFieldValidator is a validator that helps to ensure fields decorated with ResourcePathAttribute
 * when calling their object.ToString method resolve to a resource path for a non-null asset.
 **/
namespace AssetValidator {
    
    [FieldValidator(typeof(ResourcePathAttribute))]
    public sealed class ResourcePathFieldValidator : FieldValidatorBase {
        const string ResourcesFolder = "Resources";

        // Cache of previously looked-up assets as an optimization
        static readonly Dictionary<string, bool> m_isInResourcesCache = new();

        public override bool ValidateItem(object obj) {
            if (ObjectTools.IsNullReference(obj))
                return false;

            string objName = obj.ToString();
            if (m_isInResourcesCache.ContainsKey(objName))
                return true;

            // Attempt load as object
            var go = obj as GameObject;
            if (go != null) {
                string path = AssetDatabase.GetAssetPath(go);
                if (!string.IsNullOrEmpty(path) && path.Contains(ResourcesFolder)) {
                    m_isInResourcesCache[objName] = true;
                    return true;
                }
            }

            // Attempt load as string
            var resourceObj = Resources.Load(objName);
            if (resourceObj != null) {
                m_isInResourcesCache[objName] = true;
                return true;
            }

            return false;
        }

        protected override void LogFieldError(Object obj, string fieldName) {
            LogError(obj, "has field [" + fieldName + "] which must be in a " + ResourcesFolder + " folder.");
        }  
    }
}
