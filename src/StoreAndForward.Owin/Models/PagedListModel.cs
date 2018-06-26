using Newtonsoft.Json;

namespace DigitalHealth.StoreAndForward.Owin.Models
{
    /// <summary>
    /// Paged list model.
    /// </summary>
    public class PagedListModel
    {
        /// <summary>
        /// Total.
        /// </summary>
        [JsonProperty("total")]
        public int Total { get; set; }

        /// <summary>
        /// Limit.
        /// </summary>
        [JsonProperty("limit")]
        public int Limit { get; set; }

        /// <summary>
        /// Offset.
        /// </summary>
        [JsonProperty("offset")]
        public int Offset { get; set; }

    }
}