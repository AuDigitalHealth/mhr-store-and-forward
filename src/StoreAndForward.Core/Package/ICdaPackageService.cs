using DigitalHealth.StoreAndForward.Core.Package.Models;

namespace DigitalHealth.StoreAndForward.Core.Package
{
    /// <summary>
    /// CDA package service.
    /// </summary>
    public interface ICdaPackageService
    {
        /// <summary>
        /// Extract metadata from the package.
        /// </summary>
        /// <param name="cdaPackage">CDA package.</param>
        /// <returns>Package metadata.</returns>
        CdaPackageData ExtractPackageData(byte[] cdaPackage);
    }
}