using CommunityServiceProject.Models;
using CommunityServiceProject.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

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

            // -------------------------------------------------------
            // Current technician must be excluded.
            // -------------------------------------------------------

            var currentTechnicianID =
                reassignmentRequest.TechnicianID;

            // -------------------------------------------------------
            // Initially do not show replacement technicians.
            //
            // The administrator must first select the required
            // skill(s), then matching technicians will appear.
            // -------------------------------------------------------

            var technicians = new List<Technician>();

            // -------------------------------------------------------
            // Load available skills.
            // -------------------------------------------------------

            var skills = db.Skills
                .OrderBy(s => s.SkillName)
                .Select(s => new SelectListItem
                {
                    Value = s.SkillID.ToString(),
                    Text = s.SkillName
                })
                .ToList();

            // -------------------------------------------------------
            // Build ViewModel.
            // -------------------------------------------------------

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
                    }).ToList(),

                Skills = skills,

                SelectedSkillIDs =
                    new List<int>(),

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

            // -------------------------------------------------------
            // Get selected skills.
            // -------------------------------------------------------

            var selectedSkillIds =
                model.SelectedSkillIDs?
                    .Distinct()
                    .ToList() ?? new List<int>();


            // -------------------------------------------------------
            // At least one skill is required.
            // -------------------------------------------------------

            if (!selectedSkillIds.Any())
            {
                ModelState.AddModelError(
                    "SelectedSkillIDs",
                    "Please select at least one required skill."
                );
            }


            // -------------------------------------------------------
            // If validation failed, reload the page options.
            // -------------------------------------------------------

            if (!ModelState.IsValid)
            {
                var currentRequest = db.ReassignmentRequests
                    .FirstOrDefault(r =>
                        r.ReassignmentRequestID ==
                        model.ReassignmentRequestID);

                int currentTechnicianID =
                    currentRequest != null
                        ? currentRequest.TechnicianID
                        : 0;


                // Only active technicians.
                // Only technicians with at least one selected skill.
                // Current technician excluded.
                var technicians = db.Technicians
                    .Where(t =>
                        t.AccountStatus == AccountStatus.Active &&
                        t.TechnicianID != currentTechnicianID &&
                        t.TechnicianSkills.Any(ts =>
                            selectedSkillIds.Contains(ts.SkillID)
                        ))
                    .OrderBy(t => t.FirstName)
                    .ThenBy(t => t.LastName)
                    .ToList();


                model.AvailableTechnicians =
                    technicians.Select(t => new SelectListItem
                    {
                        Value = t.TechnicianID.ToString(),

                        Text =
                            t.FirstName +
                            " " +
                            t.LastName,

                        Selected =
                            model.ReplacementTechnicianID.HasValue &&
                            model.ReplacementTechnicianID.Value ==
                            t.TechnicianID

                    }).ToList();


                // Reload skills.
                model.Skills = db.Skills
                    .OrderBy(s => s.SkillName)
                    .Select(s => new SelectListItem
                    {
                        Value = s.SkillID.ToString(),
                        Text = s.SkillName,

                        Selected =
                            selectedSkillIds.Contains(s.SkillID)

                    })
                    .ToList();


                return View(model);
            }


            // -------------------------------------------------------
            // Get reassignment request.
            // -------------------------------------------------------

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


            // -------------------------------------------------------
            // Only pending reassignment requests can be approved.
            // -------------------------------------------------------

            if (reassignmentRequest.Status !=
                ReassignmentStatus.Pending)
            {
                TempData["ErrorMessage"] =
                    "This reassignment request is no longer pending.";

                return RedirectToAction("ReassignmentRequests");
            }


            // -------------------------------------------------------
            // Make sure original assignment exists.
            // -------------------------------------------------------

            if (reassignmentRequest.Assignment == null)
            {
                TempData["ErrorMessage"] =
                    "The original assignment could not be found.";

                return RedirectToAction("ReassignmentRequests");
            }


            // -------------------------------------------------------
            // Replacement technician required.
            // -------------------------------------------------------

            if (!model.ReplacementTechnicianID.HasValue)
            {
                ModelState.AddModelError(
                    "ReplacementTechnicianID",
                    "Please select a replacement technician."
                );


                var technicians = db.Technicians
                    .Where(t =>
                        t.AccountStatus == AccountStatus.Active &&
                        t.TechnicianID !=
                            reassignmentRequest.TechnicianID &&
                        t.TechnicianSkills.Any(ts =>
                            selectedSkillIds.Contains(ts.SkillID)
                        ))
                    .OrderBy(t => t.FirstName)
                    .ThenBy(t => t.LastName)
                    .ToList();


                model.AvailableTechnicians =
                    technicians.Select(t => new SelectListItem
                    {
                        Value = t.TechnicianID.ToString(),

                        Text =
                            t.FirstName +
                            " " +
                            t.LastName
                    }).ToList();


                model.Skills = db.Skills
                    .OrderBy(s => s.SkillName)
                    .Select(s => new SelectListItem
                    {
                        Value = s.SkillID.ToString(),
                        Text = s.SkillName,

                        Selected =
                            selectedSkillIds.Contains(s.SkillID)

                    }).ToList();


                return View(model);
            }


            int replacementTechnicianID =
                model.ReplacementTechnicianID.Value;


            // -------------------------------------------------------
            // Replacement technician cannot be current technician.
            // -------------------------------------------------------

            if (replacementTechnicianID ==
                reassignmentRequest.TechnicianID)
            {
                ModelState.AddModelError(
                    "ReplacementTechnicianID",
                    "The replacement technician must be different from the current technician."
                );


                var technicians = db.Technicians
                    .Where(t =>
                        t.AccountStatus == AccountStatus.Active &&
                        t.TechnicianID !=
                            reassignmentRequest.TechnicianID &&
                        t.TechnicianSkills.Any(ts =>
                            selectedSkillIds.Contains(ts.SkillID)
                        ))
                    .OrderBy(t => t.FirstName)
                    .ThenBy(t => t.LastName)
                    .ToList();


                model.AvailableTechnicians =
                    technicians.Select(t => new SelectListItem
                    {
                        Value = t.TechnicianID.ToString(),

                        Text =
                            t.FirstName +
                            " " +
                            t.LastName
                    }).ToList();


                model.Skills = db.Skills
                    .OrderBy(s => s.SkillName)
                    .Select(s => new SelectListItem
                    {
                        Value = s.SkillID.ToString(),
                        Text = s.SkillName,

                        Selected =
                            selectedSkillIds.Contains(s.SkillID)

                    }).ToList();


                return View(model);
            }


            // -------------------------------------------------------
            // Check replacement technician exists and is active.
            // -------------------------------------------------------

            var replacementTechnician = db.Technicians
                .FirstOrDefault(t =>
                    t.TechnicianID ==
                    replacementTechnicianID &&
                    t.AccountStatus ==
                    AccountStatus.Active);


            if (replacementTechnician == null)
            {
                ModelState.AddModelError(
                    "ReplacementTechnicianID",
                    "The selected replacement technician could not be found or is not active."
                );


                var technicians = db.Technicians
                    .Where(t =>
                        t.AccountStatus == AccountStatus.Active &&
                        t.TechnicianID !=
                            reassignmentRequest.TechnicianID &&
                        t.TechnicianSkills.Any(ts =>
                            selectedSkillIds.Contains(ts.SkillID)
                        ))
                    .OrderBy(t => t.FirstName)
                    .ThenBy(t => t.LastName)
                    .ToList();


                model.AvailableTechnicians =
                    technicians.Select(t => new SelectListItem
                    {
                        Value = t.TechnicianID.ToString(),

                        Text =
                            t.FirstName +
                            " " +
                            t.LastName
                    }).ToList();


                model.Skills = db.Skills
                    .OrderBy(s => s.SkillName)
                    .Select(s => new SelectListItem
                    {
                        Value = s.SkillID.ToString(),
                        Text = s.SkillName,

                        Selected =
                            selectedSkillIds.Contains(s.SkillID)

                    }).ToList();


                return View(model);
            }


            // -------------------------------------------------------
            // IMPORTANT:
            // Verify the selected technician actually possesses
            // at least ONE of the selected skills.
            // -------------------------------------------------------

            bool technicianHasRequiredSkill =
                db.TechnicianSkills.Any(ts =>
                    ts.TechnicianID ==
                        replacementTechnicianID &&
                    selectedSkillIds.Contains(ts.SkillID)
                );


            if (!technicianHasRequiredSkill)
            {
                ModelState.AddModelError(
                    "ReplacementTechnicianID",
                    "The selected technician does not possess any of the required skills."
                );


                var technicians = db.Technicians
                    .Where(t =>
                        t.AccountStatus == AccountStatus.Active &&
                        t.TechnicianID !=
                            reassignmentRequest.TechnicianID &&
                        t.TechnicianSkills.Any(ts =>
                            selectedSkillIds.Contains(ts.SkillID)
                        ))
                    .OrderBy(t => t.FirstName)
                    .ThenBy(t => t.LastName)
                    .ToList();


                model.AvailableTechnicians =
                    technicians.Select(t => new SelectListItem
                    {
                        Value = t.TechnicianID.ToString(),

                        Text =
                            t.FirstName +
                            " " +
                            t.LastName
                    }).ToList();


                model.Skills = db.Skills
                    .OrderBy(s => s.SkillName)
                    .Select(s => new SelectListItem
                    {
                        Value = s.SkillID.ToString(),
                        Text = s.SkillName,

                        Selected =
                            selectedSkillIds.Contains(s.SkillID)

                    })
                    .ToList();


                return View(model);
            }


            int administratorID =
                (int)Session["AdministratorID"];


            // =======================================================
            // 1. PRESERVE ORIGINAL ASSIGNMENT AS HISTORY
            // =======================================================

            reassignmentRequest.Assignment.Status =
                AssignmentStatus.Reassigned;


            // =======================================================
            // 2. CREATE NEW TECHNICIAN ASSIGNMENT
            // =======================================================

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


            // =======================================================
            // 3. UPDATE REQUEST
            // =======================================================

            reassignmentRequest.Assignment.Request.TechnicianID =
                replacementTechnicianID;

            reassignmentRequest.Assignment.Request.Status =
                RequestStatus.Assigned;


            // =======================================================
            // 4. APPROVE REASSIGNMENT REQUEST
            // =======================================================

            reassignmentRequest.Status =
                ReassignmentStatus.Approved;

            reassignmentRequest.ReviewedByAdministratorID =
                administratorID;

            reassignmentRequest.ReviewedDate =
                DateTime.Now;

            reassignmentRequest.AdministratorResponse =
                string.IsNullOrWhiteSpace(
                    model.AdministratorResponse)
                        ? "Reassignment approved."
                        : model.AdministratorResponse.Trim();


            // =======================================================
            // 5. SAVE EVERYTHING
            // =======================================================

            db.SaveChanges();


            TempData["SuccessMessage"] =
                "Reassignment approved successfully. The request has been assigned to the replacement technician.";


            return RedirectToAction(
                "ReassignmentRequests");
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



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TechnicianAssignmentViewModel model)
        {
            // ============================================================
            // CHECK ADMINISTRATOR LOGIN
            // ============================================================

            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Login");
            }


            // ============================================================
            // REQUIRED SKILL VALIDATION
            // At least one skill must be selected.
            // ============================================================

            if (model.SelectedSkillIDs == null || !model.SelectedSkillIDs.Any())
            {
                ModelState.AddModelError(
                    "SelectedSkillIDs",
                    "Please select at least one required skill."
                );
            }


            // ============================================================
            // IF VALIDATION FAILED
            // Reload request information and dropdown options.
            // ============================================================

            if (!ModelState.IsValid)
            {
                RestoreRequestInformation(model);
                LoadAssignmentOptions(model);

                return View(model);
            }


            // ============================================================
            // GET REQUEST
            // ============================================================

            var request = db.Requests
                .FirstOrDefault(r => r.RequestID == model.RequestID);

            if (request == null)
            {
                return HttpNotFound();
            }


            // ============================================================
            // REQUEST MUST BE APPROVED
            // ============================================================

            if (request.Status != RequestStatus.Approved)
            {
                return RedirectToAction(
                    "Details",
                    "AdministratorRequests",
                    new { id = model.RequestID }
                );
            }


            // ============================================================
            // PRIORITY REASON MUST EXIST
            // ============================================================

            if (string.IsNullOrWhiteSpace(request.PriorityReason))
            {
                return RedirectToAction(
                    "Classify",
                    "AdministratorRequests",
                    new { id = model.RequestID }
                );
            }


            // ============================================================
            // GET SELECTED SKILLS
            // Remove duplicate skill IDs.
            // ============================================================

            var selectedSkillIds = model.SelectedSkillIDs
                .Distinct()
                .ToList();


            // ============================================================
            // GET ACTIVE TECHNICIAN
            // ============================================================

            var technician = db.Technicians
                .FirstOrDefault(t =>
                    t.TechnicianID == model.TechnicianID &&
                    t.AccountStatus == AccountStatus.Active
                );


            // ============================================================
            // CHECK THAT TECHNICIAN EXISTS AND IS ACTIVE
            // ============================================================

            if (technician == null)
            {
                ModelState.AddModelError(
                    "TechnicianID",
                    "Please select an active technician."
                );

                RestoreRequestInformation(model);
                LoadAssignmentOptions(model);

                return View(model);
            }


            // ============================================================
            // CHECK TECHNICIAN HAS AT LEAST ONE SELECTED SKILL
            //
            // IMPORTANT:
            // Multiple selected skills use OR matching.
            //
            // Example:
            // Selected skills = Plumbing + Electrical
            //
            // Technician has Plumbing only
            //       -> allowed
            //
            // Technician has Electrical only
            //       -> allowed
            //
            // Technician has neither
            //       -> not allowed
            // ============================================================

            bool technicianHasRequiredSkill = db.TechnicianSkills.Any(ts =>
                ts.TechnicianID == technician.TechnicianID &&
                selectedSkillIds.Contains(ts.SkillID)
            );


            if (!technicianHasRequiredSkill)
            {
                ModelState.AddModelError(
                    "TechnicianID",
                    "The selected technician does not possess any of the required skills."
                );

                RestoreRequestInformation(model);
                LoadAssignmentOptions(model);

                return View(model);
            }


            // ============================================================
            // CHECK WHETHER REQUEST ALREADY HAS AN ACTIVE ASSIGNMENT
            // ============================================================

            bool alreadyAssigned = db.TechnicianAssignments.Any(a =>
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
                    new { id = model.RequestID }
                );
            }


            // ============================================================
            // CREATE TECHNICIAN ASSIGNMENT
            // ============================================================

            var assignment = new TechnicianAssignment
            {
                RequestID = request.RequestID,
                TechnicianID = technician.TechnicianID,
                AdministratorID = (int)Session["AdministratorID"],
                AssignedDate = DateTime.Now,
                Status = AssignmentStatus.PendingAcknowledgement
            };

            db.TechnicianAssignments.Add(assignment);


            // ============================================================
            // UPDATE REQUEST
            // ============================================================

            request.TechnicianID = technician.TechnicianID;
            request.Status = RequestStatus.Assigned;


            // ============================================================
            // SAVE REQUEST SKILLS
            //
            // Only save valid skills.
            // Prevent duplicate RequestSkill records.
            // ============================================================

            foreach (var skillId in selectedSkillIds)
            {
                bool skillExists = db.Skills.Any(s => s.SkillID == skillId);

                if (!skillExists)
                {
                    continue;
                }

                bool requestSkillExists = db.RequestSkills.Any(rs =>
                    rs.RequestID == request.RequestID &&
                    rs.SkillID == skillId
                );

                if (!requestSkillExists)
                {
                    db.RequestSkills.Add(
                        new RequestSkill
                        {
                            RequestID = request.RequestID,
                            SkillID = skillId
                        }
                    );
                }
            }


            // ============================================================
            // CITIZEN NOTIFICATION
            // ============================================================

            var notification = new Notification
            {
                CitizenID = request.CitizenID,
                RequestID = request.RequestID,

                Message =
                    "Technician " +
                    technician.FirstName +
                    " " +
                    technician.LastName +
                    " has been assigned to " +
                    request.ReferenceNumber +
                    ".",

                DateCreated = DateTime.Now,
                IsRead = false
            };

            db.Notifications.Add(notification);


            // ============================================================
            // SAVE EVERYTHING
            // ============================================================

            db.SaveChanges();


            // ============================================================
            // SUCCESS MESSAGE
            // ============================================================

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
            var selectedSkillIds = model.SelectedSkillIDs?
                .Distinct()
                .ToList() ?? new List<int>();


            // -------------------------------------------------------
            // Load technicians
            // -------------------------------------------------------

            var technicianQuery = db.Technicians
                .Where(t =>
                    t.AccountStatus == AccountStatus.Active);


            // If skills have been selected, only show technicians
            // who possess at least one selected skill.
            if (selectedSkillIds.Any())
            {
                technicianQuery = technicianQuery
                    .Where(t =>
                        t.TechnicianSkills.Any(ts =>
                            selectedSkillIds.Contains(ts.SkillID)
                        )
                    );
            }


            model.Technicians =
                technicianQuery
                    .OrderBy(t => t.LastName)
                    .ThenBy(t => t.FirstName)
                    .Select(t => new SelectListItem
                    {
                        Value = t.TechnicianID.ToString(),

                        Text =
                            t.FirstName + " " +
                            t.LastName,

                        Selected =
                            t.TechnicianID ==
                            model.TechnicianID
                    })
                    .ToList();


            // -------------------------------------------------------
            // Load skills
            // -------------------------------------------------------

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
        // GET TECHNICIANS BY REQUIRED SKILL
        // ===========================================================

        // GET: AdministratorAssignments/GetTechniciansBySkills
        public ActionResult GetTechniciansBySkills(int[] skillIds)
        {
            if (Session["AdministratorID"] == null)
            {
                return new HttpStatusCodeResult(401);
            }

            // No skills selected
            if (skillIds == null || skillIds.Length == 0)
            {
                return Json(
                    new object[0],
                    JsonRequestBehavior.AllowGet
                );
            }

            // Remove duplicate skill IDs
            var selectedSkillIds = skillIds
                .Distinct()
                .ToList();

            // Find active technicians who possess
            // at least ONE of the selected skills.
            var technicians = db.Technicians
                .Where(t =>
                    t.AccountStatus == AccountStatus.Active &&
                    t.TechnicianSkills.Any(ts =>
                        selectedSkillIds.Contains(ts.SkillID)
                    )
                )
                .OrderBy(t => t.LastName)
                .ThenBy(t => t.FirstName)
                .Select(t => new
                {
                    id = t.TechnicianID,
                    name = t.FirstName + " " + t.LastName
                })
                .ToList();

            return Json(
                technicians,
                JsonRequestBehavior.AllowGet
            );
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