namespace DigitalHealth.StoreAndForward.Infrastructure.Mhr.Models
{
    /// <summary>
    /// PCEHR header data.
    /// </summary>
    public class PcehrHeaderData
    {
        /// <summary>
        /// IHI.
        /// </summary>
        public string Ihi { get; set; }

        /// <summary>
        /// Provider ID.
        /// </summary>
        public string ProviderId { get; set; }

        /// <summary>
        /// Is provider ID local flag.
        /// </summary>
        public bool IsProviderIdLocal { get; set; }

        /// <summary>
        /// Provider name.
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// HPIO.
        /// </summary>
        public string Hpio { get; set; }

        /// <summary>
        /// Organisation name.
        /// </summary>
        public string OrganisationName { get; set; }
    }
}