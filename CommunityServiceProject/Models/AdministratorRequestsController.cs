using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CommunityServiceProject.Models;
using CommunityServiceProject.ViewModels;
using System.Data.Entity;

namespace CommunityServiceProject.Controllers
{
    public class AdministratorRequestsController : Controller
    {
        private Community db = new Community();

        public ActionResult Index(
    string search,
    RequestStatus? status,
    int? categoryId,
    int? wardId,
    Priority? priority,
    string sortOrder)
        {
            // Make sure an administrator is logged in
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            var requests = db.Requests
              .Include("Citizen")
              .Include("Category")
              .Include("Ward")
             .Include("MaintenanceWorks")
             .AsQueryable();

            // Search by request title
            if (!string.IsNullOrWhiteSpace(search))
            {
                requests = requests.Where(r =>
                    r.Title.Contains(search));
            }

            // Filter by status
            if (status.HasValue)
            {
                requests = requests.Where(r =>
                    r.Status == status.Value);
            }

            // Filter by category
            if (categoryId.HasValue)
            {
                requests = requests.Where(r =>
                    r.CategoryID == categoryId.Value);
            }

            // Filter by ward
            if (wardId.HasValue)
            {
                requests = requests.Where(r =>
                    r.WardID == wardId.Value);
            }

            // Filter by priority
            if (priority.HasValue)
            {
                requests = requests.Where(r =>
                    r.Priority == priority.Value);
            }



            // Sorting
            switch (sortOrder)
            {
                case "oldest":
                    requests = requests.OrderBy(r => r.DateSubmitted);
                    break;

                default:
                    requests = requests.OrderByDescending(r => r.DateSubmitted);
                    break;
            }

            // Find requests that currently have a pending reassignment request.
            // This is separate from the citizen RequestStatus.
            var pendingReassignmentRequestIds = db.ReassignmentRequests
                .Where(r => r.Status == ReassignmentStatus.Pending)
                .Select(r => r.Assignment.RequestID)
                .Distinct()
                .ToList();

            ViewBag.PendingReassignmentRequestIds =
                pendingReassignmentRequestIds;

            // Send filter options to the view
            ViewBag.Categories = new SelectList(
                db.Categories.OrderBy(c => c.CategoryName),
                "CategoryID",
                "CategoryName",
                categoryId
            );

            ViewBag.Wards = new SelectList(
            db.Wards
            .Where(w => w.IsActive)
            .OrderBy(w => w.WardNumber),
            "WardID",
            "WardNumber",
            wardId
            );



            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.SortOrder = sortOrder;

            ViewBag.WardId = wardId;
            ViewBag.Priority = priority;

            var pendingReassignmentLookup = db.ReassignmentRequests
    .Where(r => r.Status == ReassignmentStatus.Pending)
    .Select(r => new
    {
        RequestID = r.Assignment.RequestID,
        ReassignmentRequestID = r.ReassignmentRequestID
    })
    .ToList()
    .ToDictionary(
        r => r.RequestID,
        r => r.ReassignmentRequestID
    );

            ViewBag.PendingReassignmentLookup = pendingReassignmentLookup;

            return View(requests.ToList());
        }


        public ActionResult Details(int? id, string returnUrl)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            if (id == null)
            {
                return RedirectToAction("Index");
            }

            var request = db.Requests
                .Include("Citizen")
                .Include("Category")
                .Include("Ward")
                .FirstOrDefault(r => r.RequestID == id);

            if (request == null)
            {
                return HttpNotFound();
            }

            var linkedAsset = db.AssetRequests
                .FirstOrDefault(ar => ar.RequestID == request.RequestID);

            ViewBag.LinkedAssetID = linkedAsset != null
                ? (int?)linkedAsset.AssetID
                : null;

            ViewBag.CitizenID = request.CitizenID;

            ViewBag.ViolationRecorded = db.Violations
                .Any(v => v.RequestID == request.RequestID);

            ViewBag.ReturnUrl = returnUrl;

            return View(request);
        }

        // GET: AdministratorRequests/RecordViolation/5
        public ActionResult RecordViolation(int? id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            if (id == null)
            {
                return RedirectToAction("Index");
            }

            var request = db.Requests
                .Include("Citizen")
                .Include("Category")
                .FirstOrDefault(r => r.RequestID == id.Value);

            if (request == null)
            {
                return HttpNotFound();
            }

            return View(request);
        }

