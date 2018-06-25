using System.Collections.Generic;
using System.Threading.Tasks;
using DigitalHealth.StoreAndForward.Core.Notification.Models;

namespace DigitalHealth.StoreAndForward.Core.Notification
{
    /// <summary>
    /// Notification service.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Send notifications.
        /// </summary>
        /// <param name="notificationData">Notification data.</param>
        /// <returns></returns>
        Task SendNotification(IList<NotificationData> notificationData);
    }
}
