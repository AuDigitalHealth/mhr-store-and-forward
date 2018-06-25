using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Serilog;

namespace DigitalHealth.StoreAndForward.Owin.Logging
{
    /// <summary>
    /// Request and response logging using Serilog.
    /// </summary>
    public class SerilogLoggingMessageHandler : LoggingMessageHandler
    {
        /// <summary>
        /// Process a request.
        /// </summary>
        /// <param name="correlationId">Correlation ID.</param>
        /// <param name="requestInfo">Request information.</param>
        /// <param name="messageBody">Message body.</param>
        /// <param name="headers">HTTP headers.</param>
        public override void ProcessRequest(string correlationId, string requestInfo, string messageBody,
            List<KeyValuePair<string, IEnumerable<string>>> headers)
        {
            string requestMessageBody = messageBody;
            if (string.IsNullOrEmpty(messageBody))
            {
                requestMessageBody = "<no body>";
            }

            StringBuilder requestHeadersBuilder = new StringBuilder();
            foreach (KeyValuePair<string, IEnumerable<string>> header in headers)
            {
                requestHeadersBuilder.AppendLine($"{header.Key} = {string.Join(" ", header.Value)}");
            }

            Log.Information("{correlationId} - Request: {requestInfo}\r\n{requestMessageBody}\r\n{requestHeaders}",
                correlationId, requestInfo, requestMessageBody, requestHeadersBuilder.ToString());

        }

        /// <summary>
        /// Process a response.
        /// </summary>
        /// <param name="correlationId">Correlation ID.</param>
        /// <param name="requestInfo">Request information.</param>
        /// <param name="httpStatusCode">Response HTTP status code.</param>
        /// <param name="messageBody">Message body.</param>
        /// <param name="headers">HTTP headers.</param>
        public override void ProcessResponse(string correlationId, string requestInfo, HttpStatusCode httpStatusCode, string messageBody,
            List<KeyValuePair<string, IEnumerable<string>>> headers)
        {
            string responseMessageBody = messageBody;
            if (string.IsNullOrEmpty(responseMessageBody))
            {
                responseMessageBody = "<no body>";
            }

            StringBuilder responseHeadersBuilder = new StringBuilder();
            if (headers.Any())
            {
                foreach (KeyValuePair<string, IEnumerable<string>> header in headers)
                {
                    responseHeadersBuilder.AppendLine($"{header.Key} = {string.Join(" ", header.Value)}");
                }
            }
            else
            {
                responseHeadersBuilder.AppendLine("<no headers>");
            }

            Log.Information("{correlationId} - Response: {requestInfo}\r\nStatus code: {statusCode}\r\n{reponseMessageBody}\r\n{responseHeaders}",
                correlationId, requestInfo, httpStatusCode, responseMessageBody, responseHeadersBuilder.ToString());
        }
    }
}