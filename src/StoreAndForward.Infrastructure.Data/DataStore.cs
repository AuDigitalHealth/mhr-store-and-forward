using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DigitalHealth.StoreAndForward.Core;
using DigitalHealth.StoreAndForward.Core.Data;
using DigitalHealth.StoreAndForward.Core.Data.Entities;
using DigitalHealth.StoreAndForward.Core.Data.Entities.Enums;
using LinqKit;

namespace DigitalHealth.StoreAndForward.Infrastructure.Data
{
    /// <summary>
    /// Data store.
    /// </summary>
    public class DataStore : IDataStore
    {
        private readonly StoreAndForwardDbContext _storeAndForwardDbContext;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="storeAndForwardDbContext"></param>
        public DataStore(StoreAndForwardDbContext storeAndForwardDbContext)
        {
            _storeAndForwardDbContext = storeAndForwardDbContext;
        }

        /// <summary>
        /// Gets a document with ID.
        /// </summary>
        /// <param name="id">ID of the document.</param>
        /// <returns>Document.</returns>
        public async Task<DocumentEntity> GetDocument(int id)
        {
            return await _storeAndForwardDbContext.Documents.FindAsync(id);
        }

        /// <summary>
        /// Gets a document with the document OID.
        /// </summary>
        /// <param name="documentOid"></param>
        /// <returns>Document.</returns>
        public async Task<DocumentEntity> GetDocument(string documentOid)
        {
            return await _storeAndForwardDbContext.Documents
                .Include(e => e.Events)
                .SingleOrDefaultAsync(d => d.DocumentId == documentOid);
        }

        /// <summary>
        /// Filters documents.
        /// </summary>
        /// <param name="documentStatusList">Document status list.</param>
        /// <param name="from">From date.</param>
        /// <param name="to">To date.</param>
        /// <param name="offset">Paging offset.</param>
        /// <param name="limit">Limit offset.</param>
        /// <returns>List of documents.</returns>
        public async Task<PagedList<DocumentEntity>> FilterDocuments(IList<DocumentStatus> documentStatusList, DateTime? from = null, 
            DateTime? to = null, int? offset = null, int? limit = null)
        {
            var filterPredicate = PredicateBuilder.New<DocumentEntity>(true);

            if (documentStatusList != null && documentStatusList.Any())
            {
                Expression<Func<DocumentEntity, bool>> documentStatusPredicate = PredicateBuilder.New<DocumentEntity>(false);
                foreach (var documentStatus in documentStatusList)
                {
                    documentStatusPredicate = documentStatusPredicate.Or(a => a.Status == documentStatus);
                }

                filterPredicate = filterPredicate.And(documentStatusPredicate);
            }

            if (from.HasValue)
            {
                filterPredicate = filterPredicate.And(d => DbFunctions.DiffDays(d.QueueDate, from.Value) <= 0);
            }

            if (to.HasValue)
            {
                filterPredicate = filterPredicate.And(d => DbFunctions.DiffDays(d.QueueDate, to.Value) >= 0);
            }

            if (!offset.HasValue)
            {
                offset = 0;
            }

            if (!limit.HasValue)
            {
                limit = 10;
            }

            int total = await _storeAndForwardDbContext.Documents.AsExpandable()
                .Where(filterPredicate).CountAsync();

            List<DocumentEntity> documentEntities = await _storeAndForwardDbContext.Documents.AsExpandable()
                .Where(filterPredicate)
                .OrderByDescending(d => d.QueueDate)
                .Skip(offset.Value)
                .Take(limit.Value)
                .Include(e => e.Events)
                .ToListAsync();

            return new PagedList<DocumentEntity>(documentEntities, total, offset.Value, limit.Value);
        }

        /// <summary>
        /// Adds a document.
        /// </summary>
        /// <param name="documentEntity"></param>
        /// <returns></returns>
        public async Task<DocumentEntity> AddDocument(DocumentEntity documentEntity)
        {
            _storeAndForwardDbContext.Documents.Add(documentEntity);

            await _storeAndForwardDbContext.SaveChangesAsync();

            return documentEntity;
        }

        /// <summary>
        /// Updates a document.
        /// </summary>
        /// <param name="documentEntity"></param>
        /// <returns></returns>
        public async Task UpdateDocument(DocumentEntity documentEntity)
        {
            await _storeAndForwardDbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Add document event.
        /// </summary>
        /// <param name="documentEntity"></param>
        /// <param name="type"></param>
        /// <param name="details"></param>
        /// <returns></returns>
        public async Task AddEvent(DocumentEntity documentEntity, EventType type, string details)
        {
            documentEntity.Events.Add(new EventEntity
            {
                Details = details,
                EventDate = DateTime.Now,
                Type = type
            });

            await _storeAndForwardDbContext.SaveChangesAsync();
        }
    
        /// <summary>
        /// Filters events.
        /// </summary>
        /// <param name="eventTypeList">Event type list.</param>
        /// <param name="from">From date.</param>
        /// <param name="to">To date.</param>
        /// <param name="offset">Paging offset.</param>
        /// <param name="limit">Paging limit.</param>
        /// <returns>List of events.</returns>
        public async Task<PagedList<EventEntity>> FilterEvents(IList<EventType> eventTypeList, DateTime? from = null, DateTime? to = null, int? offset = null, int? limit = null)
        {
            var filterPredicate = PredicateBuilder.New<EventEntity>(true);

            if (eventTypeList != null && eventTypeList.Any())
            {
                Expression<Func<EventEntity, bool>> eventTypePredicate = PredicateBuilder.New<EventEntity>(false);
                foreach (var eventType in eventTypeList)
                {
                    eventTypePredicate = eventTypePredicate.Or(a => a.Type == eventType);
                }

                filterPredicate = filterPredicate.And(eventTypePredicate);
            }

            if (from.HasValue)
            {
                filterPredicate = filterPredicate.And(d => DbFunctions.DiffDays(d.EventDate, from.Value) <= 0);
            }

            if (to.HasValue)
            {
                filterPredicate = filterPredicate.And(d => DbFunctions.DiffDays(d.EventDate, to.Value) >= 0);
            }

            if (!offset.HasValue)
            {
                offset = 0;
            }

            if (!limit.HasValue)
            {
                limit = 10;
            }

            int total = await _storeAndForwardDbContext.Events.AsExpandable()
                .Where(filterPredicate).CountAsync();

            List<EventEntity> eventEntities = await _storeAndForwardDbContext.Events.AsExpandable()
                .Where(filterPredicate)
                .OrderByDescending(x => x.EventDate)
                .Skip(offset.Value)
                .Take(limit.Value)
                .Include(e => e.Document)
                .ToListAsync();

            return new PagedList<EventEntity>(eventEntities, total, offset.Value, limit.Value);
        }


        /// <summary>
        /// Filter events for a given document.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IList<EventEntity>> GetDocumentEvents(int id)
        {
            return await _storeAndForwardDbContext.Events
                .Where(x => x.Document != null && x.Document.Id == id)
                .OrderByDescending(e => e.EventDate)
                .Include(e => e.Document)
                .ToListAsync();
        }
    }
}
