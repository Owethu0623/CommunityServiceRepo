using System;
using System.Linq;
using System.Web.Mvc;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.Controllers
{
    public class CitizenNotificationController : Controller
    {
        private Community db = new Community();

        // GET: CitizenNotification
        public ActionResult Index()
        {
            if (Session["CitizenID"] == null)
                return RedirectToAction("Index", "Login");

            int citizenID = (int)Session["CitizenID"];

            var notifications = db.Notifications
                .Where(n => n.CitizenID == citizenID)
                .OrderByDescending(n => n.DateCreated)
                .ToList();

            return View(notifications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarkAllAsRead()
        {
            if (Session["CitizenID"] == null)
                return RedirectToAction("Index", "Login");

            int citizenID = (int)Session["CitizenID"];

            var unreadNotifications = db.Notifications
                .Where(n =>
                    n.CitizenID == citizenID &&
                    !n.IsRead)
                .ToList();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
            }

            db.SaveChanges();

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}