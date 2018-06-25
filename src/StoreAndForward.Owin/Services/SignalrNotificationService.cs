using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DigitalHealth.StoreAndForward.Core.Notification;
using DigitalHealth.StoreAndForward.Core.Notification.Models;
using DigitalHealth.StoreAndForward.Owin.Hubs;
using Microsoft.AspNet.SignalR;
using Serilog;

namespace DigitalHealth.StoreAndForward.Owin.Services
{
    /// <summary>
    /// SignalR notification service.
    /// </summary>
    /// <inheritdoc cref="INotificationService"/>
    public class SignalrNotificationService : INotificationService
    {
        /// <summary>
        /// Sends the notification.
        /// </summary>
        /// <param name="notificationData">Notification data.</param>
        /// <returns>Task</returns>
        public async Task SendNotification(IList<NotificationData> notificationData)
        {
            try
            {
                IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
                hubContext.Clients.All.addNotification(notificationData);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error sending notification");
            }
        }
    }
}
