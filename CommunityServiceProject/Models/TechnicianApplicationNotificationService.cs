

using System;
using System.Linq;
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
            // If this notification indicates ApprovedForOnboarding, also create
            // an administrator notification so admins can create technician accounts.
            if (notificationType == TechnicianApplicationNotificationType.ApprovedForOnboarding)
            {
                // Notify all administrators that an onboarding requires final account creation
                try
                {
                    var admins = db.Administrators.Where(a => a.AccountStatus == AccountStatus.Active).ToList();

                    foreach (var admin in admins)
                    {
                        db.AdministratorNotifications.Add(new AdministratorNotification
                        {
                            AdministratorID = admin.AdministratorID,
                            ApplicationID = application.ApplicationID,
                            Title = "New onboarding awaiting account creation",
                            Message = "An application has been approved for onboarding and requires administrator account creation for technician: " + application.ApplicationReference,
                            DateCreated = DateTime.Now,
                            IsRead = false,
                            ReadDate = null
                        });
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        var logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "notification_errors.log");
                        var log = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Administrator notification creation failed: {ex}\n\n";
                        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(logPath));
                        System.IO.File.AppendAllText(logPath, log);
                    }
                    catch { }
                }

                // Additionally, notify HR officers for recruitment events via dedicated HR notifications
                try
                {
                    var hrs = db.HROfficers.Where(h => h.AccountStatus == AccountStatus.Active).ToList();

                    foreach (var hr in hrs)
                    {
                        db.HROfficerNotifications.Add(new HROfficerNotification
                        {
                            HROfficerID = hr.HROfficerID,
                            ApplicationID = application.ApplicationID,
                            Title = "Application approved for onboarding",
                            Message = "Application " + application.ApplicationReference + " has been approved for onboarding.",
                            DateCreated = DateTime.Now,
                            IsRead = false,
                            ReadDate = null
                        });
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        var logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "notification_errors.log");
                        var log = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] HR officer notification creation failed: {ex}\n\n";
                        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(logPath));
                        System.IO.File.AppendAllText(logPath, log);
                    }
                    catch { }
                }
            }
        }
    }
}

