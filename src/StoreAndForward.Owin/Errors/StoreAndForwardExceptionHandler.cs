using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Results;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace DigitalHealth.StoreAndForward.Owin.Errors
{
    /// <summary>
    /// Store and forward exception handler.
    /// </summary>
    public class StoreAndForwardExceptionHandler : ExceptionHandler
    {
        /// <summary>
        /// Handle errors.
        /// </summary>
        /// <param name="exceptionHandlerContext"></param>
        public override void Handle(ExceptionHandlerContext exceptionHandlerContext)
        {
            Exception exception = exceptionHandlerContext.Exception;

            // Generate a unique ID for each error
            string id = Guid.NewGuid().ToString();

            var storeAndForwardError = new StoreAndForwardError
            {
                Id = id,
                Message = exception.Message,
                StackTrace = exception.StackTrace,
                DateTime = DateTime.UtcNow,
                ErrorCode = null                
            };

            // Log the error
            Log.Error(exception, "Error details {@errorDetails}", storeAndForwardError);

            // Return a generic server error
            exceptionHandlerContext.Result = new ResponseMessageResult(
                exceptionHandlerContext.Request.CreateResponse<JObject>(HttpStatusCode.InternalServerError,
                SerializeErrorResponse(storeAndForwardError)));
        }

        private static JObject SerializeErrorResponse(StoreAndForwardError errorResponse)
        {
            return JObject.FromObject(errorResponse, new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented
            });
        }
    }
}