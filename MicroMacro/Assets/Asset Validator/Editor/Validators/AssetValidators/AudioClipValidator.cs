//------------------------------------
//          Asset Validator
//     Copyright© 2025 OmniShade     
//------------------------------------

using UnityEngine;

/**
 * Base AudioClip validator, override this implementation as needed.
 **/
namespace AssetValidator {
    
    public class AudioClipValidator : AssetValidatorBase {
        public AudioClipValidator() {
            filter = "t:audioclip";
        }

        protected override bool Validate(Object obj) {
            var audio = obj as AudioClip;
            return true;
        }
    }
}
