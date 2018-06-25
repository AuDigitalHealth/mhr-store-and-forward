using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using DigitalHealth.StoreAndForward.Client;
using DigitalHealth.StoreAndForward.Client.Models;
using DigitalHealth.StoreAndForward.Service;
using Serilog;

namespace DigitalHealth.StoreAndForward.Sample.Console
{
    /// <summary>
    /// -------------------------------------------------------
    ///     Prerequisites
    /// -------------------------------------------------------
    /// 
    /// 1.  The StoreAndForwardService can only be used in projects targeting .NET Framework 4.6.2 and above.
    /// 
    /// 2.  Microsoft Sql Server (full version or express).
    /// 
    /// -------------------------------------------------------
    ///     Getting started with the StoreAndForwardService
    /// -------------------------------------------------------
    /// 
    /// 1.  Add a reference to the 'StoreAndFowardService' project.
    /// 
    /// 2.  Add a reference to the 'StoreAndForwardService.Client' project.
    /// 
    /// 3.  Install 'Microsoft.Owin.SelfHost' using NuGet. This allows the StoreAndForwardService to be self hosted without  
    ///     needing a web server such as IIS.
    /// 
    /// 4.  Install 'EntityFramework' using NuGet. This provides database support.
    /// 
    /// 5.  Add database connection strings in application configuration (app.config or web.config). The databases will automatically be
    ///     created as specified in your connection strings.
    ///     - StoreAndForwardDb: Database used for storing StoreAndForward related data (documents for uploads, information on retries, etc).
    ///     - Hangfire: System database used for Hangfire job scheduler.
    /// 
    ///     <connectionStrings>
    ///         <add name = "StoreAndForwardDb" connectionString="Data Source=[YourDatabaseServer];Initial Catalog=[StoreAndForwardDatabase];Persist Security Info=True;user id=[User Id];password=[Password];" providerName="System.Data.SqlClient" />
    ///         <add name = "Hangfire" connectionString="Data Source=[YourDatabaseServer];Initial Catalog=[HangfireDatabase];Persist Security Info=True;user id=[User Id];password=[Password];" providerName="System.Data.SqlClient" />
    ///     </connectionStrings>
    /// 
    /// 6.  Create the "config.json" in the application directory with the values customized to be relevant to your application.
    ///     This file holds the configuration settings for the Store and Forward Service. The description for each settings is noted
    ///     in the comments.
    /// 
    ///     {
    ///         /* Number of times to retry sending  */
    ///         "retry_limit": 3,
    /// 
    ///         /* Interval in minutes between retries */
    ///         "send_interval_in_minutes": 10,
    /// 
    ///         /* Thumbprint of certificate for connecting to the My Health Record */
    ///         "certificate_thumbprint": "thumbprint for certificate",
    /// 
    ///         /* My Health Record uploadDocument endpoint */
    ///         "upload_document_endpoint": "https://b2b.ehealthvendortest.health.gov.au/uploadDocument",
    /// 
    ///         /* Healthcare facility */
    ///         "healthcare_facility": "AgedCareResidentialServices",
    /// 
    ///         /* Practice setting */
    ///         "practice_setting": "AcupunctureService",
    /// 
    ///         /* Client system type */
    ///         "client_system_type": "CIS",
    /// 
    ///         /* MyHealthRecord header */
    ///         "product_info": {
    ///             "platform": "Windows 7",
    ///             "name": "Test Harness",
    ///             "version": "1.0",
    ///             "vendor": "ADHA"
    ///         }
    ///     }
    /// </summary> 
    public class Sample
    {
        public static async Task Main(string[] args)
        {
            // OPTIONAL - This is to set up logging to the console using Serilog. For this to work, install the following from NuGet:
            // 1. Serilog.Settings.AppSettings
            // 2. Serilog.Sinks.Console
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .ReadFrom.AppSettings()
                .CreateLogger();

            // Creates and starts the Store and Forward service.
            // Note: The Store and Forward Service should be hosted in a long-running application, such as a Windows Service.
            var safService = new StoreAndForwardOwinService("http://*:5000/storeandforward");
            safService.Start();
            System.Console.WriteLine("StoreAndForwardService started.");
            
            // Creates the Store and Forward client, which connects to the service hosted at the same location.
            Client.StoreAndForward storeAndForwardClient = new Client.StoreAndForward(new StoreandForwardAPI(new Uri("http://localhost:5000/storeandforward/")));

            // Add a document to the queue, using the client.
            // Note: The sample document used in this example will fail the upload.
            await AddDocumentToQueue(storeAndForwardClient);

            // These are examples of how to invoke the other operations on the client.
            // await DeleteDocumentInQueue(storeAndForwardClient);
            // await GetDocumentsInQueue(storeAndForwardClient);
            // await GetDocumentInQueue(storeAndForwardClient);
            // await GetEventsForDocument(storeAndForwardClient);
            // await GetEvents(storeAndForwardClient);

            System.Console.WriteLine("Press ENTER to end this sample program...");
            System.Console.ReadLine();

            // Stops the Store and Forward Service.
            safService.Stop();
        }

