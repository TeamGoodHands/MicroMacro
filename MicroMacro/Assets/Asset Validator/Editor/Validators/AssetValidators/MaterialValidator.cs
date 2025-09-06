//------------------------------------
//          Asset Validator
//     Copyright© 2025 OmniShade     
//------------------------------------

using UnityEngine;

/**
 * Base Material validator, override this implementation as needed.
 **/
namespace AssetValidator {

    public class MaterialValidator : AssetValidatorBase {
        const string InvalidShaderName = "Hidden/InternalErrorShader";

        public MaterialValidator() {
            filter = "t:material";
        }

        protected override bool Validate(Object obj) {
            var mat = obj as Material;

            // Materials in models may show up here, skip them
            if (mat == null)
                return true;

            // Check valid shader
            if (mat.shader.name == InvalidShaderName) {
                LogError(obj, string.Format("has invalid shader."));
                return false;
            }

            return true;
        }
    }
}
