//------------------------------------
//          Asset Validator
//     Copyright© 2025 OmniShade     
//------------------------------------

using System;
using System.Text;

/**
 * Helper methods for running the AssetValidator as part of a continuous integration setup either
 * from Unit Tests or as part of a build job.
 **/
namespace AssetValidator {

    public static class CITools {
        // Represents the result of a validation run.
        public class Result {
            public bool IsSuccessful { get; internal set; }
            public string Message { get; internal set; }
        }

        // Runs validation against the project and writes the log file to a file with a custom name.
        public static Result RunValidation() {
            try {
                var logCache = new LogCache();
                var assetValidationRunner = new AssetValidatorRunner(logCache);

                assetValidationRunner.Run();

                var result = new Result {
                    IsSuccessful = logCache.Count == 0
                };
                var errorMessages = new StringBuilder();
                foreach (var log in logCache) {
                    errorMessages.Append("Validator:\t" + log.validatorName + "\n");
                    if (log.Source != LogSource.Project) {
                        string scenePath = ProjectTools.StripAssetsPrefix(log.GetSourceDescription());
                        errorMessages.Append("Scene:\t\t" + scenePath + "\n");
                    }
                    if (log.HasObjectPath()) {
                        string objPath = ProjectTools.StripAssetsPrefix(log.objectPath);
                        errorMessages.Append("Object:\t\t" + objPath + "\n");
                    }
                    errorMessages.Append("Message:\t" + log.message + "\n\n");
                }
                result.Message = result.IsSuccessful ? "All assets passed validation." : errorMessages.ToString();
                return result;
            }
            catch (Exception ex) {
                return new Result {
                    IsSuccessful = false,
                    Message = string.Format("AssetValidation error: {0}", ex)
                };
            }
        }
    }
}
