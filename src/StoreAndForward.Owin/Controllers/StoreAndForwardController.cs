using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using DigitalHealth.StoreAndForward.Core.Data.Entities;
using DigitalHealth.StoreAndForward.Core.Data.Entities.Enums;
using DigitalHealth.StoreAndForward.Core.Queue;
using DigitalHealth.StoreAndForward.Core.Queue.Models;
using DigitalHealth.StoreAndForward.Owin.Models;
using FluentValidation;
using Ionic.Zip;
using Nehta.VendorLibrary.CDAPackage;
using Swashbuckle.Swagger.Annotations;

namespace DigitalHealth.StoreAndForward.Owin.Controllers
{
    /// <summary>
    /// Store and forward controller.
    /// </summary>
    [RoutePrefix("api")]
    public class StoreAndForwardController : ApiController
    {
        private readonly IQueueManager _queueManager;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="queueManager"></param>
        public StoreAndForwardController(IQueueManager queueManager)
        {
            _queueManager = queueManager;
        }

        /// <summary>
        /// Get all documents.
        /// </summary>
        /// <param name="start_date">Filter start date</param>
        /// <param name="end_date">Filter end date</param>
        /// <param name="document_status">Comma separated list of document status</param>
        /// <param name="offset">Page offset</param>
        /// <param name="limit">Page limit</param>
        /// <returns></returns>
        [HttpGet, Route("documents")]
        [SwaggerResponse(HttpStatusCode.OK, "Document list", typeof(DocumentListModel))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Invalid parameters")]
        [ResponseType(typeof(DocumentListModel))]
        public async Task<IHttpActionResult> GetDocuments(DateTime? start_date = null, DateTime? end_date = null, string document_status = null, int? offset = null, int? limit = null)
        {
            IList<DocumentStatus> documentStatusList = new List<DocumentStatus>();

            if (!string.IsNullOrWhiteSpace(document_status))
            {
                string[] documentStatusItems = document_status.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string documentStatusItem in documentStatusItems)
                {
                    if (Enum.TryParse(documentStatusItem, out DocumentStatus tempDocumentStatus))
                    {
                        documentStatusList.Add(tempDocumentStatus);
                    }
                    else
                    {
                        return BadRequest($"Document status '{documentStatusItem}' is invalid and must be from {GetValidEnumValues<DocumentStatus>()}");
                    }
                }                
            }

            if (offset.HasValue && offset < 0)
            {
                return BadRequest($"'offset' is '{offset}' and must be greater or equal to 0");
            }

            if (limit.HasValue && limit <= 0)
            {
                return BadRequest($"'limit' is '{limit}' and must be greater than 0");
            }

            if (start_date.HasValue && end_date.HasValue && start_date.Value > end_date.Value)
            {
                return BadRequest($"'start_date' date '{start_date}' must be larger than 'end_date' date '{end_date}'");
            }

            IList<DocumentEntity> documents;
            try
            {
                documents = await _queueManager.GetDocumentQueueList(documentStatusList, start_date, end_date, offset, limit);
            }
            catch (ArgumentException e)
            {
                return BadRequest(e.Message);
            }
            
            return Ok(MapDocumentsToDocumentList(documents));
        }

