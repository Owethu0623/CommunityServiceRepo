using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.Controllers
{
    public class CitizenDashboardController : Controller
    {
        private Community db = new Community();

        // GET: CitizenDashboard
        public ActionResult Index()
        {
            // Make sure a citizen is actually logged in
            if (Session["CitizenID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            int citizenId = (int)Session["CitizenID"];

            // Get the actual logged-in citizen
            Citizen citizen = db.Citizens.Find(citizenId);

            if (citizen == null)
            {
                Session.Clear();

                return RedirectToAction(
                    "Index",
                    "Login"
                );
            }

            // =====================================================
            // GET CITIZEN'S REQUESTS
            // =====================================================

            var requests = db.Requests
                .Where(r => r.CitizenID == citizenId)
                .OrderByDescending(r => r.DateSubmitted)
                .Take(5)
                .ToList();


            // =====================================================
            // CREATE DASHBOARD NOTIFICATIONS
            // =====================================================

            var notifications =
                new List<string>();

            foreach (var request in requests)
            {
                string notification = "";

                switch (request.Status)
                {
                    case RequestStatus.Pending:

                        notification =
                            "Your request \"" +
                            request.Title +
                            "\" has been submitted and is currently pending.";

                        break;


                    case RequestStatus.UnderReview:

                        notification =
                            "Your request \"" +
                            request.Title +
                            "\" is currently under review.";

                        break;


                    case RequestStatus.Approved:

                        notification =
                            "Your request \"" +
                            request.Title +
                            "\" has been approved.";

                        break;


                    case RequestStatus.Rejected:

                        notification =
                            "Your request \"" +
                            request.Title +
                            "\" has been rejected.";

                        break;


                    default:

                        // Ignore statuses that are not part
                        // of the current notification feature.
                        continue;
                }

                notifications.Add(notification);
            }


            // =====================================================
            // SEND NOTIFICATIONS TO VIEW
            // =====================================================

            ViewBag.Notifications = notifications;


            return View(citizen);
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