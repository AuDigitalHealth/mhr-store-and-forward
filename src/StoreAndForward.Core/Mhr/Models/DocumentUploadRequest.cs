namespace DigitalHealth.StoreAndForward.Core.Mhr.Models
{
    /// <summary>
    /// Document upload request.
    /// </summary>
    public class DocumentUploadRequest
    {
        /// <summary>
        /// Document package data.
        /// </summary>
        public byte[] DocumentData { get; set; }

        /// <summary>
        /// Replace document ID.
        /// </summary>
        public string ReplaceDocumentId { get; set; }
        
        /// <summary>
        /// Format code.
        /// </summary>
        public string FormatCode { get; set; }

        /// <summary>
        /// Format code name.
        /// </summary>
        public string FormatCodeName { get; set; }
    }
}