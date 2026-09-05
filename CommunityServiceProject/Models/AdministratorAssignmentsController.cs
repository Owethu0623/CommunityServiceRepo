using System;
using System.Linq;
using System.Web.Mvc;
using CommunityServiceProject.Models;
using CommunityServiceProject.ViewModels;

namespace CommunityServiceProject.Controllers
{
    public class AdministratorAssignmentsController : Controller
    {
        private Community db = new Community();

        // ===========================================================
        // REASSIGNMENT REQUESTS - GET
        // ===========================================================

        // GET: AdministratorAssignments/ReassignmentRequests
        public ActionResult ReassignmentRequests()
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            var requests = db.ReassignmentRequests
                .Include("Assignment")
                .Include("Assignment.Request")
                .Include("Assignment.Request.Category")
                .Include("Assignment.Request.Ward")
                .Include("Assignment.Technician")
                .Where(r => r.Status == ReassignmentStatus.Pending)
                .OrderByDescending(r => r.RequestedDate)
                .ToList();

            return View(requests);
        }

        // ===========================================================
        // REVIEW REASSIGNMENT REQUEST - GET
        // ===========================================================

        // GET: AdministratorAssignments/ReviewReassignment/5
        public ActionResult ReviewReassignment(int? id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            if (id == null)
            {
                return RedirectToAction("ReassignmentRequests");
            }

            var reassignmentRequest = db.ReassignmentRequests
                .Include("Assignment")
                .Include("Assignment.Request")
                .Include("Assignment.Request.Category")
                .Include("Assignment.Request.Ward")
                .Include("Assignment.Technician")
                .Include("Assignment.AssignmentIssues")
                .FirstOrDefault(r =>
                    r.ReassignmentRequestID == id.Value);

            if (reassignmentRequest == null)
            {
                return HttpNotFound();
            }

            // Only pending reassignment requests can be reviewed.
            if (reassignmentRequest.Status != ReassignmentStatus.Pending)
            {
                TempData["ErrorMessage"] =
                    "This reassignment request is no longer pending.";

                return RedirectToAction("ReassignmentRequests");
            }

            return View(reassignmentRequest);
        }

        // ===========================================================
        // REJECT REASSIGNMENT REQUEST - POST
        // ===========================================================

