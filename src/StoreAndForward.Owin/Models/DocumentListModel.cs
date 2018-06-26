using System.Collections.Generic;
using Newtonsoft.Json;

namespace DigitalHealth.StoreAndForward.Owin.Models
{
    /// <summary>
    /// Document list model.
    /// </summary>
    public class DocumentListModel : PagedListModel
    {
        /// <summary>
        /// Document list.
        /// </summary>
        [JsonProperty("documents")]
        public IList<DocumentReferenceModel> Documents { get; set; }
    }
}