        /// <summary>
        /// Enqueue a document.
        /// </summary>
        /// <param name="document">Document to enqueue</param>
        /// <returns></returns>
        [HttpPost, Route("documents")]
        [SwaggerResponse(HttpStatusCode.Created, "Document enqueued successfully", typeof(DocumentModel))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Invalid document")]
        [ResponseType(typeof(DocumentModel))]
        public async Task<IHttpActionResult> AddDocument([FromBody] DocumentModel document)
        {
            if (document == null)
            {
                return BadRequest("Document body required");
            }

            if (document.Data == null)
            {
                return BadRequest("Document data is missing");
            }

            if (string.IsNullOrWhiteSpace(document.FormatCodeName))
            {
                return BadRequest("Format code name is null or empty");
            }

            if (string.IsNullOrWhiteSpace(document.FormatCode))
            {
                return BadRequest("Format code is null or empty");
            }

            if (document.ReplaceId != null && document.ReplaceId == string.Empty)
            {
                return BadRequest("Replace ID cannot be empty");
            }

            var queueDocumentData = new QueueDocumentData
            {
                CdaPackage = document.Data,
                DocumentIdToReplace = document.ReplaceId,
                FormatCode = document.FormatCode,
                FormatCodeName = document.FormatCodeName
            };

            DocumentEntity documentEntity;
            try
            {
                documentEntity = await _queueManager.AddDocumentToQueue(queueDocumentData);
            }
            catch (ArgumentException e) // Thrown when the document to replace is invalid
            {
                return BadRequest(e.Message);
            }
            catch (ValidationException ve) // Thrown when the request is invalid
            {
                List<string> errors = ve.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(string.Join(" ", errors));
            }
            catch (SignatureVerificationException sve) // Thrown when the package contents are invalid
            {
                return BadRequest(sve.Message);
            }
            catch (ZipException ze) // Thrown when the zip file is invalid
            {
                return BadRequest(ze.Message);
            }

            DocumentModel createdDocument = MapDocumentEntityToDocument(documentEntity);

            return Created(new Uri(createdDocument.DocumentLink), createdDocument);
        }
       
        /// <summary>
        /// Gets a document.
        /// </summary>
        /// <param name="id">Document ID</param>
        /// <returns></returns>
        [HttpGet, Route("documents/{id:int}")]
        [SwaggerResponse(HttpStatusCode.OK, "Document", typeof(DocumentModel))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Invalid parameters")]
        [SwaggerResponse(HttpStatusCode.NotFound, "ID does not exist")]
        [ResponseType(typeof(DocumentModel))]
        public async Task<IHttpActionResult> GetDocument(int id)
        {
            if (id <= 0)
            {
                return BadRequest($"Invalid documentId '{id}'");
            }

            DocumentEntity documentEntity = await _queueManager.GetDocument(id);
            if (documentEntity == null)
            {
                return NotFound();
            }

            return Ok(MapDocumentEntityToDocument(documentEntity));           
        }

        /// <summary>
        /// Delete a document.
        /// </summary>
        /// <param name="id">Document ID</param>
        /// <returns></returns>
        [HttpDelete, Route("documents/{id:int}")]
        [SwaggerResponse(HttpStatusCode.NoContent)]
        [SwaggerResponse(HttpStatusCode.NotFound, "ID does not exist")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "ID is not valid")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> DeleteDocument(int id)
        {
            if (id <= 0)
            {
                return BadRequest($"Invalid document ID '{id}'");
            }

            try
            {
                await _queueManager.DeleteDocumentFromQueue(id);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Get all document events.
        /// </summary>
        /// <param name="id">Document ID</param>
        /// <returns></returns>
        [HttpGet, Route("documents/{id:int}/events")]
        [SwaggerResponse(HttpStatusCode.OK, "Event list", typeof(EventListModel))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Invalid parameters")]
        [ResponseType(typeof(EventListModel))]
        public async Task<IHttpActionResult> GetDocumentEvents(int id)
        {
            if (id <= 0)
            {
                return BadRequest($"Invalid document ID '{id}'");
            }

            IList<EventEntity> events = await _queueManager.GetDocumentEventHistory(id);

            return Ok(MapEventsToEventList(events));
        }

        /// <summary>
        /// Get all events.
        /// </summary>
        /// <param name="start_date">Filter start date</param>
        /// <param name="end_date">Filter end date</param>
        /// <param name="event_type">Event type</param>
        /// <param name="offset">Page offset</param>
        /// <param name="limit">Page limit</param>
        /// <returns></returns>
        [HttpGet, Route("events")]
        [SwaggerResponse(HttpStatusCode.OK, "Event list", typeof(TimelineModel))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Invalid parameters")]
        [ResponseType(typeof(TimelineModel))]
        public async Task<IHttpActionResult> GetEvents( DateTime? start_date = null, DateTime? end_date = null, string event_type = null, int? offset = null, int? limit = null)
        {
            IList<EventType> eventTypeList = new List<EventType>();

            if (!string.IsNullOrWhiteSpace(event_type))
            {
                string[] documentStatusItems = event_type.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string eventTypeItem in documentStatusItems)
                {
                    if (Enum.TryParse(eventTypeItem, out EventType tempEventType))
                    {
                        eventTypeList.Add(tempEventType);
                    }
                    else
                    {
                        return BadRequest($"Event type '{eventTypeItem}' is invalid and must be from {GetValidEnumValues<EventType>()}");
                    }
                }
            }

            if (offset.HasValue && offset < 0)
            {
                return BadRequest($"'offset' is '{offset}' and must be greater or equal to 0");
            }

            if (limit.HasValue && limit <= 0)
            {
                return BadRequest($"'limit' is '{limit}' and must be greater than 0");
            }

            if (start_date.HasValue && end_date.HasValue && start_date.Value > end_date.Value)
            {
                return BadRequest($"'start_date' date '{start_date}' must be larger than 'end_date' date '{end_date}'");
            }

            IList<EventEntity> events;
            try
            {
                events = await _queueManager.GetQueueActivityTimeline(eventTypeList, start_date, end_date, offset, limit);
            }
            catch (ArgumentException e)
            {
                return BadRequest(e.Message);
            }

            return Ok(MapEventsToTimeline(events));
        }

        private DocumentListModel MapDocumentsToDocumentList(IList<DocumentEntity> documentEntities)
        {
            return new DocumentListModel
            {
                Documents = documentEntities.Select(MapDocumentEntityToDocumentReference).ToList()
            };
        }

        private TimelineModel MapEventsToTimeline(IList<EventEntity> eventEntities)
        {
            return new TimelineModel
            {
                TimelineEvents = eventEntities.Select(MapEventToTimelineEvent).ToList()
            };
        }

        private TimelineEventModel MapEventToTimelineEvent(EventEntity eventEntity)
        {
            return new TimelineEventModel
            {
                Details = eventEntity.Details,
                DocumentLink = $"{GetDocumentsBaseUrl(Request)}/{eventEntity.Document.Id}",
                EventDateTime = eventEntity.EventDate,
                Id = eventEntity.Id,
                Type = eventEntity.Type.ToString(),
                DocumentId = eventEntity.Document.DocumentId
            };
        }

        private EventListModel MapEventsToEventList(IList<EventEntity> eventEntities)
        {           
            return new EventListModel
            {
                Events = eventEntities.Select(MapEventEntityToEvent).ToList()
            };
        }

        private DocumentReferenceModel MapDocumentEntityToDocumentReference(DocumentEntity documentEntity)
        {
            return new DocumentReferenceModel
            {
                Status = documentEntity.Status.ToString(),
                DocumentLink = $"{GetDocumentsBaseUrl(Request)}/{documentEntity.Id}",
                EventsLink = $"{GetDocumentsBaseUrl(Request)}/{documentEntity.Id}/events",
                Id = documentEntity.Id,
                QueueDateTime = documentEntity.QueueDate,
                ReplaceId = documentEntity.DocumentIdToReplace,
                DocumentId = documentEntity.DocumentId,
                FormatCodeName = documentEntity.FormatCodeName,
                FormatCode = documentEntity.FormatCode,
                Ihi = documentEntity.Ihi
            };
        }

        private DocumentModel MapDocumentEntityToDocument(DocumentEntity documentEntity)
        {
            return new DocumentModel
            {
                Data = documentEntity.DocumentData,
                Id = documentEntity.Id,
                DocumentId = documentEntity.DocumentId,
                ReplaceId = documentEntity.DocumentIdToReplace,
                FormatCode = documentEntity.FormatCode,
                FormatCodeName = documentEntity.FormatCodeName,
                Ihi = documentEntity.Ihi,
                EventsLink = $"{GetDocumentsBaseUrl(Request)}/{documentEntity.Id}/events",
                DocumentLink = $"{GetDocumentsBaseUrl(Request)}/{documentEntity.Id}",
                QueueDateTime = documentEntity.QueueDate,
                Status = documentEntity.Status.ToString()
            };
        }

        private EventModel MapEventEntityToEvent(EventEntity eventEntity)
        {
            return new EventModel
            {
                Details = eventEntity.Details,
                DocumentLink = $"{GetDocumentsBaseUrl(Request)}/{eventEntity.Document.Id}",
                EventDateTime = eventEntity.EventDate,
                Id = eventEntity.Id,
                Type = eventEntity.Type.ToString()
            };
        }

        private static string GetBaseUrl(HttpRequestMessage request)
        {
            string serverPath = request.RequestUri.GetLeftPart(UriPartial.Authority);
            string virtualPathRoot = request.GetRequestContext().VirtualPathRoot;

            return $"{serverPath}{virtualPathRoot}";
        }

        private static string GetDocumentsBaseUrl(HttpRequestMessage requestMessage)
        {
            return $"{GetBaseUrl(requestMessage)}/api/documents";
        }

        private static string GetValidEnumValues<T>()
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("Type must be an enum");
            }

            return string.Join(",", Enum.GetNames(typeof(T)).Select(e => $"'{e}'"));
        }
    }
}