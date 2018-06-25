using System;
using Newtonsoft.Json;

namespace DigitalHealth.StoreAndForward.Owin.Errors
{
    /// <summary>
    /// Standard API error.
    /// </summary>
    public class StoreAndForwardError
    {
        /// <summary>
        /// Error ID.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Date time of error.
        /// </summary>
        [JsonProperty("date_time")]
        public DateTime DateTime { get; set; }

        /// <summary>
        /// Error code.
        /// </summary>
        [JsonProperty("error_code")]
        public string ErrorCode { get; set; }

        /// <summary>
        /// Error message.
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }

        /// <summary>
        /// Stacktrace.
        /// </summary>
        [JsonProperty("stack_trace")]
        public string StackTrace { get; set; }
    }
}