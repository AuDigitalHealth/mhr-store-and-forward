using System.Collections.Generic;
using Newtonsoft.Json;

namespace DigitalHealth.StoreAndForward.Owin.Models
{
    /// <summary>
    /// Timeline model.
    /// </summary>
    public class TimelineModel : PagedListModel
    {
        /// <summary>
        /// Timeline events.
        /// </summary>
        [JsonProperty("timeline_events")]
        public IList<TimelineEventModel> TimelineEvents { get; set; }
    }
}