
namespace DigitalHealth.StoreAndForward.Owin.Config.Models
{
    /// <summary>
    /// Store and Forward configuration.
    /// </summary>
    public class StoreAndForwardConfiguration
    {
        /// <summary>
        /// Retry limit.
        /// </summary>
        public int retry_limit { get; set; }

        /// <summary>
        /// Certificate thumbprint.
        /// </summary>
        public string certificate_thumbprint { get; set; }

        /// <summary>
        /// Upload endpoint.
        /// </summary>
        public string upload_document_endpoint { get; set; }

        /// <summary>
        /// Healthcare facility.
        /// </summary>
        public string healthcare_facility { get; set; }

        /// <summary>
        /// Practice setting.
        /// </summary>
        public string practice_setting { get; set; }

        /// <summary>
        /// Client system type.
        /// </summary>
        public string client_system_type { get; set; }
        
        /// <summary>
        /// Send interval in minutes.
        /// </summary>
        public int send_interval_in_minutes { get; set; }

        /// <summary>
        /// Product info.
        /// </summary>
        public ProductInfoConfiguration product_info { get; set; }
    }
}