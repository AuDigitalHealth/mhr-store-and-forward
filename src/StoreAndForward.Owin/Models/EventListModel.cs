using System.Collections.Generic;
using Newtonsoft.Json;

namespace DigitalHealth.StoreAndForward.Owin.Models
{
    /// <summary>
    /// Event list model.
    /// </summary>
    public class EventListModel
    {
        /// <summary>
        /// Events.
        /// </summary>
        [JsonProperty("events")]
        public IList<EventModel> Events { get; set; }
    }
}