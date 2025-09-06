//------------------------------------
//          Asset Validator
//     Copyright© 2025 OmniShade     
//------------------------------------

using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

/**
 * Base SpriteAtlas validator, override this implementation as needed.
 **/
namespace AssetValidator {
    
    public class SpriteAtlasValidator : AssetValidatorBase {
        public SpriteAtlasValidator() {
            filter = "t:spriteatlas";
        }

        protected override bool Validate(Object obj) {
            var atlas = obj as SpriteAtlas;
            if (atlas == null)
                return true;

            var texSettings = SpriteAtlasExtensions.GetTextureSettings(atlas);
            if (!ObjectTools.HasLabel(obj, ReadableLabel) && texSettings.readable) {
                LogError(obj, "is marked Read/Write. Label as '" + ReadableLabel + "' if this is intended.");
                return false;
            }

            return true;
        }
    }
}
