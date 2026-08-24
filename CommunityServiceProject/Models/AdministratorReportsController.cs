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

            var requests = db.Requests.AsQueryable();

            // Filter by Status
            if (!string.IsNullOrEmpty(status))
            {
                RequestStatus selectedStatus;

                if (Enum.TryParse(status, out selectedStatus))
                {
                    requests = requests.Where(r =>
                        r.Status == selectedStatus);
                }
            }

            // Filter by Category
            if (categoryId.HasValue)
            {
                requests = requests.Where(r =>
                    r.CategoryID == categoryId.Value);
            }

            // Filter by Date From
            if (dateFrom.HasValue)
            {
                DateTime startDate = dateFrom.Value.Date;

                requests = requests.Where(r =>
                    r.DateSubmitted >= startDate);
            }

            // Filter by Date To
            if (dateTo.HasValue)
            {
                DateTime endDate = dateTo.Value.Date.AddDays(1);

                requests = requests.Where(r =>
                    r.DateSubmitted < endDate);
            }

            var requestList = requests
                .OrderByDescending(r => r.DateSubmitted)
                .ToList();

            ViewBag.Categories = db.Categories
                .OrderBy(c => c.CategoryName)
                .ToList();

            // Keep selected filters
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