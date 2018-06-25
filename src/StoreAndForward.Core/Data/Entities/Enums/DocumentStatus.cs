namespace DigitalHealth.StoreAndForward.Core.Data.Entities.Enums
{
    /// <summary>
    /// Document status.
    /// </summary>
    public enum DocumentStatus
    {
        /// <summary>
        /// Pending.
        /// </summary>
        Pending = 1,

        /// <summary>
        /// Sending.
        /// </summary>
        Sending = 2,

        /// <summary>
        /// Sent.
        /// </summary>
        Sent = 3 ,

        /// <summary>
        /// Retry limit reached.
        /// </summary>
        RetryLimitReached = 4,

        /// <summary>
        /// Removed.
        /// </summary>
        Removed = 5
    }
}
