//------------------------------------
//          Asset Validator
//     Copyright© 2025 OmniShade     
//------------------------------------

using System;

/**
 * ValidatorManagerBase is used as the base class for any validator manager 
 * that is responsible for an area of validation.
 **/
namespace AssetValidator {

    internal abstract class ValidatorManagerBase {
        protected readonly LogCache logCache;
        protected int progress;

        protected ValidatorManagerBase(LogCache _logCache) {
            logCache = _logCache;
        }

        // Search for any validation targets.
        public abstract void Search();

        // Synchronously validate validation targets at once.
        public abstract void Validate();

        // Invoked when a validator has emitted a ValidationLog via an event, adds
        // it to the cache if there is one.
        protected virtual void OnLogCreated(ValidationLog validationLog) {
            logCache.OnLogCreated(validationLog);
        }
    }
}
