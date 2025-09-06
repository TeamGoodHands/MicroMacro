//------------------------------------
//          Asset Validator
//     Copyright© 2025 OmniShade     
//------------------------------------

using UnityEngine;

/**
 * Extension methods for Transform.
 **/
namespace AssetValidator {
    
    internal static class TransformExtensions {
        const string GetPathFormat = "{0}/{1}";

        // GetPath returns the object's absolute path in a scene.
        public static string GetPath(this Transform current) {
            return current.parent == null ? current.name
                : string.Format(GetPathFormat, current.parent.GetPath(), current.name);
        }
    }
}
