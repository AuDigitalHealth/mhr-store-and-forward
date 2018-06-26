using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DigitalHealth.StoreAndForward.Core.Data.Entities;
using DigitalHealth.StoreAndForward.Core.Data.Entities.Enums;

namespace DigitalHealth.StoreAndForward.Core.Data
{
    /// <summary>
    /// Interface for accessing data.
    /// </summary>
    public interface IDataStore
    {
        /// <summary>
        /// Gets a document using the ID.
        /// </summary>
        /// <param name="id">Document ID.</param>
        /// <returns>Document.</returns>
        Task<DocumentEntity> GetDocument(int id);

        /// <summary>
        /// Gets the document using the OID.
        /// </summary>
        /// <param name="documentOid">Document OID.</param>
        /// <returns>Document.</returns>
        Task<DocumentEntity> GetDocument(string documentOid);

        /// <summary>
        /// Adds a document.
        /// </summary>
        /// <param name="documentEntity">Document to add.</param>
        /// <returns>Add document.</returns>
        Task<DocumentEntity> AddDocument(DocumentEntity documentEntity);

        /// <summary>
        /// Updates a document.
        /// </summary>
        /// <param name="documentEntity">Document to update.</param>
        /// <returns></returns>
        Task UpdateDocument(DocumentEntity documentEntity);

        /// <summary>
        /// Gets all the events for a document.
        /// </summary>
        /// <param name="id">Document ID.</param>
        /// <returns>List of events.</returns>
        Task<IList<EventEntity>> GetDocumentEvents(int id);

        /// <summary>
        /// Adds an event to the document.
        /// </summary>
        /// <param name="documentEntity">Document to add the event to.</param>
        /// <param name="type">Type of the event.</param>
        /// <param name="details">Details of the event.</param>
        /// <returns></returns>
        Task AddEvent(DocumentEntity documentEntity, EventType type, string details);    

        /// <summary>
        /// Filters documents.
        /// </summary>
        /// <param name="documentStatusList">List of document status.</param>
        /// <param name="from">From date.</param>
        /// <param name="to">To date.</param>
        /// <param name="offset">Paging offset.</param>
        /// <param name="limit">Paging limit.</param>
        /// <returns></returns>
        Task<PagedList<DocumentEntity>> FilterDocuments(IList<DocumentStatus> documentStatusList, DateTime? from = null, DateTime? to = null, int? offset = null, int? limit = null);

        /// <summary>
        /// Filters events.
        /// </summary>
        /// <param name="eventTypeList"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        Task<PagedList<EventEntity>> FilterEvents(IList<EventType> eventTypeList = null, DateTime? from = null, DateTime? to = null, int? offset = null, int? limit = null);        
    }
}