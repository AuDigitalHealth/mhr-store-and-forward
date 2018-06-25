using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DigitalHealth.StoreAndForward.Owin.Logging
{
    /// <summary>
    /// Base logging message handler.
    /// </summary>
    public abstract class LoggingMessageHandler : DelegatingHandler
    {
        /// <summary>
        /// Send.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Request
            string correlationId = $"{DateTime.Now.Ticks}{Thread.CurrentThread.ManagedThreadId}";
            string requestInfo = $"{request.Method} {request.RequestUri}";

            string requestMessageBody = await request.Content.ReadAsStringAsync();
            
            ProcessRequest(correlationId, requestInfo, requestMessageBody, request.Headers.ToList());

            // Continue the pipe
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            // Response
            string reponseMessageBody = null;
            if (response.Content != null)
            {
                reponseMessageBody = await response.Content.ReadAsStringAsync();
            }

            ProcessResponse(correlationId, requestInfo, response.StatusCode, reponseMessageBody, response.Headers.ToList());

            return response;
        }

        /// <summary>
        /// Process a request.
        /// </summary>
        /// <param name="correlationId">ID used to link the request to the response.</param>
        /// <param name="requestInfo"></param>
        /// <param name="messageBody"></param>
        /// <param name="headers"></param>
        public abstract void ProcessRequest(string correlationId, string requestInfo, string messageBody,
            List<KeyValuePair<string, IEnumerable<string>>> headers);

        /// <summary>
        /// Process a response.
        /// </summary>
        /// <param name="correlationId">ID used to link the request to the response.</param>
        /// <param name="requestInfo"></param>
        /// <param name="httpStatusCode"></param>
        /// <param name="messageBody"></param>
        /// <param name="headers"></param>
        public abstract void ProcessResponse(string correlationId, string requestInfo, HttpStatusCode httpStatusCode, string messageBody,
            List<KeyValuePair<string, IEnumerable<string>>> headers);
    }
}