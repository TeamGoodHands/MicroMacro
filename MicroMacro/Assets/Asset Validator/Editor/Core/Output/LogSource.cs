//------------------------------------
//          Asset Validator
//     Copyright© 2025 OmniShade     
//------------------------------------

using System;

/**
 * LogSource indicates the general location of the UnityEngine.Object instance, 
 * if any, the log is associated with.
 **/
namespace AssetValidator {
    
    [Serializable]
    public enum LogSource {
        Scene,
        Project
    }
}
