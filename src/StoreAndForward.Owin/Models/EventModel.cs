using System;
using Newtonsoft.Json;

namespace DigitalHealth.StoreAndForward.Owin.Models
{
    /// <summary>
    /// Event model.
    /// </summary>
    public class EventModel
    {
        /// <summary>
        /// Event ID.
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// Event date time.
        /// </summary>
        [JsonProperty("event_date_time")]
        public DateTime EventDateTime { get; set; }

        /// <summary>
        /// Event details.
        /// </summary>
        [JsonProperty("details")]
        public string Details { get; set; }

        /// <summary>
        /// Event type.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Document link.
        /// </summary>
        [JsonProperty("document_link")]
        public string DocumentLink { get; set; }
    }
}