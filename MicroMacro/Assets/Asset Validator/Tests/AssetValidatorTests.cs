//------------------------------------
//          Asset Validator
//     Copyright© 2025 OmniShade     
//------------------------------------

using NUnit.Framework;

/**
 * This test runs the Asset Validator, and can either be run as a unit test or
 * as part of a continuous integration process.
 **/
namespace AssetValidator {
    
    [TestFixture]
    public class AssetValidatorTests {
        [Test]
        public void Validate() {
            var result = CITools.RunValidation();
            Assert.True(result.IsSuccessful, result.Message);
        }
    }
}
