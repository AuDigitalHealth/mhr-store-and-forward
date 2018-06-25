using System.Threading.Tasks;
using DigitalHealth.StoreAndForward.Core.Mhr.Models;

namespace DigitalHealth.StoreAndForward.Core.Mhr
{
    /// <summary>
    /// MHR document upload client.
    /// </summary>
    public interface IMhrDocumentUploadClient
    {
        /// <summary>
        /// Uploads a document.
        /// </summary>
        /// <param name="documentUploadRequest"></param>
        /// <returns></returns>
        Task<DocumentUploadResult> UploadDocument(DocumentUploadRequest documentUploadRequest);
    }
}
