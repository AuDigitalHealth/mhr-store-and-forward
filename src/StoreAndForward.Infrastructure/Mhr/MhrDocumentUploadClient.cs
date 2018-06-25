using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using DigitalHealth.StoreAndForward.Core.Mhr;
using DigitalHealth.StoreAndForward.Core.Mhr.Models;
using DigitalHealth.StoreAndForward.Core.Mhr.Validators;
using DigitalHealth.StoreAndForward.Infrastructure.Mhr.Mappers;
using DigitalHealth.StoreAndForward.Infrastructure.Mhr.Models;
using DigitalHealth.StoreAndForward.Infrastructure.Mhr.Utils;
using FluentValidation;
using Nehta.VendorLibrary.PCEHR;
using Nehta.VendorLibrary.PCEHR.DocumentRepository;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;

namespace DigitalHealth.StoreAndForward.Infrastructure.Mhr
{
    /// <summary>
    /// Document uploader.
    /// </summary>
    public class MhrDocumentUploadClient : IMhrDocumentUploadClient
    {
        private readonly string _endpoint;
        private readonly X509Certificate2 _certificate;
        private readonly HealthcareFacilityTypeCodes _facilityType;
        private readonly PracticeSettingTypes _practiceSetting;
        private readonly CommonPcehrHeaderClientSystemType _clientSystem;
        private readonly ProductInfo _productInfo;

        private readonly DocumentUploadRequestValidator _documentUploadRequestValidator = new DocumentUploadRequestValidator();


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="certificate"></param>
        /// <param name="facilityType"></param>
        /// <param name="practiceSetting"></param>
        /// <param name="clientSystem"></param>
        /// <param name="productInfo"></param>
        public MhrDocumentUploadClient(string endpoint, X509Certificate2 certificate, HealthcareFacilityTypeCodes facilityType, 
            PracticeSettingTypes practiceSetting, CommonPcehrHeaderClientSystemType clientSystem, ProductInfo productInfo)
        {
            _endpoint = endpoint;
            _certificate = certificate;
            _facilityType = facilityType;
            _practiceSetting = practiceSetting;
            _clientSystem = clientSystem;
            _productInfo = productInfo;
        }

        /// <summary>
        /// Uploads a document.
        /// </summary>
        /// <param name="documentUploadRequest"></param>
        /// <returns></returns>
        public async Task<DocumentUploadResult> UploadDocument(DocumentUploadRequest documentUploadRequest)
        {
            // Validate the request
            await _documentUploadRequestValidator.ValidateAndThrowAsync(documentUploadRequest);

            Log.Debug("Uploading to endpoint '{endpoint}' with '{certificate}'", _endpoint, _certificate.Subject);

            // Create the client
            var uploadDocumentClient = new UploadDocumentClient(new Uri(_endpoint), _certificate, _certificate);

            DocumentUploadResult documentUploadResult;
            try
            {
                // Create the request
                ProvideAndRegisterDocumentSetRequestType provideAndRegisterDocumentSetRequest;
                if (!string.IsNullOrWhiteSpace(documentUploadRequest.ReplaceDocumentId))
                {
                    // Replace request
                    provideAndRegisterDocumentSetRequest = uploadDocumentClient.CreateRequestForReplacement(
                        documentUploadRequest.DocumentData,
                        documentUploadRequest.FormatCode,
                        documentUploadRequest.FormatCodeName,
                        _facilityType,
                        _practiceSetting,
                        documentUploadRequest.ReplaceDocumentId);
                }
                else
                {
                    // New document request
                    provideAndRegisterDocumentSetRequest = uploadDocumentClient.CreateRequestForNewDocument(
                        documentUploadRequest.DocumentData,
                        documentUploadRequest.FormatCode,
                        documentUploadRequest.FormatCodeName,
                        _facilityType,
                        _practiceSetting);
                }

                if (Log.IsEnabled(LogEventLevel.Verbose))
                {
                    Log.Verbose("Upload request '{requestJson}'", JsonConvert.SerializeObject(provideAndRegisterDocumentSetRequest));
                }

                // Create the PCEHR header
                CommonPcehrHeader commonPcehrHeader = CreateCommonPcehrHeader(provideAndRegisterDocumentSetRequest.SubmitObjectsRequest);

                if (Log.IsEnabled(LogEventLevel.Verbose))
                {
                    Log.Verbose("PCEHR header {header}", JsonConvert.SerializeObject(commonPcehrHeader));
                }

                // Upload the document
                RegistryResponseType registryResponse = uploadDocumentClient.UploadDocument(commonPcehrHeader, provideAndRegisterDocumentSetRequest);

                LogSoapMessages(uploadDocumentClient.SoapMessages);

                if (Log.IsEnabled(LogEventLevel.Verbose))
                {
                    Log.Verbose("Upload response {responseJson}", JsonConvert.SerializeObject(registryResponse));
                }

                // Map the response
                documentUploadResult = DocumentUploadResultMapper.Map(registryResponse);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error uploading document to endpoint '{endpoint}'", _endpoint);

                LogSoapMessages(uploadDocumentClient.SoapMessages);

                throw;
            }
            finally
            {
                uploadDocumentClient.Close();
            }

            return documentUploadResult;
        }

        private CommonPcehrHeader CreateCommonPcehrHeader(SubmitObjectsRequest submitObjectsRequest)
        {
            // Extract the header data from the XDS data
            PcehrHeaderData pcehrHeaderData = XdsUtils.ExtractPcehrHeaderData(submitObjectsRequest);

            // Create the common header using the data from the XDS metadata
            var commonPcehrHeader = new CommonPcehrHeader
            {
                IhiNumber = pcehrHeaderData.Ihi,
                UserId = pcehrHeaderData.ProviderId,
                UserIdType = pcehrHeaderData.IsProviderIdLocal ? CommonPcehrHeaderUserIDType.HPII : CommonPcehrHeaderUserIDType.LocalSystemIdentifier,
                UserName = pcehrHeaderData.ProviderName,
                OrganisationName = pcehrHeaderData.OrganisationName,
                OrganisationId = pcehrHeaderData.Hpio,
                ClientSystemType = _clientSystem,

                ProductPlatform = _productInfo.Platform,
                ProductName = _productInfo.Name,
                ProductVersion = _productInfo.Version,
                ProductVendor = _productInfo.Vendor
            };

            return commonPcehrHeader;
        }

        private static void LogSoapMessages(SoapMessages soapMessages)
        {
            if (Log.IsEnabled(LogEventLevel.Verbose))
            {                
                Log.Verbose("SOAP request: {soapRequest}", soapMessages.SoapRequest);
                Log.Verbose("SOAP response: {soapResponse}", soapMessages.SoapResponse);
                Log.Verbose("SOAP signature status {signatureStatus}", soapMessages.SoapResponseSignatureStatus.ToString());
            }
        }
    }
}