        // POST: AdministratorRequests/RecordViolation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RecordViolation(
            int requestID,
            string violationType,
            string description)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            // Validate violation type
            if (string.IsNullOrWhiteSpace(violationType))
            {
                ModelState.AddModelError(
                    "violationType",
                    "Violation type is required."
                );
            }

            // Validate description
            if (string.IsNullOrWhiteSpace(description))
            {
                ModelState.AddModelError(
                    "description",
                    "A reason or description is required."
                );
            }

            // Find the request
            var request = db.Requests
                .Include("Citizen")
                .Include("Category")
                .FirstOrDefault(r => r.RequestID == requestID);

            if (request == null)
            {
                return HttpNotFound();
            }

            // A violation can only be recorded while the request
            // is still within the administrator review stage.
            if (request.Status != RequestStatus.Pending &&
                request.Status != RequestStatus.UnderReview &&
                request.Status != RequestStatus.Rejected)
            {
                ModelState.AddModelError(
                    "",
                    "A violation can only be recorded for pending, under review, or rejected requests."
                );
            }

            // Prevent more than one violation from being recorded
            // against the same request.
            var existingViolation = db.Violations
                .FirstOrDefault(v => v.RequestID == request.RequestID);

            if (existingViolation != null)
            {
                ModelState.AddModelError(
                    "",
                    "A violation has already been recorded for this request. " +
                    "Another violation cannot be submitted."
                );
            }

            // If ANY validation failed, return to the same page
            // so the administrator can see the validation message.
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            int administratorId = (int)Session["AdministratorID"];

            // Find the citizen's existing compliance record.
            var complianceRecord = db.ComplianceRecords
                .FirstOrDefault(c => c.CitizenID == request.CitizenID);

            // Create a compliance record if one does not exist.
            if (complianceRecord == null)
            {
                complianceRecord = new ComplianceRecord
                {
                    CitizenID = request.CitizenID,
                    ConfirmedViolationCount = 0,
                    ComplianceStatus = ComplianceStatus.Compliant,
                    LastUpdated = DateTime.Now
                };

                db.ComplianceRecords.Add(complianceRecord);
            }

            // Create the violation.
            var violation = new Violation
            {
                ComplianceID = complianceRecord.ComplianceID,
                RequestID = request.RequestID,
                AdministratorID = administratorId,
                ViolationType = violationType.Trim(),
                Description = description.Trim(),
                Status = "Confirmed",
                DateConfirmed = DateTime.Now
            };

            db.Violations.Add(violation);

            // Update compliance information.
            complianceRecord.ConfirmedViolationCount++;
            complianceRecord.LastUpdated = DateTime.Now;

            db.SaveChanges();

            TempData["SuccessMessage"] =
                "Violation recorded successfully.";

            return RedirectToAction(
                "Details",
                new { id = request.RequestID }
            );
        }


        // GET: AdministratorRequests/Classify/5
        public ActionResult Classify(int? id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            if (id == null)
            {
                return RedirectToAction("Index");
            }

            var request = db.Requests
                .Include("Category")
                .Include("Ward")
                .FirstOrDefault(r => r.RequestID == id);

            if (request == null)
            {
                return HttpNotFound();
            }

            // Only approved requests can be classified
            if (request.Status != RequestStatus.Approved)
            {
                return RedirectToAction("Details", new { id = request.RequestID });
            }

            var model = new RequestClassificationViewModel
            {
                RequestID = request.RequestID,
                Title = request.Title,
                CategoryName = request.Category != null
                    ? request.Category.CategoryName
                    : "Unknown",
                ProblemLocation = request.ProblemLocation,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                WardID = request.WardID,
                Priority = request.Priority,
                PriorityReason = request.PriorityReason,

                Wards = db.Wards
                    .OrderBy(w => w.WardNumber)
                    .Select(w => new SelectListItem
                    {
                        Value = w.WardID.ToString(),
                        Text = w.WardNumber + " - " + w.WardName
                    })
                    .ToList()
            };

            return View(model);
        }




        // POST: AdministratorRequests/Classify
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Classify(RequestClassificationViewModel model)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            if (!ModelState.IsValid)
            {
                model.Wards = db.Wards
                    .OrderBy(w => w.WardNumber)
                    .Select(w => new SelectListItem
                    {
                        Value = w.WardID.ToString(),
                        Text = w.WardNumber + " - " + w.WardName,
                        Selected = w.WardID == model.WardID
                    })
                    .ToList();

                return View(model);
            }

            var request = db.Requests.Find(model.RequestID);

            if (request == null)
            {
                return HttpNotFound();
            }

            // Only approved requests can be classified
            if (request.Status != RequestStatus.Approved)
            {
                return RedirectToAction("Details", new { id = request.RequestID });
            }

            request.WardID = model.WardID;
            request.Priority = model.Priority;
            request.PriorityReason = model.PriorityReason;

            db.SaveChanges();

            return RedirectToAction("Details", new { id = request.RequestID });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StartReview(int id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            var request = db.Requests.Find(id);

            if (request == null)
            {
                return HttpNotFound();
            }

            if (request.Status == RequestStatus.Pending)
            {
                request.Status = RequestStatus.UnderReview;

                try
                {
                    db.SaveChanges();
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                {
                    var errors = ex.EntityValidationErrors
                        .SelectMany(e => e.ValidationErrors)
                        .Select(e => e.PropertyName + ": " + e.ErrorMessage)
                        .ToList();

                    throw new Exception(
                        "Validation errors: " +
                        string.Join(" | ", errors),
                        ex
                    );
                }
            }

            return RedirectToAction("Details", new { id = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Approve(int id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            var request = db.Requests
                .Include("Citizen")
                .FirstOrDefault(r => r.RequestID == id);

            if (request == null)
            {
                return HttpNotFound();
            }

            if (request.Status == RequestStatus.UnderReview)
            {
                request.Status = RequestStatus.Approved;
                request.AdministratorID = (int)Session["AdministratorID"];
                request.ApprovedDate = DateTime.Now;

                // Create citizen notification
                var notification = new Notification
                {
                    CitizenID = request.CitizenID,
                    RequestID = request.RequestID,
                    Message = "Request " + request.ReferenceNumber +
                              " has been approved.",
                    DateCreated = DateTime.Now,
                    IsRead = false
                };

                db.Notifications.Add(notification);

                db.SaveChanges();

                return RedirectToAction("Classify", new { id = id });
            }

            return RedirectToAction("Details", new { id = id });
        }

        // POST: AdministratorRequests/Reject
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Reject(int id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            var request = db.Requests.Find(id);

            if (request == null)
            {
                return HttpNotFound();
            }

            if (request.Status == RequestStatus.UnderReview)
            {
                request.Status = RequestStatus.Rejected;
                request.AdministratorID = (int)Session["AdministratorID"];

                db.SaveChanges();
            }

            return RedirectToAction("Details", new { id = id });
        }

        // ===========================================================
        // VERIFY COMPLETION - GET
        // ===========================================================

        // GET: AdministratorRequests/VerifyCompletion/5
        public ActionResult VerifyCompletion(int? id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            if (id == null)
            {
                return RedirectToAction("Index");
            }

            var maintenanceWork = db.MaintenanceWorks
                .Include("Request")
                .Include("Request.Citizen")
                .Include("Request.Category")
                .Include("Request.Ward")
                .Include("Technician")
                .Include("ProgressRecords")
                .Include("WorkNotes")
                .Include("Materials")
                .Include("Evidence")
                .Include("Completions")
                .FirstOrDefault(m =>
                    m.MaintenanceWorkID == id.Value);

            if (maintenanceWork == null)
            {
                return HttpNotFound();
            }

            // Only maintenance submitted for verification
            // can be reviewed by the administrator.
            if (maintenanceWork.Status != MaintenanceWorkStatus.SubmittedForVerification)
            {
                TempData["ErrorMessage"] =
                    "This maintenance work is not currently awaiting verification.";

                return RedirectToAction(
                    "Details",
                    new { id = maintenanceWork.RequestID });
            }

            return View(maintenanceWork);
        }

        // POST: AdministratorRequests/VerifyCompletion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult VerifyCompletion(int id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            var maintenanceWork = db.MaintenanceWorks
                .Include("Request")
                .Include("Completions")
                .FirstOrDefault(m =>
                    m.MaintenanceWorkID == id);

            if (maintenanceWork == null)
            {
                return HttpNotFound();
            }

            if (maintenanceWork.Status != MaintenanceWorkStatus.SubmittedForVerification)
            {
                TempData["ErrorMessage"] =
                    "This maintenance work is not awaiting verification.";

                return RedirectToAction(
                    "Details",
                    new { id = maintenanceWork.RequestID });
            }

            var completion = maintenanceWork.Completions
                .OrderByDescending(c => c.SubmittedDate)
                .FirstOrDefault();

            if (completion == null)
            {
                TempData["ErrorMessage"] =
                    "No completion submission was found.";

                return RedirectToAction(
                    "Details",
                    new { id = maintenanceWork.RequestID });
            }

            int administratorID = (int)Session["AdministratorID"];

            completion.VerificationStatus =
                CompletionVerificationStatus.Verified;

            completion.VerifiedByAdministratorID =
                administratorID;

            completion.VerifiedDate =
                DateTime.Now;

            maintenanceWork.Status =
                MaintenanceWorkStatus.Verified;

            maintenanceWork.CompletedDate =
                DateTime.Now;

            maintenanceWork.Request.Status =
    RequestStatus.Completed;

            // -------------------------------------------------------
            // Create citizen notification
            // -------------------------------------------------------

            var notification = new Notification
            {
                CitizenID = maintenanceWork.Request.CitizenID,
                RequestID = maintenanceWork.Request.RequestID,
                Message = "Your request " +
                          maintenanceWork.Request.ReferenceNumber +
                          " has been completed.",
                DateCreated = DateTime.Now,
                IsRead = false
            };

            db.Notifications.Add(notification);

            db.SaveChanges();

            TempData["SuccessMessage"] =
                "Maintenance completion has been verified successfully.";

            return RedirectToAction(
                "Details",
                new { id = maintenanceWork.RequestID });
        }

        // POST: AdministratorRequests/RejectCompletion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RejectCompletion(
            int id,
            string administratorComments)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            var maintenanceWork = db.MaintenanceWorks
                .Include("Request")
                .Include("Completions")
                .FirstOrDefault(m =>
                    m.MaintenanceWorkID == id);

            if (maintenanceWork == null)
            {
                return HttpNotFound();
            }

            if (maintenanceWork.Status !=
                MaintenanceWorkStatus.SubmittedForVerification)
            {
                TempData["ErrorMessage"] =
                    "This maintenance work is not awaiting verification.";

                return RedirectToAction(
                    "Details",
                    new { id = maintenanceWork.RequestID });
            }

            var completion = maintenanceWork.Completions
                .OrderByDescending(c => c.SubmittedDate)
                .FirstOrDefault();

            if (completion == null)
            {
                TempData["ErrorMessage"] =
                    "No completion submission was found.";

                return RedirectToAction(
                    "Details",
                    new { id = maintenanceWork.RequestID });
            }

            if (string.IsNullOrWhiteSpace(administratorComments))
            {
                TempData["ErrorMessage"] =
                    "Please provide a reason for rejecting the completion.";

                return RedirectToAction(
                    "VerifyCompletion",
                    new { id = maintenanceWork.MaintenanceWorkID });
            }

            if (administratorComments.Trim().Length > 1000)
            {
                TempData["ErrorMessage"] =
                    "Administrator comments cannot exceed 1000 characters.";

                return RedirectToAction(
                    "VerifyCompletion",
                    new { id = maintenanceWork.MaintenanceWorkID });
            }

            int administratorID = (int)Session["AdministratorID"];

            completion.VerificationStatus =
                CompletionVerificationStatus.Rejected;

            completion.VerifiedByAdministratorID =
                administratorID;

            completion.VerifiedDate =
                DateTime.Now;

            completion.AdministratorComments =
                administratorComments.Trim();

            maintenanceWork.Status =
                MaintenanceWorkStatus.Rejected;

            db.SaveChanges();

            TempData["SuccessMessage"] =
                "Maintenance completion has been rejected.";

            return RedirectToAction(
                "Details",
                new { id = maintenanceWork.RequestID });
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