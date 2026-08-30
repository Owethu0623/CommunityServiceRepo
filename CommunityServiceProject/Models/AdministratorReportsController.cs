using System;
using System.Linq;
using System.Web.Mvc;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.Controllers
{
    public class AdministratorReportsController : Controller
    {
        private Community db = new Community();

        // GET: AdministratorReports
        public ActionResult Index(
            string status,
            int? categoryId,
            DateTime? dateFrom,
            DateTime? dateTo)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            var requests = db.Requests
                .Include("Citizen")
                .Include("Category")
                .AsQueryable();


            // =========================================================
            // FILTER BY STATUS
            // =========================================================

            if (!string.IsNullOrEmpty(status))
            {
                RequestStatus selectedStatus;

                if (Enum.TryParse(status, out selectedStatus))
                {
                    requests = requests.Where(r =>
                        r.Status == selectedStatus);
                }
            }


            // =========================================================
            // FILTER BY CATEGORY
            // =========================================================

            if (categoryId.HasValue)
            {
                requests = requests.Where(r =>
                    r.CategoryID == categoryId.Value);
            }


            // =========================================================
            // FILTER BY DATE FROM
            // =========================================================

            if (dateFrom.HasValue)
            {
                DateTime startDate = dateFrom.Value.Date;

                requests = requests.Where(r =>
                    r.DateSubmitted >= startDate);
            }


            // =========================================================
            // FILTER BY DATE TO
            // =========================================================

            if (dateTo.HasValue)
            {
                DateTime endDate = dateTo.Value.Date.AddDays(1);

                requests = requests.Where(r =>
                    r.DateSubmitted < endDate);
            }


            // =========================================================
            // FINAL REPORT RESULTS
            // =========================================================

            var requestList = requests
                .OrderByDescending(r => r.DateSubmitted)
                .ToList();


            // =========================================================
            // REPORT SUMMARY
            // =========================================================

            ViewBag.ReportTotal = requestList.Count;

            ViewBag.ReportPending = requestList
                .Count(r => r.Status == RequestStatus.Pending);

            ViewBag.ReportCompleted = requestList
                .Count(r => r.Status == RequestStatus.Completed);

            ViewBag.ReportRejected = requestList
                .Count(r => r.Status == RequestStatus.Rejected);

            ViewBag.ReportCategories = requestList
                .Select(r => r.CategoryID)
                .Distinct()
                .Count();


            // =========================================================
            // CATEGORY FILTER OPTIONS
            // =========================================================

            ViewBag.Categories = db.Categories
                .OrderBy(c => c.CategoryName)
                .ToList();


            // =========================================================
            // KEEP SELECTED FILTERS
            // =========================================================

            ViewBag.SelectedStatus = status;

            ViewBag.SelectedCategoryId = categoryId;

            ViewBag.SelectedDateFrom =
                dateFrom.HasValue
                    ? dateFrom.Value.ToString("yyyy-MM-dd")
                    : "";

            ViewBag.SelectedDateTo =
                dateTo.HasValue
                    ? dateTo.Value.ToString("yyyy-MM-dd")
                    : "";


            return View(requestList);
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