        public static async Task AddDocumentToQueue(Client.StoreAndForward client)
        {
            DocumentModel document = new DocumentModel();

            // Set required supporting data for the document upload
            document.Data = File.ReadAllBytes("TestData\\SampleDischargeSummary.zip");
            document.FormatCodeName = "Discharge Summary 3A HPII";
            document.FormatCode = "1.2.36.1.2001.1006.1.20000.26";
            document.ReplaceId = null; // Document Id of the document to be superceded

            // Add document to queue
            var httpResponse = await client.AddDocumentWithHttpMessagesAsync(document);

            if (httpResponse.Response.StatusCode == HttpStatusCode.Created)
            {
                // Call has been successful.
            }
            else
            {
                // Call has failed - handle errors here.
            }
        }

        public static async Task GetDocumentsInQueue(Client.StoreAndForward client)
        {
            // Get documents in queue
            // Arguments can be passed in to limit documents added during a certain period, or with certain statuses.
            var httpResponse = await client.GetDocumentsWithHttpMessagesAsync(
                DateTime.Now.AddDays(-1),   // Documents uploaded after this time
                DateTime.Now,               // Documents uploaded before this time
                "Sent",                     // Document status (Pending, Sending, Sent, RetryLimitReached, Removed)
                0,                          // Offset (number of records to skip)
                10);                        // Limit (number of records to return)

            if (httpResponse.Response.StatusCode == HttpStatusCode.OK)
            {
                // Call has been successful.
            }
            else
            {
                // Call has failed - handle errors here.
            }
        }

        public static async Task GetDocumentInQueue(Client.StoreAndForward client)
        {
            // Id of document to be fetched. Document Ids can be obtained using the "GetDocumentsWithHttpMessagesAsync" call.
            int documentId = 1;

            var httpResponse = await client.GetDocumentWithHttpMessagesAsync(documentId);

            if (httpResponse.Response.StatusCode == HttpStatusCode.OK)
            {
                // Call has been successful.
            }
            else
            {
                // Call has failed - handle errors here.
            }
        }

        public static async Task DeleteDocumentInQueue(Client.StoreAndForward client)
        {
            // Id of document to be removed from the queue. Only documents with a 'Pending' status can be removed.
            int documentId = 1;

            var httpResponse = await client.DeleteDocumentWithHttpMessagesAsync(documentId);

            if (httpResponse.Response.StatusCode == HttpStatusCode.NoContent)
            {
                // Call has been successful.
            }
            else
            {
                // Call has failed - handle errors here.
            }
        }

        public static async Task GetEventsForDocument(Client.StoreAndForward client)
        {
            // Get events for document with this Id.
            int documentId = 1;

            var httpResponse = await client.GetDocumentEventsWithHttpMessagesAsync(documentId);

            if (httpResponse.Response.StatusCode == HttpStatusCode.OK)
            {
                // Call has been successful.
            }
            else
            {
                // Call has failed - handle errors here.
            }
        }

        public static async Task GetEvents(Client.StoreAndForward client)
        {
            // Get events 
            var httpResponse = await client.GetEventsWithHttpMessagesAsync(
                DateTime.Now.AddDays(-1),       // Events after this time
                DateTime.Now,                   // Events before this time
                "Success",                      // Event type (Created, Success, Failed, Removed, Deferred)
                0,                              // Offset (number of records to skip)
                10);                            // Limit (number of records to show)


            if (httpResponse.Response.StatusCode == HttpStatusCode.OK)
            {
                // Call has been successful.
            }
            else
            {
                // Call has failed - handle errors here.
            }
        }
    }
}
