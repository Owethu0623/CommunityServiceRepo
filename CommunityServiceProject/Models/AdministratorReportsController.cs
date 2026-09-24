using CommunityServiceProject.Models;
using CommunityServiceProject.ViewModels;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace CommunityServiceProject.Controllers
{
    public class AdministratorReportsController : Controller
    {
        private Community db = new Community();

        // =========================================================
        // REPORT INDEX
        // =========================================================

        public ActionResult Index(
            string status,
            int? categoryId,
            int? wardId,
            string priority,
            int? technicianId,
            DateTime? dateFrom,
            DateTime? dateTo,
            string reportOption = "ServiceResolutionPerformance")
        {
            // -----------------------------------------------------
            // SECURITY
            // -----------------------------------------------------

            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Login");
            }

            // -----------------------------------------------------
            // VALIDATE REPORT OPTION
            // -----------------------------------------------------

            var validReportOptions = new[]
            {
                "RequestsByCategory",
                "RequestsByWard",
                "RequestsByStatus",
                "RequestsByPriority",
                "TechnicianPerformance",
                "ServiceResolutionPerformance"
            };

            if (!validReportOptions.Contains(reportOption))
            {
                reportOption = "ServiceResolutionPerformance";
            }

            // -----------------------------------------------------
            // BASE REQUEST QUERY
            // -----------------------------------------------------

            var query = db.Requests
                .Include("Citizen")
                .Include("Category")
                .Include("Ward")
                .Include("Technician")
                .AsQueryable();

            // -----------------------------------------------------
            // STATUS FILTER
            // -----------------------------------------------------

            RequestStatus selectedStatus;

            if (!string.IsNullOrWhiteSpace(status) &&
                Enum.TryParse(status, true, out selectedStatus))
            {
                query = query.Where(r =>
                    r.Status == selectedStatus);
            }

            // -----------------------------------------------------
            // CATEGORY FILTER
            // -----------------------------------------------------

            if (categoryId.HasValue)
            {
                query = query.Where(r =>
                    r.CategoryID == categoryId.Value);
            }

            // -----------------------------------------------------
            // WARD FILTER
            // -----------------------------------------------------

            if (wardId.HasValue)
            {
                query = query.Where(r =>
                    r.WardID == wardId.Value);
            }

            // -----------------------------------------------------
            // PRIORITY FILTER
            // -----------------------------------------------------

            Priority selectedPriority;

            if (!string.IsNullOrWhiteSpace(priority) &&
                Enum.TryParse(priority, true, out selectedPriority))
            {
                query = query.Where(r =>
                    r.Priority == selectedPriority);
            }

            // -----------------------------------------------------
            // TECHNICIAN FILTER
            // -----------------------------------------------------

            if (technicianId.HasValue)
            {
                query = query.Where(r =>
                    r.TechnicianID == technicianId.Value);
            }

            // -----------------------------------------------------
            // DATE FROM
            // -----------------------------------------------------

            if (dateFrom.HasValue)
            {
                DateTime fromDate =
                    dateFrom.Value.Date;

                query = query.Where(r =>
                    r.DateSubmitted >= fromDate);
            }

            // -----------------------------------------------------
            // DATE TO
            // -----------------------------------------------------

            if (dateTo.HasValue)
            {
                DateTime toDate =
                    dateTo.Value.Date.AddDays(1);

                query = query.Where(r =>
                    r.DateSubmitted < toDate);
            }

            // -----------------------------------------------------
            // MATERIALISE FILTERED REQUESTS
            // -----------------------------------------------------

            var requestList = query
                .OrderByDescending(r => r.DateSubmitted)
                .ToList();

            var filteredRequestIds = requestList
                .Select(r => r.RequestID)
                .ToList();

            // =====================================================
            // CREATE VIEW MODEL
            // =====================================================

            var model = new AdministratorReportsViewModel
            {
                SelectedStatus = status,
                SelectedCategoryId = categoryId,
                SelectedWardId = wardId,
                SelectedPriority = priority,
                SelectedTechnicianId = technicianId,
                SelectedDateFrom = dateFrom,
                SelectedDateTo = dateTo,
                SelectedReportOption = reportOption
            };

            // =====================================================
            // FILTER OPTIONS
            // =====================================================

            model.Categories = db.Categories
                .OrderBy(c => c.CategoryName)
                .ToList();

            model.Wards = db.Wards
                .Where(w => w.IsActive)
                .ToList()
                .OrderBy(w =>
                {
                    int number;

                    return int.TryParse(
                        w.WardNumber,
                        out number)
                        ? number
                        : int.MaxValue;
                })
                .ToList();

            model.Technicians = db.Technicians
                .OrderBy(t => t.FirstName)
                .ThenBy(t => t.LastName)
                .ToList();

            // =====================================================
            // SUMMARY
            // =====================================================

            model.TotalRequests =
                requestList.Count;

            model.PendingRequests =
                requestList.Count(r =>
                    r.Status == RequestStatus.Pending);

            model.CompletedRequests =
                requestList.Count(r =>
                    r.Status == RequestStatus.Completed);

            model.RejectedRequests =
                requestList.Count(r =>
                    r.Status == RequestStatus.Rejected);

            model.CategoryCount =
                requestList
                    .Select(r => r.CategoryID)
                    .Distinct()
                    .Count();

            // =====================================================
            // REQUESTS BY CATEGORY
            // =====================================================

            model.RequestsByCategory =
                requestList
                    .GroupBy(r =>
                        r.Category != null
                            ? r.Category.CategoryName
                            : "Uncategorised")
                    .Select(g => new CategoryReportRow
                    {
                        CategoryName = g.Key,

                        RequestCount = g.Count(),

                        Percentage =
                            requestList.Count > 0
                                ? Math.Round(
                                    (double)g.Count()
                                    / requestList.Count
                                    * 100,
                                    1)
                                : 0
                    })
                    .OrderByDescending(
                        x => x.RequestCount)
                    .ToList();

            // =====================================================
            // REQUESTS BY WARD
            // =====================================================

            model.RequestsByWard =
                requestList
                    .GroupBy(r => r.Ward)
                    .Select(g => new WardReportRow
                    {
                        WardNumber =
                            g.Key != null
                                ? g.Key.WardNumber
                                : "N/A",

                        WardName =
                            g.Key != null
                                ? g.Key.WardName
                                : "Unknown",

                        RequestCount = g.Count(),

                        Percentage =
                            requestList.Count > 0
                                ? Math.Round(
                                    (double)g.Count()
                                    / requestList.Count
                                    * 100,
                                    1)
                                : 0
                    })
                    .OrderBy(x =>
                    {
                        int number;

                        return int.TryParse(
                            x.WardNumber,
                            out number)
                            ? number
                            : int.MaxValue;
                    })
                    .ToList();

            // =====================================================
            // REQUESTS BY STATUS
            // =====================================================

            model.RequestsByStatus =
                requestList
                    .GroupBy(r => r.Status)
                    .Select(g => new StatusReportRow
                    {
                        Status = g.Key.ToString(),

                        RequestCount = g.Count(),

                        Percentage =
                            requestList.Count > 0
                                ? Math.Round(
                                    (double)g.Count()
                                    / requestList.Count
                                    * 100,
                                    1)
                                : 0
                    })
                    .OrderByDescending(
                        x => x.RequestCount)
                    .ToList();

            // =====================================================
            // REQUESTS BY PRIORITY
            // =====================================================

            model.RequestsByPriority =
                requestList
                    .GroupBy(r => r.Priority)
                    .Select(g => new PriorityReportRow
                    {
                        Priority = g.Key.ToString(),

                        RequestCount = g.Count(),

                        Percentage =
                            requestList.Count > 0
                                ? Math.Round(
                                    (double)g.Count()
                                    / requestList.Count
                                    * 100,
                                    1)
                                : 0
                    })
                    .OrderByDescending(
                        x => x.RequestCount)
                    .ToList();

            // =====================================================
            // TECHNICIAN PERFORMANCE
            // =====================================================

            model.TechnicianPerformance =
                requestList
                    .Where(r => r.Technician != null)
                    .GroupBy(r => r.Technician)
                    .Select(g => new TechnicianReportRow
                    {
                        TechnicianName =
                            g.Key.FirstName +
                            " " +
                            g.Key.LastName,

                        TotalRequests =
                            g.Count(),

                        CompletedRequests =
                            g.Count(r =>
                                r.Status ==
                                RequestStatus.Completed),

                        ActiveRequests =
                            g.Count(r =>
                                r.Status !=
                                    RequestStatus.Completed &&
                                r.Status !=
                                    RequestStatus.Rejected),

                        CompletionRate =
                            g.Count() > 0
                                ? Math.Round(
                                    (double)g.Count(r =>
                                        r.Status ==
                                        RequestStatus.Completed)
                                    / g.Count()
                                    * 100,
                                    1)
                                : 0
                    })
                    .OrderByDescending(
                        x => x.CompletedRequests)
                    .ThenBy(
                        x => x.TechnicianName)
                    .ToList();

            // =====================================================
            // MAINTENANCE WORK
            // =====================================================

            var maintenanceWorks =
                db.MaintenanceWorks
                    .Include("Request")
                    .Include("Request.Category")
                    .Where(m =>
                        filteredRequestIds.Contains(
                            m.RequestID))
                    .ToList();

            // =====================================================
            // RESOLUTION TIMES
            // =====================================================

            var resolutionTimes =
                maintenanceWorks
                    .Where(m =>
                        m.CompletedDate.HasValue &&
                        m.Request != null)
                    .Select(m =>
                        (m.CompletedDate.Value -
                         m.Request.DateSubmitted)
                        .TotalDays)
                    .ToList();

            model.AverageResolutionDays =
                resolutionTimes.Any()
                    ? Math.Round(
                        resolutionTimes.Average(),
                        1)
                    : 0;

            // =====================================================
            // RESOLVED REQUESTS
            // =====================================================

            model.ResolvedRequests =
                requestList.Count(r =>
                    r.Status ==
                    RequestStatus.Completed);

            // =====================================================
            // RESOLUTION RATE
            // =====================================================

            model.ResolutionRate =
                requestList.Count > 0
                    ? Math.Round(
                        (double)model.ResolvedRequests
                        / requestList.Count
                        * 100,
                        1)
                    : 0;

            // =====================================================
            // OPEN OVER SEVEN DAYS
            // =====================================================

            model.OpenOverSevenDays =
                requestList.Count(r =>
                    r.Status !=
                        RequestStatus.Completed &&
                    r.Status !=
                        RequestStatus.Rejected &&
                    (DateTime.Now -
                     r.DateSubmitted).TotalDays > 7);

            // =====================================================
            // RESOLUTION PERFORMANCE BY CATEGORY
            // =====================================================

            model.CategoryResolutionPerformance =
                maintenanceWorks
                    .Where(m =>
                        m.CompletedDate.HasValue &&
                        m.Request != null &&
                        m.Request.Category != null)
                    .GroupBy(m =>
                        m.Request.Category.CategoryName)
                    .Select(g =>
                    {
                        var completedTimes =
                            g.Select(m =>
                                (m.CompletedDate.Value -
                                 m.Request.DateSubmitted)
                                .TotalDays)
                            .ToList();

                        return new CategoryResolutionReportRow
                        {
                            CategoryName = g.Key,

                            CompletedRequests =
                                g.Count(),

                            AverageResolutionDays =
                                completedTimes.Any()
                                    ? Math.Round(
                                        completedTimes.Average(),
                                        1)
                                    : 0
                        };
                    })
                    .OrderByDescending(
                        x => x.CompletedRequests)
                    .ToList();

            // =====================================================
            // RETURN VIEW
            // =====================================================

            return View(model);
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