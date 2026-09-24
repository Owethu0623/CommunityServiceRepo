using System;
using System.Linq;
using System.Web.Mvc;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.Controllers
{
    public class FeedbackManagementController : Controller
    {
        private Community db = new Community();

        // GET: FeedbackManagement
        public ActionResult Index(
            string searchReference,
            int? rating,
            FeedbackResolutionStatus? resolutionStatus,
            int? categoryId,
            DateTime? dateFrom,
            DateTime? dateTo)
        {
            // Administrator authentication
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            // Base query
            var query = db.Feedbacks
                .Include("Request")
                .Include("Request.Category")
                .Include("Request.Ward")
                .Include("Citizen")
                .AsQueryable();

            // Request reference search
            if (!string.IsNullOrWhiteSpace(searchReference))
            {
                searchReference = searchReference.Trim();

                query = query.Where(f =>
                    f.Request.ReferenceNumber.Contains(searchReference));
            }

            // Rating filter
            if (rating.HasValue)
            {
                query = query.Where(f => f.Rating == rating.Value);
            }

            // Resolution status filter
            if (resolutionStatus.HasValue)
            {
                query = query.Where(f =>
                    f.ResolutionStatus == resolutionStatus.Value);
            }

            // Category filter
            if (categoryId.HasValue)
            {
                query = query.Where(f =>
                    f.Request.CategoryID == categoryId.Value);
            }

            // Date From
            if (dateFrom.HasValue)
            {
                DateTime fromDate = dateFrom.Value.Date;

                query = query.Where(f =>
                    f.DateSubmitted >= fromDate);
            }

            // Date To
            if (dateTo.HasValue)
            {
                // Include the entire selected day
                DateTime toDateExclusive = dateTo.Value.Date.AddDays(1);

                query = query.Where(f =>
                    f.DateSubmitted < toDateExclusive);
            }

            // Newest feedback first
            var feedback = query
                .OrderByDescending(f => f.DateSubmitted)
                .ToList();

            // Filter values
            ViewBag.SearchReference = searchReference;
            ViewBag.SelectedRating = rating;
            ViewBag.SelectedResolutionStatus = resolutionStatus;
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.DateFrom = dateFrom;
            ViewBag.DateTo = dateTo;

            // Category dropdown
            ViewBag.Categories = new SelectList(
                db.Categories
                    .OrderBy(c => c.CategoryName)
                    .ToList(),
                "CategoryID",
                "CategoryName",
                categoryId
            );

            // Summary information based on filtered results
            ViewBag.TotalFeedback = feedback.Count;

            ViewBag.AverageRating = feedback.Any()
                ? feedback.Average(f => f.Rating)
                : 0;

            ViewBag.NotFullyResolved = feedback.Count(f =>
                f.ResolutionStatus != FeedbackResolutionStatus.Yes);

            return View(feedback);
        }

        // GET: FeedbackManagement/Details/5
        public ActionResult Details(int? id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            if (!id.HasValue)
            {
                return HttpNotFound();
            }

            var feedback = db.Feedbacks
                .Include("Request")
                .Include("Request.Category")
                .Include("Request.Ward")
                .Include("Request.Technician")
                .Include("Citizen")
                .FirstOrDefault(f => f.FeedbackID == id.Value);

            if (feedback == null)
            {
                return HttpNotFound();
            }

            return View(feedback);
        }

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