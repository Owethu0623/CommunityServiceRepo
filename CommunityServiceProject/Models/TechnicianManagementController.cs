using System;
using System.Linq;
using System.Data.Entity;
using System.Web.Mvc;
using CommunityServiceProject.Models;
using CommunityServiceProject.ViewModels;
using CommunityServiceProject.Filters;

namespace CommunityServiceProject.Controllers
{
    [RoleAuthorize("Administrator")]
    // Placeholder: Administrator-only controller for account creation tasks (no behavioral changes).
    public class TechnicianManagementController : Controller
    {
        private Community db = new Community();


        // ===========================================================
        // INDEX
        // ===========================================================

        // GET: TechnicianManagement
        public ActionResult Index()
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            var technicians = db.Technicians
                .Include(t => t.TechnicianSkills.Select(ts => ts.Skill))
                .OrderBy(t => t.LastName)
                .ThenBy(t => t.FirstName)
                .ToList();

            return View(technicians);
        }

        // ===========================================================
        // PENDING ONBOARDINGS
        // ===========================================================

        public ActionResult PendingOnboardings()
        {
            if (Session["AdministratorID"] == null)
                return RedirectToAction("Login", "Administrators");

            var pending = db.TechnicianOnboardings
                .Include(o => o.Application)
                .Include(o => o.Application.Citizen)
                .Where(o => o.TechnicianID == null && o.OnboardedByHROfficerID != null)
                .OrderByDescending(o => o.OnboardingDate)
                .ToList();

            return View(pending);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateTechnicianFromOnboarding(int id)
        {
            if (Session["AdministratorID"] == null)
                return RedirectToAction("Login", "Administrators");

            var administratorID = Session["AdministratorID"] != null
                ? (int?)Convert.ToInt32(Session["AdministratorID"]) : null;

            if (!administratorID.HasValue)
                return new HttpUnauthorizedResult();

            var onboarding = db.TechnicianOnboardings
                .Include(o => o.Application)
                .Include(o => o.Application.Citizen)
                .FirstOrDefault(o => o.OnboardingID == id);

            if (onboarding == null)
                return HttpNotFound();

            if (onboarding.TechnicianID.HasValue)
            {
                TempData["ErrorMessage"] = "This onboarding has already been processed.";
                return RedirectToAction("PendingOnboardings");
            }

            var citizen = onboarding.Application?.Citizen;

            if (citizen == null)
            {
                TempData["ErrorMessage"] = "Applicant information is missing.";
                return RedirectToAction("PendingOnboardings");
            }

            // Check for existing technician with same municipal email
            if (db.Technicians.Any(t => t.EmailAddress == onboarding.MunicipalEmail))
            {
                TempData["ErrorMessage"] = "A technician with this municipal email already exists.";
                return RedirectToAction("PendingOnboardings");
            }

            // Create temporary password
            var tempPassword = GenerateTemporaryPassword();

            var technician = new Technician
            {
                CitizenID = citizen.CitizenID,
                FirstName = citizen.FirstName,
                LastName = citizen.LastName,
                EmailAddress = onboarding.MunicipalEmail,
                PhoneNumber = citizen.PhoneNumber,
                Password = CommunityServiceProject.Helpers.PasswordHelper.HashPassword(tempPassword),
                AccountStatus = AccountStatus.Active,
                MustChangePassword = true
            };

            db.Technicians.Add(technician);
            db.SaveChanges();

            // Link onboarding to technician and record admin actor
            onboarding.TechnicianID = technician.TechnicianID;
            onboarding.OnboardedByAdministratorID = administratorID.Value;
            db.SaveChanges();

            TempData["SuccessMessage"] = "Technician account created successfully.";

            // Redirect to onboarding success view
            return RedirectToAction("OnboardingSuccess", "TechnicianApplication", new { id = onboarding.ApplicationID });
        }

        private string GenerateTemporaryPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 12).Select(s => s[random.Next(s.Length)]).ToArray());
        }



        // ===========================================================
        // DETAILS
        // ===========================================================

        public ActionResult Details(int? id)
        {
            if (Session["AdministratorID"] == null)
                return RedirectToAction("Login", "Administrators");

            if (id == null)
                return RedirectToAction("Index");

            var technician = db.Technicians
                .FirstOrDefault(t => t.TechnicianID == id);

            if (technician == null)
                return HttpNotFound();

            db.Entry(technician)
                .Collection(t => t.TechnicianSkills)
                .Query()
                .Include(ts => ts.Skill)
                .Load();

            var completedRequestCount = db.TechnicianAssignments
                .Count(a =>
                    a.TechnicianID == id.Value &&
                    a.Status == AssignmentStatus.Completed);

            var activeRequestCount = db.TechnicianAssignments
                .Count(a =>
                    a.TechnicianID == id.Value &&
                    a.Status != AssignmentStatus.Completed &&
                    a.Status != AssignmentStatus.Cancelled);

            var model = new TechnicianProfileViewModel
            {
                Technician = technician,

                Skills = technician.TechnicianSkills
                    .Where(ts => ts.Skill != null)
                    .Select(ts => ts.Skill)
                    .ToList(),

                CompletedRequestCount = completedRequestCount,
                ActiveRequestCount = activeRequestCount
            };

            return View(model);
        }

        public ActionResult Edit(int? id)
        {
            if (Session["AdministratorID"] == null)
                return RedirectToAction("Login", "Administrators");

            if (id == null)
                return RedirectToAction("Index");

            var technician = db.Technicians
                .FirstOrDefault(t => t.TechnicianID == id);

            if (technician == null)
                return HttpNotFound();

            db.Entry(technician)
                .Collection(t => t.TechnicianSkills)
                .Query()
                .Include(ts => ts.Skill)
                .Load();

            var assignedSkillIds = technician.TechnicianSkills
                .Select(ts => ts.SkillID)
                .ToList();

            var model = new EditTechnicianViewModel
            {
                TechnicianID = technician.TechnicianID,
                FirstName = technician.FirstName,
                LastName = technician.LastName,
                EmailAddress = technician.EmailAddress,
                PhoneNumber = technician.PhoneNumber,

                AssignedSkills = technician.TechnicianSkills
                    .Where(ts => ts.Skill != null)
                    .Select(ts => ts.Skill)
                    .ToList(),

                AvailableSkills = db.Skills
                    .Where(s => !assignedSkillIds.Contains(s.SkillID))
                    .OrderBy(s => s.SkillName)
                    .ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
    EditTechnicianViewModel model,
    string action,
    int? SkillID)
        {
            if (Session["AdministratorID"] == null)
                return RedirectToAction("Login", "Administrators");

            var technician = db.Technicians
                .FirstOrDefault(t => t.TechnicianID == model.TechnicianID);

            if (technician == null)
                return HttpNotFound();

            if (!string.IsNullOrEmpty(action) &&
    action.StartsWith("removeSkill:"))
            {
                int skillId;

                if (int.TryParse(
                    action.Substring("removeSkill:".Length),
                    out skillId))
                {
                    var technicianSkill = db.TechnicianSkills
                        .FirstOrDefault(ts =>
                            ts.TechnicianID == technician.TechnicianID &&
                            ts.SkillID == skillId);

                    if (technicianSkill != null)
                    {
                        db.TechnicianSkills.Remove(technicianSkill);
                        db.SaveChanges();
                    }
                }

                return RedirectToAction(
                    "Edit",
                    new { id = technician.TechnicianID });
            }

            // Clean up input
            model.FirstName = model.FirstName?.Trim();
            model.LastName = model.LastName?.Trim();
            model.EmailAddress = model.EmailAddress?.Trim().ToLower();
            model.PhoneNumber = model.PhoneNumber?.Trim();

            // Check email uniqueness
            if (db.Technicians.Any(t =>
                t.EmailAddress == model.EmailAddress &&
                t.TechnicianID != model.TechnicianID))
            {
                ModelState.AddModelError(
                    "EmailAddress",
                    "A technician with this email address already exists.");
            }

            if (!ModelState.IsValid)
            {
                LoadTechnicianSkills(model, technician);

                return View(model);
            }

            if (action == "addSkill")
            {
                if (!SkillID.HasValue)
                {
                    ModelState.AddModelError(
                        "SkillID",
                        "Please select a skill to add."
                    );

                    LoadTechnicianSkills(model, technician);
                    return View(model);
                }

                bool alreadyAssigned = db.TechnicianSkills.Any(ts =>
                    ts.TechnicianID == technician.TechnicianID &&
                    ts.SkillID == SkillID.Value);

                if (alreadyAssigned)
                {
                    ModelState.AddModelError(
                        "SkillID",
                        "This skill is already assigned to the technician."
                    );

                    LoadTechnicianSkills(model, technician);
                    return View(model);
                }

                var skill = db.Skills.Find(SkillID.Value);

                if (skill == null)
                {
                    ModelState.AddModelError(
                        "SkillID",
                        "The selected skill could not be found."
                    );

                    LoadTechnicianSkills(model, technician);
                    return View(model);
                }

                db.TechnicianSkills.Add(new TechnicianSkill
                {
                    TechnicianID = technician.TechnicianID,
                    SkillID = skill.SkillID
                });

                db.SaveChanges();

                return RedirectToAction(
                    "Edit",
                    new { id = technician.TechnicianID }
                );
            }

            // Update editable technician information
            technician.FirstName = model.FirstName;
            technician.LastName = model.LastName;
            technician.EmailAddress = model.EmailAddress;
            technician.PhoneNumber = model.PhoneNumber;

            try
            {
                db.SaveChanges();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                foreach (var entityValidationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in entityValidationErrors.ValidationErrors)
                    {
                        ModelState.AddModelError(
                            validationError.PropertyName,
                            validationError.ErrorMessage);
                    }
                }

                LoadTechnicianSkills(model, technician);

                return View(model);
            }

            return RedirectToAction(
                "Details",
                new { id = technician.TechnicianID });
        }

        private void LoadTechnicianSkills(
    EditTechnicianViewModel model,
    Technician technician)
        {
            db.Entry(technician)
                .Collection(t => t.TechnicianSkills)
                .Query()
                .Include(ts => ts.Skill)
                .Load();

            var assignedSkillIds = technician.TechnicianSkills
                .Select(ts => ts.SkillID)
                .ToList();

            model.AssignedSkills = technician.TechnicianSkills
                .Where(ts => ts.Skill != null)
                .Select(ts => ts.Skill)
                .ToList();

            model.AvailableSkills = db.Skills
                .Where(s => !assignedSkillIds.Contains(s.SkillID))
                .OrderBy(s => s.SkillName)
                .ToList();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveSkill(int technicianId, int skillId)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            var technicianSkill = db.TechnicianSkills
                .FirstOrDefault(ts =>
                    ts.TechnicianID == technicianId &&
                    ts.SkillID == skillId);

            if (technicianSkill == null)
            {
                return HttpNotFound();
            }

            db.TechnicianSkills.Remove(technicianSkill);
            db.SaveChanges();

            return RedirectToAction(
                "Edit",
                new { id = technicianId });
        }


        // GET: TechnicianManagement/Create
        public ActionResult Create()
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            ViewBag.Skills = db.Skills
                .OrderBy(s => s.SkillName)
                .ToList();

            return View();
        }


        // ===========================================================
        // CREATE - POST
        // ===========================================================

        // POST: TechnicianManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Technician technician, int? skillId)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            // -------------------------------------------------------
            // Check whether the email already exists
            // -------------------------------------------------------

            if (db.Technicians.Any(t =>
                t.EmailAddress == technician.EmailAddress))
            {
                ModelState.AddModelError(
                    "EmailAddress",
                    "A technician with this email address already exists."
                );
            }

            // -------------------------------------------------------
            // Skill is required
            // -------------------------------------------------------

            if (!skillId.HasValue)
            {
                ModelState.AddModelError(
                    "SkillID",
                    "Please select a skill for the technician."
                );
            }

            // -------------------------------------------------------
            // Validate the complete form
            // -------------------------------------------------------

            if (ModelState.IsValid)
            {
                technician.AccountStatus = AccountStatus.Active;

                db.Technicians.Add(technician);
                db.SaveChanges();

                // ---------------------------------------------------
                // Assign the selected skill
                // ---------------------------------------------------

                var technicianSkill = new TechnicianSkill
                {
                    TechnicianID = technician.TechnicianID,
                    SkillID = skillId.Value
                };

                db.TechnicianSkills.Add(technicianSkill);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            // Reload skills if validation fails
            ViewBag.Skills = db.Skills
                .OrderBy(s => s.SkillName)
                .ToList();

            return View(technician);
        }


        // ===========================================================
        // ACTIVATE
        // ===========================================================

        // POST: TechnicianManagement/Activate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Activate(int id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            var technician = db.Technicians.Find(id);

            if (technician == null)
            {
                return HttpNotFound();
            }


            technician.AccountStatus = AccountStatus.Active;

            db.SaveChanges();

            return RedirectToAction("Details", new { id = id });
        }


        // ===========================================================
        // SUSPEND
        // ===========================================================

        // POST: TechnicianManagement/Suspend
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Suspend(int id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            var technician = db.Technicians.Find(id);

            if (technician == null)
            {
                return HttpNotFound();
            }

            technician.AccountStatus = AccountStatus.Suspended;

            db.SaveChanges();

            return RedirectToAction("Details", new { id = id });
        }


        // ===========================================================
        // DEACTIVATE
        // ===========================================================

        // POST: TechnicianManagement/Deactivate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deactivate(int id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            var technician = db.Technicians.Find(id);

            if (technician == null)
            {
                return HttpNotFound();
            }

            technician.AccountStatus = AccountStatus.Inactive;

            db.SaveChanges();

            return RedirectToAction("Details", new { id = id });
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