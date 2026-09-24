using CommunityServiceProject.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace CommunityServiceProject.Controllers
{
    public class CitizenManagementController : Controller
    {
        private Community db = new Community();

        public ActionResult Index()
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            var citizens = db.Citizens
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .ToList();

            var complianceRecords = db.ComplianceRecords
                .Include("Violations")
                .ToList();

            ViewBag.ComplianceRecords = complianceRecords;

            return View(citizens);
        }

        // GET: CitizenManagement/Details/5
        public ActionResult Details(int? id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            if (id == null)
            {
                return RedirectToAction("Index");
            }

            var citizen = db.Citizens
                .FirstOrDefault(c => c.CitizenID == id);

            if (citizen == null)
            {
                return HttpNotFound();
            }

            return View(citizen);
        }

        // GET: CitizenManagement/Compliance/5
        public ActionResult Compliance(int? id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            if (id == null)
            {
                return RedirectToAction("Index");
            }

            var citizen = db.Citizens
                .FirstOrDefault(c => c.CitizenID == id.Value);

            if (citizen == null)
            {
                return HttpNotFound();
            }

            var complianceRecord = db.ComplianceRecords
                .Include("Violations")
                .FirstOrDefault(c => c.CitizenID == id.Value);

            ViewBag.Citizen = citizen;

            var latestRestriction = db.AccountRestrictions
                .Where(r => r.CitizenID == id.Value)
                .OrderByDescending(r => r.DateStarted)
                .FirstOrDefault();

            ViewBag.LatestRestriction = latestRestriction;

            return View(complianceRecord);
        }

        // GET: CitizenManagement/IssueWarning/5
        public ActionResult IssueWarning(int? id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            if (id == null)
            {
                return RedirectToAction("Index");
            }

            var violation = db.Violations
                .Include("ComplianceRecord")
                .Include("ComplianceRecord.Citizen")
                .FirstOrDefault(v => v.ViolationID == id.Value);

            if (violation == null)
            {
                return HttpNotFound();
            }

            return View(violation);
        }

        // POST: CitizenManagement/IssueWarning
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult IssueWarning(
            int violationID,
            string warningReason)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            if (string.IsNullOrWhiteSpace(warningReason))
            {
                TempData["ErrorMessage"] =
                    "A warning reason is required.";

                return RedirectToAction(
                    "IssueWarning",
                    new { id = violationID });
            }

            var violation = db.Violations
                .Include("ComplianceRecord")
                .FirstOrDefault(v => v.ViolationID == violationID);

            if (violation == null)
            {
                return HttpNotFound();
            }

            // Prevent duplicate warnings for the same violation.
            if (db.Warnings.Any(w => w.ViolationID == violationID))
            {
                TempData["ErrorMessage"] =
                    "A warning has already been issued for this violation.";

                return RedirectToAction(
                    "Compliance",
                    new { id = violation.ComplianceRecord.CitizenID });
            }

            int administratorId = (int)Session["AdministratorID"];

            var warning = new Warning
            {
                ViolationID = violation.ViolationID,
                AdministratorID = administratorId,
                WarningReason = warningReason.Trim(),
                DateIssued = DateTime.Now
            };

            db.Warnings.Add(warning);

            // Update the citizen's compliance status.
            var complianceRecord = violation.ComplianceRecord;

            if (complianceRecord != null)
            {
                complianceRecord.ComplianceStatus =
                    ComplianceStatus.Warning;

                complianceRecord.LastUpdated = DateTime.Now;
            }

            db.SaveChanges();

            TempData["SuccessMessage"] =
                "Warning issued successfully. The citizen's compliance status has been updated to Warning.";

            return RedirectToAction(
                "Compliance",
                new { id = complianceRecord.CitizenID });
        }

        // GET: CitizenManagement/RequestHistory/5
        public ActionResult RequestHistory(int? id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            if (id == null)
            {
                return RedirectToAction("Index");
            }

            var citizen = db.Citizens
                .FirstOrDefault(c => c.CitizenID == id.Value);

            if (citizen == null)
            {
                return HttpNotFound();
            }

            var requests = db.Requests
                .Where(r => r.CitizenID == id.Value)
                .OrderByDescending(r => r.DateSubmitted)
                .ToList();

            ViewBag.Citizen = citizen;

            return View(requests);
        }

     
// =========================================================
// POST: CitizenManagement/Activate
// =========================================================

