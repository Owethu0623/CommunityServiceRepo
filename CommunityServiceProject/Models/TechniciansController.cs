using CommunityServiceProject.Models;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CommunityServiceProject.Controllers
{
    public class TechniciansController : Controller
    {
        private Community db = new Community();


        // ===========================================================
        // LOGIN - GET
        // ===========================================================

        // GET: Technicians/Login
        public ActionResult Login()
        {
            return View();
        }


        // ===========================================================
        // LOGIN - POST
        // ===========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string emailAddress, string password)
        {
            if (string.IsNullOrWhiteSpace(emailAddress) ||
                string.IsNullOrWhiteSpace(password))
            {
                ViewBag.ErrorMessage =
                    "Please enter your email address and password.";

                return View();
            }


            // Find technician using email and password
            var technician = db.Technicians
                .FirstOrDefault(t =>
                    t.EmailAddress == emailAddress &&
                    t.Password == password);


            // Invalid credentials
            if (technician == null)
            {
                ViewBag.ErrorMessage =
                    "Invalid email address or password.";

                return View();
            }


            // Check account status
            if (technician.AccountStatus != AccountStatus.Active)
            {
                ViewBag.ErrorMessage =
                    "Your account is not active. Please contact an administrator.";

                return View();
            }


            // Store technician information in session
            Session["TechnicianID"] = technician.TechnicianID;

            Session["TechnicianName"] =
                technician.FirstName + " " + technician.LastName;


            // Redirect to Technician Dashboard
            return RedirectToAction("Dashboard");
        }


        // ===========================================================
        // FORGOT PASSWORD - GET
        // ===========================================================

        // GET: Technicians/ForgotPassword
        public ActionResult ForgotPassword()
        {
            return View();
        }


        // ===========================================================
        // FORGOT PASSWORD - POST
        // ===========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(
            string emailAddress,
            string phoneNumber,
            string lastName)
        {
            if (string.IsNullOrWhiteSpace(emailAddress) ||
                string.IsNullOrWhiteSpace(phoneNumber) ||
                string.IsNullOrWhiteSpace(lastName))
            {
                ViewBag.ErrorMessage =
                    "Please enter your email address, phone number, and last name.";

                return View();
            }


            // Verify technician account
            var technician = db.Technicians.FirstOrDefault(t =>
                t.EmailAddress == emailAddress &&
                t.PhoneNumber == phoneNumber &&
                t.LastName == lastName);


            // Verification failed
            if (technician == null)
            {
                ViewBag.ErrorMessage =
                    "The information provided could not be verified.";

                return View();
            }


            // Store verified technician ID temporarily
            Session["PasswordResetTechnicianID"] =
                technician.TechnicianID;


            // Move to password reset page
            return RedirectToAction("ResetPassword");
        }


        // ===========================================================
        // RESET PASSWORD - GET
        // ===========================================================

        // GET: Technicians/ResetPassword
        public ActionResult ResetPassword()
        {
            if (Session["PasswordResetTechnicianID"] == null)
            {
                return RedirectToAction("ForgotPassword");
            }

            return View();
        }


        // ===========================================================
        // RESET PASSWORD - POST
        // ===========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(
            string password,
            string confirmPassword)
        {
            if (Session["PasswordResetTechnicianID"] == null)
            {
                return RedirectToAction("ForgotPassword");
            }


            if (string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                ViewBag.ErrorMessage =
                    "Please enter and confirm your new password.";

                return View();
            }


            if (password != confirmPassword)
            {
                ViewBag.ErrorMessage =
                    "Passwords do not match.";

                return View();
            }


            // Find verified technician
            int technicianID =
                (int)Session["PasswordResetTechnicianID"];

            var technician =
                db.Technicians.Find(technicianID);


            if (technician == null)
            {
                Session.Remove("PasswordResetTechnicianID");

                return RedirectToAction("ForgotPassword");
            }


            // Update password
            technician.Password = password;

            try
            {
                db.SaveChanges();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            "Property: " + validationError.PropertyName +
                            " | Error: " + validationError.ErrorMessage
                        );
                    }
                }

                throw;
            }

            // Remove password reset session
            Session.Remove("PasswordResetTechnicianID");


            // Show success message
            TempData["SuccessMessage"] =
                "Your password has been reset successfully. You can now log in.";

            return RedirectToAction("Login");
        }

        // ===========================================================
        // TECHNICIAN DASHBOARD
        // ===========================================================

        public ActionResult Dashboard()
        {
            if (Session["TechnicianID"] == null)
            {
                return RedirectToAction("Login");
            }

            int technicianID = (int)Session["TechnicianID"];

            var model = new CommunityServiceProject.ViewModels.TechnicianDashboardViewModel
            {
                // Requests currently assigned to this technician
                AssignedCount = db.Requests.Count(r =>
                    r.TechnicianID == technicianID &&
                    r.Status == RequestStatus.Assigned),

                // Maintenance currently being performed
                InProgressCount = db.MaintenanceWorks.Count(m =>
                    m.TechnicianID == technicianID &&
                    m.Status == MaintenanceWorkStatus.InProgress),

                // Verified maintenance work
                CompletedCount = db.MaintenanceWorks.Count(m =>
                    m.TechnicianID == technicianID &&
                    m.Status == MaintenanceWorkStatus.Verified),

                // Assignments waiting for technician acknowledgement
                AwaitingAcknowledgementCount =
                    db.TechnicianAssignments.Count(a =>
                        a.TechnicianID == technicianID &&
                        a.Status == AssignmentStatus.PendingAcknowledgement),

                // Pending reassignment requests submitted by technician
                ReassignmentRequestCount =
                    db.ReassignmentRequests.Count(r =>
                        r.TechnicianID == technicianID &&
                        r.Status == ReassignmentStatus.Pending)
            };

            return View(model);
        }

        // ===========================================================
        // ASSIGNED REQUESTS
        // ===========================================================

        // GET: Technicians/AssignedRequests
        public ActionResult AssignedRequests()
        {
            if (Session["TechnicianID"] == null)
            {
                return RedirectToAction("Login");
            }

            int technicianID = (int)Session["TechnicianID"];

            var assignedRequests = db.Requests
                .Include("Citizen")
                .Include("Category")
                .Include("Ward")
                .Where(r =>
                    r.TechnicianID == technicianID &&
                    (r.Status == RequestStatus.Assigned ||
                     r.Status == RequestStatus.InProgress))
                .OrderByDescending(r => r.DateSubmitted)
                .ToList();

            return View(assignedRequests);
        }


        // ===========================================================
        // COMPLETED WORK HISTORY - US66
        // ===========================================================

        // GET: Technicians/CompletedWork
        public ActionResult CompletedWork(
            string search,
            int? categoryId,
            int? wardId)
        {
            if (Session["TechnicianID"] == null)
            {
                return RedirectToAction("Login");
            }

            int technicianID = (int)Session["TechnicianID"];

            // Get only verified/completed maintenance work
            var completedWork = db.MaintenanceWorks
                .Include("Request")
                .Include("Request.Category")
                .Include("Request.Ward")
                .Include("Completions")
                .Where(m =>
                    m.TechnicianID == technicianID &&
                    m.Status == MaintenanceWorkStatus.Verified)
                .AsQueryable();

            // ===========================================================
            // SEARCH
            // ===========================================================

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                completedWork = completedWork.Where(m =>
                    m.Request.Title.Contains(search) ||
                    m.Request.Description.Contains(search) ||
                    m.Request.RequestID.ToString().Contains(search));
            }

            // ===========================================================
            // CATEGORY FILTER
            // ===========================================================

            if (categoryId.HasValue)
            {
                completedWork = completedWork.Where(m =>
                    m.Request.CategoryID == categoryId.Value);
            }

            // ===========================================================
            // WARD FILTER
            // ===========================================================

            if (wardId.HasValue)
            {
                completedWork = completedWork.Where(m =>
                    m.Request.WardID == wardId.Value);
            }

            // Most recently completed work first
            var completedWorkList = completedWork
                .OrderByDescending(m => m.CompletedDate)
                .ToList();

            // Filter options for the view
            ViewBag.Categories = db.Categories
                .OrderBy(c => c.CategoryName)
                .ToList();

            ViewBag.Wards = db.Wards
                .OrderBy(w => w.WardNumber)
                .ToList();

            // Preserve selected filters
            ViewBag.Search = search;
            ViewBag.CategoryID = categoryId;
            ViewBag.WardID = wardId;

            return View(completedWorkList);
        }

        // ===========================================================
        // PREVIOUS RESOLUTIONS - US68
        // ===========================================================

        // GET: Technicians/PreviousResolutions
        public ActionResult PreviousResolutions(
            string search,
            int? categoryId,
            int? wardId)
        {
            if (Session["TechnicianID"] == null)
            {
                return RedirectToAction("Login");
            }

            // Get verified maintenance completions from all technicians
            var previousResolutions = db.MaintenanceCompletions
                .Include("MaintenanceWork")
                .Include("MaintenanceWork.Request")
                .Include("MaintenanceWork.Request.Category")
                .Include("MaintenanceWork.Request.Ward")
                .Include("MaintenanceWork.Technician")
                .Where(c =>
                    c.VerificationStatus ==
                    CompletionVerificationStatus.Verified)
                .AsQueryable();

            // ===========================================================
            // SEARCH
            // ===========================================================

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                previousResolutions = previousResolutions.Where(c =>
                    c.MaintenanceSummary.Contains(search) ||
                    c.ResolutionAction.Contains(search) ||
                    c.MaintenanceWork.Request.Title.Contains(search) ||
                    c.MaintenanceWork.Request.Description.Contains(search));
            }

            // ===========================================================
            // CATEGORY FILTER
            // ===========================================================

            if (categoryId.HasValue)
            {
                previousResolutions = previousResolutions.Where(c =>
                    c.MaintenanceWork.Request.CategoryID == categoryId.Value);
            }

            // ===========================================================
            // WARD FILTER
            // ===========================================================

            if (wardId.HasValue)
            {
                previousResolutions = previousResolutions.Where(c =>
                    c.MaintenanceWork.Request.WardID == wardId.Value);
            }

            // Most recently completed resolutions first
            var resolutionList = previousResolutions
                .OrderByDescending(c => c.VerifiedDate)
                .ToList();

            // Filter options
            ViewBag.Categories = db.Categories
                .OrderBy(c => c.CategoryName)
                .ToList();

            ViewBag.Wards = db.Wards
                .OrderBy(w => w.WardNumber)
                .ToList();

            // Preserve selected filters
            ViewBag.Search = search;
            ViewBag.CategoryID = categoryId;
            ViewBag.WardID = wardId;

            return View(resolutionList);
        }

        // ===========================================================
        // KNOWLEDGE BASE - US69
        // ===========================================================

        // GET: Technicians/KnowledgeBase
        public ActionResult KnowledgeBase(
            string search,
            int? categoryId)
        {
            if (Session["TechnicianID"] == null)
            {
                return RedirectToAction("Login");
            }

            var knowledgeBase = db.MaintenanceKnowledgeBases
                .Include("MaintenanceCompletion")
                .Include("MaintenanceCompletion.MaintenanceWork")
                .Include("MaintenanceCompletion.MaintenanceWork.Request")
                .Include("Category")
                .Include("CreatedByTechnician")
                .Where(k => k.IsApproved && k.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                knowledgeBase = knowledgeBase.Where(k =>
                    k.Title.Contains(search) ||
                    k.Keywords.Contains(search) ||
                    k.ProblemDescription.Contains(search) ||
                    k.RecommendedSolution.Contains(search) ||
                    k.LessonsLearned.Contains(search));
            }

            if (categoryId.HasValue)
            {
                knowledgeBase = knowledgeBase.Where(k =>
                    k.CategoryID == categoryId.Value);
            }

            var knowledgeBaseList = knowledgeBase
                .OrderByDescending(k => k.CreatedDate)
                .ToList();

            ViewBag.Categories = db.Categories
                .OrderBy(c => c.CategoryName)
                .ToList();

            ViewBag.Search = search;
            ViewBag.CategoryID = categoryId;

            return View(knowledgeBaseList);
        }


        // ===========================================================
        // REQUEST DETAILS
        // ===========================================================

        // GET: Technicians/Details/5
        public ActionResult Details(int? id)
        {
            if (Session["TechnicianID"] == null)
            {
                return RedirectToAction("Login");
            }

            if (id == null)
            {
                return RedirectToAction("Dashboard");
            }

            int technicianID = (int)Session["TechnicianID"];

            var request = db.Requests
             .Include("Citizen")
               .Include("Category")
               .Include("Ward")
             .Include("Technician")
            .Include("RequiredSkills.Skill")
            .Include("TechnicianAssignments")
            .Include("MaintenanceWorks.WorkNotes")
             .Include("MaintenanceWorks.Materials")
             .Include("MaintenanceWorks.Evidence")
             .Include("MaintenanceWorks.Evidence")
              .Include("MaintenanceWorks.Evidence")
            .FirstOrDefault(r =>
             r.RequestID == id &&
             r.TechnicianID == technicianID);

            if (request == null)
            {
                return HttpNotFound();
            }

            return View(request);
        }

        // ===========================================================
        // GET DIRECTIONS
        // ===========================================================

        public ActionResult Directions(int id)
        {
            if (Session["TechnicianID"] == null)
            {
                return RedirectToAction("Login");
            }

            int technicianID = (int)Session["TechnicianID"];

            var request = db.Requests
                .FirstOrDefault(r =>
                    r.RequestID == id &&
                    r.TechnicianID == technicianID);

            if (request == null)
            {
                return HttpNotFound();
            }

            if (!request.Latitude.HasValue ||
                !request.Longitude.HasValue)
            {
                return RedirectToAction(
                    "Details",
                    new { id = request.RequestID }
                );
            }

            string latitude =
                request.Latitude.Value.ToString(
                    System.Globalization.CultureInfo.InvariantCulture
                );

            string longitude =
                request.Longitude.Value.ToString(
                    System.Globalization.CultureInfo.InvariantCulture
                );

            string destination =
                latitude + "%2C" + longitude;

            string googleMapsUrl =
                "https://www.google.com/maps/dir/?api=1" +
                "&destination=" +
                destination;

            return Redirect(googleMapsUrl);
        }


        // ===========================================================
        // ACKNOWLEDGE ASSIGNMENT
        // ===========================================================

        // POST: Technicians/Acknowledge
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Acknowledge(int id)
        {
            if (Session["TechnicianID"] == null)
            {
                return RedirectToAction("Login");
            }

            int technicianID = (int)Session["TechnicianID"];

            var assignment = db.TechnicianAssignments
                .FirstOrDefault(a =>
                    a.AssignmentID == id &&
                    a.TechnicianID == technicianID);

            if (assignment == null)
            {
                return HttpNotFound();
            }

            if (assignment.Status == AssignmentStatus.PendingAcknowledgement)
            {
                assignment.Status = AssignmentStatus.Acknowledged;
                assignment.AcknowledgedDate = System.DateTime.Now;

                db.SaveChanges();

                TempData["SuccessMessage"] =
                    "Assignment acknowledged successfully.";
            }

            return RedirectToAction(
                "Details",
                new { id = assignment.RequestID }
            );
        }


        // ===========================================================
        // REPORT ASSIGNMENT ISSUE - GET
        // ===========================================================

        // GET: Technicians/ReportIssue/5
        public ActionResult ReportIssue(int? id)
        {
            if (Session["TechnicianID"] == null)
            {
                return RedirectToAction("Login");
            }

            if (id == null)
            {
                return RedirectToAction("Dashboard");
            }

            int technicianID = (int)Session["TechnicianID"];

            var assignment = db.TechnicianAssignments
                .Include("Request")
                .FirstOrDefault(a =>
                    a.AssignmentID == id &&
                    a.TechnicianID == technicianID);

            if (assignment == null)
            {
                return HttpNotFound();
            }

            if (assignment.Status != AssignmentStatus.PendingAcknowledgement &&
                assignment.Status != AssignmentStatus.Acknowledged)
            {
                TempData["ErrorMessage"] =
                    "This assignment cannot currently be reported.";

                return RedirectToAction(
                    "Details",
                    new { id = assignment.RequestID }
                );
            }

            var model = new CommunityServiceProject.ViewModels.AssignmentIssueViewModel
            {
                AssignmentID = assignment.AssignmentID,
                RequestID = assignment.RequestID,
                RequestTitle = assignment.Request != null
                    ? assignment.Request.Title
                    : "Request"
            };

            return View(model);
        }



        // ===========================================================
        // REPORT ASSIGNMENT ISSUE - POST
        // ===========================================================

        // POST: Technicians/ReportIssue
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReportIssue(
            CommunityServiceProject.ViewModels.AssignmentIssueViewModel model)
        {
            if (Session["TechnicianID"] == null)
            {
                return RedirectToAction("Login");
            }

            int technicianID = (int)Session["TechnicianID"];

            var assignment = db.TechnicianAssignments
                .Include("Request")
                .FirstOrDefault(a =>
                    a.AssignmentID == model.AssignmentID &&
                    a.TechnicianID == technicianID);

            if (assignment == null)
            {
                return HttpNotFound();
            }

            if (!ModelState.IsValid)
            {
                model.RequestID = assignment.RequestID;
                model.RequestTitle = assignment.Request != null
                    ? assignment.Request.Title
                    : "Request";

                return View(model);
            }

            if (assignment.Status != AssignmentStatus.PendingAcknowledgement &&
                assignment.Status != AssignmentStatus.Acknowledged)
            {
                TempData["ErrorMessage"] =
                    "This assignment cannot currently be reported.";

                return RedirectToAction(
                    "Details",
                    new { id = assignment.RequestID }
                );
            }

            var issue = new AssignmentIssue
            {
                AssignmentID = assignment.AssignmentID,
                IssueType = model.IssueType,
                Reason = model.Reason,
                ReportedDate = System.DateTime.Now,
                Status = AssignmentIssueStatus.Open
            };

            db.AssignmentIssues.Add(issue);

            assignment.Status = AssignmentStatus.IssueReported;

            db.SaveChanges();

            TempData["SuccessMessage"] =
                "Assignment issue reported successfully.";

            return RedirectToAction(
                "Details",
                new { id = assignment.RequestID }
            );
        }

        // ===========================================================
        // REQUEST REASSIGNMENT - GET
        // ===========================================================

        // GET: Technicians/RequestReassignment/5
        public ActionResult RequestReassignment(int? id)
        {
            if (Session["TechnicianID"] == null)
            {
                return RedirectToAction("Login");
            }

            if (id == null)
            {
                return RedirectToAction("Dashboard");
            }

            int technicianID = (int)Session["TechnicianID"];

            var assignment = db.TechnicianAssignments
                .Include("Request")
                .FirstOrDefault(a =>
                    a.AssignmentID == id &&
                    a.TechnicianID == technicianID);

            if (assignment == null)
            {
                return HttpNotFound();
            }

            if (assignment.Status != AssignmentStatus.IssueReported)
            {
                TempData["ErrorMessage"] =
                    "A reassignment request can only be submitted after reporting an assignment issue.";

                return RedirectToAction(
                    "Details",
                    new { id = assignment.RequestID }
                );
            }

            var existingRequest = db.ReassignmentRequests
                .FirstOrDefault(r =>
                    r.AssignmentID == assignment.AssignmentID &&
                    r.Status == ReassignmentStatus.Pending);

            if (existingRequest != null)
            {
                TempData["ErrorMessage"] =
                    "A reassignment request has already been submitted for this assignment.";

                return RedirectToAction(
                    "Details",
                    new { id = assignment.RequestID }
                );
            }

            var model =
                new CommunityServiceProject.ViewModels.ReassignmentRequestViewModel
                {
                    AssignmentID = assignment.AssignmentID,
                    RequestID = assignment.RequestID,
                    RequestTitle = assignment.Request != null
                        ? assignment.Request.Title
                        : "Request"
                };

            return View(model);
        }


        // ===========================================================
        // REQUEST REASSIGNMENT - POST
        // ===========================================================

        // POST: Technicians/RequestReassignment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RequestReassignment(
            CommunityServiceProject.ViewModels.ReassignmentRequestViewModel model)
        {
            if (Session["TechnicianID"] == null)
            {
                return RedirectToAction("Login");
            }

            int technicianID = (int)Session["TechnicianID"];

            var assignment = db.TechnicianAssignments
                .Include("Request")
                .FirstOrDefault(a =>
                    a.AssignmentID == model.AssignmentID &&
                    a.TechnicianID == technicianID);

            if (assignment == null)
            {
                return HttpNotFound();
            }

            if (!ModelState.IsValid)
            {
                model.RequestID = assignment.RequestID;

                model.RequestTitle = assignment.Request != null
                    ? assignment.Request.Title
                    : "Request";

                return View(model);
            }

            if (assignment.Status != AssignmentStatus.IssueReported)
            {
                TempData["ErrorMessage"] =
                    "A reassignment request can only be submitted after reporting an assignment issue.";

                return RedirectToAction(
                    "Details",
                    new { id = assignment.RequestID }
                );
            }

            var existingRequest = db.ReassignmentRequests
                .FirstOrDefault(r =>
                    r.AssignmentID == assignment.AssignmentID &&
                    r.Status == ReassignmentStatus.Pending);

            if (existingRequest != null)
            {
                TempData["ErrorMessage"] =
                    "A reassignment request has already been submitted for this assignment.";

                return RedirectToAction(
                    "Details",
                    new { id = assignment.RequestID }
                );
            }

            var reassignmentRequest = new ReassignmentRequest
            {
                AssignmentID = assignment.AssignmentID,
                TechnicianID = technicianID,
                Reason = model.Reason,
                RequestedDate = System.DateTime.Now,
                Status = ReassignmentStatus.Pending
            };

            db.ReassignmentRequests.Add(reassignmentRequest);

            assignment.Status =
                AssignmentStatus.ReassignmentRequested;

            db.SaveChanges();

            TempData["SuccessMessage"] =
                "Reassignment request submitted successfully.";

            return RedirectToAction(
                "Details",
                new { id = assignment.RequestID }
            );
        }

        // ===========================================================
        // START MAINTENANCE
        // ===========================================================

        // POST: Technicians/StartMaintenance
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StartMaintenance(int id)
        {
            if (Session["TechnicianID"] == null)
            {
                return RedirectToAction("Login");
            }

            int technicianID = (int)Session["TechnicianID"];

            // Find the assignment belonging to the logged-in technician
            var assignment = db.TechnicianAssignments
                .Include("Request")
                .FirstOrDefault(a =>
                    a.AssignmentID == id &&
                    a.TechnicianID == technicianID);

            if (assignment == null)
            {
                return HttpNotFound();
            }

            // Maintenance can only start after the assignment
            // has been acknowledged.
            if (assignment.Status != AssignmentStatus.Acknowledged)
            {
                TempData["ErrorMessage"] =
                    "Maintenance can only be started after the assignment has been acknowledged.";

                return RedirectToAction(
                    "Details",
                    new { id = assignment.RequestID }
                );
            }

            var request = assignment.Request;

            if (request == null)
            {
                return HttpNotFound();
            }

            // Make sure the request still belongs to this technician.
            if (request.TechnicianID != technicianID)
            {
                TempData["ErrorMessage"] =
                    "This request is no longer assigned to you.";

                return RedirectToAction("Dashboard");
            }

            // Prevent duplicate maintenance records from being started.
            var existingMaintenance = db.MaintenanceWorks
                .FirstOrDefault(m =>
                    m.RequestID == request.RequestID &&
                    m.TechnicianID == technicianID &&
                    (m.Status == MaintenanceWorkStatus.InProgress ||
                     m.Status == MaintenanceWorkStatus.SubmittedForVerification ||
                     m.Status == MaintenanceWorkStatus.Verified));

            if (existingMaintenance != null)
            {
                TempData["ErrorMessage"] =
                    "Maintenance has already been started for this request.";

                return RedirectToAction(
                    "Details",
                    new { id = request.RequestID }
                );
            }

            // Create the maintenance work record.
            var maintenanceWork = new MaintenanceWork
            {
                RequestID = request.RequestID,
                TechnicianID = technicianID,
                StartedDate = DateTime.Now,
                Status = MaintenanceWorkStatus.InProgress,
                ProgressPercentage = 0,
                CurrentActivity = "Maintenance started"
            };

            db.MaintenanceWorks.Add(maintenanceWork);

            // Update the request lifecycle.
            request.Status = RequestStatus.InProgress;

            db.SaveChanges();

            TempData["SuccessMessage"] =
                "Maintenance started successfully.";

            return RedirectToAction(
                "Details",
                new { id = request.RequestID }
            );
        }

        // ===========================================================
        // UPDATE MAINTENANCE PROGRESS - GET
        // ===========================================================

        // GET: Technicians/UpdateProgress/5
        public ActionResult UpdateProgress(int? id)
        {
            if (Session["TechnicianID"] == null)
            {
                return RedirectToAction("Login");
            }

            if (id == null)
            {
                return RedirectToAction("Dashboard");
            }

            int technicianID = (int)Session["TechnicianID"];

            var maintenanceWork = db.MaintenanceWorks
                .Include("Request")
                .FirstOrDefault(m =>
                    m.MaintenanceWorkID == id &&
                    m.TechnicianID == technicianID);

            if (maintenanceWork == null)
            {
                return HttpNotFound();
            }

            if (maintenanceWork.Status != MaintenanceWorkStatus.InProgress)
            {
                TempData["ErrorMessage"] =
                    "Maintenance progress can only be updated while maintenance is in progress.";

                return RedirectToAction(
                    "Details",
                    new { id = maintenanceWork.RequestID }
                );
            }

            // Progress cannot be updated after reaching 100%.
            if (maintenanceWork.ProgressPercentage >= 100)
            {
                TempData["ErrorMessage"] =
                    "Maintenance progress is already 100% complete.";

                return RedirectToAction(
                    "Details",
                    new { id = maintenanceWork.RequestID }
                );
            }

            // Before-maintenance evidence is required for the first
            // progress update.
            bool hasBeforeEvidence = db.MaintenanceEvidence.Any(e =>
                e.MaintenanceWorkID == maintenanceWork.MaintenanceWorkID &&
                e.EvidenceType == EvidenceType.BeforeMaintenance);

            if (maintenanceWork.ProgressPercentage == 0 && !hasBeforeEvidence)
            {
                TempData["ErrorMessage"] =
                    "Please upload before-maintenance evidence before recording maintenance progress.";

                return RedirectToAction(
                    "Details",
                    new { id = maintenanceWork.RequestID }
                );
            }

            // Once progress has already been recorded, progress evidence
            // must exist before another progress update can be made.
            bool hasProgressEvidence = db.MaintenanceEvidence.Any(e =>
                e.MaintenanceWorkID == maintenanceWork.MaintenanceWorkID &&
                e.EvidenceType == EvidenceType.Progress);

            if (maintenanceWork.ProgressPercentage > 0 && !hasProgressEvidence)
            {
                TempData["ErrorMessage"] =
                    "Please upload progress evidence before recording another maintenance progress update.";

                return RedirectToAction(
                    "Details",
                    new { id = maintenanceWork.RequestID }
                );
            }

            return View(maintenanceWork);
        }



        // ===========================================================
        // UPDATE MAINTENANCE PROGRESS - POST
        // ===========================================================

        // POST: Technicians/UpdateProgress
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProgress(
            int id,
            int progressPercentage,
            string currentActivity)
              

        {
            if (Session["TechnicianID"] == null)
            {
                return RedirectToAction("Login");
            }

            int technicianID = (int)Session["TechnicianID"];

            var maintenanceWork = db.MaintenanceWorks
                .FirstOrDefault(m =>
                    m.MaintenanceWorkID == id &&
                    m.TechnicianID == technicianID);

            if (maintenanceWork == null)
            {
                return HttpNotFound();
            }

            if (maintenanceWork.Status != MaintenanceWorkStatus.InProgress)
            {
                TempData["ErrorMessage"] =
                    "Maintenance progress can only be updated while maintenance is in progress.";

                return RedirectToAction(
                    "Details",
                    new { id = maintenanceWork.RequestID }
                );
            }

            // Do not allow progress to be changed once it is complete.
            if (maintenanceWork.ProgressPercentage >= 100)
            {
                TempData["ErrorMessage"] =
                    "Maintenance progress is already 100% complete.";

                return RedirectToAction(
                    "Details",
                    new { id = maintenanceWork.RequestID }
                );
            }

            // ===========================================================
            // EVIDENCE VALIDATION
            // ===========================================================

            bool hasBeforeEvidence = db.MaintenanceEvidence.Any(e =>
                e.MaintenanceWorkID == maintenanceWork.MaintenanceWorkID &&
                e.EvidenceType == EvidenceType.BeforeMaintenance);

            bool hasProgressEvidence = db.MaintenanceEvidence.Any(e =>
                e.MaintenanceWorkID == maintenanceWork.MaintenanceWorkID &&
                e.EvidenceType == EvidenceType.Progress);

            // First progress update requires before-maintenance evidence.
            if (maintenanceWork.ProgressPercentage == 0 &&
                !hasBeforeEvidence)
            {
                TempData["ErrorMessage"] =
                    "Please upload before-maintenance evidence before updating maintenance progress.";

                return RedirectToAction(
                    "Details",
                    new { id = maintenanceWork.RequestID }
                );
            }

            // 100% completion requires documented maintenance work.
            if (progressPercentage >= 100)
            {
                bool hasWorkNote = db.WorkNotes.Any(w =>
                    w.MaintenanceWorkID == maintenanceWork.MaintenanceWorkID);

                if (!hasWorkNote)
                {
                    ModelState.AddModelError(
                        "progressPercentage",
                        "At least one work note must be recorded before maintenance can reach 100%."
                    );
                }

                if (!hasProgressEvidence)
                {
                    ModelState.AddModelError(
                        "progressPercentage",
                        "Progress evidence must be uploaded before maintenance can reach 100%."
                    );
                }
            }


            // Any progress update after the first one requires
            // progress evidence.
            if (maintenanceWork.ProgressPercentage > 0 &&
                !hasProgressEvidence)
            {
                TempData["ErrorMessage"] =
                    "Please upload progress evidence before updating maintenance progress again.";

                return RedirectToAction(
                    "Details",
                    new { id = maintenanceWork.RequestID }
                );
            }

            // ===========================================================
            // INPUT VALIDATION
            // ===========================================================

            if (progressPercentage < 0 || progressPercentage > 100)
            {
                ModelState.AddModelError(
                    "progressPercentage",
                    "Progress must be between 0 and 100."
                );
            }

            // Prevent the technician from moving backwards.
            if (progressPercentage < maintenanceWork.ProgressPercentage)
            {
                ModelState.AddModelError(
                    "progressPercentage",
                    "Progress cannot be reduced from the current progress percentage."
                );
            }

            if (string.IsNullOrWhiteSpace(currentActivity))
            {
                ModelState.AddModelError(
                    "currentActivity",
                    "Please describe the current maintenance activity."
                );
            }

            if (currentActivity != null &&
                currentActivity.Length > 500)
            {
                ModelState.AddModelError(
                    "currentActivity",
                    "The current activity cannot exceed 500 characters."
                );
            }



            if (!ModelState.IsValid)
            {
                return View(maintenanceWork);
            }

            // ===========================================================
            // UPDATE MAINTENANCE STATE
            // ===========================================================

            maintenanceWork.ProgressPercentage = progressPercentage;
            maintenanceWork.CurrentActivity = currentActivity.Trim();

            // Preserve this update in progress history.
            var progress = new MaintenanceProgress
            {
                MaintenanceWorkID = maintenanceWork.MaintenanceWorkID,
                ProgressPercentage = progressPercentage,
                CurrentActivity = currentActivity.Trim(),
                RecordedDate = DateTime.Now
            };

            db.MaintenanceProgress.Add(progress);

            db.SaveChanges();

            TempData["SuccessMessage"] =
                "Maintenance progress updated successfully.";

            return RedirectToAction(
                "Details",
                new { id = maintenanceWork.RequestID }
            );
        }

        [HttpGet]
        public ActionResult AddWorkNote(int? id)
        {
            if (Session["TechnicianID"] == null)
            {
                return RedirectToAction("Login");
            }

            if (id == null)
            {
                return RedirectToAction("Dashboard");
            }

            int technicianID = (int)Session["TechnicianID"];

            var maintenanceWork = db.MaintenanceWorks
                .Include("Request")
                .FirstOrDefault(m =>
                    m.MaintenanceWorkID == id.Value &&
                    m.TechnicianID == technicianID);

            if (maintenanceWork == null)
            {
                return HttpNotFound();
            }

            if (maintenanceWork.Status != MaintenanceWorkStatus.InProgress)
            {
                TempData["Error"] =
                    "Work notes can only be added while maintenance is in progress.";

                return RedirectToAction(
                    "Details",
                    new { id = maintenanceWork.RequestID }
                );
            }

            return View(maintenanceWork);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddWorkNote(int id, string noteText)
        {
            if (Session["TechnicianID"] == null)
            {
                return RedirectToAction("Login");
            }

            int technicianID = (int)Session["TechnicianID"];

            var maintenanceWork = db.MaintenanceWorks
                .Include("Request")
                .FirstOrDefault(m =>
                    m.MaintenanceWorkID == id &&
                    m.TechnicianID == technicianID);

            if (maintenanceWork == null)
            {
                return HttpNotFound();
            }

            if (maintenanceWork.Status != MaintenanceWorkStatus.InProgress)
            {
                TempData["Error"] = "Work notes can only be added while maintenance is in progress.";
                return RedirectToAction("Details", new { id = maintenanceWork.RequestID });
            }

            if (string.IsNullOrWhiteSpace(noteText))
            {
                ModelState.AddModelError("noteText", "Please enter a work note.");
            }

            if (noteText != null && noteText.Length > 1000)
            {
                ModelState.AddModelError("noteText", "Work notes cannot exceed 1000 characters.");
            }

            if (!ModelState.IsValid)
            {
                return View(maintenanceWork);
            }

            var workNote = new WorkNote
            {
                MaintenanceWorkID = maintenanceWork.MaintenanceWorkID,
                NoteText = noteText.Trim(),
                CreatedDate = DateTime.Now
            };

            db.WorkNotes.Add(workNote);
            db.SaveChanges();

            TempData["Success"] = "Work note added successfully.";

            return RedirectToAction("Details", new { id = maintenanceWork.RequestID });
        }

        [HttpGet]
        public ActionResult AddMaterial(int? id)
        {
            if (Session["TechnicianID"] == null)
            {
                return RedirectToAction("Login");
            }

            if (id == null)
            {
                return RedirectToAction("Dashboard");
            }

            int technicianID = (int)Session["TechnicianID"];

            var maintenanceWork = db.MaintenanceWorks
                .Include("Request")
                .Include("Materials")
                .FirstOrDefault(m =>
                    m.MaintenanceWorkID == id.Value &&
                    m.TechnicianID == technicianID);

            if (maintenanceWork == null)
            {
                return HttpNotFound();
            }

            if (maintenanceWork.Status != MaintenanceWorkStatus.InProgress)
            {
                TempData["Error"] =
                    "Materials can only be recorded while maintenance is in progress.";

                return RedirectToAction(
                    "Details",
                    new { id = maintenanceWork.RequestID });
            }

            return View(maintenanceWork);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddMaterial(
    int id,
    string materialName,
    int quantity,
    string unit)
        {
            if (Session["TechnicianID"] == null)
            {
                return RedirectToAction("Login");
            }

            int technicianID = (int)Session["TechnicianID"];

            var maintenanceWork = db.MaintenanceWorks
                .Include("Request")
                .FirstOrDefault(m =>
                    m.MaintenanceWorkID == id &&
                    m.TechnicianID == technicianID);

            if (maintenanceWork == null)
            {
                return HttpNotFound();
            }

            if (maintenanceWork.Status != MaintenanceWorkStatus.InProgress)
            {
                TempData["Error"] =
                    "Materials can only be recorded while maintenance is in progress.";

                return RedirectToAction(
                    "Details",
                    new { id = maintenanceWork.RequestID });
            }

            if (string.IsNullOrWhiteSpace(materialName))
            {
                ModelState.AddModelError(
                    "materialName",
                    "Please enter the material name.");
            }

            if (materialName != null && materialName.Length > 100)
            {
                ModelState.AddModelError(
                    "materialName",
                    "Material name cannot exceed 100 characters.");
            }

            if (quantity <= 0)
            {
                ModelState.AddModelError(
                    "quantity",
                    "Quantity must be greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(unit))
            {
                ModelState.AddModelError(
                    "unit",
                    "Please enter the unit.");
            }

            if (unit != null && unit.Length > 30)
            {
                ModelState.AddModelError(
                    "unit",
                    "Unit cannot exceed 30 characters.");
            }

            if (!ModelState.IsValid)
            {
                return View(maintenanceWork);
            }

            var material = new MaintenanceMaterial
            {
                MaintenanceWorkID = maintenanceWork.MaintenanceWorkID,
                MaterialName = materialName.Trim(),
                Quantity = quantity,
                Unit = unit.Trim(),
                RecordedDate = DateTime.Now
            };

            db.MaintenanceMaterials.Add(material);
            db.SaveChanges();

            TempData["Success"] = "Material recorded successfully.";

            return RedirectToAction(
                "Details",
                new { id = maintenanceWork.RequestID });
        }

        [HttpGet]
        public ActionResult UploadEvidence(int? id)
        {
            if (Session["TechnicianID"] == null)
            {
                return RedirectToAction("Login");
            }

            if (id == null)
            {
                return RedirectToAction("Dashboard");
            }

            int technicianID = (int)Session["TechnicianID"];

            var maintenanceWork = db.MaintenanceWorks
                .Include("Request")
                .Include("Evidence")
                .FirstOrDefault(m =>
                    m.MaintenanceWorkID == id.Value &&
                    m.TechnicianID == technicianID);

            if (maintenanceWork == null)
            {
                return HttpNotFound();
            }

            if (maintenanceWork.Status != MaintenanceWorkStatus.InProgress)
            {
                TempData["Error"] =
                    "Evidence can only be uploaded while maintenance is in progress.";

                return RedirectToAction(
                    "Details",
                    new { id = maintenanceWork.RequestID });
            }

            return View(maintenanceWork);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadEvidence(
     int id,
     HttpPostedFileBase evidenceFile,
     string description)
        {
            if (Session["TechnicianID"] == null)
            {
                return RedirectToAction("Login");
            }

            int technicianID = (int)Session["TechnicianID"];

            var maintenanceWork = db.MaintenanceWorks
                .Include("Request")
                .FirstOrDefault(m =>
                    m.MaintenanceWorkID == id &&
                    m.TechnicianID == technicianID);

            if (maintenanceWork == null)
            {
                return HttpNotFound();
            }

            if (maintenanceWork.Status != MaintenanceWorkStatus.InProgress)
            {
                TempData["Error"] =
                    "Evidence can only be uploaded while maintenance is in progress.";

                return RedirectToAction(
                    "Details",
                    new { id = maintenanceWork.RequestID });
            }

            if (evidenceFile == null || evidenceFile.ContentLength == 0)
            {
                ModelState.AddModelError(
                    "evidenceFile",
                    "Please select an evidence file.");
            }

            if (!string.IsNullOrWhiteSpace(description) &&
                description.Length > 500)
            {
                ModelState.AddModelError(
                    "description",
                    "Description cannot exceed 500 characters.");
            }

            if (!ModelState.IsValid)
            {
                return View(maintenanceWork);
            }

            string uploadsFolder = Server.MapPath("~/Uploads/MaintenanceEvidence");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string fileExtension =
                Path.GetExtension(evidenceFile.FileName);

            string fileName =
                Guid.NewGuid().ToString() + fileExtension;

            string filePath =
                Path.Combine(uploadsFolder, fileName);

            evidenceFile.SaveAs(filePath);

            var evidence = new MaintenanceEvidence
            {
                MaintenanceWorkID = maintenanceWork.MaintenanceWorkID,
                EvidenceType = EvidenceType.BeforeMaintenance,
                FilePath = "~/Uploads/MaintenanceEvidence/" + fileName,
                Description = string.IsNullOrWhiteSpace(description)
                    ? null
                    : description.Trim(),
                UploadedDate = DateTime.Now
            };

            db.MaintenanceEvidence.Add(evidence);
            db.SaveChanges();

            TempData["Success"] =
                "Before-maintenance evidence uploaded successfully.";

            return RedirectToAction(
                "Details",
                new { id = maintenanceWork.RequestID });
        }

        [HttpGet]
        public ActionResult UploadProgressEvidence(int? id)
        {
            if (Session["TechnicianID"] == null)
            {
                return RedirectToAction("Login");
            }

            if (id == null)
            {
                return RedirectToAction("Dashboard");
            }

            int technicianID = (int)Session["TechnicianID"];

            var maintenanceWork = db.MaintenanceWorks
                .Include("Request")
                .Include("Evidence")
                .FirstOrDefault(m =>
                    m.MaintenanceWorkID == id.Value &&
                    m.TechnicianID == technicianID);

            if (maintenanceWork == null)
            {
                return HttpNotFound();
            }

            if (maintenanceWork.Status != MaintenanceWorkStatus.InProgress)
            {
                TempData["Error"] =
                    "Progress evidence can only be uploaded while maintenance is in progress.";

                return RedirectToAction(
                    "Details",
                    new { id = maintenanceWork.RequestID });
            }

            return View(maintenanceWork);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadProgressEvidence(
    int id,
    HttpPostedFileBase evidenceFile,
    string description)
        {
            if (Session["TechnicianID"] == null)
            {
                return RedirectToAction("Login");
            }

            int technicianID = (int)Session["TechnicianID"];

            var maintenanceWork = db.MaintenanceWorks
                .Include("Request")
                .FirstOrDefault(m =>
                    m.MaintenanceWorkID == id &&
                    m.TechnicianID == technicianID);

            if (maintenanceWork == null)
            {
                return HttpNotFound();
            }

            if (maintenanceWork.Status != MaintenanceWorkStatus.InProgress)
            {
                TempData["Error"] =
                    "Progress evidence can only be uploaded while maintenance is in progress.";

                return RedirectToAction(
                    "Details",
                    new { id = maintenanceWork.RequestID });
            }

            if (evidenceFile == null || evidenceFile.ContentLength == 0)
            {
                ModelState.AddModelError(
                    "evidenceFile",
                    "Please select an evidence file.");
            }

            if (!string.IsNullOrWhiteSpace(description) &&
                description.Length > 500)
            {
                ModelState.AddModelError(
                    "description",
                    "Description cannot exceed 500 characters.");
            }

            if (!ModelState.IsValid)
            {
                return View(maintenanceWork);
            }

            string uploadsFolder =
                Server.MapPath("~/Uploads/MaintenanceEvidence");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string fileExtension =
                Path.GetExtension(evidenceFile.FileName);

            string fileName =
                Guid.NewGuid().ToString() + fileExtension;

            string filePath =
                Path.Combine(uploadsFolder, fileName);

            evidenceFile.SaveAs(filePath);

            var evidence = new MaintenanceEvidence
            {
                MaintenanceWorkID =
                    maintenanceWork.MaintenanceWorkID,

                EvidenceType =
                    EvidenceType.Progress,

                FilePath =
                    "~/Uploads/MaintenanceEvidence/" + fileName,

                Description =
                    string.IsNullOrWhiteSpace(description)
                        ? null
                        : description.Trim(),

                UploadedDate = DateTime.Now
            };

            db.MaintenanceEvidence.Add(evidence);
            db.SaveChanges();

            TempData["Success"] =
                "Progress evidence uploaded successfully.";

            return RedirectToAction(
                "Details",
                new { id = maintenanceWork.RequestID });
        }

        [HttpGet]
        public ActionResult UploadCompletionEvidence(int? id)
        {
            if (Session["TechnicianID"] == null)
            {
                return RedirectToAction("Login");
            }

            if (id == null)
            {
                return RedirectToAction("Dashboard");
            }

            int technicianID = (int)Session["TechnicianID"];

            var maintenanceWork = db.MaintenanceWorks
                .Include("Request")
                .Include("Evidence")
                .FirstOrDefault(m =>
                    m.MaintenanceWorkID == id.Value &&
                    m.TechnicianID == technicianID);

            if (maintenanceWork == null)
            {
                return HttpNotFound();
            }

            if (maintenanceWork.Status != MaintenanceWorkStatus.InProgress)
            {
                TempData["Error"] =
                    "Completion evidence can only be uploaded while maintenance is in progress.";

                return RedirectToAction(
                    "Details",
                    new { id = maintenanceWork.RequestID });
            }

            if (maintenanceWork.ProgressPercentage < 100)
            {
                TempData["Error"] =
                    "Completion evidence can only be uploaded after maintenance progress reaches 100%.";

                return RedirectToAction(
                    "Details",
                    new { id = maintenanceWork.RequestID });
            }

            return View(maintenanceWork);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadCompletionEvidence(
    int id,
    HttpPostedFileBase evidenceFile,
    string description)
        {
            if (Session["TechnicianID"] == null)
            {
                return RedirectToAction("Login");
            }

            int technicianID = (int)Session["TechnicianID"];

            var maintenanceWork = db.MaintenanceWorks
                .Include("Request")
                .FirstOrDefault(m =>
                    m.MaintenanceWorkID == id &&
                    m.TechnicianID == technicianID);

            if (maintenanceWork == null)
            {
                return HttpNotFound();
            }

            if (maintenanceWork.Status != MaintenanceWorkStatus.InProgress)
            {
                TempData["Error"] =
                    "Completion evidence can only be uploaded while maintenance is in progress.";

                return RedirectToAction(
                    "Details",
                    new { id = maintenanceWork.RequestID });
            }

            // Completion evidence is only allowed at 100%.
            if (maintenanceWork.ProgressPercentage < 100)
            {
                TempData["Error"] =
                    "Maintenance must reach 100% progress before completion evidence can be uploaded.";

                return RedirectToAction(
                    "Details",
                    new { id = maintenanceWork.RequestID });
            }

            if (evidenceFile == null || evidenceFile.ContentLength == 0)
            {
                ModelState.AddModelError(
                    "evidenceFile",
                    "Completion evidence is required. Please select a file.");
            }

            // Completion evidence must contain a meaningful description.
            if (string.IsNullOrWhiteSpace(description))
            {
                ModelState.AddModelError(
                    "description",
                    "Please describe what the completion evidence shows.");
            }
            else if (description.Trim().Length < 10)
            {
                ModelState.AddModelError(
                    "description",
                    "Please provide a meaningful completion evidence description.");
            }
            else if (description.Length > 500)
            {
                ModelState.AddModelError(
                    "description",
                    "Description cannot exceed 500 characters.");
            }

            if (!ModelState.IsValid)
            {
                return View(maintenanceWork);
            }

            string uploadsFolder =
                Server.MapPath("~/Uploads/MaintenanceEvidence");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string fileExtension =
                Path.GetExtension(evidenceFile.FileName);

            string fileName =
                Guid.NewGuid().ToString() + fileExtension;

            string filePath =
                Path.Combine(uploadsFolder, fileName);

            evidenceFile.SaveAs(filePath);

            var evidence = new MaintenanceEvidence
            {
                MaintenanceWorkID =
                    maintenanceWork.MaintenanceWorkID,

                EvidenceType =
                    EvidenceType.Completion,

                FilePath =
                    "~/Uploads/MaintenanceEvidence/" + fileName,

                Description =
                    description.Trim(),

                UploadedDate = DateTime.Now
            };

            db.MaintenanceEvidence.Add(evidence);

            db.SaveChanges();

            TempData["Success"] =
                "Completion evidence uploaded successfully.";

            return RedirectToAction(
                "Details",
                new { id = maintenanceWork.RequestID });
        }



        [HttpGet]
        public ActionResult SubmitCompletion(int? id)
        {
            if (Session["TechnicianID"] == null)
            {
                return RedirectToAction("Login");
            }

            if (id == null)
            {
                return RedirectToAction("Dashboard");
            }

            int technicianID = (int)Session["TechnicianID"];

            var maintenanceWork = db.MaintenanceWorks
                .Include("Request")
                .Include("Completions")
                .FirstOrDefault(m =>
                    m.MaintenanceWorkID == id.Value &&
                    m.TechnicianID == technicianID);

            if (maintenanceWork == null)
            {
                return HttpNotFound();
            }

            if (maintenanceWork.Status != MaintenanceWorkStatus.InProgress)
            {
                TempData["Error"] =
                    "Completion can only be submitted while maintenance is in progress.";

                return RedirectToAction(
                    "Details",
                    new { id = maintenanceWork.RequestID });
            }

            if (maintenanceWork.ProgressPercentage < 100)
            {
                TempData["Error"] =
                    "Maintenance must reach 100% progress before completion can be submitted.";

                return RedirectToAction(
                    "Details",
                    new { id = maintenanceWork.RequestID });
            }

            bool hasCompletionEvidence = db.MaintenanceEvidence.Any(e =>
                e.MaintenanceWorkID == maintenanceWork.MaintenanceWorkID &&
                e.EvidenceType == EvidenceType.Completion);

            if (!hasCompletionEvidence)
            {
                TempData["Error"] =
                    "Please upload completion evidence before submitting the maintenance work.";

                return RedirectToAction(
                    "Details",
                    new { id = maintenanceWork.RequestID });
            }

            return View(maintenanceWork);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitCompletion(
    int id,
    string maintenanceSummary,
    string resolutionAction)
        {
            if (Session["TechnicianID"] == null)
            {
                return RedirectToAction("Login");
            }

            int technicianID = (int)Session["TechnicianID"];

            var maintenanceWork = db.MaintenanceWorks
                .Include("Request")
                .FirstOrDefault(m =>
                    m.MaintenanceWorkID == id &&
                    m.TechnicianID == technicianID);

            if (maintenanceWork == null)
            {
                return HttpNotFound();
            }

            if (maintenanceWork.Status != MaintenanceWorkStatus.InProgress)
            {
                TempData["Error"] =
                    "Completion can only be submitted while maintenance is in progress.";

                return RedirectToAction(
                    "Details",
                    new { id = maintenanceWork.RequestID });
            }

            // ===========================================================
            // COMPLETION REQUIREMENTS
            // ===========================================================

            if (maintenanceWork.ProgressPercentage < 100)
            {
                TempData["Error"] =
                    "Maintenance must reach 100% progress before completion can be submitted.";

                return RedirectToAction(
                    "Details",
                    new { id = maintenanceWork.RequestID });
            }

            bool hasCompletionEvidence = db.MaintenanceEvidence.Any(e =>
                e.MaintenanceWorkID == maintenanceWork.MaintenanceWorkID &&
                e.EvidenceType == EvidenceType.Completion);

            if (!hasCompletionEvidence)
            {
                TempData["Error"] =
                    "Please upload completion evidence before submitting the maintenance work.";

                return RedirectToAction(
                    "Details",
                    new { id = maintenanceWork.RequestID });
            }

            // ===========================================================
            // TEXT VALIDATION
            // ===========================================================

            if (string.IsNullOrWhiteSpace(maintenanceSummary))
            {
                ModelState.AddModelError(
                    "maintenanceSummary",
                    "Please provide a maintenance summary.");
            }
            else if (maintenanceSummary.Trim().Length < 10)
            {
                ModelState.AddModelError(
                    "maintenanceSummary",
                    "Please provide a meaningful maintenance summary.");
            }
            else if (maintenanceSummary.Length > 2000)
            {
                ModelState.AddModelError(
                    "maintenanceSummary",
                    "Maintenance summary cannot exceed 2000 characters.");
            }

            if (string.IsNullOrWhiteSpace(resolutionAction))
            {
                ModelState.AddModelError(
                    "resolutionAction",
                    "Please describe the resolution action.");
            }
            else if (resolutionAction.Trim().Length < 10)
            {
                ModelState.AddModelError(
                    "resolutionAction",
                    "Please provide a meaningful description of the resolution action.");
            }
            else if (resolutionAction.Length > 2000)
            {
                ModelState.AddModelError(
                    "resolutionAction",
                    "Resolution action cannot exceed 2000 characters.");
            }

            if (!ModelState.IsValid)
            {
                return View(maintenanceWork);
            }

            // ===========================================================
            // CREATE COMPLETION SUBMISSION
            // ===========================================================

            var completion = new MaintenanceCompletion
            {
                MaintenanceWorkID =
                    maintenanceWork.MaintenanceWorkID,

                MaintenanceSummary =
                    maintenanceSummary.Trim(),

                ResolutionAction =
                    resolutionAction.Trim(),

                SubmittedDate = DateTime.Now,

                VerificationStatus =
                    CompletionVerificationStatus.Pending
            };

            db.MaintenanceCompletions.Add(completion);

            maintenanceWork.Status =
                MaintenanceWorkStatus.SubmittedForVerification;

            maintenanceWork.CompletedDate = DateTime.Now;

            db.SaveChanges();

            TempData["Success"] =
                "Maintenance completion submitted successfully for verification.";

            return RedirectToAction(
                "Details",
                new { id = maintenanceWork.RequestID });
        }


        // ===========================================================
        // LOGOUT
        // ===========================================================

        // GET: Technicians/Logout
        public ActionResult Logout()
        {
            Session["TechnicianID"] = null;
            Session["TechnicianName"] = null;

            Session.Clear();

            return RedirectToAction("Login");
        }


        // ===========================================================
        // DISPOSE
        // ===========================================================

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