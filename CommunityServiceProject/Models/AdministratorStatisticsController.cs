using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
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
            // OVERALL SERVICE PERFORMANCE
            // =========================================================

            var allRequests = db.Requests.ToList();

            ViewBag.TotalRequests = allRequests.Count;

            ViewBag.CompletedRequests = allRequests
                .Count(r => r.Status == RequestStatus.Completed);


            // =========================================================
            // AVERAGE APPROVAL TIME
            // DateSubmitted -> ApprovedDate
            // =========================================================

            var approvalTimes = allRequests
                .Where(r => r.ApprovedDate.HasValue)
                .Select(r =>
                    (r.ApprovedDate.Value - r.DateSubmitted).TotalDays)
                .ToList();

            ViewBag.AverageApprovalTime =
                approvalTimes.Any()
                    ? approvalTimes.Average()
                    : (double?)null;


            // =========================================================
            // AVERAGE ASSIGNMENT TIME
            // ApprovedDate -> AssignedDate
            // =========================================================

            var assignments = db.TechnicianAssignments
                .Include(a => a.Request)
                .ToList();

            var assignmentTimes = assignments
                .Where(a =>
                    a.Request != null &&
                    a.Request.ApprovedDate.HasValue)
                .Select(a =>
                    (a.AssignedDate -
                     a.Request.ApprovedDate.Value).TotalDays)
                .ToList();

            ViewBag.AverageAssignmentTime =
                assignmentTimes.Any()
                    ? assignmentTimes.Average()
                    : (double?)null;


            // =========================================================
            // AVERAGE RESOLUTION TIME
            // DateSubmitted -> CompletedDate
            // =========================================================

            var maintenanceWorks = db.MaintenanceWorks
                .Include(m => m.Request)
                .ToList();

            var resolutionTimes = maintenanceWorks
                .Where(m =>
                    m.Request != null &&
                    m.CompletedDate.HasValue)
                .Select(m =>
                    (m.CompletedDate.Value -
                     m.Request.DateSubmitted).TotalDays)
                .ToList();

            ViewBag.AverageResolutionTime =
                resolutionTimes.Any()
                    ? resolutionTimes.Average()
                    : (double?)null;


            // =========================================================
            // CATEGORY PERFORMANCE
            // =========================================================

            var categoryPerformance = new List<Dictionary<string, object>>();

            var categories = db.Categories
                .Include(c => c.Requests)
                .ToList();

            foreach (var category in categories)
            {
                var requests = category.Requests.ToList();

                var categoryResolutionTimes = maintenanceWorks
                    .Where(m =>
                        m.Request != null &&
                        m.Request.CategoryID == category.CategoryID &&
                        m.CompletedDate.HasValue)
                    .Select(m =>
                        (m.CompletedDate.Value -
                         m.Request.DateSubmitted).TotalDays)
                    .ToList();

                var openMoreThan7Days = requests
                    .Count(r =>
                        r.Status != RequestStatus.Completed &&
                        r.DateSubmitted <= DateTime.Now.AddDays(-7));

                categoryPerformance.Add(
                    new Dictionary<string, object>
                    {
                        { "CategoryName", category.CategoryName },

                        { "RequestCount", requests.Count },

                        {
                            "AverageResolutionTime",
                            categoryResolutionTimes.Any()
                                ? (object)categoryResolutionTimes.Average()
                                : null
                        },

                        {
                            "OpenMoreThan7Days",
                            openMoreThan7Days
                        }
                    }
                );
            }

            ViewBag.CategoryPerformance = categoryPerformance
                .OrderByDescending(c =>
                    (int)c["RequestCount"])
                .ToList();


            // =========================================================
            // WARD PERFORMANCE
            // =========================================================

            var wardPerformance = new List<Dictionary<string, object>>();

            var wards = db.Wards
                .Include(w => w.Requests)
                .ToList();

            foreach (var ward in wards)
            {
                var requests = ward.Requests.ToList();

                if (!requests.Any())
                {
                    continue;
                }

                var completedRequests = requests
                    .Count(r =>
                        r.Status == RequestStatus.Completed);

                var wardResolutionTimes = maintenanceWorks
                    .Where(m =>
                        m.Request != null &&
                        m.Request.WardID == ward.WardID &&
                        m.CompletedDate.HasValue)
                    .Select(m =>
                        (m.CompletedDate.Value -
                         m.Request.DateSubmitted).TotalDays)
                    .ToList();

                wardPerformance.Add(
                    new Dictionary<string, object>
                    {
                        {
                            "WardName",
                            "Ward " + ward.WardNumber
                        },

                        {
                            "RequestCount",
                            requests.Count
                        },

                        {
                            "CompletedRequests",
                            completedRequests
                        },

                        {
                            "AverageResolutionTime",
                            wardResolutionTimes.Any()
                                ? (object)wardResolutionTimes.Average()
                                : null
                        }
                    }
                );
            }

            ViewBag.WardPerformance = wardPerformance
                .OrderByDescending(w =>
                    (int)w["RequestCount"])
                .ToList();


            // =========================================================
            // TECHNICIAN PERFORMANCE
            // =========================================================

            var technicianPerformance =
                new List<Dictionary<string, object>>();

            var technicians = db.Technicians
                .ToList();

            foreach (var technician in technicians)
            {
                var technicianWorks = maintenanceWorks
                    .Where(m =>
                        m.TechnicianID == technician.TechnicianID &&
                        m.CompletedDate.HasValue)
                    .ToList();

                if (!technicianWorks.Any())
                {
                    continue;
                }

                var technicianResolutionTimes =
                    technicianWorks
                        .Where(m => m.Request != null)
                        .Select(m =>
                            (m.CompletedDate.Value -
                             m.Request.DateSubmitted).TotalDays)
                        .ToList();

                technicianPerformance.Add(
                    new Dictionary<string, object>
                    {
                        {
                            "TechnicianName",
                            technician.FirstName + " " +
                            technician.LastName
                        },

                        {
                            "CompletedRequests",
                            technicianWorks.Count
                        },

                        {
                            "AverageResolutionTime",
                            technicianResolutionTimes.Any()
                                ? (object)technicianResolutionTimes.Average()
                                : null
                        }
                    }
                );
            }

            ViewBag.TechnicianPerformance =
                technicianPerformance
                    .OrderByDescending(t =>
                        (int)t["CompletedRequests"])
                    .ToList();


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