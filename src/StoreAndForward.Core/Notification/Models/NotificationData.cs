using DigitalHealth.StoreAndForward.Core.Data.Entities.Enums;

namespace DigitalHealth.StoreAndForward.Core.Notification.Models
{
    /// <summary>
    /// Notification data.
    /// </summary>
    public class NotificationData
    {
        /// <summary>
        /// Document ID.
        /// </summary>
        public string DocumentId { get; set; }

        /// <summary>
        /// Document event type.
        /// </summary>
        public EventType DocumentEvent { get; set; }
    }
}