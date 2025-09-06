//------------------------------------
//          Asset Validator
//     Copyright© 2025 OmniShade     
//------------------------------------

using UnityEngine.SceneManagement;

/**
 * ValidationLog represents a single validation log instance. It can be associated with a
 * specific validator, object and/or scene, and area of the Unity project.
 **/
namespace AssetValidator {

    public sealed class ValidationLog {
        public LogSource Source => source;        
        internal LogSource source;
        internal string validatorName;
        internal string message;
        internal string scenePath;
        internal string objectPath;

        internal ValidationLog() { }

        // Returns a human-readable description of the LogSource.
        public string GetSourceDescription() {
            return source switch {
                LogSource.Scene => scenePath,
                LogSource.Project => "Project",
                _ => "None",
            };
        }

        // Returns true or false if an object path is present.
        public bool HasObjectPath() {
            return !string.IsNullOrEmpty(objectPath);
        }

        // Can the object relating to this log be pinged in the scene or Project Assets view?
        public bool CanPingObject() {
            return HasObjectPath() &&
                   (source == LogSource.Scene && SceneManager.GetActiveScene().path == scenePath ||
                    source == LogSource.Project);
        }
    }
}
