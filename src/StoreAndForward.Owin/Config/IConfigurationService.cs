using System.Security.Cryptography.X509Certificates;
using DigitalHealth.StoreAndForward.Infrastructure.Mhr.Models;
using Nehta.VendorLibrary.PCEHR;

namespace DigitalHealth.StoreAndForward.Owin.Config
{
    /// <summary>
    /// Configuration service. 
    /// </summary>
    public interface IConfigurationService
    {
        /// <summary>
        /// Certificate for MHR requests.
        /// </summary>
        X509Certificate2 Certificate { get; }

        /// <summary>
        /// Upload document endpoint.
        /// </summary>
        string UploadDocumentEndpoint { get; }

        /// <summary>
        /// Retry limit.
        /// </summary>
        int RetryLimit { get; }

        /// <summary>
        /// Send interval in minutes.
        /// </summary>
        int SendIntervalInMinutes { get; }

        /// <summary>
        /// Product info for uploads.
        /// </summary>
        ProductInfo ProductInfo { get; }

        /// <summary>
        /// Healthcare facility for uploads.
        /// </summary>
        HealthcareFacilityTypeCodes HealthcareFacility { get; }

        /// <summary>
        /// Practice setting for uploads.
        /// </summary>
        PracticeSettingTypes PracticeSetting { get; }

        /// <summary>
        /// Client system type for uploads.
        /// </summary>
        CommonPcehrHeaderClientSystemType ClientSystemType { get; }
    }
}