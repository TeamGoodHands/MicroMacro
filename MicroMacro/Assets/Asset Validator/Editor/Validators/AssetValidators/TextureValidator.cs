//------------------------------------
//          Asset Validator
//     Copyright© 2025 OmniShade     
//------------------------------------

using UnityEngine;
using UnityEditor;

/**
 * Base Texture validator, override this implementation as needed.
 **/
namespace AssetValidator {

    public class TextureValidator : AssetValidatorBase {
        // Override these values as needed in the constructor of derived implementations.
        protected int CompressedMaxSize = 2048;
        protected int UncompressedMaxSize = 1024;
        protected int MinDimensionChecked = 512;

        const string LargeTextureLabel = "LargeTexture";

        public TextureValidator() {
            filter = "t:texture";
        }

        protected override bool Validate(Object obj) {
            var tex = obj as Texture;
            if (tex == null)
                return true;
            string path = AssetDatabase.GetAssetPath(tex);
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null)
                return true;

            // Skip render textures
            if (tex is RenderTexture)
                return true;

            // Check Read/Write
            bool isValid = true;
            if (!ObjectTools.HasLabel(obj, ReadableLabel) && ti.isReadable) {
                LogError(obj, "should not be marked Read/Write.");
                isValid = false;
            }

            // Check large textures
            if (!ObjectTools.HasLabel(obj, LargeTextureLabel) && !CheckLargeTextures(ti, tex))
                isValid = false;

            return isValid;
        }

        protected bool CheckLargeTextures(TextureImporter ti, Texture tex) {
            if (!IsCompressed(ti, tex) && IsTextureLargerThan(tex, UncompressedMaxSize)) {
                LogError(tex, "(" + tex.width + " x " + tex.height + 
                    ") is not compressed or its dimensions are not multiples of 4, and larger than " + UncompressedMaxSize + ". Label as '" + LargeTextureLabel + "' if this is intended.");
                return false;
            }

            if (IsTextureLargerThan(tex, CompressedMaxSize)) {
                LogError(tex, "(" + tex.width + " x " + tex.height + ") is larger than " + 
                    CompressedMaxSize + ".  Use label '" + LargeTextureLabel + "' if this is intended.");
                return false;
            }

            return true;
        }

        bool IsTextureLargerThan(Texture tex, int maxSize) {
            return (tex.height > maxSize && tex.width >= MinDimensionChecked) ||
                (tex.width > maxSize && tex.height >= MinDimensionChecked);
        }

        bool IsCompressed(TextureImporter ti, Texture tex) {
            return ti.textureCompression != TextureImporterCompression.Uncompressed &&
                tex.width % 4 == 0 && tex.height % 4 == 0;
        }
    }
}
