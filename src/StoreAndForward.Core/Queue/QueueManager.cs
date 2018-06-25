using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DigitalHealth.StoreAndForward.Core.Data;
using DigitalHealth.StoreAndForward.Core.Data.Entities;
using DigitalHealth.StoreAndForward.Core.Data.Entities.Enums;
using DigitalHealth.StoreAndForward.Core.Mhr;
using DigitalHealth.StoreAndForward.Core.Mhr.Models;
using DigitalHealth.StoreAndForward.Core.Notification;
using DigitalHealth.StoreAndForward.Core.Notification.Models;
using DigitalHealth.StoreAndForward.Core.Package;
using DigitalHealth.StoreAndForward.Core.Package.Models;
using DigitalHealth.StoreAndForward.Core.Queue.Models;
using DigitalHealth.StoreAndForward.Core.Queue.Validators;
using FluentValidation;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;

namespace DigitalHealth.StoreAndForward.Core.Queue
{
    /// <summary>
    /// Queue manager
    /// </summary>
    public class QueueManager : IQueueManager
    {
        private readonly IDataStore _dataStore;
        private readonly IMhrDocumentUploadClient _mhrDocumentUploadClient;
        private readonly INotificationService _notificationService;
        private readonly ICdaPackageService _cdaPackageService;

        private readonly int _retryLimit;

        private readonly QueueDocumentDataValidator _queueDocumentDataValidator = new QueueDocumentDataValidator();


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataStore"></param>
        /// <param name="mhrDocumentUploadClient"></param>
        /// <param name="notificationService"></param>
        /// <param name="cdaPackageService"></param>
        /// <param name="retryLimit"></param>
        public QueueManager(IDataStore dataStore, IMhrDocumentUploadClient mhrDocumentUploadClient, INotificationService notificationService, 
            ICdaPackageService cdaPackageService, int retryLimit = 3)
        {
            _dataStore = dataStore;
            _mhrDocumentUploadClient = mhrDocumentUploadClient;
            _notificationService = notificationService;
            _cdaPackageService = cdaPackageService;
            _retryLimit = retryLimit;
        }
        
        /// <summary>
        /// Get a document.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DocumentEntity> GetDocument(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException($"Invalid document ID '{id}'", nameof(id));
            }