[HttpPost]
[ValidateAntiForgeryToken]
public ActionResult Activate(int id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction(
                    "Login",
                    "Administrators"
                );
            }


            // -----------------------------------------------------
            // Check citizen exists
            // -----------------------------------------------------

            bool citizenExists =
                db.Citizens.Any(c => c.CitizenID == id);

            if (!citizenExists)
            {
                return HttpNotFound();
            }


            // -----------------------------------------------------
            // Restore account status
            // -----------------------------------------------------

            db.Database.ExecuteSqlCommand(
                "UPDATE Citizens SET AccountStatus = @p0 WHERE CitizenID = @p1",
                (int)AccountStatus.Active,
                id
            );


            // -----------------------------------------------------
            // Close any account-status restriction
            // -----------------------------------------------------

            var statusRestriction =
                db.AccountRestrictions.FirstOrDefault(r =>
                    r.CitizenID == id &&
                    r.IsActive &&
                    (r.RestrictionType == "Account Suspension" ||
                     r.RestrictionType == "Account Deactivation")
                );

            if (statusRestriction != null)
            {
                statusRestriction.IsActive = false;
                statusRestriction.DateEnded = DateTime.Now;

                db.SaveChanges();
            }


            return RedirectToAction(
                "Details",
                new { id = id }
            );
        }


        // =========================================================
        // POST: CitizenManagement/Suspend
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Suspend(int id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction(
                    "Login",
                    "Administrators"
                );
            }


            // -----------------------------------------------------
            // Check citizen exists
            // -----------------------------------------------------

            bool citizenExists =
                db.Citizens.Any(c => c.CitizenID == id);

            if (!citizenExists)
            {
                return HttpNotFound();
            }


            int administratorId =
                (int)Session["AdministratorID"];


            // -----------------------------------------------------
            // Update account status
            // -----------------------------------------------------

            db.Database.ExecuteSqlCommand(
                "UPDATE Citizens SET AccountStatus = @p0 WHERE CitizenID = @p1",
                (int)AccountStatus.Suspended,
                id
            );


            // -----------------------------------------------------
            // Check whether an account-status restriction already exists
            // -----------------------------------------------------

            var existingRestriction =
                db.AccountRestrictions.FirstOrDefault(r =>
                    r.CitizenID == id &&
                    r.IsActive &&
                    (r.RestrictionType == "Account Suspension" ||
                     r.RestrictionType == "Account Deactivation")
                );


            // -----------------------------------------------------
            // Create restriction when none exists
            // -----------------------------------------------------

            if (existingRestriction == null)
            {
                var restriction = new AccountRestriction
                {
                    CitizenID = id,
                    AdministratorID = administratorId,
                    RestrictionType = "Account Suspension",
                    Reason = "Account suspended by administrator.",
                    DateStarted = DateTime.Now,
                    DateEnded = null,
                    IsActive = true
                };

                db.AccountRestrictions.Add(restriction);

                db.SaveChanges();
            }


            return RedirectToAction(
                "Details",
                new { id = id }
            );
        }


        // =========================================================
        // POST: CitizenManagement/Deactivate
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deactivate(int id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction(
                    "Login",
                    "Administrators"
                );
            }


            // -----------------------------------------------------
            // Check citizen exists
            // -----------------------------------------------------

            bool citizenExists =
                db.Citizens.Any(c => c.CitizenID == id);

            if (!citizenExists)
            {
                return HttpNotFound();
            }


            int administratorId =
                (int)Session["AdministratorID"];


            // -----------------------------------------------------
            // Update account status
            // -----------------------------------------------------

            db.Database.ExecuteSqlCommand(
                "UPDATE Citizens SET AccountStatus = @p0 WHERE CitizenID = @p1",
                (int)AccountStatus.Inactive,
                id
            );


            // -----------------------------------------------------
            // Check whether an account-status restriction already exists
            // -----------------------------------------------------

            var existingRestriction =
                db.AccountRestrictions.FirstOrDefault(r =>
                    r.CitizenID == id &&
                    r.IsActive &&
                    (r.RestrictionType == "Account Suspension" ||
                     r.RestrictionType == "Account Deactivation")
                );


            // -----------------------------------------------------
            // Create restriction when none exists
            // -----------------------------------------------------

            if (existingRestriction == null)
            {
                var restriction = new AccountRestriction
                {
                    CitizenID = id,
                    AdministratorID = administratorId,
                    RestrictionType = "Account Deactivation",
                    Reason = "Account deactivated by administrator.",
                    DateStarted = DateTime.Now,
                    DateEnded = null,
                    IsActive = true
                };

                db.AccountRestrictions.Add(restriction);

                db.SaveChanges();
            }


            return RedirectToAction(
                "Details",
                new { id = id }
            );
        }



        

        // GET: CitizenManagement/ApplyRestriction/5
        public ActionResult ApplyRestriction(int? id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            if (id == null)
            {
                return RedirectToAction("Index");
            }

            var citizen = db.Citizens
                .FirstOrDefault(c => c.CitizenID == id.Value);

            if (citizen == null)
            {
                return HttpNotFound();
            }

            var complianceRecord = db.ComplianceRecords
                .FirstOrDefault(c => c.CitizenID == id.Value);

            if (complianceRecord == null ||
                complianceRecord.ComplianceStatus != ComplianceStatus.Warning)
            {
                TempData["ErrorMessage"] =
                    "A restriction can only be applied to a citizen whose compliance status is Warning.";

                return RedirectToAction(
                    "Compliance",
                    new { id = id.Value });
            }

            var activeRestriction = db.AccountRestrictions
                .FirstOrDefault(r =>
                    r.CitizenID == id.Value &&
                    r.IsActive);

            if (activeRestriction != null)
            {
                TempData["ErrorMessage"] =
                    "This citizen already has an active restriction.";

                return RedirectToAction(
                    "Compliance",
                    new { id = id.Value });
            }

            ViewBag.Citizen = citizen;
            ViewBag.ComplianceRecord = complianceRecord;

            return View(citizen);
        }


        // POST: CitizenManagement/ApplyRestriction
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApplyRestriction(
            int citizenID,
            string reason)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["ErrorMessage"] =
                    "A restriction reason is required.";

                return RedirectToAction(
                    "ApplyRestriction",
                    new { id = citizenID });
            }

            var citizen = db.Citizens
                .FirstOrDefault(c => c.CitizenID == citizenID);

            if (citizen == null)
            {
                return HttpNotFound();
            }

            var complianceRecord = db.ComplianceRecords
                .FirstOrDefault(c => c.CitizenID == citizenID);

            if (complianceRecord == null ||
                complianceRecord.ComplianceStatus != ComplianceStatus.Warning)
            {
                TempData["ErrorMessage"] =
                    "A restriction can only be applied to a citizen whose compliance status is Warning.";

                return RedirectToAction(
                    "Compliance",
                    new { id = citizenID });
            }

            var activeRestriction = db.AccountRestrictions
                .FirstOrDefault(r =>
                    r.CitizenID == citizenID &&
                    r.IsActive);

            if (activeRestriction != null)
            {
                TempData["ErrorMessage"] =
                    "This citizen already has an active restriction.";

                return RedirectToAction(
                    "Compliance",
                    new { id = citizenID });
            }

            int administratorId = (int)Session["AdministratorID"];

            var restriction = new AccountRestriction
            {
                CitizenID = citizenID,
                AdministratorID = administratorId,
                RestrictionType = "New Request Submission",
                Reason = reason.Trim(),
                DateStarted = DateTime.Now,
                DateEnded = DateTime.Now.AddDays(30),
                IsActive = true
            };

            db.AccountRestrictions.Add(restriction);

            complianceRecord.ComplianceStatus =
                ComplianceStatus.Restricted;

            complianceRecord.LastUpdated =
                DateTime.Now;

            db.SaveChanges();

            TempData["SuccessMessage"] =
                "Account restriction applied successfully. The citizen cannot submit new maintenance requests while the restriction is active.";

            return RedirectToAction(
                "Compliance",
                new { id = citizenID });
        }

        // GET: CitizenManagement/RemoveRestriction/5
        public ActionResult RemoveRestriction(int? id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            if (id == null)
            {
                return RedirectToAction("Index");
            }

            var citizen = db.Citizens
                .FirstOrDefault(c => c.CitizenID == id.Value);

            if (citizen == null)
            {
                return HttpNotFound();
            }

            var activeRestriction = db.AccountRestrictions
                .FirstOrDefault(r =>
                    r.CitizenID == id.Value &&
                    r.IsActive);

            if (activeRestriction == null)
            {
                TempData["ErrorMessage"] =
                    "This citizen does not have an active restriction.";

                return RedirectToAction(
                    "Compliance",
                    new { id = id.Value });
            }

            ViewBag.Citizen = citizen;

            return View(activeRestriction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveRestriction(int id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            var citizen = db.Citizens
                .FirstOrDefault(c => c.CitizenID == id);

            if (citizen == null)
            {
                return HttpNotFound();
            }

            var activeRestriction = db.AccountRestrictions
                .FirstOrDefault(r =>
                    r.CitizenID == id &&
                    r.IsActive);

            if (activeRestriction == null)
            {
                TempData["ErrorMessage"] =
                    "This citizen does not have an active restriction.";

                return RedirectToAction(
                    "Compliance",
                    new { id = id });
            }

            var complianceRecord = db.ComplianceRecords
                .FirstOrDefault(c => c.CitizenID == id);

            // Remove the active restriction
            activeRestriction.IsActive = false;
            activeRestriction.DateEnded = DateTime.Now;

            // Return the citizen's compliance status to Warning
            if (complianceRecord != null)
            {
                complianceRecord.ComplianceStatus =
                    ComplianceStatus.Warning;

                complianceRecord.LastUpdated =
                    DateTime.Now;
            }

            db.SaveChanges();

            TempData["SuccessMessage"] =
                "Account restriction removed successfully. The citizen can now submit new maintenance requests.";

            return RedirectToAction(
                "Compliance",
                new { id = id });
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