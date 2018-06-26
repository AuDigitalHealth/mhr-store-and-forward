using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DigitalHealth.StoreAndForward.Core.Data.Entities;
using DigitalHealth.StoreAndForward.Core.Data.Entities.Enums;
using DigitalHealth.StoreAndForward.Core.Queue.Models;

namespace DigitalHealth.StoreAndForward.Core.Queue
{
    /// <summary>
    /// Queue manager.
    /// </summary>
    public interface IQueueManager
    {
        /// <summary>
        /// Adds a document to the queue.
        /// </summary>
        /// <param name="queueDocumentData">Document data.</param>
        /// <returns>Created document.</returns>
        Task<DocumentEntity> AddDocumentToQueue(QueueDocumentData queueDocumentData);

        /// <summary>
        /// Gets a document from the queue.
        /// </summary>
        /// <param name="id">ID of the document.</param>
        /// <returns>Document with ID.</returns>
        Task<DocumentEntity> GetDocument(int id);

        /// <summary>
        /// Deletes a document from the queue.
        /// </summary>
        /// <param name="id">ID of the document.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">ID does not exist</exception>
        /// <exception cref="InvalidOperationException">Document is not in the 'Pending' state</exception>
        Task DeleteDocumentFromQueue(int id);

        /// <summary>
        /// Gets all the events for a document.
        /// </summary>
        /// <param name="id">ID of the document.</param>
        /// <returns>List of document events.</returns>
        Task<IList<EventEntity>> GetDocumentEventHistory(int id);

        /// <summary>
        /// Gets a list of document matching the filter.
        /// </summary>
        /// <param name="statusList">List of document status.</param>
        /// <param name="from">From date.</param>
        /// <param name="to">To date.</param>
        /// <param name="offset">Paging offset.</param>
        /// <param name="limit">Paging limit.</param>
        /// <returns>Paged list of documents matching the filter.</returns>
        Task<PagedList<DocumentEntity>> GetDocumentQueueList(IList<DocumentStatus> statusList, DateTime? from = null, DateTime? to = null, int? offset = null, int? limit = null);

        /// <summary>
        /// Gets all the queue events matching the filter.
        /// </summary>
        /// <param name="eventTypeList">List of event types.</param>
        /// <param name="from">From date.</param>
        /// <param name="to">To date.</param>
        /// <param name="offset">Paging offset.</param>
        /// <param name="limit">Paging limit.</param>
        /// <returns>Paged list of event entities.</returns>
        Task<PagedList<EventEntity>> GetQueueActivityTimeline(IList<EventType> eventTypeList, DateTime? from = null, DateTime? to = null, int? offset = null, int? limit = null);

        /// <summary>
        /// Sends all documents in the queue. Note this method is not thread safe.
        /// </summary>
        /// <returns></returns>
        Task SendDocumentsInQueue();
    }
}
