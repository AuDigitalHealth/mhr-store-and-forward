namespace DigitalHealth.StoreAndForward.Core.Queue.Models
{
    /// <summary>
    /// Queue document data.
    /// </summary>
    public class QueueDocumentData
    {
        /// <summary>
        /// CDA package.
        /// </summary>
        public byte[] CdaPackage { get; set; }

        /// <summary>
        /// Format code.
        /// </summary>
        public string FormatCode { get; set; }

        /// <summary>
        /// Format code name.
        /// </summary>
        public string FormatCodeName { get; set; }

        /// <summary>
        /// Document ID to replace.
        /// </summary>
        public string DocumentIdToReplace { get; set; }
    }
}