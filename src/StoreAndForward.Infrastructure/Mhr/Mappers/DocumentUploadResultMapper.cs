using System.Linq;
using DigitalHealth.StoreAndForward.Core.Mhr.Models;
using Nehta.VendorLibrary.PCEHR.DocumentRepository;

namespace DigitalHealth.StoreAndForward.Infrastructure.Mhr.Mappers
{
    /// <summary>
    /// Maps the result from the PCEHR call.
    /// </summary>
    public static class DocumentUploadResultMapper
    {
        /// <summary>
        /// Failure status.
        /// </summary>
        private const string ResponseStatusFailure = "urn:oasis:names:tc:ebxml-regrep:ResponseStatusType:Failure";

        /// <summary>
        /// Success status.
        /// </summary>
        private const string ResponseStatusSuccess = "urn:oasis:names:tc:ebxml-regrep:ResponseStatusType:Success";


        /// <summary>
        /// Maps the response.
        /// </summary>
        /// <param name="registryResponse"></param>
        /// <returns></returns>
        public static DocumentUploadResult Map(RegistryResponseType registryResponse)
        {
            string message = null;
            string errorCode = null;
            DocumentUploadResultStatus documentUploadResultStatus = DocumentUploadResultStatus.Failed;
            if (registryResponse.status == ResponseStatusFailure)
            {
                documentUploadResultStatus = DocumentUploadResultStatus.Failed;
                RegistryError registryError = registryResponse.RegistryErrorList?.RegistryError?.SingleOrDefault();
                if (registryError != null)
                {
                    message = registryError.codeContext;
                    errorCode = registryError.errorCode;
                }
            }
            else if (registryResponse.status == ResponseStatusSuccess)
            {
                documentUploadResultStatus = DocumentUploadResultStatus.Success;
            }

            return new DocumentUploadResult
            {
                Status = documentUploadResultStatus,
                ErrorCode = errorCode,
                AdditionalInfo = message
            };
        }
    }
}
