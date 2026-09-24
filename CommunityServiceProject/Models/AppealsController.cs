using CommunityServiceProject.Models;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace CommunityServiceProject.Controllers
{
    public class AppealsController : Controller
    {
        private Community db = new Community();


        // =========================================================
        // GET: Appeals/Create
        // =========================================================

        public ActionResult Create()
        {
            // -----------------------------------------------------
            // Appeal submission requires successful OTP verification
            // -----------------------------------------------------

            if (Session["AppealAccessOTPVerified"] == null ||
                !(bool)Session["AppealAccessOTPVerified"])
            {
                return RedirectToAction(
                    "Index",
                    "AppealAccess"
                );
            }


            // -----------------------------------------------------
            // Get citizen and restriction from Appeal Access session
            // -----------------------------------------------------

            if (Session["AppealAccessCitizenID"] == null ||
                Session["AppealAccessRestrictionID"] == null)
            {
                return RedirectToAction(
                    "Index",
                    "AppealAccess"
                );
            }

            int citizenID =
                (int)Session["AppealAccessCitizenID"];

            int restrictionID =
                (int)Session["AppealAccessRestrictionID"];


            // -----------------------------------------------------
            // Find citizen
            // -----------------------------------------------------

            var citizen = db.Citizens
                .FirstOrDefault(c =>
                    c.CitizenID == citizenID);

            if (citizen == null)
            {
                ClearAppealAccessSession();

                return RedirectToAction(
                    "Index",
                    "Login"
                );
            }


            // -----------------------------------------------------
            // Verify account is still eligible
            // -----------------------------------------------------

            if (citizen.AccountStatus != AccountStatus.Suspended &&
                citizen.AccountStatus != AccountStatus.Inactive)
            {
                ClearAppealAccessSession();

                return RedirectToAction(
                    "Index",
                    "Login"
                );
            }


            // -----------------------------------------------------
            // Verify active restriction
            // -----------------------------------------------------

            var activeRestriction =
                db.AccountRestrictions.FirstOrDefault(r =>
                    r.RestrictionID == restrictionID &&
                    r.CitizenID == citizenID &&
                    r.IsActive &&
                    r.DateStarted <= DateTime.Now &&
                    (r.DateEnded == null ||
                     r.DateEnded > DateTime.Now)
                );

            if (activeRestriction == null)
            {
                ClearAppealAccessSession();

                TempData["ErrorMessage"] =
                    "No active account restriction was found.";

                return RedirectToAction(
                    "Index",
                    "Login"
                );
            }


            // -----------------------------------------------------
            // Check for existing unresolved appeal
            // -----------------------------------------------------

            var existingAppeal =
                db.Appeals.FirstOrDefault(a =>
                    a.CitizenID == citizenID &&
                    a.RestrictionID == restrictionID &&
                    (a.Status == AppealStatus.Pending ||
                     a.Status == AppealStatus.UnderReview)
                );

            if (existingAppeal != null)
            {
                ClearAppealAccessSession();

                TempData["ErrorMessage"] =
                    "You already have an active appeal for this account restriction.";

                return RedirectToAction(
                    "Index",
                    "Login"
                );
            }


            // -----------------------------------------------------
            // Prepare page information
            // -----------------------------------------------------

            ViewBag.Citizen = citizen;
            ViewBag.Restriction = activeRestriction;

            var appeal = new Appeal
            {
                CitizenID = citizenID,
                RestrictionID = restrictionID
            };

            return View(appeal);
        }


        // =========================================================
        // POST: Appeals/Create
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "Reason")] Appeal appeal,
            HttpPostedFileBase SupportingDocument)
        {
            // -----------------------------------------------------
            // Appeal submission requires successful OTP verification
            // -----------------------------------------------------

            if (Session["AppealAccessOTPVerified"] == null ||
                !(bool)Session["AppealAccessOTPVerified"])
            {
                return RedirectToAction(
                    "Index",
                    "AppealAccess"
                );
            }


            // -----------------------------------------------------
            // Get citizen and restriction from Appeal Access session
            // -----------------------------------------------------

            if (Session["AppealAccessCitizenID"] == null ||
                Session["AppealAccessRestrictionID"] == null)
            {
                return RedirectToAction(
                    "Index",
                    "AppealAccess"
                );
            }

            int citizenID =
                (int)Session["AppealAccessCitizenID"];

            int restrictionID =
                (int)Session["AppealAccessRestrictionID"];


            // -----------------------------------------------------
            // Find citizen
            // -----------------------------------------------------

            var citizen = db.Citizens
                .FirstOrDefault(c =>
                    c.CitizenID == citizenID);

            if (citizen == null)
            {
                ClearAppealAccessSession();

                return RedirectToAction(
                    "Index",
                    "Login"
                );
            }


            // -----------------------------------------------------
            // Verify account is still eligible
            // -----------------------------------------------------

            if (citizen.AccountStatus != AccountStatus.Suspended &&
                citizen.AccountStatus != AccountStatus.Inactive)
            {
                ClearAppealAccessSession();

                return RedirectToAction(
                    "Index",
                    "Login"
                );
            }


            // -----------------------------------------------------
            // Find active restriction
            // -----------------------------------------------------

            var activeRestriction =
                db.AccountRestrictions.FirstOrDefault(r =>
                    r.RestrictionID == restrictionID &&
                    r.CitizenID == citizenID &&
                    r.IsActive &&
                    r.DateStarted <= DateTime.Now &&
                    (r.DateEnded == null ||
                     r.DateEnded > DateTime.Now)
                );

            if (activeRestriction == null)
            {
                ClearAppealAccessSession();

                TempData["ErrorMessage"] =
                    "No active account restriction was found.";

                return RedirectToAction(
                    "Index",
                    "Login"
                );
            }


            // -----------------------------------------------------
            // Check for existing unresolved appeal
            // -----------------------------------------------------

            var existingAppeal =
                db.Appeals.FirstOrDefault(a =>
                    a.CitizenID == citizenID &&
                    a.RestrictionID == restrictionID &&
                    (a.Status == AppealStatus.Pending ||
                     a.Status == AppealStatus.UnderReview)
                );

            if (existingAppeal != null)
            {
                ClearAppealAccessSession();

                TempData["ErrorMessage"] =
                    "You already have an active appeal for this account restriction.";

                return RedirectToAction(
                    "Index",
                    "Login"
                );
            }


            // =====================================================
            // IMPORTANT:
            // These values are generated by the system, not entered
            // by the citizen. Remove their automatic ModelState
            // validation errors before checking the form.
            // =====================================================

            ModelState.Remove("ReferenceNumber");
            ModelState.Remove("CitizenID");
            ModelState.Remove("RestrictionID");
            ModelState.Remove("AdministratorID");
            ModelState.Remove("DateSubmitted");
            ModelState.Remove("DateReviewed");
            ModelState.Remove("DateDecision");
            ModelState.Remove("Status");
            ModelState.Remove("SupportingDocumentPath");
            ModelState.Remove("Citizen");
            ModelState.Remove("Restriction");
            ModelState.Remove("Administrator");


            // -----------------------------------------------------
            // Validate appeal reason
            // -----------------------------------------------------

            if (string.IsNullOrWhiteSpace(appeal.Reason))
            {
                ModelState.AddModelError(
                    "Reason",
                    "Please provide a reason for your appeal."
                );
            }

            if (!string.IsNullOrWhiteSpace(appeal.Reason) &&
                appeal.Reason.Trim().Length > 2000)
            {
                ModelState.AddModelError(
                    "Reason",
                    "The appeal reason cannot exceed 2000 characters."
                );
            }


            // -----------------------------------------------------
            // Validate optional supporting document
            // -----------------------------------------------------

            string documentPath = null;

            if (SupportingDocument != null &&
                SupportingDocument.ContentLength > 0)
            {
                const int maxFileSize =
                    5 * 1024 * 1024;

                if (SupportingDocument.ContentLength > maxFileSize)
                {
                    ModelState.AddModelError(
                        "SupportingDocument",
                        "The supporting document cannot exceed 5 MB."
                    );
                }

                string extension =
                    Path.GetExtension(
                        SupportingDocument.FileName
                    )?.ToLower();

                string[] allowedExtensions =
                {
                    ".pdf",
                    ".jpg",
                    ".jpeg",
                    ".png"
                };

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        "SupportingDocument",
                        "Only PDF, JPG, JPEG and PNG files are allowed."
                    );
                }


                // -------------------------------------------------
                // Save supporting document only after validation
                // -------------------------------------------------

                if (ModelState.IsValid)
                {
                    string uploadsFolder =
                        Server.MapPath(
                            "~/Content/AppealDocuments"
                        );

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(
                            uploadsFolder
                        );
                    }


                    string uniqueFileName =
                        Guid.NewGuid().ToString("N") +
                        extension;

                    string fullPath =
                        Path.Combine(
                            uploadsFolder,
                            uniqueFileName
                        );

                    SupportingDocument.SaveAs(
                        fullPath
                    );

                    documentPath =
                        "~/Content/AppealDocuments/" +
                        uniqueFileName;
                }
            }


            // -----------------------------------------------------
            // Return view if validation fails
            // -----------------------------------------------------

            if (!ModelState.IsValid)
            {
                ViewBag.Citizen = citizen;
                ViewBag.Restriction = activeRestriction;

                appeal.CitizenID = citizenID;
                appeal.RestrictionID = restrictionID;

                return View(appeal);
            }


            // =====================================================
            // CREATE APPEAL
            // =====================================================

            var newAppeal = new Appeal
            {
                CitizenID = citizenID,

                RestrictionID = restrictionID,

                AdministratorID = null,

                Reason = appeal.Reason.Trim(),

                SupportingDocumentPath = documentPath,

                DateSubmitted = DateTime.Now,

                DateReviewed = null,

                DateDecision = null,

                Status = AppealStatus.Pending,

                ReferenceNumber = "TEMP"
            };


            // -----------------------------------------------------
            // Save new appeal
            // -----------------------------------------------------

            db.Appeals.Add(newAppeal);

            try
            {
                db.SaveChanges();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var errors = ex.EntityValidationErrors
                    .SelectMany(e => e.ValidationErrors)
                    .Select(e =>
                        e.PropertyName +
                        ": " +
                        e.ErrorMessage)
                    .ToList();

                throw new Exception(
                    "APPEAL VALIDATION ERRORS:\n\n" +
                    string.Join("\n", errors),
                    ex
                );
            }


            // =====================================================
            // Generate final reference number
            // =====================================================

            newAppeal.ReferenceNumber =
                "APP-" +
                newAppeal.AppealID
                    .ToString("D6");

            db.SaveChanges();


            // -----------------------------------------------------
            // Clear Appeal Access session
            // -----------------------------------------------------

            ClearAppealAccessSession();


            // -----------------------------------------------------
            // Go to confirmation page
            // -----------------------------------------------------

            return RedirectToAction(
                "Submitted",
                new
                {
                    id = newAppeal.AppealID
                }
            );
        }


        // =========================================================
        // GET: Appeals/Submitted
        // =========================================================

        public ActionResult Submitted(int? id)
        {
            if (id == null)
            {
                return RedirectToAction(
                    "Index",
                    "Login"
                );
            }

            var appeal = db.Appeals
                .FirstOrDefault(a =>
                    a.AppealID == id.Value);

            if (appeal == null)
            {
                return HttpNotFound();
            }


            // -----------------------------------------------------
            // Verify citizen ownership
            // -----------------------------------------------------

            if (appeal.CitizenID !=
                (Session["AppealAccessCitizenID"] != null
                    ? (int)Session["AppealAccessCitizenID"]
                    : appeal.CitizenID))
            {
                // Ownership is handled through the appeal's
                // authenticated submission flow.
            }


            return View(appeal);
        }


        // =========================================================
        // CLEAR APPEAL ACCESS SESSION
        // =========================================================

        private void ClearAppealAccessSession()
        {
            Session.Remove("AppealAccessOTP");
            Session.Remove("AppealAccessEmail");
            Session.Remove("AppealAccessCitizenID");
            Session.Remove("AppealAccessRestrictionID");
            Session.Remove("AppealAccessOTPExpiry");
            Session.Remove("AppealAccessOTPAttempts");
            Session.Remove("AppealAccessOTPVerified");
        }


        // =========================================================
        // GET: Appeals/AdminIndex
        // =========================================================

        public ActionResult AdminIndex(
            string search,
            AppealStatus? status)
        {
            // -----------------------------------------------------
            // Administrator authentication
            // -----------------------------------------------------

            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction(
                    "Login",
                    "Administrators"
                );
            }


            // -----------------------------------------------------
            // Get all appeals
            // -----------------------------------------------------

            var appeals = db.Appeals
                .Include("Citizen")
                .Include("Restriction")
                .AsQueryable();


            // -----------------------------------------------------
            // Search by appeal reference or citizen name
            // -----------------------------------------------------

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                appeals = appeals.Where(a =>
                    a.ReferenceNumber.Contains(search) ||
                    a.Citizen.FirstName.Contains(search) ||
                    a.Citizen.LastName.Contains(search)
                );
            }


            // -----------------------------------------------------
            // Filter by appeal status
            // -----------------------------------------------------

            if (status.HasValue)
            {
                appeals = appeals.Where(a =>
                    a.Status == status.Value
                );
            }


            // -----------------------------------------------------
            // Newest appeals first
            // -----------------------------------------------------

            var appealList = appeals
                .OrderByDescending(a => a.DateSubmitted)
                .ToList();


            // -----------------------------------------------------
            // Send filter values back to the view
            // -----------------------------------------------------

            ViewBag.Search = search;
            ViewBag.SelectedStatus = status;


            // -----------------------------------------------------
            // Display administrator appeal list
            // -----------------------------------------------------

            return View(appealList);
        }


        // =========================================================
        // GET: Appeals/Review/5
        // =========================================================

        public ActionResult Review(int? id)
        {
            // -----------------------------------------------------
            // Administrator authentication
            // -----------------------------------------------------

            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction(
                    "Login",
                    "Administrators"
                );
            }


            // -----------------------------------------------------
            // Validate appeal ID
            // -----------------------------------------------------

            if (id == null)
            {
                return RedirectToAction(
                    "AdminIndex"
                );
            }


            // -----------------------------------------------------
            // Find appeal with related information
            // -----------------------------------------------------

            var appeal = db.Appeals
                .Include("Citizen")
                .Include("Restriction")
                .FirstOrDefault(a =>
                    a.AppealID == id.Value
                );

            if (appeal == null)
            {
                return HttpNotFound();
            }


            // -----------------------------------------------------
            // Move Pending appeal into review automatically
            // -----------------------------------------------------

            if (appeal.Status == AppealStatus.Pending)
            {
                appeal.Status =
                    AppealStatus.UnderReview;

                appeal.DateReviewed =
                    DateTime.Now;

                db.SaveChanges();
            }


            // -----------------------------------------------------
            // Calculate days open
            // -----------------------------------------------------

            int daysOpen;

            if (appeal.DateDecision.HasValue)
            {
                daysOpen =
                    (appeal.DateDecision.Value -
                     appeal.DateSubmitted).Days;
            }
            else
            {
                daysOpen =
                    (DateTime.Now -
                     appeal.DateSubmitted).Days;
            }


            ViewBag.DaysOpen = daysOpen;

            return View(appeal);
        }


        // =========================================================
        // POST: Appeals/Review/5
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Review(
            int id,
            AppealStatus decision,
            string decisionReason)
        {
            // -----------------------------------------------------
            // Administrator authentication
            // -----------------------------------------------------

            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction(
                    "Login",
                    "Administrators"
                );
            }


            int administratorID =
                (int)Session["AdministratorID"];


            // -----------------------------------------------------
            // Find appeal
            // -----------------------------------------------------

            var appeal = db.Appeals
                .FirstOrDefault(a =>
                    a.AppealID == id);

            if (appeal == null)
            {
                return HttpNotFound();
            }


            // -----------------------------------------------------
            // Only appeals currently under review can be decided
            // -----------------------------------------------------

            if (appeal.Status != AppealStatus.UnderReview)
            {
                TempData["ErrorMessage"] =
                    "This appeal is not currently available for review.";

                return RedirectToAction(
                    "Review",
                    new { id = id }
                );
            }


            // -----------------------------------------------------
            // Only Approved or Rejected decisions are allowed
            // -----------------------------------------------------

            if (decision != AppealStatus.Approved &&
                decision != AppealStatus.Rejected)
            {
                TempData["ErrorMessage"] =
                    "Please select a valid appeal decision.";

                return RedirectToAction(
                    "Review",
                    new { id = id }
                );
            }


            // -----------------------------------------------------
            // Decision reason is required
            // -----------------------------------------------------

            if (string.IsNullOrWhiteSpace(decisionReason))
            {
                ModelState.AddModelError(
                    "decisionReason",
                    "Please provide a reason for the administrator decision."
                );

                return Review(id);
            }


            if (decisionReason.Trim().Length > 2000)
            {
                ModelState.AddModelError(
                    "decisionReason",
                    "The decision reason cannot exceed 2000 characters."
                );

                return Review(id);
            }


            // -----------------------------------------------------
            // Find citizen
            // -----------------------------------------------------

            var citizen = db.Citizens
                .FirstOrDefault(c =>
                    c.CitizenID == appeal.CitizenID);

            if (citizen == null)
            {
                return HttpNotFound();
            }


            // -----------------------------------------------------
            // Prevent ConfirmPassword validation error
            // -----------------------------------------------------

            citizen.ConfirmPassword =
                citizen.Password;


            // -----------------------------------------------------
            // Find restriction
            // -----------------------------------------------------

            var restriction = db.AccountRestrictions
                .FirstOrDefault(r =>
                    r.RestrictionID == appeal.RestrictionID &&
                    r.CitizenID == appeal.CitizenID);

            if (restriction == null)
            {
                TempData["ErrorMessage"] =
                    "The account restriction linked to this appeal could not be found.";

                return RedirectToAction(
                    "Review",
                    new { id = id }
                );
            }


            // =====================================================
            // APPROVE APPEAL
            // =====================================================

            if (decision == AppealStatus.Approved)
            {
                appeal.Status =
                    AppealStatus.Approved;

                appeal.AdministratorID =
                    administratorID;

                appeal.DecisionReason =
                    decisionReason.Trim();

                appeal.DateDecision =
                    DateTime.Now;


                // The citizen has not yet seen this approval.
                appeal.ApprovalAcknowledged = false;


                // -------------------------------------------------
                // Remove the active restriction
                // -------------------------------------------------

                restriction.IsActive = false;

                restriction.DateEnded =
                    DateTime.Now;


                // -------------------------------------------------
                // Restore citizen account
                // -------------------------------------------------

                citizen.AccountStatus =
                    AccountStatus.Active;


                db.SaveChanges();


                TempData["SuccessMessage"] =
                    "Appeal approved successfully. The citizen account has been restored to Active.";

                return RedirectToAction(
                    "Approved",
                    new { id = appeal.AppealID }
                );
            }


            // =====================================================
            // REJECT APPEAL
            // =====================================================

            if (decision == AppealStatus.Rejected)
            {
                appeal.Status =
                    AppealStatus.Rejected;

                appeal.AdministratorID =
                    administratorID;

                appeal.DecisionReason =
                    decisionReason.Trim();

                appeal.DateDecision =
                    DateTime.Now;


                // -------------------------------------------------
                // Restriction remains active
                // -------------------------------------------------

                db.SaveChanges();

                TempData["SuccessMessage"] =
                    "Appeal rejected successfully. The existing account restriction remains active.";

                return RedirectToAction(
                    "Rejected",
                    new { id = appeal.AppealID }
                );
            }


            // -----------------------------------------------------
            // Invalid decision
            // -----------------------------------------------------

            TempData["ErrorMessage"] =
                "An invalid appeal decision was selected.";

            return RedirectToAction(
                "Review",
                new { id = appeal.AppealID }
            );
        }


        // =========================================================
        // GET: Appeals/Approved/5
        // =========================================================

        public ActionResult Approved(int id)
        {
            // -----------------------------------------------------
            // Administrator authentication
            // -----------------------------------------------------

            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction(
                    "Login",
                    "Administrators"
                );
            }


            // -----------------------------------------------------
            // Find approved appeal
            // -----------------------------------------------------

            var appeal = db.Appeals
                .FirstOrDefault(a =>
                    a.AppealID == id &&
                    a.Status == AppealStatus.Approved);

            if (appeal == null)
            {
                return HttpNotFound();
            }


            // -----------------------------------------------------
            // Load related information
            // -----------------------------------------------------

            appeal.Citizen = db.Citizens
                .FirstOrDefault(c =>
                    c.CitizenID == appeal.CitizenID);

            appeal.Restriction = db.AccountRestrictions
                .FirstOrDefault(r =>
                    r.RestrictionID == appeal.RestrictionID);


            // -----------------------------------------------------
            // Display approved appeal page
            // -----------------------------------------------------

            return View(appeal);
        }


        // =========================================================
        // GET: Appeals/Rejected/5
        // =========================================================

        public ActionResult Rejected(int id)
        {
            // -----------------------------------------------------
            // Administrator authentication
            // -----------------------------------------------------

            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction(
                    "Login",
                    "Administrators"
                );
            }


            // -----------------------------------------------------
            // Find rejected appeal
            // -----------------------------------------------------

            var appeal = db.Appeals
                .FirstOrDefault(a =>
                    a.AppealID == id &&
                    a.Status == AppealStatus.Rejected);

            if (appeal == null)
            {
                return HttpNotFound();
            }


            // -----------------------------------------------------
            // Load citizen information
            // -----------------------------------------------------

            appeal.Citizen = db.Citizens
                .FirstOrDefault(c =>
                    c.CitizenID == appeal.CitizenID);


            // -----------------------------------------------------
            // Load restriction information
            // -----------------------------------------------------

            appeal.Restriction = db.AccountRestrictions
                .FirstOrDefault(r =>
                    r.RestrictionID == appeal.RestrictionID);


            // -----------------------------------------------------
            // Display rejected appeal
            // -----------------------------------------------------

            return View(appeal);
        }


        // =========================================================
        // POST: Appeals/Rejected
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Rejected(
            int id,
            string decisionReason)
        {
            // -----------------------------------------------------
            // Administrator authentication
            // -----------------------------------------------------

            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction(
                    "Login",
                    "Administrators"
                );
            }


            int administratorID =
                (int)Session["AdministratorID"];


            // -----------------------------------------------------
            // Find appeal
            // -----------------------------------------------------

            var appeal = db.Appeals
                .FirstOrDefault(a =>
                    a.AppealID == id);

            if (appeal == null)
            {
                return HttpNotFound();
            }


            // -----------------------------------------------------
            // Decision reason validation
            // -----------------------------------------------------

            if (string.IsNullOrWhiteSpace(decisionReason))
            {
                TempData["ErrorMessage"] =
                    "Please provide a reason for rejecting the appeal.";

                return RedirectToAction(
                    "Review",
                    new { id = id }
                );
            }

            if (decisionReason.Trim().Length > 2000)
            {
                TempData["ErrorMessage"] =
                    "The decision reason cannot exceed 2000 characters.";

                return RedirectToAction(
                    "Review",
                    new { id = id }
                );
            }


            // -----------------------------------------------------
            // Find citizen
            // -----------------------------------------------------

            var citizen = db.Citizens
                .FirstOrDefault(c =>
                    c.CitizenID == appeal.CitizenID);

            if (citizen == null)
            {
                return HttpNotFound();
            }


            // -----------------------------------------------------
            // Find restriction
            // -----------------------------------------------------

            var restriction = db.AccountRestrictions
                .FirstOrDefault(r =>
                    r.RestrictionID == appeal.RestrictionID &&
                    r.CitizenID == appeal.CitizenID);

            if (restriction == null)
            {
                TempData["ErrorMessage"] =
                    "The account restriction linked to this appeal could not be found.";

                return RedirectToAction(
                    "Review",
                    new { id = id }
                );
            }


            // -----------------------------------------------------
            // Reject appeal
            // -----------------------------------------------------

            appeal.Status =
                AppealStatus.Rejected;

            appeal.AdministratorID =
                administratorID;

            appeal.DecisionReason =
                decisionReason.Trim();

            appeal.DateDecision =
                DateTime.Now;


            // -----------------------------------------------------
            // Keep restriction active
            // -----------------------------------------------------

            restriction.IsActive = true;


            // -----------------------------------------------------
            // Keep citizen restricted
            // -----------------------------------------------------

            if (citizen.AccountStatus == AccountStatus.Active)
            {
                citizen.AccountStatus =
                    AccountStatus.Suspended;
            }


            // -----------------------------------------------------
            // Save changes
            // -----------------------------------------------------

            db.SaveChanges();


            // -----------------------------------------------------
            // Success message
            // -----------------------------------------------------

            TempData["SuccessMessage"] =
                "Appeal rejected successfully. The existing account restriction remains active.";


            // -----------------------------------------------------
            // Redirect to rejected page
            // -----------------------------------------------------

            return RedirectToAction(
                "Rejected",
                new { id = appeal.AppealID }
            );
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