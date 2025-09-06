//------------------------------------
//          Asset Validator
//     Copyright© 2025 OmniShade     
//------------------------------------

using System.Collections.Generic;
using Object = UnityEngine.Object;

/**
 * InstanceValidatorManagerBase is the abstract base class for all validator 
 * managers that inspect object instances.
 **/
namespace AssetValidator {

    internal abstract class InstanceValidatorManagerBase : ValidatorManagerBase {
        protected readonly List<Object> objectsToValidate;
        protected readonly InstanceValidatorCache instanceValidatorCache;

        protected InstanceValidatorManagerBase(LogCache logCache)
            : base(logCache) {
            instanceValidatorCache = new InstanceValidatorCache();
            for (int i = 0; i < instanceValidatorCache.Count; i++)
                instanceValidatorCache[i].LogCreated += OnLogCreated;
            objectsToValidate = new List<Object>();
        }

        public override void Validate() {
            for (progress = 0; progress < objectsToValidate.Count; progress++) {
                for (int j = 0; j < instanceValidatorCache.Count; j++) {
                    if (instanceValidatorCache[j].AppliesTo(objectsToValidate[progress]))
                        instanceValidatorCache[j].Validate(objectsToValidate[progress]);
                }
            }
        }
    }
}
