using System;
using DigitalHealth.StoreAndForward.Core.Data.Entities.Enums;

namespace DigitalHealth.StoreAndForward.Core.Data.Entities
{
    /// <summary>
    /// Event entity.
    /// </summary>
    public class EventEntity
    {
        /// <summary>
        /// Event ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Event details.
        /// </summary>
        public string Details { get; set; }

        /// <summary>
        /// Event date.
        /// </summary>
        public DateTime EventDate { get; set; }

        /// <summary>
        /// Event type.
        /// </summary>
        public EventType Type { get; set; }

        /// <summary>
        /// Document.
        /// </summary>
        public virtual DocumentEntity Document { get; set; }
    }
}