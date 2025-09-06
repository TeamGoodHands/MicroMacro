//------------------------------------
//          Asset Validator
//     Copyright© 2025 OmniShade     
//------------------------------------

using UnityEditor;
using UnityEngine;

/**
 * Base Model validator class, override this implementation as needed.
 **/
namespace AssetValidator {

    public class ModelValidator : AssetValidatorBase {
        public ModelValidator() {
            filter = "t:model";
        }

        protected override bool Validate(Object obj) {
            string path = AssetDatabase.GetAssetPath(obj);
            var mi = AssetImporter.GetAtPath(path) as ModelImporter;
            if (mi == null)
                return true;

            // Check Read/Write
            if (!ObjectTools.HasLabel(obj, ReadableLabel) && mi.isReadable) {
                LogError(obj, "is marked Read/Write. Label as '" + ReadableLabel + "' if this is intended.");
                return false;
            }

            return true;
        }
    }
}
