using System;
using System.Threading.Tasks;
using DigitalHealth.StoreAndForward.Core.Queue;
using Hangfire;
using Serilog;

namespace DigitalHealth.StoreAndForward.Owin.Jobs
{
    /// <summary>
    /// Send documents job. Calls send on the queue using the specified interval.
    /// </summary>
    [DisableConcurrentExecution(30)]
    public class SendDocumentsJob
    {
        private readonly IQueueManager _queueManager;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="queueManager"></param>
        public SendDocumentsJob(IQueueManager queueManager)
        {
            _queueManager = queueManager;
        }

        /// <summary>
        /// Execute.
        /// </summary>
        /// <returns></returns>
        public async Task Execute()
        {            
            Log.Information("Sending documents at '{message}'", DateTime.Now);

            try
            {
                await _queueManager.SendDocumentsInQueue();
            }
            catch (Exception e)
            {
                Log.Error(e, "SendDocumentsJob.Execute: error sending documents");
            }
        }
    }
}