using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.Controllers
{
    public class AdministratorStatisticsController : Controller
    {
        private Community db = new Community();

        // GET: AdministratorStatistics
        public ActionResult Index()
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            // =========================================================
            // REQUEST STATISTICS
            // =========================================================

            ViewBag.TotalRequests = db.Requests.Count();

            ViewBag.PendingRequests = db.Requests
                .Count(r => r.Status == RequestStatus.Pending);

            ViewBag.UnderReviewRequests = db.Requests
                .Count(r => r.Status == RequestStatus.UnderReview);

            ViewBag.ApprovedRequests = db.Requests
                .Count(r => r.Status == RequestStatus.Approved);

            ViewBag.AssignedRequests = db.Requests
                .Count(r => r.Status == RequestStatus.Assigned);

            ViewBag.InProgressRequests = db.Requests
                .Count(r => r.Status == RequestStatus.InProgress);

            ViewBag.CompletedRequests = db.Requests
                .Count(r => r.Status == RequestStatus.Completed);

            ViewBag.RejectedRequests = db.Requests
                .Count(r => r.Status == RequestStatus.Rejected);


            // =========================================================
            // REQUESTS BY CATEGORY
            // =========================================================

            var categoryStatistics = db.Categories
                .Select(c => new
                {
                    CategoryName = c.CategoryName,
                    RequestCount = c.Requests.Count()
                })
                .OrderByDescending(c => c.RequestCount)
                .ToList();

            var categoryList = new List<Dictionary<string, object>>();

            foreach (var category in categoryStatistics)
            {
                categoryList.Add(
                    new Dictionary<string, object>
                    {
                        { "CategoryName", category.CategoryName },
                        { "RequestCount", category.RequestCount }
                    }
                );
            }

            ViewBag.CategoryStatistics = categoryList;


            // =========================================================
            // CITIZEN STATISTICS
            // =========================================================

            ViewBag.TotalCitizens = db.Citizens.Count();

            ViewBag.ActiveCitizens = db.Citizens
                .Count(c => c.AccountStatus == AccountStatus.Active);

            ViewBag.CitizensWithRequests = db.Citizens
                .Count(c => db.Requests
                    .Any(r => r.CitizenID == c.CitizenID));


            return View();
        }


        // =========================================================
        // DISPOSE
        // =========================================================

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}