            return await _dataStore.GetDocument(id);
        }

        /// <summary>
        /// View all events.
        /// </summary>
        /// <param name="eventTypeList"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async Task<IList<EventEntity>> GetQueueActivityTimeline(IList<EventType> eventTypeList, DateTime? from = null, DateTime? to = null, int? offset = null, int? limit = null)
        {
            if (offset.HasValue && offset < 0)
            {
                throw new ArgumentException($"'offset' is '{offset}' and must be greater or equal to 0", nameof(offset));
            }

            if (limit.HasValue && limit <= 0)
            {
                throw new ArgumentException($"'limit' is '{limit}' and must be greater than 0", nameof(limit));
            }

            if (from.HasValue && to.HasValue && from > to)
            {
                throw new ArgumentException($"'from' date '{from}' must be larger than 'to' date '{to}'");
            }

            return await _dataStore.FilterEvents(eventTypeList, from, to, offset, limit);
        }

        /// <summary>
        /// View document event history.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IList<EventEntity>> GetDocumentEventHistory(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException($"Invalid document ID '{id}'", nameof(id));
            }

            return await _dataStore.GetDocumentEvents(id);
        }

        /// <summary>
        /// Delete a document from the queue. The document to be deleted must be in the 'Pending' state.
        /// </summary>
        /// <param name="id">Document ID</param>
        /// <returns></returns>
        public async Task DeleteDocumentFromQueue(int id)
        {
            DocumentEntity documentToDelete = await _dataStore.GetDocument(id);
            if (documentToDelete == null)
            {
                throw new ArgumentException($"Document with ID '{id}' does not exist");
            }

            if (documentToDelete.Status != DocumentStatus.Pending)
            {
                throw new InvalidOperationException($"Document with ID '{documentToDelete.DocumentId}' must be in the '{DocumentStatus.Pending.ToString()}' state to be deleted but is in the '{documentToDelete.Status.ToString()}' state");
            }

            documentToDelete.Status = DocumentStatus.Removed;

            // Add the event
            documentToDelete.AddEvent(new EventEntity
            {
                Type = EventType.Removed,
                EventDate = DateTime.Now
            });

            await _dataStore.UpdateDocument(documentToDelete);
        }

        /// <summary>
        /// Get document list.
        /// </summary>
        /// <param name="documentStatusList"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async Task<IList<DocumentEntity>> GetDocumentQueueList(IList<DocumentStatus> documentStatusList, DateTime? from = null, DateTime? to = null,
            int? offset = null, int? limit = null)
        {
            if (offset.HasValue && offset < 0)
            {
                throw new ArgumentException($"'offset' is '{offset}' and must be greater or equal to 0", nameof(offset));
            }

            if (limit.HasValue && limit <= 0)
            {
                throw new ArgumentException($"'limit' is '{limit}' and must be greater than 0", nameof(limit));
            }

            if (from.HasValue && to.HasValue && from > to)
            {
                throw new ArgumentException($"'from' date '{from}' must be larger than 'to' date '{to}'");
            }

            IList<DocumentEntity> documentEntities = await _dataStore.FilterDocuments(documentStatusList, from, to, offset, limit);

            return documentEntities;
        }


        /// <summary>
        /// Adds a document to the queue.
        /// </summary>
        /// <param name="queueDocumentData"></param>
        /// <returns></returns>
        public async Task<DocumentEntity> AddDocumentToQueue(QueueDocumentData queueDocumentData)
        {            
            // Validate the document
            await _queueDocumentDataValidator.ValidateAndThrowAsync(queueDocumentData);

            if (Log.IsEnabled(LogEventLevel.Verbose))
            {
                Log.Information("Queue document data {@queueDocumentData}", JsonConvert.SerializeObject(queueDocumentData));
            }

            // Get the metadata from the package
            CdaPackageData cdaPackageData = _cdaPackageService.ExtractPackageData(queueDocumentData.CdaPackage);

            // Check if the document already exists with the same ID
            DocumentEntity documentEntity = await _dataStore.GetDocument(cdaPackageData.DocumentId);
            if (documentEntity != null)
            {
                throw new ArgumentException($"Document with ID '{cdaPackageData.DocumentId}' already exists in the queue with the '{documentEntity.Status.ToString()}' state");
            }

            // Validate the replace 
            if (!string.IsNullOrEmpty(queueDocumentData.DocumentIdToReplace))
            {
                // Check if the document to replace exists
                var documentToReplace = await _dataStore.GetDocument(queueDocumentData.DocumentIdToReplace);
                if (documentToReplace == null)
                {
                    throw new ArgumentException($"Document to replace with ID '{queueDocumentData.DocumentIdToReplace}' does not exist in the queue");
                }

                // Check the replace document status
                if (documentToReplace.Status == DocumentStatus.RetryLimitReached || documentToReplace.Status == DocumentStatus.Removed)
                {
                    throw new ArgumentException($"Document to replace cannot have a status of '{DocumentStatus.RetryLimitReached.ToString()}' or '{DocumentStatus.Removed.ToString()}' and is '{documentToReplace.Status.ToString()}'");
                }  
            }

            DateTime addedDateTime = DateTime.Now;

            // Create the document
            DocumentEntity documentToQueue = new DocumentEntity
            {
                QueueDate = addedDateTime,
                Status = DocumentStatus.Pending,
                DocumentId = cdaPackageData.DocumentId,
                DocumentIdToReplace = queueDocumentData.DocumentIdToReplace,
                FormatCodeName = queueDocumentData.FormatCodeName,
                FormatCode = queueDocumentData.FormatCode,
                DocumentData = queueDocumentData.CdaPackage,
                Ihi = cdaPackageData.Ihi
            };

            // Add the create event
            documentToQueue.AddEvent(new EventEntity
            {
                Type = EventType.Created,
                EventDate = addedDateTime
            });

            await _dataStore.AddDocument(documentToQueue);

            return documentToQueue;
        }
    
        /// <summary>
        /// Sends all documents.
        /// </summary>
        /// <returns></returns>
        public async Task SendDocumentsInQueue()
        {
            var notificationItems = new List<NotificationData>();

            // Get all the 'Pending' documents
            var pendingDocuments = await _dataStore.FilterDocuments(new[] { DocumentStatus.Pending });

            // Set documents statuses to 'Sending'
            foreach (var document in pendingDocuments)
            {
                document.Status = DocumentStatus.Sending;
                await _dataStore.UpdateDocument(document);
            }

            Log.Information("Found '{documentsToSend}' document(s) to send", pendingDocuments.Count);

            // Process each document
            foreach (var document in pendingDocuments)
            {
                EventType? newEventType = null;

                string newEventMessage = null;
                DocumentStatus? newDocumentStatus = null;

                bool skipDocument = false;

                // Check if current document supercedes a document that hasn't been sent yet then set status back to 'Pending' and do not process document
                if (!string.IsNullOrEmpty(document.DocumentIdToReplace))
                {
                    var replaceDocument = await _dataStore.GetDocument(document.DocumentIdToReplace);

                    if (replaceDocument != null && replaceDocument.Status != DocumentStatus.Sent)
                    {
                        newEventType = EventType.Deferred;
                        newEventMessage = $"Document to supercede ({replaceDocument.DocumentId}) hasn't been sent";

                        newDocumentStatus = DocumentStatus.Pending;

                        skipDocument = true;
                    }
                }

                if (!skipDocument)
                {
                    var uploadRequest = new DocumentUploadRequest
                    {
                        DocumentData = document.DocumentData,
                        FormatCode = document.FormatCode,
                        FormatCodeName = document.FormatCodeName
                    };

                    // Upload document
                    bool wasSendSuccessful;
                    try
                    {
                        DocumentUploadResult documentUploadResult = await _mhrDocumentUploadClient.UploadDocument(uploadRequest);

                        Log.Information("Document with ID '{documentId}' upload status is '{uploadStatus}'", document.DocumentId, documentUploadResult.Status);

                        wasSendSuccessful = documentUploadResult.Status == DocumentUploadResultStatus.Success;
                        if (!wasSendSuccessful)
                        {
                            // Determine the message
                            newEventMessage = documentUploadResult.AdditionalInfo;
                            if (!string.IsNullOrEmpty(documentUploadResult.ErrorCode))
                            {
                                newEventMessage = $"Code: {documentUploadResult.ErrorCode} - {newEventMessage}";
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "Error uploading document");

                        wasSendSuccessful = false;
                        newEventMessage = e.Message;
                    }

                    // Determine the result
                    if (wasSendSuccessful)
                    {
                        newEventType = EventType.Success;

                        newDocumentStatus = DocumentStatus.Sent;
                    }
                    else
                    {
                        newEventType = EventType.Failed;

                        // Check if the retry limit has been reached
                        if (document.Events.Count(e => e.Type == EventType.Failed) + 1 >= _retryLimit)
                        {
                            Log.Information("Retry limit reached for document with ID '{documentId}'", document.DocumentId);

                            newDocumentStatus = DocumentStatus.RetryLimitReached;
                        }
                        else
                        {
                            // Change back to pending
                            newDocumentStatus = DocumentStatus.Pending;
                        }
                    }                    
                }
               
                // Add an event
                await _dataStore.AddEvent(document, newEventType.Value, newEventMessage);

                document.Status = newDocumentStatus.Value;

                // Update the document
                await _dataStore.UpdateDocument(document);

                // Add a notification
                notificationItems.Add(new NotificationData
                {
                    DocumentEvent = newEventType.Value,
                    DocumentId = document.DocumentId
                });
            }

            // Send any notifications
            if (notificationItems.Any())
            {
                await _notificationService.SendNotification(notificationItems);
            }
        }
    }
}
