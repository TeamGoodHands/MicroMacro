//------------------------------------
//          Asset Validator
//     Copyright© 2025 OmniShade     
//------------------------------------

using System.Collections.Generic;

/**
 * AssetValidatorCache is a cache of AssetValidatorBase derived instances.
 **/
namespace AssetValidator {

    internal class AssetValidatorCache {
        public int Count => validators.Count;

        readonly List<AssetValidatorBase> validators;

        public AssetValidatorCache() {
            validators = new List<AssetValidatorBase>();

            // Get and add all asset validators, excluding override disabled ones.
            var pvs = ReflectionTools.GetAllDerivedInstancesOfType<AssetValidatorBase>();
            foreach (var assetValidator in pvs)
                validators.Add(assetValidator);
        }

        // Returns the AssetValidatorBase derived instance at position
        public AssetValidatorBase this[int index] {
            get { return validators[index]; }
        }
    }
}
