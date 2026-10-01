
using System;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.Services
{
    public class MunicipalServiceRequestNotificationService
    {
        private readonly Community db;

        public MunicipalServiceRequestNotificationService(Community db)
        {
            this.db = db;
        }

        public void Create(
            MunicipalServiceRequest request,
            MunicipalServiceRequestNotificationType notificationType,
            string title,
            string message)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException(
                    "Notification title is required.",
                    "title");
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException(
                    "Notification message is required.",
                    "message");
            }

            var notification =
                new MunicipalServiceRequestNotification
                {
                    CitizenID = request.CitizenID,

                    MunicipalServiceRequestID =
                        request.MunicipalServiceRequestID,

                    NotificationType =
                        notificationType,

                    Title =
                        title.Trim(),

                    Message =
                        message.Trim(),

                    DateCreated =
                        DateTime.Now,

                    IsRead = false,

                    ReadDate = null
                };

            db.MunicipalServiceRequestNotifications.Add(
                notification);
        }
    }
}

