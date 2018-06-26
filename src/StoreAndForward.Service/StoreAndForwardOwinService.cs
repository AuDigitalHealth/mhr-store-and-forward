using System;
using Microsoft.Owin.Hosting;
using Serilog;

namespace DigitalHealth.StoreAndForward.Service
{
    /// <summary>
    /// Store and forward OWIN service.
    /// </summary>
    public class StoreAndForwardOwinService
    {
        /// <summary>
        /// Endpoint.
        /// </summary>
        private readonly string _endpoint;
        
        /// <summary>
        /// OWIN web server.
        /// </summary>
        private IDisposable _webServer;

        /// <summary>
        /// Creates an instance of the StoreAndForwardOwinService on the specified endpoint.
        /// </summary>
        /// <param name="hostEndpoint"></param>
        public StoreAndForwardOwinService(string hostEndpoint)
        {
            _endpoint = hostEndpoint;
        }

        /// <summary>
        /// Starts the server.
        /// </summary>
        public void Start()
        {
            Log.Information("OWIN starting on endpoint {endpoint}", _endpoint);

            try
            {
                _webServer = WebApp.Start(_endpoint);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error starting OWIN");
                throw;
            }
        }

        /// <summary>
        /// Stops the server.
        /// </summary>
        public void Stop()
        {
            Log.Information("OWIN shutting down");

            _webServer.Dispose();            
        }
    }
}