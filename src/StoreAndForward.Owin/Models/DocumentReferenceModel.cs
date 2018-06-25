using System;
using Newtonsoft.Json;

namespace DigitalHealth.StoreAndForward.Owin.Models
{
    /// <summary>
    /// Document reference model.
    /// </summary>
    public class DocumentReferenceModel
    {
        /// <summary>
        /// Document ID.
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// Document OID.
        /// </summary>
        [JsonProperty("document_id")]
        public string DocumentId { get; set; }

        /// <summary>
        /// Document replace ID.
        /// </summary>
        [JsonProperty("replace_id")]
        public string ReplaceId { get; set; }

        /// <summary>
        /// Document queue date time.
        /// </summary>
        [JsonProperty("queue_date_time")]
        public DateTime QueueDateTime { get; set; }

        /// <summary>
        /// Document status.
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; }

        /// <summary>
        /// Document link.
        /// </summary>
        [JsonProperty("document_link")]
        public string DocumentLink { get; set; }

        /// <summary>
        /// Events link.
        /// </summary>
        [JsonProperty("events_link")]
        public string EventsLink { get; set; }

        /// <summary>
        /// Format code.
        /// </summary>
        [JsonProperty("format_code")]
        public string FormatCode { get; set; }

        /// <summary>
        /// Format code name.
        /// </summary>
        [JsonProperty("format_code_name")]
        public string FormatCodeName { get; set; }

        /// <summary>
        /// IHI.
        /// </summary>
        [JsonProperty("ihi")]
        public string Ihi { get; set; }
    }
}