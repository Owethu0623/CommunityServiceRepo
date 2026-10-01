using CommunityServiceProject.Models;
using CommunityServiceProject.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using static System.Collections.Specialized.BitVector32;
using static System.Net.Mime.MediaTypeNames;
using static System.Web.Razor.Parser.SyntaxConstants;
using System.Data.Entity;

namespace CommunityServiceProject.Controllers
{
    public class CitizenNotificationController : Controller
    {
        private Community db = new Community();

        // =========================================================
        // NOTIFICATION CENTRE
        // =========================================================

        [HttpGet]
        public ActionResult Index(string category = "All")
        {
            if (Session["CitizenID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            int citizenID = (int)Session["CitizenID"];

            category = string.IsNullOrWhiteSpace(category)
                ? "All"
                : category.Trim();

            var notifications =
                new List<CitizenNotificationViewModel>();

            // =====================================================
            // MAINTENANCE NOTIFICATIONS
            // =====================================================

            if (category.Equals(
                    "All",
                    StringComparison.OrdinalIgnoreCase) ||
                category.Equals(
                    "Maintenance",
                    StringComparison.OrdinalIgnoreCase))
            {
                var maintenanceNotifications =
                    db.Notifications
                        .Where(n =>
                            n.CitizenID == citizenID)
                        .Select(n =>
                            new CitizenNotificationViewModel
                            {
                                NotificationID =
                                    n.NotificationID,

                                NotificationSource =
                                    "Maintenance",

                                Title =
                                    "Service Request Notification",

                                Message =
                                    n.Message,

                                Category =
                                    "Maintenance",

                                DateCreated =
                                    n.DateCreated,

                                IsRead =
                                    n.IsRead,

                                ReadDate =
                                    null,

                                RequestID =
                                    n.RequestID,

                                RequestReference =
                                    n.Request != null
                                        ? n.Request.ReferenceNumber
                                        : null
                            })
                        .ToList();

                notifications.AddRange(
                    maintenanceNotifications);
            }

            // =====================================================
            // TECHNICIAN APPLICATION NOTIFICATIONS
            // =====================================================

            if (category.Equals(
                    "All",
                    StringComparison.OrdinalIgnoreCase) ||
                category.Equals(
                    "Applications",
                    StringComparison.OrdinalIgnoreCase))
            {
                var applicationNotifications =
                    db.TechnicianApplicationNotifications
                        .Where(n =>
                            n.CitizenID == citizenID)
                        .Select(n =>
                            new CitizenNotificationViewModel
                            {
                                NotificationID =
                                    n.TechnicianApplicationNotificationID,

                                NotificationSource =
                                    "Applications",

                                Title =
                                    n.Title,

                                Message =
                                    n.Message,

                                Category =
                                    "Technician Application",

                                DateCreated =
                                    n.DateCreated,

                                IsRead =
                                    n.IsRead,

                                ReadDate =
                                    n.ReadDate,

                                ApplicationID =
                                    n.ApplicationID,

                                ApplicationReference =
                                    n.Application != null
                                        ? n.Application.ApplicationReference
                                        : null,

                                OpportunityTitle =
                                    n.Application != null &&
                                    n.Application.Opportunity != null
                                        ? n.Application.Opportunity.Title
                                        : null
                            })
                        .ToList();

                // =====================================================
                // MUNICIPAL SERVICE REQUEST NOTIFICATIONS
                // =====================================================

                if (category.Equals(
                        "All",
                        StringComparison.OrdinalIgnoreCase) ||
                    category.Equals(
                        "MunicipalServices",
                        StringComparison.OrdinalIgnoreCase))
                {
                    var municipalServiceNotifications =
                        db.MunicipalServiceRequestNotifications
                            .Where(n =>
                                n.CitizenID == citizenID)
                            .Select(n =>
                                new CitizenNotificationViewModel
                                {
                                    NotificationID =
                                        n.MunicipalServiceRequestNotificationID,

                                    NotificationSource =
                                        "MunicipalServices",

                                    Title =
                                        n.Title,

                                    Message =
                                        n.Message,

                                    Category =
                                        "Municipal Services",

                                    DateCreated =
                                        n.DateCreated,

                                    IsRead =
                                        n.IsRead,

                                    ReadDate =
                                        n.ReadDate,

                                    MunicipalServiceRequestID =
                                        n.MunicipalServiceRequestID,

                                    MunicipalServiceRequestReference =
                                        n.MunicipalServiceRequest != null
                                            ? n.MunicipalServiceRequest.ReferenceNumber
                                            : null
                                })
                            .ToList();

                    notifications.AddRange(
                        municipalServiceNotifications);
                }

                notifications.AddRange(
                    applicationNotifications);
            }

            notifications =
                notifications
                    .OrderByDescending(n => n.DateCreated)
                    .ToList();

            ViewBag.Category = category;

            ViewBag.TotalCount =
                notifications.Count;

            ViewBag.UnreadCount =
                notifications.Count(n => !n.IsRead);

            ViewBag.MaintenanceUnreadCount =
                db.Notifications.Count(n =>
                    n.CitizenID == citizenID &&
                    !n.IsRead);

            ViewBag.ApplicationUnreadCount =
                db.TechnicianApplicationNotifications.Count(n =>
                    n.CitizenID == citizenID &&
                    !n.IsRead);
            ViewBag.MunicipalServiceUnreadCount =
    db.MunicipalServiceRequestNotifications.Count(n =>
        n.CitizenID == citizenID &&
        !n.IsRead);

            return View(notifications);
        }

      
public ActionResult Details(
    string source,
    int? id)
        {
            if (Session["CitizenID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (!id.HasValue ||
                string.IsNullOrWhiteSpace(source))
            {
                return RedirectToAction("Index");
            }

            int citizenID =
                (int)Session["CitizenID"];

            source =
                source.Trim();

            // =====================================================
            // MAINTENANCE NOTIFICATION
            // =====================================================

            if (source.Equals(
                    "Maintenance",
                    StringComparison.OrdinalIgnoreCase))
            {
                var notification =
                    db.Notifications
                        .FirstOrDefault(n =>
                            n.NotificationID == id.Value &&
                            n.CitizenID == citizenID);

                if (notification == null)
                {
                    return HttpNotFound();
                }

                if (!notification.IsRead)
                {
                    notification.IsRead = true;

                    db.SaveChanges();
                }

                var model =
                    new CitizenNotificationViewModel
                    {
                        NotificationID =
                            notification.NotificationID,

                        NotificationSource =
                            "Maintenance",

                        Title =
                            "Service Request Notification",

                        Message =
                            notification.Message,

                        Category =
                            "Maintenance",

                        DateCreated =
                            notification.DateCreated,

                        IsRead =
                            notification.IsRead,

                        RequestID =
                            notification.RequestID,

                        RequestReference =
                            notification.Request != null
                                ? notification.Request.ReferenceNumber
                                : null
                    };

                return View(model);
            }

            // =====================================================
            // TECHNICIAN APPLICATION NOTIFICATION
            // =====================================================

            if (source.Equals(
                    "Applications",
                    StringComparison.OrdinalIgnoreCase))
            {
                var notification =
                    db.TechnicianApplicationNotifications
                        .FirstOrDefault(n =>
                            n.TechnicianApplicationNotificationID ==
                                id.Value &&
                            n.CitizenID ==
                                citizenID);

                if (notification == null)
                {
                    return HttpNotFound();
                }

                if (!notification.IsRead)
                {
                    notification.IsRead = true;
                    notification.ReadDate = DateTime.Now;

                    db.SaveChanges();
                }

                var model =
                    new CitizenNotificationViewModel
                    {
                        NotificationID =
                            notification.TechnicianApplicationNotificationID,

                        NotificationSource =
                            "Applications",

                        Title =
                            notification.Title,

                        Message =
                            notification.Message,

                        Category =
                            "Technician Application",

                        DateCreated =
                            notification.DateCreated,

                        IsRead =
                            notification.IsRead,

                        ReadDate =
                            notification.ReadDate,

                        ApplicationID =
                            notification.ApplicationID,

                        ApplicationReference =
                            notification.Application != null
                                ? notification.Application.ApplicationReference
                                : null,

                        OpportunityTitle =
                            notification.Application != null &&
                            notification.Application.Opportunity != null
                                ? notification.Application.Opportunity.Title
                                : null
                    };

                return View(model);
            }

            // =====================================================
            // MUNICIPAL SERVICE REQUEST NOTIFICATION
            // =====================================================

            if (source.Equals(
                    "MunicipalServices",
                    StringComparison.OrdinalIgnoreCase))
            {
                var notification =
                    db.MunicipalServiceRequestNotifications
                        .Include(n =>
                            n.MunicipalServiceRequest)
                        .FirstOrDefault(n =>
                            n.MunicipalServiceRequestNotificationID ==
                                id.Value &&
                            n.CitizenID ==
                                citizenID);

                if (notification == null)
                {
                    return HttpNotFound();
                }

                if (!notification.IsRead)
                {
                    notification.IsRead = true;
                    notification.ReadDate = DateTime.Now;

                    db.SaveChanges();
                }

                var model =
                    new CitizenNotificationViewModel
                    {
                        NotificationID =
                            notification.MunicipalServiceRequestNotificationID,

                        NotificationSource =
                            "MunicipalServices",

                        Title =
                            notification.Title,

                        Message =
                            notification.Message,

                        Category =
                            "Municipal Services",

                        DateCreated =
                            notification.DateCreated,

                        IsRead =
                            notification.IsRead,

                        ReadDate =
                            notification.ReadDate,

                        MunicipalServiceRequestID =
                            notification.MunicipalServiceRequestID,

                        MunicipalServiceRequestReference =
                            notification.MunicipalServiceRequest != null
                                ? notification.MunicipalServiceRequest.ReferenceNumber
                                : null
                    };

                return View(model);
            }

            return HttpNotFound();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarkAsRead(
            string source,
            int? id)
        {
            if (Session["CitizenID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (!id.HasValue ||
                string.IsNullOrWhiteSpace(source))
            {
                return RedirectToAction("Index");
            }

            int citizenID =
                (int)Session["CitizenID"];

            source =
                source.Trim();

            if (source.Equals(
                    "Maintenance",
                    StringComparison.OrdinalIgnoreCase))
            {
                var notification =
                    db.Notifications
                        .FirstOrDefault(n =>
                            n.NotificationID == id.Value &&
                            n.CitizenID == citizenID);

                if (notification != null &&
                    !notification.IsRead)
                {
                    notification.IsRead = true;
                    db.SaveChanges();
                }
            }
            else if (source.Equals(
                        "Applications",
                        StringComparison.OrdinalIgnoreCase))
            {
                var notification =
                    db.TechnicianApplicationNotifications
                        .FirstOrDefault(n =>
                            n.TechnicianApplicationNotificationID ==
                                id.Value &&
                            n.CitizenID ==
                                citizenID);

                if (notification != null &&
                    !notification.IsRead)
                {
                    notification.IsRead = true;
                    notification.ReadDate =
                        DateTime.Now;

                    db.SaveChanges();
                }
            }
            else if (source.Equals(
            "MunicipalServices",
            StringComparison.OrdinalIgnoreCase))
            {
                var notification =
                    db.MunicipalServiceRequestNotifications
                        .FirstOrDefault(n =>
                            n.MunicipalServiceRequestNotificationID ==
                                id.Value &&
                            n.CitizenID ==
                                citizenID);

                if (notification != null &&
                    !notification.IsRead)
                {
                    notification.IsRead = true;
                    notification.ReadDate = DateTime.Now;

                    db.SaveChanges();
                }
            }
            return RedirectToAction(
                "Index",
                new
                {
                    category = "All"
                });
        }

        // =========================================================
        // MARK ALL AS READ
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarkAllAsRead()
        {
            if (Session["CitizenID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            int citizenID =
                (int)Session["CitizenID"];

            // -----------------------------------------------------
            // Maintenance notifications
            // -----------------------------------------------------

            var maintenanceNotifications =
                db.Notifications
                    .Where(n =>
                        n.CitizenID == citizenID &&
                        !n.IsRead)
                    .ToList();

            foreach (var notification
                in maintenanceNotifications)
            {
                notification.IsRead = true;
            }

            // -----------------------------------------------------
            // Technician application notifications
            // -----------------------------------------------------

            var applicationNotifications =
                db.TechnicianApplicationNotifications
                    .Where(n =>
                        n.CitizenID == citizenID &&
                        !n.IsRead)
                    .ToList();

            foreach (var notification
                in applicationNotifications)
            {
                notification.IsRead = true;
                notification.ReadDate =
                    DateTime.Now;
            }
            // =====================================================
            // MUNICIPAL SERVICE REQUEST NOTIFICATIONS
            // =====================================================

            var municipalServiceNotifications =
                db.MunicipalServiceRequestNotifications
                    .Where(n =>
                        n.CitizenID == citizenID &&
                        !n.IsRead)
                    .ToList();

            foreach (var notification
                in municipalServiceNotifications)
            {
                notification.IsRead = true;
                notification.ReadDate = DateTime.Now;
            }
            db.SaveChanges();

            return RedirectToAction(
                "Index",
                new
                {
                    category = "All"
                });
        }

        // =========================================================
        // DISPOSE
        // =========================================================

        protected override void Dispose(
            bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
