using System;
using System.Collections.Generic;
using DigitalHealth.StoreAndForward.Core.Data.Entities.Enums;

namespace DigitalHealth.StoreAndForward.Core.Data.Entities
{
    /// <summary>
    /// Document entity.
    /// </summary>
    public class DocumentEntity
    {
        /// <summary>
        /// Document ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Document OID
        /// </summary>
        public string DocumentId { get; set; }

        /// <summary>
        /// Document OID to replace
        /// </summary>
        public string DocumentIdToReplace { get; set; }

        /// <summary>
        /// CDA package data
        /// </summary>
        public byte[] DocumentData { get; set; }

        /// <summary>
        /// Format code
        /// </summary>
        public string FormatCode { get; set; }

        /// <summary>
        /// Format code name
        /// </summary>
        public string FormatCodeName { get; set; }

        /// <summary>
        /// IHI
        /// </summary>
        public string Ihi { get; set; }

        /// <summary>
        /// Date time the document was queued
        /// </summary>
        public DateTime QueueDate { get; set; }

        private DocumentStatus? _status;

        /// <summary>
        /// Document status
        /// </summary>
        public DocumentStatus? Status
        {
            get => _status;
            set
            {
                if (_status.HasValue && value.HasValue)
                {
                    ValidateDocumentStatus(_status.Value, value.Value);
                }
                _status = value;
            }
        }

        /// <summary>
        /// Document events
        /// </summary>
        public virtual IList<EventEntity> Events { get; set; } = new List<EventEntity>(); // Lazy-load

        /// <summary>
        /// Add an event
        /// </summary>
        /// <param name="eventEntity"></param>
        public void AddEvent(EventEntity eventEntity)
        {
            Events.Add(eventEntity);
        }

        /// <summary>
        /// Validates the state transition is valid.
        /// </summary>
        /// <param name="currentDocumentStatus">Current state.</param>
        /// <param name="newDocumentStatus">New state.</param>
        private static void ValidateDocumentStatus(DocumentStatus currentDocumentStatus, DocumentStatus newDocumentStatus)
        {
            switch (newDocumentStatus)
            {
                case DocumentStatus.Pending:
                    if (currentDocumentStatus != DocumentStatus.Sending)
                    {
                        ThrowDocumentStatusStateChangeError(currentDocumentStatus, newDocumentStatus);
                    }
                    break;
                case DocumentStatus.Removed:
                    if (currentDocumentStatus != DocumentStatus.Pending)
                    {
                        ThrowDocumentStatusStateChangeError(currentDocumentStatus, newDocumentStatus);
                    }
                    break;
                case DocumentStatus.Sending:
                    if (currentDocumentStatus != DocumentStatus.Pending)
                    {
                        ThrowDocumentStatusStateChangeError(currentDocumentStatus, newDocumentStatus);
                    }
                    break;
                case DocumentStatus.Sent:
                    if (currentDocumentStatus != DocumentStatus.Sending)
                    {
                        ThrowDocumentStatusStateChangeError(currentDocumentStatus, newDocumentStatus);
                    }
                    break;
                case DocumentStatus.RetryLimitReached:
                    if (currentDocumentStatus != DocumentStatus.Sending)
                    {
                        ThrowDocumentStatusStateChangeError(currentDocumentStatus, newDocumentStatus);
                    }
                    break;                    
            }
        }

        private static void ThrowDocumentStatusStateChangeError(DocumentStatus currentState, DocumentStatus newState)
        {
            throw new InvalidOperationException($"Invalid document status transition from '{currentState.ToString()}' to '{newState.ToString()}'");
        }
    }
}
