using Newtonsoft.Json;

namespace DigitalHealth.StoreAndForward.Owin.Models
{
    /// <summary>
    /// Document model.
    /// </summary>
    public class DocumentModel : DocumentReferenceModel
    {
        /// <summary>
        /// Document data.
        /// </summary>
        [JsonProperty("data")]
        public byte[] Data { get; set; }
    }
}