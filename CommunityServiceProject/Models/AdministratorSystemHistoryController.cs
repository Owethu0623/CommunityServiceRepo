using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CommunityServiceProject.Models;
using CommunityServiceProject.ViewModels;

namespace CommunityServiceProject.Controllers
{
    public class AdministratorSystemHistoryController : Controller
    {
        private readonly Community db = new Community();

        // GET: AdministratorSystemHistory
        public ActionResult Index(
            string search,
            string type = "All",
            string user = "All",
            string date = "All")
        {
            var history = new List<SystemHistoryViewModel>();

            // =========================================================
            // REQUEST SUBMITTED
            // =========================================================

            var requests = db.Requests
                .Include("Citizen")
                .Include("Administrator")
                .ToList();

            foreach (var request in requests)
            {
                history.Add(new SystemHistoryViewModel
                {
                    DateTime = request.DateSubmitted,
                    UserName = request.Citizen.FirstName + " " + request.Citizen.LastName,
                    UserRole = "Citizen",
                    Action = "Request Submitted",
                    ReferenceNumber = request.ReferenceNumber,
                    RequestID = request.RequestID
                });

                // =====================================================
                // REQUEST APPROVED
                // =====================================================

                if (request.ApprovedDate.HasValue)
                {
                    string administratorName = "Administrator";

                    if (request.Administrator != null)
                    {
                        administratorName =
                            request.Administrator.FirstName + " " +
                            request.Administrator.LastName;
                    }

                    history.Add(new SystemHistoryViewModel
                    {
                        DateTime = request.ApprovedDate.Value,
                        UserName = administratorName,
                        UserRole = "Administrator",
                        Action = "Request Approved",
                        ReferenceNumber = request.ReferenceNumber,
                        RequestID = request.RequestID
                    });
                }
            }

            // =========================================================
            // TECHNICIAN ASSIGNMENTS
            // =========================================================

            var assignments = db.TechnicianAssignments
                .Include("Request")
                .Include("Technician")
                .Include("Administrator")
                .ToList();

            foreach (var assignment in assignments)
            {
                // Technician Assigned
                history.Add(new SystemHistoryViewModel
                {
                    DateTime = assignment.AssignedDate,
                    UserName = assignment.Administrator.FirstName + " " +
                               assignment.Administrator.LastName,
                    UserRole = "Administrator",
                    Action = "Technician Assigned",
                    ReferenceNumber = assignment.Request.ReferenceNumber,
                    RequestID = assignment.RequestID
                });

                // Assignment Acknowledged
                if (assignment.AcknowledgedDate.HasValue)
                {
                    history.Add(new SystemHistoryViewModel
                    {
                        DateTime = assignment.AcknowledgedDate.Value,
                        UserName = assignment.Technician.FirstName + " " +
                                   assignment.Technician.LastName,
                        UserRole = "Technician",
                        Action = "Assignment Acknowledged",
                        ReferenceNumber = assignment.Request.ReferenceNumber,
                        RequestID = assignment.RequestID
                    });
                }
            }

            // =========================================================
            // MAINTENANCE COMPLETION
            // =========================================================

            var completions = db.MaintenanceCompletions
                .Include("MaintenanceWork")
                .Include("MaintenanceWork.Request")
                .Include("MaintenanceWork.Technician")
                .Include("VerifiedByAdministrator")
                .ToList();

            foreach (var completion in completions)
            {
                // Completion Submitted
                history.Add(new SystemHistoryViewModel
                {
                    DateTime = completion.SubmittedDate,
                    UserName = completion.MaintenanceWork.Technician.FirstName +
                               " " +
                               completion.MaintenanceWork.Technician.LastName,
                    UserRole = "Technician",
                    Action = "Completion Submitted",
                    ReferenceNumber =
                        completion.MaintenanceWork.Request.ReferenceNumber,
                    RequestID = completion.MaintenanceWork.RequestID
                });

                // Completion Verified
                if (completion.VerifiedDate.HasValue &&
                    completion.VerifiedByAdministrator != null)
                {
                    history.Add(new SystemHistoryViewModel
                    {
                        DateTime = completion.VerifiedDate.Value,
                        UserName =
                            completion.VerifiedByAdministrator.FirstName +
                            " " +
                            completion.VerifiedByAdministrator.LastName,
                        UserRole = "Administrator",
                        Action = "Completion Verified",
                        ReferenceNumber =
                            completion.MaintenanceWork.Request.ReferenceNumber,
                        RequestID = completion.MaintenanceWork.RequestID
                    });
                }
            }

            // =========================================================
            // SEARCH
            // =========================================================

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();

                history = history
                    .Where(h =>
                        h.UserName.ToLower().Contains(search) ||
                        h.ReferenceNumber.ToLower().Contains(search) ||
                        h.Action.ToLower().Contains(search))
                    .ToList();
            }

            // =========================================================
            // TYPE FILTER
            // =========================================================

            if (!string.IsNullOrEmpty(type) && type != "All")
            {
                if (type == "Request")
                {
                    history = history
                        .Where(h =>
                            h.Action == "Request Submitted" ||
                            h.Action == "Request Approved")
                        .ToList();
                }
                else if (type == "Assignment")
                {
                    history = history
                        .Where(h =>
                            h.Action == "Technician Assigned" ||
                            h.Action == "Assignment Acknowledged")
                        .ToList();
                }
                else if (type == "Maintenance")
                {
                    history = history
                        .Where(h =>
                            h.Action == "Completion Submitted" ||
                            h.Action == "Completion Verified")
                        .ToList();
                }
            }

            // =========================================================
            // USER FILTER
            // =========================================================

            if (!string.IsNullOrEmpty(user) && user != "All")
            {
                history = history
                    .Where(h => h.UserName == user)
                    .ToList();
            }

            // =========================================================
            // DATE FILTER
            // =========================================================

            DateTime today = DateTime.Today;

            if (date == "Today")
            {
                history = history
                    .Where(h => h.DateTime.Date == today)
                    .ToList();
            }
            else if (date == "Last7Days")
            {
                history = history
                    .Where(h => h.DateTime >= today.AddDays(-7))
                    .ToList();
            }
            else if (date == "Last30Days")
            {
                history = history
                    .Where(h => h.DateTime >= today.AddDays(-30))
                    .ToList();
            }

            // =========================================================
            // DROPDOWN DATA
            // =========================================================

            ViewBag.Users = history
                .Select(h => h.UserName)
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            ViewBag.SelectedSearch = search;
            ViewBag.SelectedType = type;
            ViewBag.SelectedUser = user;
            ViewBag.SelectedDate = date;

            // Newest history first
            history = history
                .OrderByDescending(h => h.DateTime)
                .ToList();

            return View(history);
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