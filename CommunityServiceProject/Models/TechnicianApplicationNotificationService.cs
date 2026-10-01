

using System;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.Services
{
    public class TechnicianApplicationNotificationService
    {
        private readonly Community db;

        public TechnicianApplicationNotificationService(Community db)
        {
            this.db = db;
        }

        public void Create(
            TechnicianApplication application,
            TechnicianApplicationNotificationType notificationType,
            string title,
            string message)
        {
            if (application == null)
                throw new ArgumentNullException("application");

            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException(
                    "Notification title is required.",
                    "title");

            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException(
                    "Notification message is required.",
                    "message");

            var notification =
                new TechnicianApplicationNotification
                {
                    ApplicationID = application.ApplicationID,

                    CitizenID = application.CitizenID,

                    NotificationType = notificationType,

                    Title = title.Trim(),

                    Message = message.Trim(),

                    DateCreated = DateTime.Now,

                    IsRead = false,

                    ReadDate = null
                };

            db.TechnicianApplicationNotifications.Add(notification);
        }
    }
}

