using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using DigitalHealth.StoreAndForward.Infrastructure.Mhr.Models;
using DigitalHealth.StoreAndForward.Owin.Config.Models;
using Nehta.VendorLibrary.PCEHR;
using Newtonsoft.Json;
using Serilog;

namespace DigitalHealth.StoreAndForward.Owin.Config
{
    /// <summary>
    /// JSON file configuration service.
    /// </summary>
    public class JsonConfigurationService : IConfigurationService
    {
        private readonly StoreAndForwardConfiguration _storeAndForwardConfiguration;

        private readonly X509Certificate2 _certificate;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="filePath"></param>
        public JsonConfigurationService(string filePath)
        {
            // Load the configuration using the path
            string configuration = File.ReadAllText(filePath);
            Log.Information("Store and forward configuration {configuration}", configuration);
            _storeAndForwardConfiguration = JsonConvert.DeserializeObject<StoreAndForwardConfiguration>(configuration);

            // Load the certificate from the local store using the thumbprint (Local / My)
            using (X509Store x509Store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
            {
                x509Store.Open(OpenFlags.ReadOnly);

                X509Certificate2Collection certificates = x509Store.Certificates.Find(X509FindType.FindByThumbprint,
                    _storeAndForwardConfiguration.certificate_thumbprint, true);

                _certificate = certificates[0];

                Log.Information("Loaded certificate with subject '{subject}'", _certificate.Subject);
            }

            // Upload metadata
            HealthcareFacility = (HealthcareFacilityTypeCodes)Enum.Parse(typeof(HealthcareFacilityTypeCodes), _storeAndForwardConfiguration.healthcare_facility);
            PracticeSetting = (PracticeSettingTypes)Enum.Parse(typeof(PracticeSettingTypes), _storeAndForwardConfiguration.practice_setting);
            ClientSystemType = (CommonPcehrHeaderClientSystemType)Enum.Parse(typeof(CommonPcehrHeaderClientSystemType), _storeAndForwardConfiguration.client_system_type);

            ProductInfo = new ProductInfo
            {
                Vendor = _storeAndForwardConfiguration.product_info.vendor,
                Version = _storeAndForwardConfiguration.product_info.version,
                Platform = _storeAndForwardConfiguration.product_info.platform,
                Name = _storeAndForwardConfiguration.product_info.name,
            };
        }

        /// <summary>
        /// Certificate used when connecting to the MHR.
        /// </summary>
        public X509Certificate2 Certificate => _certificate;

        /// <summary>
        /// MHR upload endpoint.
        /// </summary>
        public string UploadDocumentEndpoint => _storeAndForwardConfiguration.upload_document_endpoint;

        /// <summary>
        /// Upload retry limit.
        /// </summary>
        public int RetryLimit => _storeAndForwardConfiguration.retry_limit;

        /// <summary>
        /// Time in minutes between send attempts.
        /// </summary>
        public int SendIntervalInMinutes => _storeAndForwardConfiguration.send_interval_in_minutes;

        /// <summary>
        /// Product info for uploads.
        /// </summary>
        public ProductInfo ProductInfo { get; }

        /// <summary>
        /// Healthcare facility.
        /// </summary>
        public HealthcareFacilityTypeCodes HealthcareFacility { get; }

        /// <summary>
        /// Practice settings.
        /// </summary>
        public PracticeSettingTypes PracticeSetting { get; }

        /// <summary>
        /// Client system.
        /// </summary>
        public CommonPcehrHeaderClientSystemType ClientSystemType { get; }
    }
}