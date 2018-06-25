using Newtonsoft.Json;

namespace DigitalHealth.StoreAndForward.Owin.Models
{
    /// <summary>
    /// Timeline event model.
    /// </summary>
    public class TimelineEventModel : EventModel
    {
        /// <summary>
        /// Document ID.
        /// </summary>
        [JsonProperty("document_id")]
        public string DocumentId { get; set; }
    }
}