namespace DigitalHealth.StoreAndForward.Owin.Config.Models
{
    /// <summary>
    /// Product information configuration item. Used when uploading to the MHR.
    /// </summary>
    public class ProductInfoConfiguration
    {
        /// <summary>
        /// Platform.
        /// </summary>
        public string platform { get; set; }

        /// <summary>
        /// Name.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// Version.
        /// </summary>
        public string version { get; set; }

        /// <summary>
        /// Vendor.
        /// </summary>
        public string vendor { get; set; }
    }
}