        // POST: AdministratorAssignments/RejectReassignment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RejectReassignment(
            int id,
            string administratorResponse)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            var reassignmentRequest = db.ReassignmentRequests
                .Include("Assignment")
                .FirstOrDefault(r =>
                    r.ReassignmentRequestID == id);

            if (reassignmentRequest == null)
            {
                return HttpNotFound();
            }

            // Only pending requests can be rejected.
            if (reassignmentRequest.Status != ReassignmentStatus.Pending)
            {
                TempData["ErrorMessage"] =
                    "This reassignment request is no longer pending.";

                return RedirectToAction("ReassignmentRequests");
            }

            // Administrator response is required.
            if (string.IsNullOrWhiteSpace(administratorResponse))
            {
                TempData["ErrorMessage"] =
                    "Please provide a reason for rejecting the reassignment request.";

                return RedirectToAction(
                    "ReviewReassignment",
                    new { id = id });
            }

            if (administratorResponse.Trim().Length > 1000)
            {
                TempData["ErrorMessage"] =
                    "Administrator response cannot exceed 1000 characters.";

                return RedirectToAction(
                    "ReviewReassignment",
                    new { id = id });
            }

            int administratorID =
                (int)Session["AdministratorID"];

            // -------------------------------------------------------
            // Update reassignment request
            // -------------------------------------------------------

            reassignmentRequest.Status =
                ReassignmentStatus.Rejected;

            reassignmentRequest.ReviewedByAdministratorID =
                administratorID;

            reassignmentRequest.ReviewedDate =
                DateTime.Now;

            reassignmentRequest.AdministratorResponse =
                administratorResponse.Trim();


            // -------------------------------------------------------
            // Restore the original assignment
            // -------------------------------------------------------

            if (reassignmentRequest.Assignment != null)
            {
                reassignmentRequest.Assignment.Status =
                    AssignmentStatus.Acknowledged;
            }

            db.SaveChanges();

            TempData["SuccessMessage"] =
                "Reassignment request rejected successfully.";

            return RedirectToAction("ReassignmentRequests");
        }

        // ===========================================================
        // APPROVE REASSIGNMENT - GET
        // ===========================================================

        // GET: AdministratorAssignments/ApproveReassignment/5
        public ActionResult ApproveReassignment(int? id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            if (id == null)
            {
                return RedirectToAction("ReassignmentRequests");
            }

            var reassignmentRequest = db.ReassignmentRequests
                .Include("Assignment")
                .Include("Assignment.Request")
                .Include("Assignment.Request.Category")
                .Include("Assignment.Request.Ward")
                .Include("Assignment.Technician")
                .FirstOrDefault(r =>
                    r.ReassignmentRequestID == id.Value);

            if (reassignmentRequest == null)
            {
                return HttpNotFound();
            }

            // Only pending reassignment requests can be approved.
            if (reassignmentRequest.Status != ReassignmentStatus.Pending)
            {
                TempData["ErrorMessage"] =
                    "This reassignment request is no longer pending.";

                return RedirectToAction("ReassignmentRequests");
            }

            // Get active technicians who can receive the reassignment.
            var currentTechnicianID = reassignmentRequest.TechnicianID;

            var technicians = db.Technicians
    .Where(t =>
        t.TechnicianID != currentTechnicianID)
    .OrderBy(t => t.FirstName)
    .ThenBy(t => t.LastName)
    .ToList();

            var model = new ReassignmentApprovalViewModel
            {
                ReassignmentRequestID =
                    reassignmentRequest.ReassignmentRequestID,

                RequestID =
                    reassignmentRequest.Assignment.RequestID,

                RequestTitle =
                    reassignmentRequest.Assignment.Request.Title,

                CategoryName =
                    reassignmentRequest.Assignment.Request.Category != null
                        ? reassignmentRequest.Assignment.Request.Category.CategoryName
                        : "Unknown",

                WardName =
                    reassignmentRequest.Assignment.Request.Ward != null
                        ? reassignmentRequest.Assignment.Request.Ward.WardName
                        : "Unknown",

                Priority =
                    reassignmentRequest.Assignment.Request.Priority.ToString(),

                CurrentTechnicianName =
                    reassignmentRequest.Technician != null
                        ? reassignmentRequest.Technician.FirstName + " " +
                          reassignmentRequest.Technician.LastName
                        : "Unknown",

                Reason =
                    reassignmentRequest.Reason,

                RequestedDate =
                    reassignmentRequest.RequestedDate,

                AvailableTechnicians =
                    technicians.Select(t => new SelectListItem
                    {
                        Value = t.TechnicianID.ToString(),

                        Text = t.FirstName + " " + t.LastName
                    }),

                AdministratorResponse = ""
            };

            return View(model);
        }

        // ===========================================================
        // APPROVE REASSIGNMENT - POST
        // ===========================================================

        // POST: AdministratorAssignments/ApproveReassignment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApproveReassignment(
            ReassignmentApprovalViewModel model)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            if (!ModelState.IsValid)
            {
                // Reload replacement technicians if validation fails.
                var currentRequest = db.ReassignmentRequests
                    .FirstOrDefault(r =>
                        r.ReassignmentRequestID == model.ReassignmentRequestID);

                int currentTechnicianID =
                    currentRequest != null
                        ? currentRequest.TechnicianID
                        : 0;

                var technicians = db.Technicians
                    .Where(t =>
                        t.TechnicianID != currentTechnicianID)
                    .OrderBy(t => t.FirstName)
                    .ThenBy(t => t.LastName)
                    .ToList();

                model.AvailableTechnicians =
                    technicians.Select(t => new SelectListItem
                    {
                        Value = t.TechnicianID.ToString(),
                        Text = t.FirstName + " " + t.LastName
                    });

                return View(model);
            }

            var reassignmentRequest = db.ReassignmentRequests
                .Include("Assignment")
                .Include("Assignment.Request")
                .Include("Assignment.Technician")
                .FirstOrDefault(r =>
                    r.ReassignmentRequestID ==
                    model.ReassignmentRequestID);

            if (reassignmentRequest == null)
            {
                return HttpNotFound();
            }

            // Only pending reassignment requests can be approved.
            if (reassignmentRequest.Status != ReassignmentStatus.Pending)
            {
                TempData["ErrorMessage"] =
                    "This reassignment request is no longer pending.";

                return RedirectToAction("ReassignmentRequests");
            }

            if (reassignmentRequest.Assignment == null)
            {
                TempData["ErrorMessage"] =
                    "The original assignment could not be found.";

                return RedirectToAction("ReassignmentRequests");
            }

            if (model.ReplacementTechnicianID == null)
            {
                TempData["ErrorMessage"] =
                    "Please select a replacement technician.";

                return RedirectToAction(
                    "ApproveReassignment",
                    new { id = model.ReassignmentRequestID });
            }

            int replacementTechnicianID =
                model.ReplacementTechnicianID.Value;

            // The replacement technician cannot be the current technician.
            if (replacementTechnicianID ==
                reassignmentRequest.TechnicianID)
            {
                TempData["ErrorMessage"] =
                    "The replacement technician must be different from the current technician.";

                return RedirectToAction(
                    "ApproveReassignment",
                    new { id = model.ReassignmentRequestID });
            }

            // Check that the replacement technician exists.
            var replacementTechnician = db.Technicians
                .FirstOrDefault(t =>
                    t.TechnicianID == replacementTechnicianID);

            if (replacementTechnician == null)
            {
                TempData["ErrorMessage"] =
                    "The selected replacement technician could not be found.";

                return RedirectToAction(
                    "ApproveReassignment",
                    new { id = model.ReassignmentRequestID });
            }

            int administratorID =
                (int)Session["AdministratorID"];

            // -------------------------------------------------------
            // 1. Preserve the original assignment as history
            // -------------------------------------------------------

            reassignmentRequest.Assignment.Status =
                AssignmentStatus.Reassigned;

            // -------------------------------------------------------
            // 2. Create the new technician assignment
            // -------------------------------------------------------

            var newAssignment = new TechnicianAssignment
            {
                RequestID =
                    reassignmentRequest.Assignment.RequestID,

                TechnicianID =
                    replacementTechnicianID,

                AdministratorID =
                    administratorID,

                AssignedDate =
                    DateTime.Now,

                Status =
                    AssignmentStatus.PendingAcknowledgement
            };

            db.TechnicianAssignments.Add(newAssignment);

            // -------------------------------------------------------
            // 3. Update the current technician on the Request
            // -------------------------------------------------------

            reassignmentRequest.Assignment.Request.TechnicianID =
                replacementTechnicianID;

            // Request remains Assigned.
            reassignmentRequest.Assignment.Request.Status =
                RequestStatus.Assigned;

            // -------------------------------------------------------
            // 4. Approve the reassignment request
            // -------------------------------------------------------

            reassignmentRequest.Status =
                ReassignmentStatus.Approved;

            reassignmentRequest.ReviewedByAdministratorID =
                administratorID;

            reassignmentRequest.ReviewedDate =
                DateTime.Now;

            reassignmentRequest.AdministratorResponse =
                string.IsNullOrWhiteSpace(model.AdministratorResponse)
                    ? "Reassignment approved."
                    : model.AdministratorResponse.Trim();

            // -------------------------------------------------------
            // 5. Save everything
            // -------------------------------------------------------

            db.SaveChanges();

            TempData["SuccessMessage"] =
                "Reassignment approved successfully. The request has been assigned to the replacement technician.";

            return RedirectToAction("ReassignmentRequests");
        }

        // ===========================================================
        // CREATE - GET
        // ===========================================================

        // GET: AdministratorAssignments/Create/5
        public ActionResult Create(int? id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            if (id == null)
            {
                return RedirectToAction(
                    "Index",
                    "AdministratorRequests"
                );
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


            // -------------------------------------------------------
            // Request must be approved
            // -------------------------------------------------------

            if (request.Status != RequestStatus.Approved)
            {
                return RedirectToAction(
                    "Details",
                    "AdministratorRequests",
                    new { id = request.RequestID }
                );
            }


            // -------------------------------------------------------
            // Request must be classified
            // -------------------------------------------------------

            if (string.IsNullOrWhiteSpace(request.PriorityReason))
            {
                return RedirectToAction(
                    "Classify",
                    "AdministratorRequests",
                    new { id = request.RequestID }
                );
            }


            // -------------------------------------------------------
            // Build ViewModel
            // -------------------------------------------------------
            var model = new TechnicianAssignmentViewModel
            {
                RequestID = request.RequestID,

                Title = request.Title,

                CategoryName = request.Category != null
        ? request.Category.CategoryName
        : "Unknown",

                WardName = request.Ward != null
        ? request.Ward.WardName
        : "Unknown",

                ProblemLocation = request.ProblemLocation,

                Priority = request.Priority,

                PriorityReason = request.PriorityReason,

                Technicians = db.Technicians
        .Where(t =>
            t.AccountStatus == AccountStatus.Active)
        .OrderBy(t => t.LastName)
        .ThenBy(t => t.FirstName)
        .Select(t => new SelectListItem
        {
            Value = t.TechnicianID.ToString(),
            Text = t.FirstName + " " + t.LastName
        })
        .ToList(),

                Skills = db.Skills
        .OrderBy(s => s.SkillName)
        .Select(s => new SelectListItem
        {
            Value = s.SkillID.ToString(),
            Text = s.SkillName
        })
        .ToList()
            };
            return View(model);

        }


        // ===========================================================
        // CREATE - POST
        // ===========================================================

        // POST: AdministratorAssignments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            TechnicianAssignmentViewModel model)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction(
                    "Login",
                    "Administrators"
                );
            }

            if (model.SelectedSkillIDs == null ||
    !model.SelectedSkillIDs.Any())
            {
                ModelState.AddModelError(
                    "SelectedSkillIDs",
                    "Please select at least one required skill."
                );
            }

            if (!ModelState.IsValid)
            {
                RestoreRequestInformation(model);

                LoadAssignmentOptions(model);

                return View(model);
            }

            // -------------------------------------------------------
            // Find request
            // -------------------------------------------------------

            var request = db.Requests
                .FirstOrDefault(r =>
                    r.RequestID == model.RequestID);

            if (request == null)
            {
                return HttpNotFound();
            }


            // -------------------------------------------------------
            // Request must still be approved
            // -------------------------------------------------------

            if (request.Status != RequestStatus.Approved)
            {
                return RedirectToAction(
                    "Details",
                    "AdministratorRequests",
                    new { id = request.RequestID }
                );
            }


            // -------------------------------------------------------
            // Request must be classified
            // -------------------------------------------------------

            if (string.IsNullOrWhiteSpace(request.PriorityReason))
            {
                return RedirectToAction(
                    "Classify",
                    "AdministratorRequests",
                    new { id = request.RequestID }
                );
            }


            // -------------------------------------------------------
            // Find selected technician
            // -------------------------------------------------------

            var technician = db.Technicians
                .FirstOrDefault(t =>
                    t.TechnicianID == model.TechnicianID &&
                    t.AccountStatus == AccountStatus.Active
                );

            if (technician == null)
            {
                ModelState.AddModelError(
                    "TechnicianID",
                    "Please select an active technician."
                );
                LoadAssignmentOptions(model);
                return View(model);
            }


            // -------------------------------------------------------
            // Prevent duplicate active assignment
            // -------------------------------------------------------

            bool alreadyAssigned =
                db.TechnicianAssignments.Any(a =>
                    a.RequestID == model.RequestID &&
                    a.Status != AssignmentStatus.Completed &&
                    a.Status != AssignmentStatus.Cancelled
                );

            if (alreadyAssigned)
            {
                TempData["ErrorMessage"] =
                    "This request already has an active technician assignment.";

                return RedirectToAction(
                    "Details",
                    "AdministratorRequests",
                    new { id = request.RequestID }
                );
            }


            // -------------------------------------------------------
            // Create assignment history
            // -------------------------------------------------------

            var assignment = new TechnicianAssignment
            {
                RequestID = request.RequestID,

                TechnicianID = technician.TechnicianID,

                AdministratorID =
                    (int)Session["AdministratorID"],

                AssignedDate = DateTime.Now,

                Status =
                    AssignmentStatus.PendingAcknowledgement
            };


            db.TechnicianAssignments.Add(assignment);


            // -------------------------------------------------------
            // Update current responsible technician
            // -------------------------------------------------------

            request.TechnicianID =
                technician.TechnicianID;

            request.Status =
                RequestStatus.Assigned;


            // -------------------------------------------------------
            // Save required skills
            // -------------------------------------------------------

            if (model.SelectedSkillIDs != null)
            {
                foreach (int skillID in model.SelectedSkillIDs.Distinct())
                {
                    bool skillExists =
                        db.Skills.Any(s =>
                            s.SkillID == skillID);

                    if (!skillExists)
                    {
                        continue;
                    }

                    bool alreadyExists =
                        db.RequestSkills.Any(rs =>
                            rs.RequestID == request.RequestID &&
                            rs.SkillID == skillID);

                    if (!alreadyExists)
                    {
                        db.RequestSkills.Add(new RequestSkill
                        {
                            RequestID = request.RequestID,
                            SkillID = skillID
                        });
                    }
                }
            }

            db.SaveChanges();

            // -------------------------------------------------------
            // Return to request details
            // -------------------------------------------------------

            TempData["SuccessMessage"] =
                "Technician assigned successfully.";

            return RedirectToAction(
                "Details",
                "AdministratorRequests",
                new { id = request.RequestID }
            );
        }


        private void LoadAssignmentOptions(
    TechnicianAssignmentViewModel model)
        {
            model.Technicians =
                db.Technicians
                    .Where(t =>
                        t.AccountStatus == AccountStatus.Active)
                    .OrderBy(t => t.LastName)
                    .ThenBy(t => t.FirstName)
                    .Select(t => new SelectListItem
                    {
                        Value = t.TechnicianID.ToString(),
                        Text = t.FirstName + " " + t.LastName,
                        Selected =
                            t.TechnicianID ==
                            model.TechnicianID
                    })
                    .ToList();


            model.Skills =
                db.Skills
                    .OrderBy(s => s.SkillName)
                    .Select(s => new SelectListItem
                    {
                        Value = s.SkillID.ToString(),
                        Text = s.SkillName
                    })
                    .ToList();
        }


        private void RestoreRequestInformation(
    TechnicianAssignmentViewModel model)
        {
            var request = db.Requests
                .Include("Category")
                .Include("Ward")
                .FirstOrDefault(r =>
                    r.RequestID == model.RequestID);

            if (request == null)
            {
                return;
            }

            model.Title = request.Title;

            model.CategoryName =
                request.Category != null
                    ? request.Category.CategoryName
                    : "Unknown";

            model.WardName =
                request.Ward != null
                    ? request.Ward.WardName
                    : "Unknown";

            model.ProblemLocation =
                request.ProblemLocation;

            model.Priority =
                request.Priority;

            model.PriorityReason =
                request.PriorityReason;
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