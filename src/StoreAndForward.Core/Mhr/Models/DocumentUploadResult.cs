namespace DigitalHealth.StoreAndForward.Core.Mhr.Models
{
    /// <summary>
    /// Document upload result.
    /// </summary>
    public class DocumentUploadResult
    {
        /// <summary>
        /// Document upload result status.
        /// </summary>
        public DocumentUploadResultStatus Status { get; set; }

        /// <summary>
        /// Additional information.
        /// </summary>
        public string AdditionalInfo { get; set; }

        /// <summary>
        /// Error code.
        /// </summary>
        public string ErrorCode { get; set; }
    }
}