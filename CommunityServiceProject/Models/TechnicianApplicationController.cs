using System;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using CommunityServiceProject.Models;
using CommunityServiceProject.ViewModels;

namespace CommunityServiceProject.Controllers
{
    public class TechnicianApplicationController : Controller
    {
        private readonly Community db =
            new Community();


        // ============================================================
        // AUTHENTICATION
        // ============================================================

        private bool IsCitizen()
        {
            return Session["CitizenID"] != null;
        }


        private int? GetCitizenID()
        {
            if (Session["CitizenID"] == null)
                return null;

            int citizenID;

            if (int.TryParse(
                Session["CitizenID"].ToString(),
                out citizenID))
            {
                return citizenID;
            }

            return null;
        }

        // ============================================================
        // CREATE APPLICATION - GET
        // ============================================================

        [HttpGet]
        public ActionResult Create(int? opportunityID)
        {
            if (!IsCitizen())
                return new HttpUnauthorizedResult();

            if (!opportunityID.HasValue)
                return HttpNotFound();

            var citizenID = GetCitizenID();

            if (!citizenID.HasValue)
                return new HttpUnauthorizedResult();

            var today = DateTime.Today;


            // --------------------------------------------------------
            // FIND OPPORTUNITY
            // --------------------------------------------------------

            var opportunity =
                db.TechnicianOpportunities
                    .AsNoTracking()
                    .FirstOrDefault(o =>
                        o.OpportunityID == opportunityID.Value);

            if (opportunity == null)
                return HttpNotFound();


            // --------------------------------------------------------
            // OPPORTUNITY STATUS
            // --------------------------------------------------------

            if (opportunity.Status !=
                TechnicianOpportunityStatus.Published)
            {
                TempData["ErrorMessage"] =
                    "This opportunity is no longer available for applications.";

                return RedirectToAction(
                    "Details",
                    "TechnicianOpportunity",
                    new
                    {
                        id = opportunity.OpportunityID
                    });
            }


            // --------------------------------------------------------
            // APPLICATION PERIOD
            // --------------------------------------------------------

            if (opportunity.ApplicationStartDate.Date > today)
            {
                TempData["ErrorMessage"] =
                    "Applications for this opportunity have not opened yet.";

                return RedirectToAction(
                    "Details",
                    "TechnicianOpportunity",
                    new
                    {
                        id = opportunity.OpportunityID
                    });
            }

            if (opportunity.ApplicationDeadline.Date < today)
            {
                TempData["ErrorMessage"] =
                    "The application deadline for this opportunity has passed.";

                return RedirectToAction(
                    "Details",
                    "TechnicianOpportunity",
                    new
                    {
                        id = opportunity.OpportunityID
                    });
            }


            // --------------------------------------------------------
            // CHECK FOR EXISTING APPLICATION
            // --------------------------------------------------------

            var existingApplication =
                db.TechnicianApplications
                    .AsNoTracking()
                    .FirstOrDefault(a =>
                        a.OpportunityID ==
                            opportunity.OpportunityID &&
                        a.CitizenID ==
                            citizenID.Value);

            if (existingApplication != null)
            {
                // ----------------------------------------------------
                // EXISTING DRAFT
                // ----------------------------------------------------

                if (existingApplication.Status ==
                    TechnicianApplicationStatus.Draft)
                {
                    TempData["SuccessMessage"] =
                        "You already have a draft application for this opportunity. " +
                        "Continue your application by reviewing your details, " +
                        "uploading your supporting documents, and submitting it.";

                    return RedirectToAction(
                        "Status",
                        new
                        {
                            id = existingApplication.ApplicationID
                        });
                }


                // ----------------------------------------------------
                // EXISTING SUBMITTED APPLICATION
                // ----------------------------------------------------

                TempData["ErrorMessage"] =
                    "You have already submitted an application for this opportunity. " +
                    "You can view its current status below.";

                return RedirectToAction(
                    "Status",
                    new
                    {
                        id = existingApplication.ApplicationID
                    });
            }


            // --------------------------------------------------------
            // CREATE VIEW MODEL
            // --------------------------------------------------------

            var model =
                new TechnicianApplicationCreateViewModel
                {
                    OpportunityID =
                        opportunity.OpportunityID,

                    OpportunityCode =
                        opportunity.OpportunityCode,

                    OpportunityTitle =
                        opportunity.Title,

                    EmploymentType =
                        string.IsNullOrWhiteSpace(
                            opportunity.EmploymentType)
                            ? "Not specified"
                            : opportunity.EmploymentType,

                    ApplicationDeadline =
                        opportunity.ApplicationDeadline,

                    NumberOfPositions =
                        opportunity.NumberOfPositions
                };

            return View(model);
        }

        // ============================================================
        // CREATE APPLICATION - POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            TechnicianApplicationCreateViewModel model)
        {
            if (!IsCitizen())
                return new HttpUnauthorizedResult();

            if (model == null)
                return HttpNotFound();

            var citizenID = GetCitizenID();

            if (!citizenID.HasValue)
                return new HttpUnauthorizedResult();


            // --------------------------------------------------------
            // CLEAN INPUT
            // --------------------------------------------------------

            model.CoverLetter =
                model.CoverLetter?.Trim();


            // --------------------------------------------------------
            // FIND OPPORTUNITY
            // --------------------------------------------------------

            var opportunity =
                db.TechnicianOpportunities
                    .FirstOrDefault(o =>
                        o.OpportunityID ==
                            model.OpportunityID);

            if (opportunity == null)
                return HttpNotFound();

            var today = DateTime.Today;


            // --------------------------------------------------------
            // VALIDATE OPPORTUNITY STATUS
            // --------------------------------------------------------

            if (opportunity.Status !=
                TechnicianOpportunityStatus.Published)
            {
                ModelState.AddModelError(
                    "",
                    "This opportunity is no longer available for applications.");
            }


            // --------------------------------------------------------
            // VALIDATE APPLICATION START DATE
            // --------------------------------------------------------

            if (opportunity.ApplicationStartDate.Date > today)
            {
                ModelState.AddModelError(
                    "",
                    "Applications for this opportunity have not opened yet.");
            }


            // --------------------------------------------------------
            // VALIDATE APPLICATION DEADLINE
            // --------------------------------------------------------

            if (opportunity.ApplicationDeadline.Date < today)
            {
                ModelState.AddModelError(
                    "",
                    "The application deadline for this opportunity has passed.");
            }


            // --------------------------------------------------------
            // CHECK FOR EXISTING APPLICATION
            // --------------------------------------------------------

            var existingApplication =
                db.TechnicianApplications
                    .FirstOrDefault(a =>
                        a.OpportunityID ==
                            opportunity.OpportunityID &&
                        a.CitizenID ==
                            citizenID.Value);

            if (existingApplication != null)
            {
                // ----------------------------------------------------
                // EXISTING DRAFT
                // ----------------------------------------------------

                if (existingApplication.Status ==
                    TechnicianApplicationStatus.Draft)
                {
                    TempData["SuccessMessage"] =
                        "You already have a draft application for this opportunity. " +
                        "Continue your application by reviewing your details, " +
                        "uploading your supporting documents, and submitting it.";

                    return RedirectToAction(
                        "Status",
                        new
                        {
                            id =
                                existingApplication.ApplicationID
                        });
                }


                // ----------------------------------------------------
                // EXISTING SUBMITTED APPLICATION
                // ----------------------------------------------------

                TempData["ErrorMessage"] =
                    "You have already submitted an application for this opportunity. " +
                    "You can view its current status below.";

                return RedirectToAction(
                    "Status",
                    new
                    {
                        id =
                            existingApplication.ApplicationID
                    });
            }


            // --------------------------------------------------------
            // RETURN FORM IF VALIDATION FAILED
            // --------------------------------------------------------

            if (!ModelState.IsValid)
            {
                model.OpportunityCode =
                    opportunity.OpportunityCode;

                model.OpportunityTitle =
                    opportunity.Title;

                model.EmploymentType =
                    string.IsNullOrWhiteSpace(
                        opportunity.EmploymentType)
                        ? "Not specified"
                        : opportunity.EmploymentType;

                model.ApplicationDeadline =
                    opportunity.ApplicationDeadline;

                model.NumberOfPositions =
                    opportunity.NumberOfPositions;

                return View(model);
            }


            // --------------------------------------------------------
            // CREATE APPLICATION AS DRAFT
            // --------------------------------------------------------

            var application =
                new TechnicianApplication
                {
                    ApplicationReference =
                        "TEMP",

                    OpportunityID =
                        opportunity.OpportunityID,

                    CitizenID =
                        citizenID.Value,

                    ApplicationDate =
                        DateTime.Now,

                    Status =
                        TechnicianApplicationStatus.Draft,

                    CoverLetter =
                        model.CoverLetter,

                    LastUpdatedDate =
                        DateTime.Now
                };

            db.TechnicianApplications.Add(
                application);

            db.SaveChanges();


            // --------------------------------------------------------
            // GENERATE APPLICATION REFERENCE
            // --------------------------------------------------------

            application.ApplicationReference =
                "APP-" +
                DateTime.Now.Year +
                "-" +
                application.ApplicationID.ToString("D6");

            db.SaveChanges();


            // --------------------------------------------------------
            // REDIRECT TO APPLICATION STATUS
            // --------------------------------------------------------

            TempData["SuccessMessage"] =
                "Your application has been saved as a draft. " +
                "Continue your application by reviewing your details, " +
                "uploading at least one supporting document, and submitting " +
                "the completed application before the deadline.";

            return RedirectToAction(
                "Status",
                new
                {
                    id =
                        application.ApplicationID
                });
        }

        // ============================================================
        // US110 - VIEW APPLICATION STATUS
        // ============================================================

        [HttpGet]
        public ActionResult Index(
            string searchTerm,
            string statusFilter)
        {
            if (!IsCitizen())
                return new HttpUnauthorizedResult();

            var citizenID = GetCitizenID();

            if (!citizenID.HasValue)
                return new HttpUnauthorizedResult();

            var query =
                db.TechnicianApplications
                    .AsNoTracking()
                    .Include(a => a.Opportunity)
                    .Where(a =>
                        a.CitizenID ==
                        citizenID.Value);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();

                query = query.Where(a =>
                    a.ApplicationReference.Contains(searchTerm) ||
                    a.Opportunity.Title.Contains(searchTerm) ||
                    a.Opportunity.OpportunityCode.Contains(searchTerm));
            }

            TechnicianApplicationStatus selectedStatus;

            if (!string.IsNullOrWhiteSpace(statusFilter) &&
                Enum.TryParse(
                    statusFilter,
                    true,
                    out selectedStatus))
            {
                query = query.Where(
                    a => a.Status == selectedStatus);
            }

            var applications =
                query
                    .OrderByDescending(a => a.ApplicationDate)
                    .ToList();

            var model =
                new TechnicianApplicationStatusViewModel
                {
                    SearchTerm = searchTerm,
                    StatusFilter = statusFilter,
                    TotalApplications =
                        applications.Count,

                    ActiveApplications =
                        applications.Count(
                            IsActiveApplication),

                    SelectedApplications =
                        applications.Count(
                            a =>
                                a.Status ==
                                TechnicianApplicationStatus.Selected ||
                                a.Status ==
                                TechnicianApplicationStatus
                                    .ApprovedForOnboarding ||
                                a.Status ==
                                TechnicianApplicationStatus.Onboarded),

                    UnsuccessfulApplications =
                        applications.Count(
                            a =>
                                a.Status ==
                                TechnicianApplicationStatus.NotSelected)
                };

            foreach (var application in applications)
            {
                var isSuccessful =
                    application.Status ==
                        TechnicianApplicationStatus.Selected ||
                    application.Status ==
                        TechnicianApplicationStatus
                            .ApprovedForOnboarding ||
                    application.Status ==
                        TechnicianApplicationStatus.Onboarded;

                var isUnsuccessful =
                    application.Status ==
                        TechnicianApplicationStatus.NotSelected;

                model.Applications.Add(
                    new TechnicianApplicationStatusItemViewModel
                    {
                        ApplicationID =
                            application.ApplicationID,

                        ApplicationReference =
                            application.ApplicationReference,

                        OpportunityID =
                            application.OpportunityID,

                        OpportunityCode =
                            application.Opportunity.OpportunityCode,

                        OpportunityTitle =
                            application.Opportunity.Title,

                        EmploymentType =
                            string.IsNullOrWhiteSpace(
                                application.Opportunity.EmploymentType)
                                ? "Not specified"
                                : application.Opportunity.EmploymentType,

                        ApplicationDate =
                            application.ApplicationDate,

                        ApplicationDeadline =
                            application.Opportunity
                                .ApplicationDeadline,

                        Status =
                            application.Status,

                        IsActive =
                            IsActiveApplication(
                                application),

                        IsSuccessful =
                            isSuccessful,

                        IsUnsuccessful =
                            isUnsuccessful
                    });
            }

            return View(model);
        }

        private bool IsActiveApplication(
    TechnicianApplication application)
        {
            return application.Status !=
                       TechnicianApplicationStatus.NotSelected &&
                   application.Status !=
                       TechnicianApplicationStatus.Withdrawn &&
                   application.Status !=
                       TechnicianApplicationStatus.Onboarded;
        }

       
          [HttpGet]
  public ActionResult Status(int? id)
        {
            if (!IsCitizen())
                return new HttpUnauthorizedResult();

            if (!id.HasValue)
                return HttpNotFound();

            var citizenID = GetCitizenID();

            if (!citizenID.HasValue)
                return new HttpUnauthorizedResult();

            var application =
                db.TechnicianApplications
                    .AsNoTracking()
                    .Include(a => a.Opportunity)
                    .Include(a => a.ApplicationDocuments)
                    .FirstOrDefault(a =>
                        a.ApplicationID == id.Value &&
                        a.CitizenID == citizenID.Value);

            if (application == null)
                return HttpNotFound();

            var isSuccessful =
                application.Status ==
                    TechnicianApplicationStatus.Selected ||

                application.Status ==
                    TechnicianApplicationStatus.ApprovedForOnboarding ||

                application.Status ==
                    TechnicianApplicationStatus.Onboarded;

            var isUnsuccessful =
                application.Status ==
                    TechnicianApplicationStatus.NotSelected;

            var hasDocuments =
                application.ApplicationDocuments != null &&
                application.ApplicationDocuments.Any();

            var isEditable =
                application.Status ==
                    TechnicianApplicationStatus.Draft &&
                DateTime.Today <=
                    application.Opportunity.ApplicationDeadline.Date;

            var canSubmit =
                application.Status ==
                    TechnicianApplicationStatus.Draft &&

                hasDocuments &&

                DateTime.Today <=
                    application.Opportunity.ApplicationDeadline.Date;

            var technicianOnboardings =
                db.TechnicianOnboardings
                    .AsNoTracking()
                    .Include(o => o.Technician)
                    .Where(o =>
                        o.ApplicationID ==
                        application.ApplicationID)
                    .OrderBy(o => o.OnboardingDate)
                    .ToList();

            var onboardingModels =
                technicianOnboardings
                    .Select(o => new TechnicianOnboardingStatusViewModel
                    {
                        OnboardingID =
                            o.OnboardingID,

                        TechnicianID =
                            o.TechnicianID,

                        TechnicianName =
                            o.Technician != null
                                ? (
                                    o.Technician.FirstName +
                                    " " +
                                    o.Technician.LastName
                                  ).Trim()
                                : "Technician",

                        MunicipalEmail =
                            o.MunicipalEmail,

                        AccountStatus =
                            o.Technician != null
                                ? o.Technician.AccountStatus.ToString()
                                : "Unknown",

                        OnboardingDate =
                            o.OnboardingDate
                    })
                    .ToList();

            var model =
                new TechnicianApplicationDetailsViewModel
                {
                    ApplicationID =
                        application.ApplicationID,

                    OpportunityID =
                        application.OpportunityID,

                    ApplicationReference =
                        application.ApplicationReference,

                    OpportunityCode =
                        application.Opportunity.OpportunityCode,

                    OpportunityTitle =
                        application.Opportunity.Title,

                    EmploymentType =
                        string.IsNullOrWhiteSpace(
                            application.Opportunity.EmploymentType)
                            ? "Not specified"
                            : application.Opportunity.EmploymentType,

                    NumberOfPositions =
                        application.Opportunity.NumberOfPositions,

                    ApplicationDate =
                        application.ApplicationDate,

                    ApplicationDeadline =
                        application.Opportunity.ApplicationDeadline,

                    Status =
                        application.Status,

                    CoverLetter =
                        application.CoverLetter,

                    LastUpdatedDate =
                        application.LastUpdatedDate,

                    IsSuccessful =
                        isSuccessful,

                    IsUnsuccessful =
                        isUnsuccessful,

                    IsEditable =
                        isEditable,

                    HasDocuments =
                        hasDocuments,

                    DocumentCount =
                        application.ApplicationDocuments == null
                            ? 0
                            : application.ApplicationDocuments.Count,

                    CanSubmit =
                        canSubmit,

                    TechnicianOnboardings =
                        onboardingModels
                };

            return View(model);
        }



       

        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if (!IsCitizen())
                return new HttpUnauthorizedResult();

            if (!id.HasValue)
                return HttpNotFound();

            var citizenID = GetCitizenID();

            if (!citizenID.HasValue)
                return new HttpUnauthorizedResult();

            var application =
                db.TechnicianApplications
                    .AsNoTracking()
                    .Include(a => a.Opportunity)
                    .FirstOrDefault(a =>
                        a.ApplicationID == id.Value &&
                        a.CitizenID == citizenID.Value);

            if (application == null)
                return HttpNotFound();

            // -------------------------------------------------------
            // Only Draft applications can be edited
            // -------------------------------------------------------

            if (application.Status !=
                TechnicianApplicationStatus.Draft)
            {
                TempData["ErrorMessage"] =
                    "Only draft applications can be edited.";

                return RedirectToAction(
                    "Status",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            // -------------------------------------------------------
            // Check application deadline
            // -------------------------------------------------------

            if (DateTime.Today >
                application.Opportunity.ApplicationDeadline.Date)
            {
                TempData["ErrorMessage"] =
                    "This application can no longer be edited because the application deadline has passed.";

                return RedirectToAction(
                    "Status",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            var model =
                new TechnicianApplicationEditViewModel
                {
                    ApplicationID =
                        application.ApplicationID,

                    ApplicationReference =
                        application.ApplicationReference,

                    OpportunityCode =
                        application.Opportunity.OpportunityCode,

                    OpportunityTitle =
                        application.Opportunity.Title,

                    EmploymentType =
                        string.IsNullOrWhiteSpace(
                            application.Opportunity.EmploymentType)
                            ? "Not specified"
                            : application.Opportunity.EmploymentType,

                    CoverLetter =
                        application.CoverLetter
                };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
    TechnicianApplicationEditViewModel model)
        {
            if (!IsCitizen())
                return new HttpUnauthorizedResult();

            var citizenID = GetCitizenID();

            if (!citizenID.HasValue)
                return new HttpUnauthorizedResult();

            if (model == null)
                return HttpNotFound();

            var application =
                db.TechnicianApplications
                    .Include(a => a.Opportunity)
                    .FirstOrDefault(a =>
                        a.ApplicationID == model.ApplicationID &&
                        a.CitizenID == citizenID.Value);

            if (application == null)
                return HttpNotFound();

            // -------------------------------------------------------
            // Only Draft applications can be edited
            // -------------------------------------------------------

            if (application.Status !=
                TechnicianApplicationStatus.Draft)
            {
                TempData["ErrorMessage"] =
                    "Only draft applications can be edited.";

                return RedirectToAction(
                    "Status",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            // -------------------------------------------------------
            // Check application deadline
            // -------------------------------------------------------

            if (DateTime.Today >
                application.Opportunity.ApplicationDeadline.Date)
            {
                TempData["ErrorMessage"] =
                    "This application can no longer be edited because the application deadline has passed.";

                return RedirectToAction(
                    "Status",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            // -------------------------------------------------------
            // Clean cover letter
            // -------------------------------------------------------

            model.CoverLetter =
                string.IsNullOrWhiteSpace(model.CoverLetter)
                    ? null
                    : model.CoverLetter.Trim();

            if (string.IsNullOrWhiteSpace(model.CoverLetter))
            {
                ModelState.AddModelError(
                    "CoverLetter",
                    "Please provide a cover letter."
                );
            }

            // -------------------------------------------------------
            // Validate model
            // -------------------------------------------------------

            if (!ModelState.IsValid)
            {
                model.ApplicationReference =
                    application.ApplicationReference;

                model.OpportunityCode =
                    application.Opportunity.OpportunityCode;

                model.OpportunityTitle =
                    application.Opportunity.Title;

                model.EmploymentType =
                    string.IsNullOrWhiteSpace(
                        application.Opportunity.EmploymentType)
                        ? "Not specified"
                        : application.Opportunity.EmploymentType;

                return View(model);
            }

            // -------------------------------------------------------
            // Update draft
            // -------------------------------------------------------

            application.CoverLetter =
                model.CoverLetter;

            application.LastUpdatedDate =
                DateTime.Now;

            db.SaveChanges();

            TempData["SuccessMessage"] =
                "Your draft application was updated successfully.";

            return RedirectToAction(
                "Status",
                new
                {
                    id = application.ApplicationID
                });
        }

       

        [HttpGet]
        public ActionResult Documents(int? id)
        {
            if (!IsCitizen())
                return new HttpUnauthorizedResult();

            if (!id.HasValue)
                return HttpNotFound();

            var citizenID = GetCitizenID();

            if (!citizenID.HasValue)
                return new HttpUnauthorizedResult();

            var application =
                db.TechnicianApplications
                    .AsNoTracking()
                    .Include(a => a.Opportunity)
                    .Include(a =>
                        a.ApplicationDocuments.Select(
                            d => d.VerifiedByAdministrator))
                    .FirstOrDefault(a =>
                        a.ApplicationID == id.Value &&
                        a.CitizenID == citizenID.Value);

            if (application == null)
                return HttpNotFound();

            ViewBag.Application = application;

            ViewBag.CanUpload =
                application.Status ==
                    TechnicianApplicationStatus.Draft &&
                DateTime.Today <=
                    application.Opportunity.ApplicationDeadline.Date;

            ViewBag.CanUpdateDocuments =
    application.Status ==
        TechnicianApplicationStatus.DocumentsPending;

            ViewBag.RejectedDocumentCount =
    application.ApplicationDocuments
        .Count(d =>
            d.VerificationStatus ==
            ApplicationDocumentVerificationStatus.Rejected);

            return View(
                application.ApplicationDocuments
                    .OrderByDescending(d => d.DateSubmitted)
                    .ToList()
            );
        }


        // =======================================================
        // UPLOAD DOCUMENT - GET
        // =======================================================

        [HttpGet]
        public ActionResult UploadDocument(int? id)
        {
            if (!IsCitizen())
                return new HttpUnauthorizedResult();

            if (!id.HasValue)
                return HttpNotFound();

            var citizenID = GetCitizenID();

            if (!citizenID.HasValue)
                return new HttpUnauthorizedResult();

            var application =
                db.TechnicianApplications
                    .AsNoTracking()
                    .Include(a => a.Opportunity)
                    .FirstOrDefault(a =>
                        a.ApplicationID == id.Value &&
                        a.CitizenID == citizenID.Value);

            if (application == null)
                return HttpNotFound();

            if (application.Status !=
                TechnicianApplicationStatus.Draft)
            {
                TempData["ErrorMessage"] =
                    "Supporting documents can only be uploaded while your application is in Draft status.";

                return RedirectToAction(
                    "Documents",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            if (DateTime.Today >
                application.Opportunity.ApplicationDeadline.Date)
            {
                TempData["ErrorMessage"] =
                    "Supporting documents can no longer be uploaded because the application deadline has passed.";

                return RedirectToAction(
                    "Documents",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            var model =
                new ApplicationDocumentCreateViewModel
                {
                    ApplicationID =
                        application.ApplicationID,

                    ApplicationReference =
                        application.ApplicationReference,

                    OpportunityCode =
                        application.Opportunity.OpportunityCode,

                    OpportunityTitle =
                        application.Opportunity.Title
                };

            return View(model);
        }


        // =======================================================
        // UPLOAD DOCUMENT - POST
        // =======================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadDocument(
            ApplicationDocumentCreateViewModel model)
        {
            if (!IsCitizen())
                return new HttpUnauthorizedResult();

            var citizenID = GetCitizenID();

            if (!citizenID.HasValue)
                return new HttpUnauthorizedResult();

            if (model == null)
                return HttpNotFound();

            var application =
                db.TechnicianApplications
                    .Include(a => a.Opportunity)
                    .FirstOrDefault(a =>
                        a.ApplicationID == model.ApplicationID &&
                        a.CitizenID == citizenID.Value);

            if (application == null)
                return HttpNotFound();

            if (application.Status !=
                TechnicianApplicationStatus.Draft)
            {
                TempData["ErrorMessage"] =
                    "Supporting documents can only be uploaded while your application is in Draft status.";

                return RedirectToAction(
                    "Documents",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            if (DateTime.Today >
                application.Opportunity.ApplicationDeadline.Date)
            {
                TempData["ErrorMessage"] =
                    "Supporting documents can no longer be uploaded because the application deadline has passed.";

                return RedirectToAction(
                    "Documents",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            // ---------------------------------------------------
            // CLEAN INPUT
            // ---------------------------------------------------

            model.DocumentTitle =
                model.DocumentTitle?.Trim();

            model.DocumentType =
                model.DocumentType?.Trim();


            // ---------------------------------------------------
            // VALIDATE FILE
            // ---------------------------------------------------

            if (model.DocumentFile == null ||
                model.DocumentFile.ContentLength <= 0)
            {
                ModelState.AddModelError(
                    "DocumentFile",
                    "Please select a document to upload.");
            }

            const int maxFileSize =
                10 * 1024 * 1024;

            if (model.DocumentFile != null &&
                model.DocumentFile.ContentLength > maxFileSize)
            {
                ModelState.AddModelError(
                    "DocumentFile",
                    "The document cannot exceed 10 MB.");
            }

            var allowedExtensions =
                new[]
                {
            ".pdf",
            ".doc",
            ".docx",
            ".jpg",
            ".jpeg",
            ".png"
                };

            if (model.DocumentFile != null &&
                model.DocumentFile.ContentLength > 0)
            {
                var extension =
                    System.IO.Path
                        .GetExtension(
                            model.DocumentFile.FileName)
                        ?.ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        "DocumentFile",
                        "Only PDF, DOC, DOCX, JPG, JPEG, and PNG files are allowed.");
                }
            }


            // ---------------------------------------------------
            // RETURN TO FORM IF INVALID
            // ---------------------------------------------------

            if (!ModelState.IsValid)
            {
                model.ApplicationReference =
                    application.ApplicationReference;

                model.OpportunityCode =
                    application.Opportunity.OpportunityCode;

                model.OpportunityTitle =
                    application.Opportunity.Title;

                return View(model);
            }


            // ---------------------------------------------------
            // CREATE UPLOAD DIRECTORY
            // ---------------------------------------------------

            var extensionName =
                System.IO.Path
                    .GetExtension(
                        model.DocumentFile.FileName)
                    .ToLowerInvariant();

            var uploadFolder =
                Server.MapPath(
                    "~/Uploads/ApplicationDocuments");

            if (!System.IO.Directory.Exists(uploadFolder))
            {
                System.IO.Directory.CreateDirectory(
                    uploadFolder);
            }


            // ---------------------------------------------------
            // GENERATE UNIQUE FILE NAME
            // ---------------------------------------------------

            var uniqueFileName =
                Guid.NewGuid().ToString("N") +
                extensionName;

            var physicalPath =
                System.IO.Path.Combine(
                    uploadFolder,
                    uniqueFileName);


            // ---------------------------------------------------
            // SAVE FILE
            // ---------------------------------------------------

            model.DocumentFile.SaveAs(
                physicalPath);


            // ---------------------------------------------------
            // CREATE DATABASE RECORD
            // ---------------------------------------------------

            var document =
                new ApplicationDocument
                {
                    ApplicationID =
                        application.ApplicationID,

                    DocumentType =
                        model.DocumentType,

                    DocumentTitle =
                        model.DocumentTitle,

                    FileName =
                        System.IO.Path.GetFileName(
                            model.DocumentFile.FileName),

                    FilePath =
                        "~/Uploads/ApplicationDocuments/" +
                        uniqueFileName,

                    ContentType =
                        model.DocumentFile.ContentType,

                    FileSize =
                        model.DocumentFile.ContentLength,

                    DateSubmitted =
                        DateTime.Now,

                    VerificationStatus =
                        ApplicationDocumentVerificationStatus.Pending
                };

            db.ApplicationDocuments.Add(document);

            application.LastUpdatedDate =
                DateTime.Now;

            db.SaveChanges();


            // ---------------------------------------------------
            // SUCCESS
            // ---------------------------------------------------

            TempData["SuccessMessage"] =
                "Supporting document uploaded successfully. " +
                "Your application remains in Draft status until you submit it.";

            return RedirectToAction(
                "Documents",
                new
                {
                    id = application.ApplicationID
                });
        }


        // =======================================================
        // VIEW / DOWNLOAD DOCUMENT
        // =======================================================

        [HttpGet]
        public ActionResult ViewDocument(int? id)
        {
            if (!IsCitizen())
                return new HttpUnauthorizedResult();

            if (!id.HasValue)
                return HttpNotFound();

            var citizenID = GetCitizenID();

            if (!citizenID.HasValue)
                return new HttpUnauthorizedResult();

            var document =
                db.ApplicationDocuments
                    .Include(d => d.Application)
                    .FirstOrDefault(d =>
                        d.ApplicationDocumentID == id.Value &&
                        d.Application.CitizenID == citizenID.Value);

            if (document == null)
                return HttpNotFound();

            if (string.IsNullOrWhiteSpace(
                document.FilePath))
            {
                return HttpNotFound();
            }

            var physicalPath =
                Server.MapPath(
                    document.FilePath);

            if (string.IsNullOrWhiteSpace(physicalPath) ||
                !System.IO.File.Exists(physicalPath))
            {
                return HttpNotFound();
            }

            return File(
                physicalPath,
                string.IsNullOrWhiteSpace(document.ContentType)
                    ? "application/octet-stream"
                    : document.ContentType,
                document.FileName);
        }




        
[HttpPost]
[ValidateAntiForgeryToken]
public ActionResult Submit(int? id)
        {
            if (!IsCitizen())
                return new HttpUnauthorizedResult();

            if (!id.HasValue)
                return HttpNotFound();

            var citizenID = GetCitizenID();

            if (!citizenID.HasValue)
                return new HttpUnauthorizedResult();

            var application =
                db.TechnicianApplications
                    .Include(a => a.Opportunity)
                    .Include(a => a.ApplicationDocuments)
                    .FirstOrDefault(a =>
                        a.ApplicationID == id.Value &&
                        a.CitizenID == citizenID.Value);

            if (application == null)
                return HttpNotFound();

            // -------------------------------------------------------
            // Only Draft applications can be submitted
            // -------------------------------------------------------

            if (application.Status !=
                TechnicianApplicationStatus.Draft)
            {
                TempData["ErrorMessage"] =
                    "This application can no longer be submitted.";

                return RedirectToAction(
                    "Status",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            // -------------------------------------------------------
            // Check deadline
            // -------------------------------------------------------

            if (DateTime.Today >
                application.Opportunity.ApplicationDeadline.Date)
            {
                TempData["ErrorMessage"] =
                    "The application deadline has passed. " +
                    "This application can no longer be submitted.";

                return RedirectToAction(
                    "Status",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            // -------------------------------------------------------
            // Check opportunity is still published
            // -------------------------------------------------------

            if (application.Opportunity.Status !=
                TechnicianOpportunityStatus.Published)
            {
                TempData["ErrorMessage"] =
                    "This opportunity is no longer accepting applications.";

                return RedirectToAction(
                    "Status",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            // -------------------------------------------------------
            // Check cover letter
            // -------------------------------------------------------

            if (string.IsNullOrWhiteSpace(
                application.CoverLetter))
            {
                TempData["ErrorMessage"] =
                    "Please complete your cover letter before submitting your application.";

                return RedirectToAction(
                    "Status",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            // -------------------------------------------------------
            // Check supporting documents
            // -------------------------------------------------------

            var hasDocuments =
                application.ApplicationDocuments != null &&
                application.ApplicationDocuments.Any();

            if (!hasDocuments)
            {
                TempData["ErrorMessage"] =
                    "You must upload at least one supporting document before submitting your application.";

                return RedirectToAction(
                    "Status",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            // -------------------------------------------------------
            // FINAL SUBMISSION
            // -------------------------------------------------------

            application.Status =
                TechnicianApplicationStatus.Submitted;

            application.LastUpdatedDate =
                DateTime.Now;

            // -------------------------------------------------------
            // CREATE CITIZEN RECRUITMENT NOTIFICATION
            // -------------------------------------------------------

            var notificationService =
                new CommunityServiceProject.Services
                    .TechnicianApplicationNotificationService(db);

            notificationService.Create(
                application,
                TechnicianApplicationNotificationType.ApplicationSubmitted,
                "Application Submitted",
                "Your application for " +
                application.Opportunity.Title +
                " has been submitted successfully. " +
                "You can no longer edit the application while it is under review."
            );

            // -------------------------------------------------------
            // SAVE APPLICATION + NOTIFICATION TOGETHER
            // -------------------------------------------------------

            db.SaveChanges();

            TempData["SuccessMessage"] =
                "Your application has been submitted successfully. " +
                "You can no longer edit the application while it is under review.";

            return RedirectToAction(
                "Status",
                new
                {
                    id = application.ApplicationID
                });
        }





        [HttpGet]
        public ActionResult Review(int? id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction(
                    "Login",
                    "Administrators");
            }

            if (!id.HasValue)
            {
                return HttpNotFound();
            }

            var application =
                db.TechnicianApplications
                    .AsNoTracking()
                    .Include(a => a.Opportunity)
                    .Include(a => a.Citizen)
                    .Include(a => a.ApplicationDocuments)
                    .FirstOrDefault(a =>
                        a.ApplicationID == id.Value &&
                        a.Status !=
                        TechnicianApplicationStatus.Draft);

            if (application == null)
            {
                return HttpNotFound();
            }

            var scheduledAssessment =
                db.ApplicationAssessments
                    .AsNoTracking()
                    .FirstOrDefault(a =>
                        a.ApplicationID ==
                        application.ApplicationID &&
                        a.Status ==
                        ApplicationAssessmentStatus.Scheduled);

            ViewBag.ScheduledAssessmentID =
                scheduledAssessment != null
                    ? (int?)scheduledAssessment.AssessmentID
                    : null;

            var scheduledInterview =
    db.ApplicationInterviews
        .AsNoTracking()
        .FirstOrDefault(i =>
            i.ApplicationID == application.ApplicationID &&
            i.Status == ApplicationInterviewStatus.Scheduled);

            ViewBag.ScheduledInterviewID =
                scheduledInterview != null
                    ? (int?)scheduledInterview.InterviewID
                    : null;

            return View(application);
        }

        // =======================================================
        // REPLACE REJECTED DOCUMENT - GET
        // =======================================================

        [HttpGet]
        public ActionResult ReplaceRejectedDocument(int? id)
        {
            if (!IsCitizen())
                return new HttpUnauthorizedResult();

            var citizenID = GetCitizenID();

            if (!citizenID.HasValue)
                return new HttpUnauthorizedResult();

            if (!id.HasValue)
                return HttpNotFound();

            // ---------------------------------------------------
            // FIND REJECTED DOCUMENT
            // ---------------------------------------------------

            var document =
                db.ApplicationDocuments
                    .Include(d => d.Application)
                    .Include(d => d.Application.Opportunity)
                    .FirstOrDefault(d =>
                        d.ApplicationDocumentID == id.Value &&
                        d.Application.CitizenID == citizenID.Value);

            if (document == null)
                return HttpNotFound();

            var application =
                document.Application;

            // ---------------------------------------------------
            // APPLICATION MUST BE DOCUMENTS PENDING
            // ---------------------------------------------------

            if (application.Status !=
                TechnicianApplicationStatus.DocumentsPending)
            {
                TempData["ErrorMessage"] =
                    "This application is not currently awaiting document updates.";

                return RedirectToAction(
                    "Documents",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            // ---------------------------------------------------
            // DOCUMENT MUST BE REJECTED
            // ---------------------------------------------------

            if (document.VerificationStatus !=
                ApplicationDocumentVerificationStatus.Rejected)
            {
                TempData["ErrorMessage"] =
                    "Only rejected documents can be replaced.";

                return RedirectToAction(
                    "Documents",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            var model =
    new ApplicationDocumentCreateViewModel
    {
        ApplicationID =
            application.ApplicationID,

        ApplicationReference =
            application.ApplicationReference,

        OpportunityCode =
            application.Opportunity.OpportunityCode,

        OpportunityTitle =
            application.Opportunity.Title,

        DocumentTitle =
            document.DocumentTitle,

        DocumentType =
            document.DocumentType
    };

            // ---------------------------------------------------
            // VIEW INFORMATION
            // ---------------------------------------------------

            ViewBag.RejectedDocumentID =
                document.ApplicationDocumentID;

            ViewBag.RejectionReason =
                document.VerificationComments;

            ViewBag.ExistingFileName =
                document.FileName;

            ViewBag.DocumentTitle =
                document.DocumentTitle;

            ViewBag.DocumentType =
                document.DocumentType;

            return View(model);
        }


        // =======================================================
        // REPLACE REJECTED DOCUMENT - POST
        // =======================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReplaceRejectedDocument(
            ApplicationDocumentCreateViewModel model,
            int rejectedDocumentID)
        {
            if (!IsCitizen())
                return new HttpUnauthorizedResult();

            var citizenID = GetCitizenID();

            if (!citizenID.HasValue)
                return new HttpUnauthorizedResult();

            if (model == null)
                return HttpNotFound();

            // ---------------------------------------------------
            // FIND EXISTING DOCUMENT
            // ---------------------------------------------------

            var document =
                db.ApplicationDocuments
                    .Include(d => d.Application)
                    .Include(d => d.Application.Opportunity)
                    .FirstOrDefault(d =>
                        d.ApplicationDocumentID ==
                            rejectedDocumentID &&
                        d.Application.CitizenID ==
                            citizenID.Value);

            if (document == null)
                return HttpNotFound();

            var application =
                document.Application;

            // ---------------------------------------------------
            // APPLICATION MUST BE DOCUMENTS PENDING
            // ---------------------------------------------------

            if (application.Status !=
                TechnicianApplicationStatus.DocumentsPending)
            {
                TempData["ErrorMessage"] =
                    "This application is not currently awaiting document updates.";

                return RedirectToAction(
                    "Documents",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            // ---------------------------------------------------
            // DOCUMENT MUST STILL BE REJECTED
            // ---------------------------------------------------

            if (document.VerificationStatus !=
                ApplicationDocumentVerificationStatus.Rejected)
            {
                TempData["ErrorMessage"] =
                    "Only rejected documents can be replaced.";

                return RedirectToAction(
                    "Documents",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            // ---------------------------------------------------
            // VALIDATE FILE
            // ---------------------------------------------------

            if (model.DocumentFile == null ||
                model.DocumentFile.ContentLength <= 0)
            {
                ModelState.AddModelError(
                    "DocumentFile",
                    "Please select a replacement document.");
            }

            const int maxFileSize =
                10 * 1024 * 1024;

            if (model.DocumentFile != null &&
                model.DocumentFile.ContentLength > maxFileSize)
            {
                ModelState.AddModelError(
                    "DocumentFile",
                    "The document cannot exceed 10 MB.");
            }

            var allowedExtensions =
                new[]
                {
            ".pdf",
            ".doc",
            ".docx",
            ".jpg",
            ".jpeg",
            ".png"
                };

            if (model.DocumentFile != null &&
                model.DocumentFile.ContentLength > 0)
            {
                var extension =
                    System.IO.Path
                        .GetExtension(
                            model.DocumentFile.FileName)
                        ?.ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        "DocumentFile",
                        "Only PDF, DOC, DOCX, JPG, JPEG, and PNG files are allowed.");
                }
            }

            if (!ModelState.IsValid)
            {
                model.ApplicationID =
                    application.ApplicationID;

                model.ApplicationReference =
                    application.ApplicationReference;

                model.OpportunityCode =
                    application.Opportunity.OpportunityCode;

                model.OpportunityTitle =
                    application.Opportunity.Title;

                model.DocumentTitle =
                    document.DocumentTitle;

                model.DocumentType =
                    document.DocumentType;

                ViewBag.RejectedDocumentID =
                    document.ApplicationDocumentID;

                ViewBag.RejectionReason =
                    document.VerificationComments;

                ViewBag.ExistingFileName =
                    document.FileName;

                ViewBag.DocumentTitle =
                    document.DocumentTitle;

                ViewBag.DocumentType =
                    document.DocumentType;

                return View(model);
            }

            // ---------------------------------------------------
            // CREATE UPLOAD DIRECTORY
            // ---------------------------------------------------

            var extensionName =
                System.IO.Path
                    .GetExtension(
                        model.DocumentFile.FileName)
                    .ToLowerInvariant();

            var uploadFolder =
                Server.MapPath(
                    "~/Uploads/ApplicationDocuments");

            if (!System.IO.Directory.Exists(uploadFolder))
            {
                System.IO.Directory.CreateDirectory(
                    uploadFolder);
            }

            // ---------------------------------------------------
            // GENERATE NEW FILE NAME
            // ---------------------------------------------------

            var uniqueFileName =
                Guid.NewGuid().ToString("N") +
                extensionName;

            var physicalPath =
                System.IO.Path.Combine(
                    uploadFolder,
                    uniqueFileName);

            // ---------------------------------------------------
            // SAVE NEW FILE
            // ---------------------------------------------------

            model.DocumentFile.SaveAs(
                physicalPath);

            // ---------------------------------------------------
            // UPDATE EXISTING DOCUMENT RECORD
            // ---------------------------------------------------

            document.FileName =
                System.IO.Path.GetFileName(
                    model.DocumentFile.FileName);

            document.FilePath =
                "~/Uploads/ApplicationDocuments/" +
                uniqueFileName;

            document.ContentType =
                model.DocumentFile.ContentType;

            document.FileSize =
                model.DocumentFile.ContentLength;

            document.DateSubmitted =
                DateTime.Now;

            // ---------------------------------------------------
            // RESET VERIFICATION
            // ---------------------------------------------------

            document.VerificationStatus =
                ApplicationDocumentVerificationStatus.Pending;

            document.VerificationComments =
                null;

            document.VerifiedByAdministratorID =
                null;

            document.VerificationDate =
                null;

            // ---------------------------------------------------
            // APPLICATION RETURNS TO REVIEW
            // ---------------------------------------------------

            application.Status =
                TechnicianApplicationStatus.UnderReview;

            application.LastUpdatedDate =
                DateTime.Now;

            db.SaveChanges();

            // ---------------------------------------------------
            // SUCCESS MESSAGE
            // ---------------------------------------------------

            TempData["SuccessMessage"] =
                "Your replacement document has been submitted successfully. " +
                "The document is now awaiting administrator review.";

            // ---------------------------------------------------
            // RETURN TO APPLICATION STATUS
            // ---------------------------------------------------

            return RedirectToAction(
                "Status",
                new
                {
                    id = application.ApplicationID
                });
        }

        [HttpGet]
        public ActionResult ReviewApplications(
    string searchTerm,
    string statusFilter,
    int? opportunityFilter)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction(
                    "Login",
                    "Administrators");
            }

            var query =
                db.TechnicianApplications
                    .AsNoTracking()
                    .Include(a => a.Opportunity)
                    .Include(a => a.Citizen)
                    .Include(a => a.ApplicationDocuments)
                    .Where(a =>
                        a.Status !=
                        TechnicianApplicationStatus.Draft);


            // ============================================================
            // SEARCH
            // ============================================================

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm =
                    searchTerm.Trim();

                query = query.Where(a =>
                    a.ApplicationReference.Contains(searchTerm) ||
                    a.Opportunity.OpportunityCode.Contains(searchTerm) ||
                    a.Opportunity.Title.Contains(searchTerm) ||
                    a.Citizen.FirstName.Contains(searchTerm) ||
                    a.Citizen.LastName.Contains(searchTerm) ||
                    a.Citizen.EmailAddress.Contains(searchTerm));
            }


            // ============================================================
            // STATUS FILTER
            // ============================================================

            TechnicianApplicationStatus parsedStatus;

            if (!string.IsNullOrWhiteSpace(statusFilter) &&
                Enum.TryParse(
                    statusFilter,
                    true,
                    out parsedStatus))
            {
                query = query.Where(a =>
                    a.Status == parsedStatus);
            }


            // ============================================================
            // OPPORTUNITY FILTER
            // ============================================================

            if (opportunityFilter.HasValue)
            {
                query = query.Where(a =>
                    a.OpportunityID ==
                    opportunityFilter.Value);
            }


            // ============================================================
            // LOAD APPLICATIONS
            // ============================================================

            var applications =
                query
                    .OrderByDescending(a => a.ApplicationDate)
                    .ToList();


            // ============================================================
            // BUILD VIEW MODEL
            // ============================================================

            var model =
                new TechnicianApplicationReviewViewModel
                {
                    SearchTerm =
                        searchTerm,

                    StatusFilter =
                        statusFilter,

                    OpportunityFilter =
                        opportunityFilter,

                    TotalApplications =
                        applications.Count,

                    SubmittedApplications =
                        applications.Count(a =>
                            a.Status ==
                            TechnicianApplicationStatus.Submitted),

                    UnderReviewApplications =
                        applications.Count(a =>
                            a.Status ==
                            TechnicianApplicationStatus.UnderReview),

                    ShortlistedApplications =
                        applications.Count(a =>
                            a.Status ==
                            TechnicianApplicationStatus.Shortlisted),

                    SelectedApplications =
                        applications.Count(a =>
                            a.Status ==
                            TechnicianApplicationStatus.Selected)
                };


            // ============================================================
            // APPLICATION LIST
            // ============================================================

            foreach (var application in applications)
            {
                model.Applications.Add(
                    new TechnicianApplicationReviewItemViewModel
                    {
                        ApplicationID =
                            application.ApplicationID,

                        ApplicationReference =
                            application.ApplicationReference,

                        CitizenID =
                            application.CitizenID,

                        ApplicantName =
                            application.Citizen.FirstName +
                            " " +
                            application.Citizen.LastName,

                        ApplicantEmail =
                            application.Citizen.EmailAddress,

                        OpportunityCode =
                            application.Opportunity.OpportunityCode,

                        OpportunityTitle =
                            application.Opportunity.Title,

                        ApplicationDate =
                            application.ApplicationDate,

                        ApplicationDeadline =
                            application.Opportunity.ApplicationDeadline,

                        Status =
                            application.Status,

                        DocumentCount =
                            application.ApplicationDocuments.Count,

                        PendingDocumentCount =
                            application.ApplicationDocuments.Count(d =>
                                d.VerificationStatus ==
                                ApplicationDocumentVerificationStatus.Pending),

                        HasDocuments =
                            application.ApplicationDocuments.Any(),

                        CanReview = true
                    });
            }


            // ============================================================
            // OPPORTUNITY FILTER OPTIONS
            // ============================================================

            model.Opportunities =
                db.TechnicianOpportunities
                    .AsNoTracking()
                    .Where(o =>
                        db.TechnicianApplications
                            .Any(a =>
                                a.OpportunityID ==
                                o.OpportunityID &&
                                a.Status !=
                                TechnicianApplicationStatus.Draft))
                    .OrderByDescending(o =>
                        o.DateCreated)
                    .Select(o =>
                        new TechnicianOpportunityFilterViewModel
                        {
                            OpportunityID =
                                o.OpportunityID,

                            OpportunityCode =
                                o.OpportunityCode,

                            Title =
                                o.Title
                        })
                    .ToList();


            return View(model);
        }

        [HttpGet]
        public ActionResult Screen(int? id)
        {
            if (Session["AdministratorID"] == null)
                return RedirectToAction("Login", "Administrators");

            if (!id.HasValue)
                return HttpNotFound();

            var application =
                db.TechnicianApplications
                    .AsNoTracking()
                    .Include(a => a.Opportunity)
                    .Include(a => a.Citizen)
                    .Include(a => a.ApplicationDocuments)
                    .FirstOrDefault(a =>
                        a.ApplicationID == id.Value &&
                        a.Status != TechnicianApplicationStatus.Draft);

            if (application == null)
                return HttpNotFound();

            /*
             * Screening can only begin after all supporting documents
             * have been accepted by an administrator.
             *
             * Screening is also allowed again when the application is
             * already in Screening status because the administrator may
             * need to complete further review.
             */
            if (application.Status != TechnicianApplicationStatus.DocumentsVerified &&
                application.Status != TechnicianApplicationStatus.Screening)
            {
                TempData["ErrorMessage"] =
                    "This application is not currently available for screening. " +
                    "All supporting documents must be verified before screening can begin.";

                return RedirectToAction("Review", new
                {
                    id = application.ApplicationID
                });
            }

            var hasDocuments =
                application.ApplicationDocuments != null &&
                application.ApplicationDocuments.Any();

            if (!hasDocuments)
            {
                TempData["ErrorMessage"] =
                    "This application cannot be screened because no supporting documents were submitted.";

                return RedirectToAction("Review", new
                {
                    id = application.ApplicationID
                });
            }

            var allDocumentsAccepted =
                application.ApplicationDocuments.All(d =>
                    d.VerificationStatus ==
                    ApplicationDocumentVerificationStatus.Accepted);

            if (!allDocumentsAccepted)
            {
                TempData["ErrorMessage"] =
                    "This application cannot be screened until all supporting documents have been accepted.";

                return RedirectToAction("Review", new
                {
                    id = application.ApplicationID
                });
            }

            var existingScreening =
                db.TechnicianApplicationScreenings
                    .AsNoTracking()
                    .FirstOrDefault(s =>
                        s.ApplicationID == application.ApplicationID);

            var model =
                new TechnicianApplicationScreeningViewModel
                {
                    ApplicationID = application.ApplicationID,

                    ApplicationReference =
                        application.ApplicationReference,

                    ApplicantName =
                        application.Citizen.FirstName + " " +
                        application.Citizen.LastName,

                    ApplicantEmail =
                        application.Citizen.EmailAddress,

                    OpportunityCode =
                        application.Opportunity.OpportunityCode,

                    OpportunityTitle =
                        application.Opportunity.Title,

                    RequiredQualifications =
                        application.Opportunity.RequiredQualifications,

                    RequiredExperience =
                        application.Opportunity.RequiredExperience,

                    Requirements =
                        application.Opportunity.Requirements,

                    CoverLetter =
                        application.CoverLetter,

                    QualificationAssessment =
                        existingScreening != null
                            ? existingScreening.QualificationAssessment
                            : ApplicantScreeningResult.Pending,

                    ExperienceAssessment =
                        existingScreening != null
                            ? existingScreening.ExperienceAssessment
                            : ApplicantScreeningResult.Pending,

                    RequirementsAssessment =
                        existingScreening != null
                            ? existingScreening.RequirementsAssessment
                            : ApplicantScreeningResult.Pending,

                    OverallResult =
                        existingScreening != null
                            ? existingScreening.OverallResult
                            : ApplicantScreeningResult.Pending,

                    ScreeningComments =
                        existingScreening != null
                            ? existingScreening.ScreeningComments
                            : string.Empty
                };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Screen(
    TechnicianApplicationScreeningViewModel model)
        {
            if (Session["AdministratorID"] == null)
                return RedirectToAction("Login", "Administrators");

            if (model == null)
                return HttpNotFound();

            var application =
                db.TechnicianApplications
                    .Include(a => a.Opportunity)
                    .Include(a => a.Citizen)
                    .Include(a => a.ApplicationDocuments)
                    .FirstOrDefault(a =>
                        a.ApplicationID == model.ApplicationID);

            if (application == null)
                return HttpNotFound();

            /*
             * Screening is only permitted once all supporting documents
             * have been accepted.
             */
            if (application.Status != TechnicianApplicationStatus.DocumentsVerified &&
                application.Status != TechnicianApplicationStatus.Screening)
            {
                TempData["ErrorMessage"] =
                    "This application is no longer available for screening.";

                return RedirectToAction("Review", new
                {
                    id = application.ApplicationID
                });
            }

            var hasDocuments =
                application.ApplicationDocuments != null &&
                application.ApplicationDocuments.Any();

            if (!hasDocuments)
            {
                TempData["ErrorMessage"] =
                    "The applicant cannot be screened until supporting documents have been submitted.";

                return RedirectToAction("Review", new
                {
                    id = application.ApplicationID
                });
            }

            var allDocumentsAccepted =
                application.ApplicationDocuments.All(d =>
                    d.VerificationStatus ==
                    ApplicationDocumentVerificationStatus.Accepted);

            if (!allDocumentsAccepted)
            {
                TempData["ErrorMessage"] =
                    "The applicant cannot be screened until all supporting documents have been accepted.";

                return RedirectToAction("Review", new
                {
                    id = application.ApplicationID
                });
            }

            /*
             * Pending cannot be used as a completed screening result.
             */
            if (model.QualificationAssessment ==
                    ApplicantScreeningResult.Pending ||
                model.ExperienceAssessment ==
                    ApplicantScreeningResult.Pending ||
                model.RequirementsAssessment ==
                    ApplicantScreeningResult.Pending)
            {
                ModelState.AddModelError(
                    "",
                    "Please complete all individual screening assessments before completing the screening."
                );
            }

            if (model.OverallResult ==
                ApplicantScreeningResult.Pending)
            {
                ModelState.AddModelError(
                    "OverallResult",
                    "Please select an overall screening result."
                );
            }

            /*
             * Prevent contradictory screening results.
             */
            var hasDoesNotMeetRequirement =
                model.QualificationAssessment ==
                    ApplicantScreeningResult.DoesNotMeetRequirements ||
                model.ExperienceAssessment ==
                    ApplicantScreeningResult.DoesNotMeetRequirements ||
                model.RequirementsAssessment ==
                    ApplicantScreeningResult.DoesNotMeetRequirements;

            var hasFurtherReview =
                model.QualificationAssessment ==
                    ApplicantScreeningResult.RequiresFurtherReview ||
                model.ExperienceAssessment ==
                    ApplicantScreeningResult.RequiresFurtherReview ||
                model.RequirementsAssessment ==
                    ApplicantScreeningResult.RequiresFurtherReview;

            if (hasDoesNotMeetRequirement &&
                model.OverallResult ==
                    ApplicantScreeningResult.MeetsRequirements)
            {
                ModelState.AddModelError(
                    "OverallResult",
                    "The overall result cannot be 'Meets Requirements' when one or more individual assessments do not meet the requirements."
                );
            }

            if (hasFurtherReview &&
                model.OverallResult ==
                    ApplicantScreeningResult.MeetsRequirements)
            {
                ModelState.AddModelError(
                    "OverallResult",
                    "The overall result cannot be 'Meets Requirements' when one or more individual assessments require further review."
                );
            }

            /*
             * If the individual assessments all meet requirements,
             * the administrator should not mark the overall result
             * as DoesNotMeetRequirements.
             */
            var allMeetRequirements =
                model.QualificationAssessment ==
                    ApplicantScreeningResult.MeetsRequirements &&
                model.ExperienceAssessment ==
                    ApplicantScreeningResult.MeetsRequirements &&
                model.RequirementsAssessment ==
                    ApplicantScreeningResult.MeetsRequirements;

            if (allMeetRequirements &&
                model.OverallResult ==
                    ApplicantScreeningResult.DoesNotMeetRequirements)
            {
                ModelState.AddModelError(
                    "OverallResult",
                    "The overall result cannot be 'Does Not Meet Requirements' when all individual assessments meet the requirements."
                );
            }

            if (!ModelState.IsValid)
            {
                LoadScreeningInformation(model);
                return View(model);
            }

            var administratorID =
                Convert.ToInt32(Session["AdministratorID"]);

            var screening =
                db.TechnicianApplicationScreenings
                    .FirstOrDefault(s =>
                        s.ApplicationID == application.ApplicationID);

            if (screening == null)
            {
                screening = new TechnicianApplicationScreening
                {
                    ApplicationID = application.ApplicationID
                };

                db.TechnicianApplicationScreenings.Add(screening);
            }

            screening.QualificationAssessment =
                model.QualificationAssessment;

            screening.ExperienceAssessment =
                model.ExperienceAssessment;

            screening.RequirementsAssessment =
                model.RequirementsAssessment;

            screening.OverallResult =
                model.OverallResult;

            screening.ScreeningComments =
                string.IsNullOrWhiteSpace(model.ScreeningComments)
                    ? null
                    : model.ScreeningComments.Trim();

            screening.ScreenedByAdministratorID =
                administratorID;

            screening.ScreeningDate =
                DateTime.Now;

            /*
             * Determine the next stage of the recruitment workflow.
             */
            switch (model.OverallResult)
            {
                case ApplicantScreeningResult.MeetsRequirements:

                    application.Status =
                        TechnicianApplicationStatus.Shortlisted;

                    break;

                case ApplicantScreeningResult.DoesNotMeetRequirements:

                    application.Status =
                        TechnicianApplicationStatus.NotSelected;

                    break;

                case ApplicantScreeningResult.RequiresFurtherReview:

                    application.Status =
                        TechnicianApplicationStatus.Screening;

                    break;

                default:

                    ModelState.AddModelError(
                        "OverallResult",
                        "A valid screening result must be selected."
                    );

                    LoadScreeningInformation(model);
                    return View(model);
            }

            application.LastUpdatedDate =
                DateTime.Now;

            db.SaveChanges();

            switch (model.OverallResult)
            {
                case ApplicantScreeningResult.MeetsRequirements:

                    TempData["SuccessMessage"] =
                        "Applicant screening has been completed successfully. " +
                        "The applicant has been shortlisted.";

                    break;

                case ApplicantScreeningResult.DoesNotMeetRequirements:

                    TempData["SuccessMessage"] =
                        "Applicant screening has been completed. " +
                        "The applicant has not been selected for the next stage.";

                    break;

                case ApplicantScreeningResult.RequiresFurtherReview:

                    TempData["SuccessMessage"] =
                        "Applicant screening has been saved. " +
                        "The application requires further review.";

                    break;
            }

            return RedirectToAction(
                "Review",
                new { id = application.ApplicationID });
        }

        private void LoadScreeningInformation(
    TechnicianApplicationScreeningViewModel model)
        {
            var application =
                db.TechnicianApplications
                    .AsNoTracking()
                    .Include(a => a.Opportunity)
                    .Include(a => a.Citizen)
                    .FirstOrDefault(a =>
                        a.ApplicationID ==
                        model.ApplicationID);

            if (application == null)
            {
                return;
            }

            model.ApplicationReference =
                application.ApplicationReference;

            model.ApplicantName =
                application.Citizen.FirstName +
                " " +
                application.Citizen.LastName;

            model.ApplicantEmail =
                application.Citizen.EmailAddress;

            model.OpportunityCode =
                application.Opportunity.OpportunityCode;

            model.OpportunityTitle =
                application.Opportunity.Title;

            model.RequiredQualifications =
                application.Opportunity.RequiredQualifications;

            model.RequiredExperience =
                application.Opportunity.RequiredExperience;

            model.Requirements =
                application.Opportunity.Requirements;

            model.CoverLetter =
                application.CoverLetter;
        }

        [HttpGet]
        public ActionResult VerifyDocuments(int? id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction(
                    "Login",
                    "Administrators");
            }

            if (!id.HasValue)
            {
                return HttpNotFound();
            }

            var application =
                db.TechnicianApplications
                    .AsNoTracking()
                    .Include(a => a.Opportunity)
                    .Include(a => a.Citizen)
                    .Include(a => a.ApplicationDocuments)
                    .FirstOrDefault(a =>
                        a.ApplicationID == id.Value &&
                        a.Status !=
                        TechnicianApplicationStatus.Draft);

            if (application == null)
            {
                return HttpNotFound();
            }

            /*
             * Supporting document verification is available during
             * the document verification stage.
             *
             * Submitted:
             * Application has been submitted and documents can be reviewed.
             *
             * UnderReview:
             * Application is currently being reviewed.
             *
             * DocumentsPending:
             * One or more documents require correction/replacement
             * or verification is still incomplete.
             *
             * DocumentsVerified:
             * All documents have been accepted. The page can still
             * be viewed, but no further verification should be recorded.
             */
            if (application.Status !=
                    TechnicianApplicationStatus.Submitted &&
                application.Status !=
                    TechnicianApplicationStatus.UnderReview &&
                application.Status !=
                    TechnicianApplicationStatus.DocumentsPending &&
                application.Status !=
                    TechnicianApplicationStatus.DocumentsVerified)
            {
                TempData["ErrorMessage"] =
                    "Supporting documents are not currently available for verification.";

                return RedirectToAction(
                    "Review",
                    new { id = application.ApplicationID });
            }

            var model =
                new TechnicianApplicationDocumentVerificationViewModel
                {
                    ApplicationID =
                        application.ApplicationID,

                    ApplicationReference =
                        application.ApplicationReference,

                    ApplicantName =
                        application.Citizen.FirstName +
                        " " +
                        application.Citizen.LastName,

                    ApplicantEmail =
                        application.Citizen.EmailAddress,

                    OpportunityCode =
                        application.Opportunity.OpportunityCode,

                    OpportunityTitle =
                        application.Opportunity.Title,

                    ApplicationStatus =
                        application.Status,

                    TotalDocuments =
                        application.ApplicationDocuments.Count,

                    PendingDocuments =
                        application.ApplicationDocuments.Count(d =>
                            d.VerificationStatus ==
                            ApplicationDocumentVerificationStatus.Pending),

                    AcceptedDocuments =
                        application.ApplicationDocuments.Count(d =>
                            d.VerificationStatus ==
                            ApplicationDocumentVerificationStatus.Accepted),

                    RejectedDocuments =
                        application.ApplicationDocuments.Count(d =>
                            d.VerificationStatus ==
                            ApplicationDocumentVerificationStatus.Rejected)
                };

            foreach (var document in
                application.ApplicationDocuments
                    .OrderByDescending(d => d.DateSubmitted))
            {
                model.Documents.Add(
                    new TechnicianApplicationDocumentVerificationItemViewModel
                    {
                        ApplicationDocumentID =
                            document.ApplicationDocumentID,

                        DocumentType =
                            document.DocumentType,

                        DocumentTitle =
                            document.DocumentTitle,

                        FileName =
                            document.FileName,

                        ContentType =
                            document.ContentType,

                        FileSize =
                            document.FileSize,

                        DateSubmitted =
                            document.DateSubmitted,

                        VerificationStatus =
                            document.VerificationStatus,

                        VerificationComments =
                            document.VerificationComments,

                        VerificationDate =
                            document.VerificationDate,

                        VerifiedByAdministratorName =
                            document.VerifiedByAdministrator != null
                                ? document.VerifiedByAdministrator.FirstName +
                                  " " +
                                  document.VerifiedByAdministrator.LastName
                                : null,

                        CanRecordVerification =
                            document.VerificationStatus ==
                            ApplicationDocumentVerificationStatus.Pending
                    });
            }

            return View(model);
        }

        [HttpGet]
        public ActionResult ViewApplicationDocument(int? id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction(
                    "Login",
                    "Administrators");
            }

            if (!id.HasValue)
            {
                return HttpNotFound();
            }

            var document =
                db.ApplicationDocuments
                    .Include(d => d.Application)
                    .FirstOrDefault(d =>
                        d.ApplicationDocumentID == id.Value);

            if (document == null)
            {
                return HttpNotFound();
            }

            if (string.IsNullOrWhiteSpace(document.FilePath))
            {
                return HttpNotFound();
            }

            var physicalPath =
                Server.MapPath(document.FilePath);

            if (!System.IO.File.Exists(physicalPath))
            {
                return HttpNotFound();
            }

            return File(
                physicalPath,
                document.ContentType,
                document.FileName);
        }

        [HttpGet]
        public ActionResult RecordDocumentVerification(int? id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction(
                    "Login",
                    "Administrators");
            }

            if (!id.HasValue)
            {
                return HttpNotFound();
            }

            var document =
                db.ApplicationDocuments
                    .AsNoTracking()
                    .Include(d => d.Application)
                    .Include(d => d.Application.Opportunity)
                    .Include(d => d.Application.Citizen)
                    .FirstOrDefault(d =>
                        d.ApplicationDocumentID == id.Value);

            if (document == null)
            {
                return HttpNotFound();
            }

            var application =
                document.Application;

            /*
             * A document can only be individually verified while
             * the application is in one of the document verification
             * stages.
             */
            if (application.Status !=
                    TechnicianApplicationStatus.Submitted &&
                application.Status !=
                    TechnicianApplicationStatus.UnderReview &&
                application.Status !=
                    TechnicianApplicationStatus.DocumentsPending)
            {
                TempData["ErrorMessage"] =
                    "Document verification is no longer available for this application.";

                return RedirectToAction(
                    "Review",
                    new { id = application.ApplicationID });
            }

            if (document.VerificationStatus !=
                ApplicationDocumentVerificationStatus.Pending)
            {
                TempData["ErrorMessage"] =
                    "This document has already had a verification result recorded.";

                return RedirectToAction(
                    "VerifyDocuments",
                    new { id = application.ApplicationID });
            }

            var model =
                new TechnicianApplicationDocumentVerificationResultViewModel
                {
                    ApplicationDocumentID =
                        document.ApplicationDocumentID,

                    ApplicationID =
                        application.ApplicationID,

                    ApplicationReference =
                        application.ApplicationReference,

                    ApplicantName =
                        application.Citizen.FirstName +
                        " " +
                        application.Citizen.LastName,

                    OpportunityCode =
                        application.Opportunity.OpportunityCode,

                    OpportunityTitle =
                        application.Opportunity.Title,

                    DocumentType =
                        document.DocumentType,

                    DocumentTitle =
                        document.DocumentTitle,

                    FileName =
                        document.FileName,

                    ContentType =
                        document.ContentType,

                    FileSize =
                        document.FileSize,

                    DateSubmitted =
                        document.DateSubmitted,

                    VerificationStatus =
                        ApplicationDocumentVerificationStatus.Pending,

                    VerificationComments =
                        string.Empty
                };

            return View(model);
        }

        
[HttpPost]
[ValidateAntiForgeryToken]
public ActionResult RecordDocumentVerification(
    TechnicianApplicationDocumentVerificationResultViewModel model)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction(
                    "Login",
                    "Administrators");
            }

            if (model == null)
            {
                return HttpNotFound();
            }

            if (!ModelState.IsValid)
            {
                LoadDocumentVerificationInformation(model);

                return View(model);
            }

            if (model.VerificationStatus ==
                ApplicationDocumentVerificationStatus.Pending)
            {
                ModelState.AddModelError(
                    "VerificationStatus",
                    "Please select Accepted or Rejected.");

                LoadDocumentVerificationInformation(model);

                return View(model);
            }

            if (model.VerificationStatus ==
                    ApplicationDocumentVerificationStatus.Rejected &&
                string.IsNullOrWhiteSpace(model.VerificationComments))
            {
                ModelState.AddModelError(
                    "VerificationComments",
                    "Please provide verification comments when rejecting a document.");

                LoadDocumentVerificationInformation(model);

                return View(model);
            }

            var document =
                db.ApplicationDocuments
                    .Include(d => d.Application)
                    .FirstOrDefault(d =>
                        d.ApplicationDocumentID ==
                        model.ApplicationDocumentID);

            if (document == null)
            {
                return HttpNotFound();
            }

            var application =
                document.Application;

            /*
             * Prevent document verification from being performed
             * after the application has moved beyond the document
             * verification stage.
             */
            if (application.Status !=
                    TechnicianApplicationStatus.Submitted &&
                application.Status !=
                    TechnicianApplicationStatus.UnderReview &&
                application.Status !=
                    TechnicianApplicationStatus.DocumentsPending)
            {
                TempData["ErrorMessage"] =
                    "Document verification is no longer available for this application.";

                return RedirectToAction(
                    "Review",
                    new { id = application.ApplicationID });
            }

            /*
             * Prevent a document from being verified more than once.
             */
            if (document.VerificationStatus !=
                ApplicationDocumentVerificationStatus.Pending)
            {
                TempData["ErrorMessage"] =
                    "This document has already had a verification result recorded.";

                return RedirectToAction(
                    "VerifyDocuments",
                    new { id = application.ApplicationID });
            }

            var administratorID =
                Convert.ToInt32(
                    Session["AdministratorID"]);

            document.VerificationStatus =
                model.VerificationStatus;

            document.VerificationComments =
                string.IsNullOrWhiteSpace(
                    model.VerificationComments)
                    ? null
                    : model.VerificationComments.Trim();

            document.VerifiedByAdministratorID =
                administratorID;

            document.VerificationDate =
                DateTime.Now;

            application.LastUpdatedDate =
                DateTime.Now;

            /*
             * Re-evaluate the verification state of every document
             * belonging to the application.
             */
            var allDocuments =
                db.ApplicationDocuments
                    .Where(d =>
                        d.ApplicationID ==
                        application.ApplicationID)
                    .ToList();

            var allDocumentsVerified =
                allDocuments.Any() &&
                allDocuments.All(d =>
                    d.VerificationStatus ==
                    ApplicationDocumentVerificationStatus.Accepted);

            var anyDocumentsRejected =
                allDocuments.Any(d =>
                    d.VerificationStatus ==
                    ApplicationDocumentVerificationStatus.Rejected);

            var pendingDocuments =
                allDocuments.Any(d =>
                    d.VerificationStatus ==
                    ApplicationDocumentVerificationStatus.Pending);

            /*
             * Move the application to the correct document-verification
             * status.
             */
            if (allDocumentsVerified)
            {
                application.Status =
                    TechnicianApplicationStatus.DocumentsVerified;
            }
            else if (anyDocumentsRejected)
            {
                application.Status =
                    TechnicianApplicationStatus.DocumentsPending;
            }
            else if (pendingDocuments)
            {
                application.Status =
                    TechnicianApplicationStatus.DocumentsPending;
            }

            // -------------------------------------------------------
            // CREATE APPLICANT NOTIFICATION
            // -------------------------------------------------------

            var notificationService =
                new CommunityServiceProject.Services
                    .TechnicianApplicationNotificationService(db);

            /*
             * Notify the applicant only when the overall document
             * verification state has changed to a meaningful stage.
             */

            if (anyDocumentsRejected)
            {
                notificationService.Create(
                    application,
                    TechnicianApplicationNotificationType
                        .DocumentReplacementRequired,
                    "Document Replacement Required",
                    "One or more documents submitted with your technician application " +
                    "could not be accepted. Please review your application and replace " +
                    "the rejected document(s) before the application can continue."
                );
            }
            else if (allDocumentsVerified)
            {
                notificationService.Create(
                    application,
                    TechnicianApplicationNotificationType
                        .DocumentsVerified,
                    "Documents Verified",
                    "All documents submitted with your technician application have " +
                    "been successfully verified. Your application can now proceed " +
                    "to the next stage of the recruitment process."
                );
            }

            // -------------------------------------------------------
            // SAVE APPLICATION + DOCUMENT + NOTIFICATION TOGETHER
            // -------------------------------------------------------

            db.SaveChanges();

            TempData["SuccessMessage"] =
                "The document verification result has been recorded successfully.";

            return RedirectToAction(
                "VerifyDocuments",
                new { id = application.ApplicationID });
        }



        private void LoadDocumentVerificationInformation(
    TechnicianApplicationDocumentVerificationResultViewModel model)
        {
            var document =
                db.ApplicationDocuments
                    .AsNoTracking()
                    .Include(d => d.Application)
                    .Include(d => d.Application.Opportunity)
                    .Include(d => d.Application.Citizen)
                    .FirstOrDefault(d =>
                        d.ApplicationDocumentID ==
                        model.ApplicationDocumentID);

            if (document == null)
            {
                return;
            }

            model.ApplicationID =
                document.ApplicationID;

            model.ApplicationReference =
                document.Application.ApplicationReference;

            model.ApplicantName =
                document.Application.Citizen.FirstName +
                " " +
                document.Application.Citizen.LastName;

            model.OpportunityCode =
                document.Application.Opportunity.OpportunityCode;

            model.OpportunityTitle =
                document.Application.Opportunity.Title;

            model.DocumentType =
                document.DocumentType;

            model.DocumentTitle =
                document.DocumentTitle;

            model.FileName =
                document.FileName;

            model.ContentType =
                document.ContentType; 

            model.FileSize =
                document.FileSize;

            model.DateSubmitted =
                document.DateSubmitted;
        }
             
       
// ===============================================================
// GET: ShortlistApplicant
// ===============================================================

[HttpGet]
public ActionResult ShortlistApplicant(int? id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction(
                    "Login",
                    "Administrators");
            }

            if (!id.HasValue)
            {
                return HttpNotFound();
            }

            var application =
                db.TechnicianApplications
                    .AsNoTracking()
                    .Include(a => a.Opportunity)
                    .Include(a => a.Citizen)
                    .Include(a => a.ApplicationDocuments)
                    .FirstOrDefault(a =>
                        a.ApplicationID == id.Value);

            if (application == null)
            {
                return HttpNotFound();
            }

            var screening =
                db.TechnicianApplicationScreenings
                    .AsNoTracking()
                    .FirstOrDefault(s =>
                        s.ApplicationID == application.ApplicationID);

            if (screening == null)
            {
                TempData["ErrorMessage"] =
                    "This applicant cannot be shortlisted because screening has not been completed.";

                return RedirectToAction(
                    "Review",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            var documents =
                application.ApplicationDocuments.ToList();

            var totalDocuments =
                documents.Count;

            var acceptedDocuments =
                documents.Count(d =>
                    d.VerificationStatus ==
                    ApplicationDocumentVerificationStatus.Accepted);

            var rejectedDocuments =
                documents.Count(d =>
                    d.VerificationStatus ==
                    ApplicationDocumentVerificationStatus.Rejected);

            var pendingDocuments =
                documents.Count(d =>
                    d.VerificationStatus ==
                    ApplicationDocumentVerificationStatus.Pending);

            var screeningPassed =
                screening.OverallResult ==
                ApplicantScreeningResult.MeetsRequirements;

            var documentsVerified =
                totalDocuments > 0 &&
                acceptedDocuments == totalDocuments;

            var eligible =
                application.Status !=
                    TechnicianApplicationStatus.Shortlisted &&
                application.Status !=
                    TechnicianApplicationStatus.Selected &&
                application.Status !=
                    TechnicianApplicationStatus.NotSelected &&
                application.Status !=
                    TechnicianApplicationStatus.Withdrawn &&
                application.Status !=
                    TechnicianApplicationStatus.Onboarded &&
                screeningPassed &&
                documentsVerified;

            var model =
                new TechnicianApplicationShortlistViewModel
                {
                    ApplicationID =
                        application.ApplicationID,

                    ApplicationReference =
                        application.ApplicationReference,

                    ApplicantName =
                        application.Citizen.FirstName +
                        " " +
                        application.Citizen.LastName,

                    ApplicantEmail =
                        application.Citizen.EmailAddress,

                    OpportunityCode =
                        application.Opportunity.OpportunityCode,

                    OpportunityTitle =
                        application.Opportunity.Title,

                    ApplicationDate =
                        application.ApplicationDate,

                    ApplicationStatus =
                        application.Status,

                    ScreeningResult =
                        screening.OverallResult,

                    TotalDocuments =
                        totalDocuments,

                    AcceptedDocuments =
                        acceptedDocuments,

                    RejectedDocuments =
                        rejectedDocuments,

                    PendingDocuments =
                        pendingDocuments,

                    ScreeningPassed =
                        screeningPassed,

                    DocumentsVerified =
                        documentsVerified,

                    EligibleForShortlisting =
                        eligible
                };

            return View("Shortlist", model);
        }


        // ===============================================================
        // POST: ConfirmShortlisting
        // ===============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmShortlisting(
            TechnicianApplicationShortlistViewModel model)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction(
                    "Login",
                    "Administrators");
            }

            if (model == null)
            {
                return HttpNotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(
                    "Shortlist",
                    model);
            }

            var application =
                db.TechnicianApplications
                    .Include(a => a.Opportunity)
                    .Include(a => a.Citizen)
                    .Include(a => a.ApplicationDocuments)
                    .FirstOrDefault(a =>
                        a.ApplicationID ==
                        model.ApplicationID);

            if (application == null)
            {
                return HttpNotFound();
            }

            // -----------------------------------------------------------
            // PREVENT DUPLICATE / INVALID SHORTLISTING
            // -----------------------------------------------------------

            if (application.Status ==
                    TechnicianApplicationStatus.Shortlisted ||
                application.Status ==
                    TechnicianApplicationStatus.Selected ||
                application.Status ==
                    TechnicianApplicationStatus.NotSelected ||
                application.Status ==
                    TechnicianApplicationStatus.Withdrawn ||
                application.Status ==
                    TechnicianApplicationStatus.Onboarded)
            {
                TempData["ErrorMessage"] =
                    "This application is no longer available for shortlisting.";

                return RedirectToAction(
                    "Review",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            // -----------------------------------------------------------
            // VERIFY SCREENING
            // -----------------------------------------------------------

            var screening =
                db.TechnicianApplicationScreenings
                    .FirstOrDefault(s =>
                        s.ApplicationID ==
                        application.ApplicationID);

            if (screening == null)
            {
                TempData["ErrorMessage"] =
                    "The applicant cannot be shortlisted because screening has not been completed.";

                return RedirectToAction(
                    "Review",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            if (screening.OverallResult !=
                ApplicantScreeningResult.MeetsRequirements)
            {
                TempData["ErrorMessage"] =
                    "The applicant cannot be shortlisted because the screening requirements have not been met.";

                return RedirectToAction(
                    "Review",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            // -----------------------------------------------------------
            // VERIFY DOCUMENTS
            // -----------------------------------------------------------

            var documents =
                application.ApplicationDocuments.ToList();

            if (!documents.Any())
            {
                TempData["ErrorMessage"] =
                    "The applicant cannot be shortlisted because no supporting documents have been submitted.";

                return RedirectToAction(
                    "Review",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            var allDocumentsAccepted =
                documents.All(d =>
                    d.VerificationStatus ==
                    ApplicationDocumentVerificationStatus.Accepted);

            if (!allDocumentsAccepted)
            {
                TempData["ErrorMessage"] =
                    "The applicant cannot be shortlisted until all submitted supporting documents have been accepted.";

                return RedirectToAction(
                    "Review",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            // -----------------------------------------------------------
            // CONFIRM SHORTLISTING
            // -----------------------------------------------------------

            application.Status =
                TechnicianApplicationStatus.Shortlisted;

            application.LastUpdatedDate =
                DateTime.Now;

            // -----------------------------------------------------------
            // CREATE CITIZEN NOTIFICATION
            // -----------------------------------------------------------

            var notification =
                new TechnicianApplicationNotification
                {
                    ApplicationID =
                        application.ApplicationID,

                    CitizenID =
                        application.CitizenID,

                    NotificationType =
                        TechnicianApplicationNotificationType
                            .ApplicationShortlisted,

                    Title =
                        "Application Shortlisted",

                    Message =
                        "Your application for " +
                        application.Opportunity.Title +
                        " has been shortlisted. " +
                        "You have progressed to the next stage of the technician recruitment process.",

                    DateCreated =
                        DateTime.Now,

                    IsRead =
                        false,

                    ReadDate =
                        null
                };

            db.TechnicianApplicationNotifications.Add(
                notification);

            // -----------------------------------------------------------
            // SAVE STATUS + NOTIFICATION TOGETHER
            // -----------------------------------------------------------

            using (var transaction =
                db.Database.BeginTransaction())
            {
                try
                {
                    db.SaveChanges();

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }

            TempData["SuccessMessage"] =
                "Applicant " +
                application.ApplicationReference +
                " has been successfully shortlisted.";

            return RedirectToAction(
                "Review",
                new
                {
                    id = application.ApplicationID
                });
        }

                              
[HttpGet]
public ActionResult ShortlistedApplicants(
    string searchTerm,
    int? opportunityFilter)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            var query =
                db.TechnicianApplications
                    .AsNoTracking()
                    .Include(a => a.Opportunity)
                    .Include(a => a.Citizen)
                    .Where(a =>
                        a.Status ==
                        TechnicianApplicationStatus.Shortlisted);

            // Search by applicant name, email, or application reference
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();

                query = query.Where(a =>
                    a.ApplicationReference.Contains(searchTerm) ||
                    a.Citizen.FirstName.Contains(searchTerm) ||
                    a.Citizen.LastName.Contains(searchTerm) ||
                    a.Citizen.EmailAddress.Contains(searchTerm));
            }

            // Filter by opportunity
            if (opportunityFilter.HasValue)
            {
                query = query.Where(a =>
                    a.OpportunityID == opportunityFilter.Value);
            }

            var applications =
                query
                    .OrderByDescending(a => a.LastUpdatedDate)
                    .ThenByDescending(a => a.ApplicationDate)
                    .ToList();

            // Populate opportunity filter dropdown
            ViewBag.Opportunities =
                db.TechnicianOpportunities
                    .AsNoTracking()
                    .OrderBy(o => o.Title)
                    .ToList();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.OpportunityFilter = opportunityFilter;

            return View(applications);
        }

        [HttpGet]
        public ActionResult ScheduleAssessment(int? id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction(
                    "Login",
                    "Administrators");
            }

            if (!id.HasValue)
            {
                return HttpNotFound();
            }

            var application =
                db.TechnicianApplications
                    .AsNoTracking()
                    .Include(a => a.Opportunity)
                    .Include(a => a.Citizen)
                    .FirstOrDefault(a =>
                        a.ApplicationID == id.Value &&
                        a.Status ==
                        TechnicianApplicationStatus.Shortlisted);

            if (application == null)
            {
                return HttpNotFound();
            }

            var existingAssessment =
                db.ApplicationAssessments
                    .AsNoTracking()
                    .FirstOrDefault(a =>
                        a.ApplicationID ==
                        application.ApplicationID &&
                        a.Status ==
                        ApplicationAssessmentStatus.Scheduled);

            if (existingAssessment != null)
            {
                TempData["Error"] =
                    "This applicant already has a scheduled assessment.";

                return RedirectToAction(
                    "ShortlistedApplicants");
            }

            var model =
                new TechnicianApplicationScheduleAssessmentViewModel
                {
                    ApplicationID =
                        application.ApplicationID,

                    ApplicationReference =
                        application.ApplicationReference,

                    ApplicantName =
                        application.Citizen.FirstName +
                        " " +
                        application.Citizen.LastName,

                    ApplicantEmail =
                        application.Citizen.EmailAddress,

                    OpportunityCode =
                        application.Opportunity.OpportunityCode,

                    OpportunityTitle =
                        application.Opportunity.Title,

                    ApplicationDate =
                        application.ApplicationDate,

                    ApplicationStatus =
                        application.Status,

                    AssessmentDate =
                        DateTime.Now.AddDays(1)
                };

            return View(model);
        }

             
[HttpPost]
[ValidateAntiForgeryToken]
public ActionResult ScheduleAssessment(
    TechnicianApplicationScheduleAssessmentViewModel model)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction(
                    "Login",
                    "Administrators");
            }

            if (model == null)
            {
                return HttpNotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.AssessmentDate <= DateTime.Now)
            {
                ModelState.AddModelError(
                    "AssessmentDate",
                    "The assessment date and time must be in the future.");

                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.AssessmentType))
            {
                ModelState.AddModelError(
                    "AssessmentType",
                    "Please select an assessment type.");

                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.Location))
            {
                ModelState.AddModelError(
                    "Location",
                    "Please provide the assessment location.");

                return View(model);
            }

            var application =
                db.TechnicianApplications
                    .Include(a => a.Opportunity)
                    .Include(a => a.Citizen)
                    .FirstOrDefault(a =>
                        a.ApplicationID ==
                        model.ApplicationID);

            if (application == null)
            {
                return HttpNotFound();
            }

            if (application.Status !=
                TechnicianApplicationStatus.Shortlisted)
            {
                TempData["Error"] =
                    "This applicant is no longer eligible for assessment scheduling.";

                return RedirectToAction(
                    "Review",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            var existingAssessment =
                db.ApplicationAssessments
                    .FirstOrDefault(a =>
                        a.ApplicationID ==
                        application.ApplicationID &&
                        a.Status ==
                        ApplicationAssessmentStatus.Scheduled);

            if (existingAssessment != null)
            {
                TempData["Error"] =
                    "This applicant already has a scheduled assessment.";

                return RedirectToAction(
                    "Review",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            var administratorID =
                Convert.ToInt32(
                    Session["AdministratorID"]);

            var assessment =
                new ApplicationAssessment
                {
                    ApplicationID =
                        application.ApplicationID,

                    AssessmentType =
                        model.AssessmentType.Trim(),

                    AssessmentDate =
                        model.AssessmentDate,

                    Location =
                        model.Location.Trim(),

                    Instructions =
                        string.IsNullOrWhiteSpace(
                            model.Instructions)
                            ? null
                            : model.Instructions.Trim(),

                    Status =
                        ApplicationAssessmentStatus.Scheduled,

                    Result = null,

                    Score = null,

                    Comments = null,

                    RecordedByAdministratorID =
                        administratorID,

                    DateRecorded =
                        DateTime.Now
                };

            db.ApplicationAssessments.Add(assessment);

            application.Status =
                TechnicianApplicationStatus.AssessmentScheduled;

            application.LastUpdatedDate =
                DateTime.Now;

            // =========================================================
            // CITIZEN RECRUITMENT NOTIFICATION
            // =========================================================

            var notificationService =
                new CommunityServiceProject.Services
                    .TechnicianApplicationNotificationService(db);

            var notificationMessage =
                "Your assessment for " +
                application.Opportunity.Title +
                " has been scheduled. " +
                "Assessment type: " +
                assessment.AssessmentType +
                ". Date and time: " +
                assessment.AssessmentDate.ToString("dd MMM yyyy HH:mm") +
                ". Location: " +
                assessment.Location +
                ".";

            if (!string.IsNullOrWhiteSpace(
                assessment.Instructions))
            {
                notificationMessage +=
                    " Instructions: " +
                    assessment.Instructions;
            }

            notificationService.Create(
                application,
                TechnicianApplicationNotificationType.AssessmentScheduled,
                "Assessment Scheduled",
                notificationMessage);

            db.SaveChanges();

            TempData["Success"] =
                "Assessment scheduled successfully for " +
                application.Citizen.FirstName +
                " " +
                application.Citizen.LastName +
                ".";

            return RedirectToAction(
                "Review",
                new
                {
                    id = application.ApplicationID
                });
        }



        [HttpGet]
        public ActionResult RecordAssessmentResult(int? id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction(
                    "Login",
                    "Administrators");
            }

            if (!id.HasValue)
            {
                return HttpNotFound();
            }

            var assessment =
                db.ApplicationAssessments
                    .AsNoTracking()
                    .Include(a => a.Application)
                    .Include(a => a.Application.Opportunity)
                    .Include(a => a.Application.Citizen)
                    .FirstOrDefault(a =>
                        a.AssessmentID == id.Value);

            if (assessment == null)
            {
                return HttpNotFound();
            }

            if (assessment.Application == null ||
                assessment.Application.Opportunity == null ||
                assessment.Application.Citizen == null)
            {
                return HttpNotFound();
            }

            if (assessment.Status !=
                ApplicationAssessmentStatus.Scheduled)
            {
                TempData["Error"] =
                    "This assessment cannot be updated because it is no longer scheduled.";

                return RedirectToAction(
    "Review",
    new
    {
        id = assessment.ApplicationID
    });
            }

            var model =
                new TechnicianApplicationAssessmentResultViewModel
                {
                    AssessmentID =
                        assessment.AssessmentID,

                    ApplicationID =
                        assessment.ApplicationID,

                    ApplicationReference =
                        assessment.Application.ApplicationReference,

                    ApplicantName =
                        assessment.Application.Citizen.FirstName +
                        " " +
                        assessment.Application.Citizen.LastName,

                    ApplicantEmail =
                        assessment.Application.Citizen.EmailAddress,

                    OpportunityCode =
                        assessment.Application.Opportunity.OpportunityCode,

                    OpportunityTitle =
                        assessment.Application.Opportunity.Title,

                    AssessmentType =
                        assessment.AssessmentType,

                    AssessmentDate =
                        assessment.AssessmentDate,

                    Location =
                        assessment.Location,

                    Instructions =
                        assessment.Instructions,

                    AssessmentStatus =
                        assessment.Status,

                    CanRecordResult =
                        true
                };

            return View(model);
        }

               
[HttpPost]
[ValidateAntiForgeryToken]
public ActionResult RecordAssessmentResult(
    TechnicianApplicationAssessmentResultViewModel model)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction(
                    "Login",
                    "Administrators");
            }

            if (model == null)
            {
                return HttpNotFound();
            }

            if (!ModelState.IsValid)
            {
                return ReloadAssessmentResultView(model);
            }

            if (string.IsNullOrWhiteSpace(model.Result))
            {
                ModelState.AddModelError(
                    "Result",
                    "Please provide the assessment result.");

                return ReloadAssessmentResultView(model);
            }

            if (model.Score.HasValue &&
                (model.Score.Value < 0 ||
                 model.Score.Value > 100))
            {
                ModelState.AddModelError(
                    "Score",
                    "The assessment score must be between 0 and 100.");

                return ReloadAssessmentResultView(model);
            }

            var assessment =
                db.ApplicationAssessments
                    .Include(a => a.Application)
                    .Include(a => a.Application.Opportunity)
                    .Include(a => a.Application.Citizen)
                    .FirstOrDefault(a =>
                        a.AssessmentID ==
                        model.AssessmentID);

            if (assessment == null)
            {
                return HttpNotFound();
            }

            if (assessment.Application == null)
            {
                return HttpNotFound();
            }

            if (assessment.Application.Opportunity == null ||
                assessment.Application.Citizen == null)
            {
                return HttpNotFound();
            }

            if (assessment.Status !=
                ApplicationAssessmentStatus.Scheduled)
            {
                TempData["Error"] =
                    "This assessment has already been completed or is no longer available for result recording.";

                return RedirectToAction(
                    "Review",
                    new
                    {
                        id = assessment.ApplicationID
                    });
            }

            if (assessment.Application.Status !=
                TechnicianApplicationStatus.AssessmentScheduled)
            {
                TempData["Error"] =
                    "This applicant is not currently at the assessment stage.";

                return RedirectToAction(
                    "Review",
                    new
                    {
                        id = assessment.ApplicationID
                    });
            }

            var administratorID =
                Convert.ToInt32(
                    Session["AdministratorID"]);

            // =========================================================
            // UPDATE ASSESSMENT
            // =========================================================

            assessment.Result =
                model.Result.Trim();

            assessment.Score =
                model.Score;

            assessment.Comments =
                string.IsNullOrWhiteSpace(model.Comments)
                    ? null
                    : model.Comments.Trim();

            assessment.Status =
                ApplicationAssessmentStatus.Completed;

            assessment.DateRecorded =
                DateTime.Now;

            assessment.RecordedByAdministratorID =
                administratorID;

            // =========================================================
            // UPDATE APPLICATION STATUS
            // =========================================================

            assessment.Application.Status =
                TechnicianApplicationStatus.Assessed;

            assessment.Application.LastUpdatedDate =
                DateTime.Now;

            // =========================================================
            // CITIZEN RECRUITMENT NOTIFICATION
            // =========================================================
            //
            // Do NOT expose:
            // - Assessment result
            // - Assessment score
            // - Administrator comments
            //
            // The citizen is only informed that the assessment
            // stage has been completed.
            // =========================================================

            var notificationService =
                new CommunityServiceProject.Services
                    .TechnicianApplicationNotificationService(db);

            notificationService.Create(
                assessment.Application,
                TechnicianApplicationNotificationType.AssessmentCompleted,
                "Assessment Completed",
                "The assessment stage for your application for " +
                assessment.Application.Opportunity.Title +
                " has been completed. " +
                "Your application will now proceed to the next stage of the technician recruitment process.");

            // =========================================================
            // SAVE EVERYTHING TOGETHER
            // =========================================================

            db.SaveChanges();

            TempData["Success"] =
                "Assessment results for " +
                assessment.Application.Citizen.FirstName +
                " " +
                assessment.Application.Citizen.LastName +
                " were recorded successfully.";

            return RedirectToAction(
                "Review",
                new
                {
                    id = assessment.ApplicationID
                });
        }



        private ActionResult ReloadAssessmentResultView(
    TechnicianApplicationAssessmentResultViewModel model)
        {
            var assessment =
                db.ApplicationAssessments
                    .AsNoTracking()
                    .Include(a => a.Application)
                    .Include(a => a.Application.Opportunity)
                    .Include(a => a.Application.Citizen)
                    .FirstOrDefault(a =>
                        a.AssessmentID ==
                        model.AssessmentID);

            if (assessment == null)
            {
                return HttpNotFound();
            }

            model.ApplicationID =
                assessment.ApplicationID;

            model.ApplicationReference =
                assessment.Application.ApplicationReference;

            model.ApplicantName =
                assessment.Application.Citizen.FirstName +
                " " +
                assessment.Application.Citizen.LastName;

            model.ApplicantEmail =
                assessment.Application.Citizen.EmailAddress;

            model.OpportunityCode =
                assessment.Application.Opportunity.OpportunityCode;

            model.OpportunityTitle =
                assessment.Application.Opportunity.Title;

            model.AssessmentType =
                assessment.AssessmentType;

            model.AssessmentDate =
                assessment.AssessmentDate;

            model.Location =
                assessment.Location;

            model.Instructions =
                assessment.Instructions;

            model.AssessmentStatus =
                assessment.Status;

            model.CanRecordResult =
                assessment.Status ==
                ApplicationAssessmentStatus.Scheduled;

            return View("RecordAssessmentResult", model);
        }

        [HttpGet]
        public ActionResult ScheduleInterview(int? id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction(
                    "Login",
                    "Administrators");
            }

            if (!id.HasValue)
            {
                return HttpNotFound();
            }

            var application =
                db.TechnicianApplications
                    .AsNoTracking()
                    .Include(a => a.Opportunity)
                    .Include(a => a.Citizen)
                    .FirstOrDefault(a =>
                        a.ApplicationID == id.Value);

            if (application == null)
            {
                return HttpNotFound();
            }

            if (application.Opportunity == null ||
                application.Citizen == null)
            {
                return HttpNotFound();
            }

            if (application.Status !=
                TechnicianApplicationStatus.Assessed)
            {
                TempData["Error"] =
                    "Only applicants who have completed the assessment stage can be scheduled for an interview.";

                return RedirectToAction(
                    "Review",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            var existingInterview =
                db.ApplicationInterviews
                    .AsNoTracking()
                    .FirstOrDefault(i =>
                        i.ApplicationID ==
                        application.ApplicationID &&
                        i.Status ==
                        ApplicationInterviewStatus.Scheduled);

            if (existingInterview != null)
            {
                TempData["Error"] =
                    "This applicant already has a scheduled interview.";

                return RedirectToAction(
                    "Review",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            var model =
                new TechnicianApplicationScheduleInterviewViewModel
                {
                    ApplicationID =
                        application.ApplicationID,

                    ApplicationReference =
                        application.ApplicationReference,

                    ApplicantName =
                        application.Citizen.FirstName +
                        " " +
                        application.Citizen.LastName,

                    ApplicantEmail =
                        application.Citizen.EmailAddress,

                    OpportunityCode =
                        application.Opportunity.OpportunityCode,

                    OpportunityTitle =
                        application.Opportunity.Title,

                    ApplicationDate =
                        application.ApplicationDate,

                    ApplicationStatus =
                        application.Status,

                    InterviewDate =
                        DateTime.Now.AddDays(1)
                };

            return View(model);
        }

            
[HttpPost]
[ValidateAntiForgeryToken]
public ActionResult ScheduleInterview(
    TechnicianApplicationScheduleInterviewViewModel model)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction(
                    "Login",
                    "Administrators");
            }

            if (model == null)
            {
                return HttpNotFound();
            }

            if (string.IsNullOrWhiteSpace(model.InterviewMethod))
            {
                ModelState.AddModelError(
                    "InterviewMethod",
                    "Please select an interview method.");
            }

            if (string.IsNullOrWhiteSpace(model.Location))
            {
                ModelState.AddModelError(
                    "Location",
                    "Please provide the interview location.");
            }

            if (!ModelState.IsValid)
            {
                return ReloadScheduleInterviewView(model);
            }

            if (model.InterviewDate <= DateTime.Now)
            {
                ModelState.AddModelError(
                    "InterviewDate",
                    "The interview date and time must be in the future.");

                return ReloadScheduleInterviewView(model);
            }

            var application =
                db.TechnicianApplications
                    .Include(a => a.Opportunity)
                    .Include(a => a.Citizen)
                    .FirstOrDefault(a =>
                        a.ApplicationID == model.ApplicationID);

            if (application == null)
            {
                return HttpNotFound();
            }

            if (application.Opportunity == null ||
                application.Citizen == null)
            {
                return HttpNotFound();
            }

            if (application.Status !=
                TechnicianApplicationStatus.Assessed)
            {
                TempData["Error"] =
                    "This applicant is no longer eligible for interview scheduling.";

                return RedirectToAction(
                    "Review",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            var existingInterview =
                db.ApplicationInterviews
                    .FirstOrDefault(i =>
                        i.ApplicationID ==
                        application.ApplicationID &&
                        i.Status ==
                        ApplicationInterviewStatus.Scheduled);

            if (existingInterview != null)
            {
                TempData["Error"] =
                    "This applicant already has a scheduled interview.";

                return RedirectToAction(
                    "Review",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            var administratorID =
                Convert.ToInt32(
                    Session["AdministratorID"]);

            var interview =
                new ApplicationInterview
                {
                    ApplicationID =
                        application.ApplicationID,

                    InterviewMethod =
                        model.InterviewMethod.Trim(),

                    InterviewDate =
                        model.InterviewDate,

                    Location =
                        model.Location.Trim(),

                    Instructions =
                        string.IsNullOrWhiteSpace(model.Instructions)
                            ? null
                            : model.Instructions.Trim(),

                    Status =
                        ApplicationInterviewStatus.Scheduled,

                    Outcome = null,

                    Comments = null,

                    RecordedByAdministratorID =
                        administratorID,

                    DateRecorded =
                        DateTime.Now
                };

            db.ApplicationInterviews.Add(interview);

            application.Status =
                TechnicianApplicationStatus.InterviewScheduled;

            application.LastUpdatedDate =
                DateTime.Now;

            // =========================================================
            // CITIZEN RECRUITMENT NOTIFICATION
            // =========================================================

            var notificationService =
                new CommunityServiceProject.Services
                    .TechnicianApplicationNotificationService(db);

            var notificationMessage =
                "Your interview for " +
                application.Opportunity.Title +
                " has been scheduled. " +
                "Interview method: " +
                interview.InterviewMethod +
                ". Date and time: " +
                interview.InterviewDate.ToString("dd MMM yyyy HH:mm") +
                ". Location: " +
                interview.Location +
                ".";

            if (!string.IsNullOrWhiteSpace(
                interview.Instructions))
            {
                notificationMessage +=
                    " Instructions: " +
                    interview.Instructions;
            }

            notificationService.Create(
                application,
                TechnicianApplicationNotificationType.InterviewScheduled,
                "Interview Scheduled",
                notificationMessage);

            // =========================================================
            // SAVE EVERYTHING
            // =========================================================

            db.SaveChanges();

            TempData["Success"] =
                "Interview scheduled successfully for " +
                application.Citizen.FirstName +
                " " +
                application.Citizen.LastName +
                ".";

            return RedirectToAction(
                "Review",
                new
                {
                    id = application.ApplicationID
                });
        }



        private ActionResult ReloadScheduleInterviewView(
    TechnicianApplicationScheduleInterviewViewModel model)
        {
            var application =
                db.TechnicianApplications
                    .AsNoTracking()
                    .Include(a => a.Opportunity)
                    .Include(a => a.Citizen)
                    .FirstOrDefault(a =>
                        a.ApplicationID == model.ApplicationID);

            if (application == null)
            {
                return HttpNotFound();
            }

            if (application.Opportunity == null ||
                application.Citizen == null)
            {
                return HttpNotFound();
            }

            model.ApplicationReference =
                application.ApplicationReference;

            model.ApplicantName =
                application.Citizen.FirstName +
                " " +
                application.Citizen.LastName;

            model.ApplicantEmail =
                application.Citizen.EmailAddress;

            model.OpportunityCode =
                application.Opportunity.OpportunityCode;

            model.OpportunityTitle =
                application.Opportunity.Title;

            model.ApplicationDate =
                application.ApplicationDate;

            model.ApplicationStatus =
                application.Status;

            return View(
                "ScheduleInterview",
                model);
        }




        [HttpGet]
        public ActionResult RecordInterviewOutcome(int? id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction(
                    "Login",
                    "Administrators");
            }

            if (!id.HasValue)
            {
                return HttpNotFound();
            }

            var interview =
                db.ApplicationInterviews
                    .AsNoTracking()
                    .Include(i => i.Application)
                    .Include(i => i.Application.Opportunity)
                    .Include(i => i.Application.Citizen)
                    .FirstOrDefault(i =>
                        i.InterviewID == id.Value);

            if (interview == null)
            {
                return HttpNotFound();
            }

            if (interview.Application == null ||
                interview.Application.Opportunity == null ||
                interview.Application.Citizen == null)
            {
                return HttpNotFound();
            }

            if (interview.Status !=
                ApplicationInterviewStatus.Scheduled)
            {
                TempData["Error"] =
                    "This interview cannot be updated because it is no longer scheduled.";

                return RedirectToAction(
                    "Review",
                    new
                    {
                        id = interview.ApplicationID
                    });
            }

            if (interview.Application.Status !=
                TechnicianApplicationStatus.InterviewScheduled)
            {
                TempData["Error"] =
                    "This applicant is not currently at the interview stage.";

                return RedirectToAction(
                    "Review",
                    new
                    {
                        id = interview.ApplicationID
                    });
            }

            var model =
                new TechnicianApplicationInterviewOutcomeViewModel
                {
                    InterviewID =
                        interview.InterviewID,

                    ApplicationID =
                        interview.ApplicationID,

                    ApplicationReference =
                        interview.Application.ApplicationReference,

                    ApplicantName =
                        interview.Application.Citizen.FirstName +
                        " " +
                        interview.Application.Citizen.LastName,

                    ApplicantEmail =
                        interview.Application.Citizen.EmailAddress,

                    OpportunityCode =
                        interview.Application.Opportunity.OpportunityCode,

                    OpportunityTitle =
                        interview.Application.Opportunity.Title,

                    ApplicationDate =
                        interview.Application.ApplicationDate,

                    InterviewMethod =
                        interview.InterviewMethod,

                    InterviewDate =
                        interview.InterviewDate,

                    Location =
                        interview.Location,

                    InterviewStatus =
                        interview.Status,

                    CanRecordOutcome =
                        true
                };

            return View(model);
        }

           
[HttpPost]
[ValidateAntiForgeryToken]
public ActionResult RecordInterviewOutcome(
    TechnicianApplicationInterviewOutcomeViewModel model)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction(
                    "Login",
                    "Administrators");
            }

            if (model == null)
            {
                return HttpNotFound();
            }

            if (string.IsNullOrWhiteSpace(model.Outcome))
            {
                ModelState.AddModelError(
                    "Outcome",
                    "Please provide the interview outcome.");
            }

            if (!ModelState.IsValid)
            {
                return ReloadInterviewOutcomeView(model);
            }

            var interview =
                db.ApplicationInterviews
                    .Include(i => i.Application)
                    .Include(i => i.Application.Opportunity)
                    .Include(i => i.Application.Citizen)
                    .FirstOrDefault(i =>
                        i.InterviewID == model.InterviewID);

            if (interview == null)
            {
                return HttpNotFound();
            }

            if (interview.Application == null ||
                interview.Application.Opportunity == null ||
                interview.Application.Citizen == null)
            {
                return HttpNotFound();
            }

            if (interview.Status !=
                ApplicationInterviewStatus.Scheduled)
            {
                TempData["Error"] =
                    "This interview has already been completed or is no longer available for outcome recording.";

                return RedirectToAction(
                    "Review",
                    new
                    {
                        id = interview.ApplicationID
                    });
            }

            if (interview.Application.Status !=
                TechnicianApplicationStatus.InterviewScheduled)
            {
                TempData["Error"] =
                    "This applicant is not currently at the interview stage.";

                return RedirectToAction(
                    "Review",
                    new
                    {
                        id = interview.ApplicationID
                    });
            }

            // =========================================================
            // RECORD INTERVIEW OUTCOME
            // =========================================================

            interview.Outcome =
                model.Outcome.Trim();

            interview.Comments =
                string.IsNullOrWhiteSpace(model.Comments)
                    ? null
                    : model.Comments.Trim();

            interview.Status =
                ApplicationInterviewStatus.Completed;

            interview.DateRecorded =
                DateTime.Now;

            interview.RecordedByAdministratorID =
                Convert.ToInt32(
                    Session["AdministratorID"]);

            // =========================================================
            // UPDATE APPLICATION STATUS
            // =========================================================

            interview.Application.Status =
                TechnicianApplicationStatus.Interviewed;

            interview.Application.LastUpdatedDate =
                DateTime.Now;

            // =========================================================
            // CITIZEN RECRUITMENT NOTIFICATION
            // =========================================================
            //
            // Do NOT expose:
            // - Interview outcome
            // - Administrator comments
            //
            // The citizen is only informed that the interview
            // stage has been completed.
            // =========================================================

            var notificationService =
                new CommunityServiceProject.Services
                    .TechnicianApplicationNotificationService(db);

            notificationService.Create(
                interview.Application,
                TechnicianApplicationNotificationType.InterviewCompleted,
                "Interview Completed",
                "The interview stage for your application for " +
                interview.Application.Opportunity.Title +
                " has been completed. " +
                "Your application will now proceed to the next stage of the technician recruitment process.");

            // =========================================================
            // SAVE EVERYTHING TOGETHER
            // =========================================================

            db.SaveChanges();

            TempData["Success"] =
                "Interview outcome for " +
                interview.Application.Citizen.FirstName +
                " " +
                interview.Application.Citizen.LastName +
                " was recorded successfully.";

            return RedirectToAction(
                "Review",
                new
                {
                    id = interview.ApplicationID
                });
        }




        private ActionResult ReloadInterviewOutcomeView(
    TechnicianApplicationInterviewOutcomeViewModel model)
        {
            var interview =
                db.ApplicationInterviews
                    .AsNoTracking()
                    .Include(i => i.Application)
                    .Include(i => i.Application.Opportunity)
                    .Include(i => i.Application.Citizen)
                    .FirstOrDefault(i =>
                        i.InterviewID == model.InterviewID);

            if (interview == null)
            {
                return HttpNotFound();
            }

            if (interview.Application == null ||
                interview.Application.Opportunity == null ||
                interview.Application.Citizen == null)
            {
                return HttpNotFound();
            }

            model.ApplicationID =
                interview.ApplicationID;

            model.ApplicationReference =
                interview.Application.ApplicationReference;

            model.ApplicantName =
                interview.Application.Citizen.FirstName +
                " " +
                interview.Application.Citizen.LastName;

            model.ApplicantEmail =
                interview.Application.Citizen.EmailAddress;

            model.OpportunityCode =
                interview.Application.Opportunity.OpportunityCode;

            model.OpportunityTitle =
                interview.Application.Opportunity.Title;

            model.ApplicationDate =
                interview.Application.ApplicationDate;

            model.InterviewMethod =
                interview.InterviewMethod;

            model.InterviewDate =
                interview.InterviewDate;

            model.Location =
                interview.Location;

            model.InterviewStatus =
                interview.Status;

            model.CanRecordOutcome =
                interview.Status ==
                ApplicationInterviewStatus.Scheduled;

            return View(
                "RecordInterviewOutcome",
                model);
        }


        [HttpGet]
        public ActionResult SelectApplicant(int? id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction(
                    "Login",
                    "Administrators");
            }

            if (!id.HasValue)
            {
                return HttpNotFound();
            }

            var application =
                db.TechnicianApplications
                    .AsNoTracking()
                    .Include(a => a.Opportunity)
                    .Include(a => a.Citizen)
                    .FirstOrDefault(a =>
                        a.ApplicationID == id.Value);

            if (application == null)
            {
                return HttpNotFound();
            }

            if (application.Opportunity == null ||
                application.Citizen == null)
            {
                return HttpNotFound();
            }

            if (application.Status !=
                TechnicianApplicationStatus.Interviewed)
            {
                TempData["Error"] =
                    "Only applicants who have completed the interview can be selected.";

                return RedirectToAction(
                    "Review",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            var existingSelection =
                db.TechnicianApplicationSelections
                    .AsNoTracking()
                    .FirstOrDefault(s =>
                        s.ApplicationID ==
                        application.ApplicationID);

            if (existingSelection != null)
            {
                TempData["Error"] =
                    "This applicant has already been selected.";

                return RedirectToAction(
                    "Review",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            var completedInterview =
                db.ApplicationInterviews
                    .AsNoTracking()
                    .FirstOrDefault(i =>
                        i.ApplicationID ==
                        application.ApplicationID &&
                        i.Status ==
                        ApplicationInterviewStatus.Completed);

            if (completedInterview == null)
            {
                TempData["Error"] =
                    "A completed interview record is required before this applicant can be selected.";

                return RedirectToAction(
                    "Review",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            var model =
                new TechnicianApplicationSelectViewModel
                {
                    ApplicationID =
                        application.ApplicationID,

                    ApplicationReference =
                        application.ApplicationReference,

                    ApplicantName =
                        application.Citizen.FirstName +
                        " " +
                        application.Citizen.LastName,

                    ApplicantEmail =
                        application.Citizen.EmailAddress,

                    OpportunityCode =
                        application.Opportunity.OpportunityCode,

                    OpportunityTitle =
                        application.Opportunity.Title,

                    ApplicationDate =
                        application.ApplicationDate,

                    ApplicationStatus =
                        application.Status,

                    InterviewMethod =
                        completedInterview.InterviewMethod,

                    InterviewDate =
                        completedInterview.InterviewDate,

                    InterviewLocation =
                        completedInterview.Location,

                    InterviewOutcome =
                        completedInterview.Outcome,

                    InterviewComments =
                        completedInterview.Comments,

                    ConfirmSelection = false
                };

            return View(model);
        }

               
[HttpPost]
[ValidateAntiForgeryToken]
public ActionResult SelectApplicant(
    TechnicianApplicationSelectViewModel model)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction(
                    "Login",
                    "Administrators");
            }

            if (model == null)
            {
                return HttpNotFound();
            }

            if (!model.ConfirmSelection)
            {
                ModelState.AddModelError(
                    "ConfirmSelection",
                    "Please confirm that you want to select this applicant.");
            }

            if (!ModelState.IsValid)
            {
                return ReloadSelectApplicantView(model);
            }

            var application =
                db.TechnicianApplications
                    .Include(a => a.Opportunity)
                    .Include(a => a.Citizen)
                    .FirstOrDefault(a =>
                        a.ApplicationID ==
                        model.ApplicationID);

            if (application == null)
            {
                return HttpNotFound();
            }

            if (application.Opportunity == null ||
                application.Citizen == null)
            {
                return HttpNotFound();
            }

            if (application.Status !=
                TechnicianApplicationStatus.Interviewed)
            {
                TempData["Error"] =
                    "This applicant is no longer eligible for selection.";

                return RedirectToAction(
                    "Review",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            var existingSelection =
                db.TechnicianApplicationSelections
                    .FirstOrDefault(s =>
                        s.ApplicationID ==
                        application.ApplicationID);

            if (existingSelection != null)
            {
                TempData["Error"] =
                    "This applicant has already been selected.";

                return RedirectToAction(
                    "Review",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            var completedInterview =
                db.ApplicationInterviews
                    .FirstOrDefault(i =>
                        i.ApplicationID ==
                        application.ApplicationID &&
                        i.Status ==
                        ApplicationInterviewStatus.Completed);

            if (completedInterview == null)
            {
                TempData["Error"] =
                    "A completed interview record is required before this applicant can be selected.";

                return RedirectToAction(
                    "Review",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            if (string.IsNullOrWhiteSpace(
                completedInterview.Outcome))
            {
                TempData["Error"] =
                    "The completed interview must have an outcome before the applicant can be selected.";

                return RedirectToAction(
                    "Review",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            var administratorID =
                Convert.ToInt32(
                    Session["AdministratorID"]);

            // =========================================================
            // CREATE SELECTION RECORD
            // =========================================================

            var selection =
                new TechnicianApplicationSelection
                {
                    ApplicationID =
                        application.ApplicationID,

                    Comments =
                        string.IsNullOrWhiteSpace(model.Comments)
                            ? null
                            : model.Comments.Trim(),

                    SelectedByAdministratorID =
                        administratorID,

                    SelectionDate =
                        DateTime.Now
                };

            db.TechnicianApplicationSelections.Add(
                selection);

            // =========================================================
            // UPDATE APPLICATION STATUS
            // =========================================================

            application.Status =
                TechnicianApplicationStatus.Selected;

            application.LastUpdatedDate =
                DateTime.Now;

            // =========================================================
            // CITIZEN RECRUITMENT NOTIFICATION
            // =========================================================
            //
            // Do NOT expose:
            // - Administrator selection comments
            // - Interview outcome
            // - Interview comments
            //
            // The citizen is simply informed that they
            // have been selected.
            // =========================================================

            var notificationService =
                new CommunityServiceProject.Services
                    .TechnicianApplicationNotificationService(db);

            notificationService.Create(
                application,
                TechnicianApplicationNotificationType.ApplicationSelected,
                "Application Selected",
                "Your application for " +
                application.Opportunity.Title +
                " has been selected. " +
                "Your application will now proceed to the final verification stage of the technician recruitment process.");

            // =========================================================
            // SAVE SELECTION + STATUS + NOTIFICATION
            // =========================================================

            db.SaveChanges();

            TempData["Success"] =
                "Applicant " +
                application.Citizen.FirstName +
                " " +
                application.Citizen.LastName +
                " has been selected successfully.";

            return RedirectToAction(
                "Review",
                new
                {
                    id = application.ApplicationID
                });
        }



        private ActionResult ReloadSelectApplicantView(
    TechnicianApplicationSelectViewModel model)
        {
            var application =
                db.TechnicianApplications
                    .AsNoTracking()
                    .Include(a => a.Opportunity)
                    .Include(a => a.Citizen)
                    .FirstOrDefault(a =>
                        a.ApplicationID ==
                        model.ApplicationID);

            if (application == null)
            {
                return HttpNotFound();
            }

            if (application.Opportunity == null ||
                application.Citizen == null)
            {
                return HttpNotFound();
            }

            var completedInterview =
                db.ApplicationInterviews
                    .AsNoTracking()
                    .FirstOrDefault(i =>
                        i.ApplicationID ==
                        application.ApplicationID &&
                        i.Status ==
                        ApplicationInterviewStatus.Completed);

            model.ApplicationReference =
                application.ApplicationReference;

            model.ApplicantName =
                application.Citizen.FirstName +
                " " +
                application.Citizen.LastName;

            model.ApplicantEmail =
                application.Citizen.EmailAddress;

            model.OpportunityCode =
                application.Opportunity.OpportunityCode;

            model.OpportunityTitle =
                application.Opportunity.Title;

            model.ApplicationDate =
                application.ApplicationDate;

            model.ApplicationStatus =
                application.Status;

            if (completedInterview != null)
            {
                model.InterviewMethod =
                    completedInterview.InterviewMethod;

                model.InterviewDate =
                    completedInterview.InterviewDate;

                model.InterviewLocation =
                    completedInterview.Location;

                model.InterviewOutcome =
                    completedInterview.Outcome;

                model.InterviewComments =
                    completedInterview.Comments;
            }

            return View(
                "SelectApplicant",
                model);
        }



        [HttpGet]
        public ActionResult FinalApplicantVerification(int? id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction(
                    "Login",
                    "Administrators");
            }

            if (!id.HasValue)
            {
                return HttpNotFound();
            }

            var application =
                db.TechnicianApplications
                    .AsNoTracking()
                    .Include(a => a.Opportunity)
                    .Include(a => a.Citizen)
                    .FirstOrDefault(a =>
                        a.ApplicationID == id.Value);

            if (application == null)
            {
                return HttpNotFound();
            }

            if (application.Opportunity == null ||
                application.Citizen == null)
            {
                return HttpNotFound();
            }

            if (application.Status !=
                TechnicianApplicationStatus.Selected)
            {
                TempData["Error"] =
                    "Only selected applicants can proceed to final verification.";

                return RedirectToAction(
                    "Review",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            var existingVerification =
                db.TechnicianApplicationFinalVerifications
                    .AsNoTracking()
                    .FirstOrDefault(v =>
                        v.ApplicationID ==
                        application.ApplicationID);

            if (existingVerification != null)
            {
                TempData["Error"] =
                    "Final verification has already been recorded for this applicant.";

                return RedirectToAction(
                    "Review",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            var screening =
                db.TechnicianApplicationScreenings
                    .AsNoTracking()
                    .FirstOrDefault(s =>
                        s.ApplicationID ==
                        application.ApplicationID);

            var documents =
                db.ApplicationDocuments
                    .AsNoTracking()
                    .Where(d =>
                        d.ApplicationID ==
                        application.ApplicationID)
                    .ToList();

            var assessment =
                db.ApplicationAssessments
                    .AsNoTracking()
                    .Where(a =>
                        a.ApplicationID ==
                        application.ApplicationID &&
                        a.Status ==
                        ApplicationAssessmentStatus.Completed)
                    .OrderByDescending(a => a.AssessmentDate)
                    .FirstOrDefault();

            var interview =
                db.ApplicationInterviews
                    .AsNoTracking()
                    .Where(i =>
                        i.ApplicationID ==
                        application.ApplicationID &&
                        i.Status ==
                        ApplicationInterviewStatus.Completed)
                    .OrderByDescending(i => i.InterviewDate)
                    .FirstOrDefault();

            var selection =
                db.TechnicianApplicationSelections
                    .AsNoTracking()
                    .FirstOrDefault(s =>
                        s.ApplicationID ==
                        application.ApplicationID);

            var model =
                new TechnicianApplicationFinalVerificationViewModel
                {
                    ApplicationID =
                        application.ApplicationID,

                    ApplicationReference =
                        application.ApplicationReference,

                    ApplicantName =
                        application.Citizen.FirstName +
                        " " +
                        application.Citizen.LastName,

                    ApplicantEmail =
                        application.Citizen.EmailAddress,

                    OpportunityCode =
                        application.Opportunity.OpportunityCode,

                    OpportunityTitle =
                        application.Opportunity.Title,

                    ApplicationDate =
                        application.ApplicationDate,

                    ApplicationStatus =
                        application.Status,

                    ScreeningResult =
                        screening != null
                            ? screening.OverallResult.ToString()
                            : "Not available",

                    ScreeningComments =
                        screening != null
                            ? screening.ScreeningComments
                            : null,

                    TotalDocuments =
                        documents.Count,

                    AcceptedDocuments =
                        documents.Count(d =>
                            d.VerificationStatus ==
                            ApplicationDocumentVerificationStatus.Accepted),

                    RejectedDocuments =
                        documents.Count(d =>
                            d.VerificationStatus ==
                            ApplicationDocumentVerificationStatus.Rejected),

                    AssessmentType =
                        assessment != null
                            ? assessment.AssessmentType
                            : null,

                    AssessmentDate =
                        assessment != null
                            ? assessment.AssessmentDate
                            : (DateTime?)null,

                    AssessmentResult =
                        assessment != null
                            ? assessment.Result
                            : null,

                    AssessmentScore =
                        assessment != null
                            ? assessment.Score
                            : (decimal?)null,

                    AssessmentComments =
                        assessment != null
                            ? assessment.Comments
                            : null,

                    InterviewMethod =
                        interview != null
                            ? interview.InterviewMethod
                            : null,

                    InterviewDate =
                        interview != null
                            ? interview.InterviewDate
                            : (DateTime?)null,

                    InterviewLocation =
                        interview != null
                            ? interview.Location
                            : null,

                    InterviewOutcome =
                        interview != null
                            ? interview.Outcome
                            : null,

                    InterviewComments =
                        interview != null
                            ? interview.Comments
                            : null,

                    SelectionComments =
                        selection != null
                            ? selection.Comments
                            : null,

                    SelectionDate =
                        selection != null
                            ? selection.SelectionDate
                            : (DateTime?)null,

                    ConfirmVerification = false
                };

            return View(model);
        }

             
[HttpPost]
[ValidateAntiForgeryToken]
public ActionResult FinalApplicantVerification(
    TechnicianApplicationFinalVerificationViewModel model)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction(
                    "Login",
                    "Administrators");
            }

            if (model == null)
            {
                return HttpNotFound();
            }

            if (!model.ConfirmVerification)
            {
                ModelState.AddModelError(
                    "ConfirmVerification",
                    "Please confirm that you have completed the final verification.");
            }

            if (!model.Result.HasValue)
            {
                ModelState.AddModelError(
                    "Result",
                    "Please select the final verification result.");
            }

            if (!ModelState.IsValid)
            {
                return ReloadFinalApplicantVerificationView(model);
            }

            var application =
                db.TechnicianApplications
                    .Include(a => a.Opportunity)
                    .Include(a => a.Citizen)
                    .FirstOrDefault(a =>
                        a.ApplicationID ==
                        model.ApplicationID);

            if (application == null)
            {
                return HttpNotFound();
            }

            if (application.Opportunity == null ||
                application.Citizen == null)
            {
                return HttpNotFound();
            }

            if (application.Status !=
                TechnicianApplicationStatus.Selected)
            {
                TempData["Error"] =
                    "This applicant is no longer eligible for final verification.";

                return RedirectToAction(
                    "Review",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            // =========================================================
            // PREVENT DUPLICATE FINAL VERIFICATION
            // =========================================================

            var existingVerification =
                db.TechnicianApplicationFinalVerifications
                    .FirstOrDefault(v =>
                        v.ApplicationID ==
                        application.ApplicationID);

            if (existingVerification != null)
            {
                TempData["Error"] =
                    "Final verification has already been recorded for this applicant.";

                return RedirectToAction(
                    "Review",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            // =========================================================
            // RETRIEVE RECRUITMENT HISTORY
            // =========================================================

            var screening =
                db.TechnicianApplicationScreenings
                    .FirstOrDefault(s =>
                        s.ApplicationID ==
                        application.ApplicationID);

            var documents =
                db.ApplicationDocuments
                    .Where(d =>
                        d.ApplicationID ==
                        application.ApplicationID)
                    .ToList();

            var assessment =
                db.ApplicationAssessments
                    .Where(a =>
                        a.ApplicationID ==
                        application.ApplicationID &&
                        a.Status ==
                        ApplicationAssessmentStatus.Completed)
                    .OrderByDescending(a =>
                        a.AssessmentDate)
                    .FirstOrDefault();

            var interview =
                db.ApplicationInterviews
                    .Where(i =>
                        i.ApplicationID ==
                        application.ApplicationID &&
                        i.Status ==
                        ApplicationInterviewStatus.Completed)
                    .OrderByDescending(i =>
                        i.InterviewDate)
                    .FirstOrDefault();

            var selection =
                db.TechnicianApplicationSelections
                    .FirstOrDefault(s =>
                        s.ApplicationID ==
                        application.ApplicationID);

            // =========================================================
            // VALIDATE RECRUITMENT HISTORY
            // =========================================================

            if (screening == null)
            {
                TempData["Error"] =
                    "Final verification cannot proceed because the applicant has no screening record.";

                return RedirectToAction(
                    "Review",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            if (screening.OverallResult !=
                ApplicantScreeningResult.MeetsRequirements)
            {
                TempData["Error"] =
                    "Final verification cannot proceed because the applicant's screening result does not confirm that the requirements were met.";

                return RedirectToAction(
                    "Review",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            if (documents.Count == 0)
            {
                TempData["Error"] =
                    "Final verification cannot proceed because no supporting documents have been verified.";

                return RedirectToAction(
                    "Review",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            var allDocumentsAccepted =
                documents.All(d =>
                    d.VerificationStatus ==
                    ApplicationDocumentVerificationStatus.Accepted);

            if (!allDocumentsAccepted)
            {
                TempData["Error"] =
                    "Final verification cannot proceed because all supporting documents must be accepted.";

                return RedirectToAction(
                    "Review",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            if (assessment == null)
            {
                TempData["Error"] =
                    "Final verification cannot proceed because no completed assessment record exists.";

                return RedirectToAction(
                    "Review",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            if (string.IsNullOrWhiteSpace(
                assessment.Result))
            {
                TempData["Error"] =
                    "Final verification cannot proceed because the assessment result has not been recorded.";

                return RedirectToAction(
                    "Review",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            if (interview == null)
            {
                TempData["Error"] =
                    "Final verification cannot proceed because no completed interview record exists.";

                return RedirectToAction(
                    "Review",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            if (string.IsNullOrWhiteSpace(
                interview.Outcome))
            {
                TempData["Error"] =
                    "Final verification cannot proceed because the interview outcome has not been recorded.";

                return RedirectToAction(
                    "Review",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            if (selection == null)
            {
                TempData["Error"] =
                    "Final verification cannot proceed because no applicant selection record exists.";

                return RedirectToAction(
                    "Review",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            // =========================================================
            // CREATE FINAL VERIFICATION RECORD
            // =========================================================

            var administratorID =
                Convert.ToInt32(
                    Session["AdministratorID"]);

            var verification =
                new TechnicianApplicationFinalVerification
                {
                    ApplicationID =
                        application.ApplicationID,

                    Result =
                        model.Result.Value,

                    Comments =
                        string.IsNullOrWhiteSpace(model.Comments)
                            ? null
                            : model.Comments.Trim(),

                    VerifiedByAdministratorID =
                        administratorID,

                    VerificationDate =
                        DateTime.Now
                };

            db.TechnicianApplicationFinalVerifications.Add(
                verification);

            // =========================================================
            // UPDATE APPLICATION STATUS
            // =========================================================

            if (model.Result.Value ==
                ApplicationFinalVerificationResult.Verified)
            {
                application.Status =
                    TechnicianApplicationStatus.ApprovedForOnboarding;
            }
            else
            {
                application.Status =
                    TechnicianApplicationStatus.NotSelected;
            }

            application.LastUpdatedDate =
                DateTime.Now;

            // =========================================================
            // CREATE CITIZEN NOTIFICATION
            // =========================================================

            var notificationService =
                new CommunityServiceProject.Services
                    .TechnicianApplicationNotificationService(db);

            if (model.Result.Value ==
                ApplicationFinalVerificationResult.Verified)
            {
                notificationService.Create(
                    application,
                    TechnicianApplicationNotificationType
                        .ApprovedForOnboarding,
                    "Approved for Onboarding",
                    "Your application for " +
                    application.Opportunity.Title +
                    " has successfully completed the final verification stage. " +
                    "You have been approved for technician onboarding. " +
                    "Further onboarding information will be provided by the municipality.");
            }
            else
            {
                notificationService.Create(
                    application,
                    TechnicianApplicationNotificationType
                        .ApplicationNotSelected,
                    "Application Not Selected",
                    "Your application for " +
                    application.Opportunity.Title +
                    " has completed the final verification stage. " +
                    "Unfortunately, your application has not been approved to proceed to technician onboarding.");
            }

            // =========================================================
            // SAVE VERIFICATION + STATUS + NOTIFICATION
            // =========================================================

            db.SaveChanges();

            // =========================================================
            // ADMIN CONFIRMATION
            // =========================================================

            if (model.Result.Value ==
                ApplicationFinalVerificationResult.Verified)
            {
                TempData["Success"] =
                    "Final verification completed successfully. " +
                    application.Citizen.FirstName +
                    " " +
                    application.Citizen.LastName +
                    " is approved for onboarding.";
            }
            else
            {
                TempData["Success"] =
                    "Final verification was recorded. " +
                    application.Citizen.FirstName +
                    " " +
                    application.Citizen.LastName +
                    " was not approved for onboarding.";
            }

            return RedirectToAction(
                "Review",
                new
                {
                    id = application.ApplicationID
                });
        }



        private ActionResult ReloadFinalApplicantVerificationView(
    TechnicianApplicationFinalVerificationViewModel model)
        {
            var application =
                db.TechnicianApplications
                    .AsNoTracking()
                    .Include(a => a.Opportunity)
                    .Include(a => a.Citizen)
                    .FirstOrDefault(a =>
                        a.ApplicationID ==
                        model.ApplicationID);

            if (application == null)
            {
                return HttpNotFound();
            }

            if (application.Opportunity == null ||
                application.Citizen == null)
            {
                return HttpNotFound();
            }

            var screening =
                db.TechnicianApplicationScreenings
                    .AsNoTracking()
                    .FirstOrDefault(s =>
                        s.ApplicationID ==
                        application.ApplicationID);

            var documents =
                db.ApplicationDocuments
                    .AsNoTracking()
                    .Where(d =>
                        d.ApplicationID ==
                        application.ApplicationID)
                    .ToList();

            var assessment =
                db.ApplicationAssessments
                    .AsNoTracking()
                    .Where(a =>
                        a.ApplicationID ==
                        application.ApplicationID &&
                        a.Status ==
                        ApplicationAssessmentStatus.Completed)
                    .OrderByDescending(a => a.AssessmentDate)
                    .FirstOrDefault();

            var interview =
                db.ApplicationInterviews
                    .AsNoTracking()
                    .Where(i =>
                        i.ApplicationID ==
                        application.ApplicationID &&
                        i.Status ==
                        ApplicationInterviewStatus.Completed)
                    .OrderByDescending(i => i.InterviewDate)
                    .FirstOrDefault();

            var selection =
                db.TechnicianApplicationSelections
                    .AsNoTracking()
                    .FirstOrDefault(s =>
                        s.ApplicationID ==
                        application.ApplicationID);

            model.ApplicationReference =
                application.ApplicationReference;

            model.ApplicantName =
                application.Citizen.FirstName +
                " " +
                application.Citizen.LastName;

            model.ApplicantEmail =
                application.Citizen.EmailAddress;

            model.OpportunityCode =
                application.Opportunity.OpportunityCode;

            model.OpportunityTitle =
                application.Opportunity.Title;

            model.ApplicationDate =
                application.ApplicationDate;

            model.ApplicationStatus =
                application.Status;

            model.ScreeningResult =
                screening != null
                    ? screening.OverallResult.ToString()
                    : "Not available";

            model.ScreeningComments =
                screening != null
                    ? screening.ScreeningComments
                    : null;

            model.TotalDocuments =
                documents.Count;

            model.AcceptedDocuments =
                documents.Count(d =>
                    d.VerificationStatus ==
                    ApplicationDocumentVerificationStatus.Accepted);

            model.RejectedDocuments =
                documents.Count(d =>
                    d.VerificationStatus ==
                    ApplicationDocumentVerificationStatus.Rejected);

            model.AssessmentType =
                assessment != null
                    ? assessment.AssessmentType
                    : null;

            model.AssessmentDate =
                assessment != null
                    ? assessment.AssessmentDate
                    : (DateTime?)null;

            model.AssessmentResult =
                assessment != null
                    ? assessment.Result
                    : null;

            model.AssessmentScore =
                assessment != null
                    ? assessment.Score
                    : (decimal?)null;

            model.AssessmentComments =
                assessment != null
                    ? assessment.Comments
                    : null;

            model.InterviewMethod =
                interview != null
                    ? interview.InterviewMethod
                    : null;

            model.InterviewDate =
                interview != null
                    ? interview.InterviewDate
                    : (DateTime?)null;

            model.InterviewLocation =
                interview != null
                    ? interview.Location
                    : null;

            model.InterviewOutcome =
                interview != null
                    ? interview.Outcome
                    : null;

            model.InterviewComments =
                interview != null
                    ? interview.Comments
                    : null;

            model.SelectionComments =
                selection != null
                    ? selection.Comments
                    : null;

            model.SelectionDate =
                selection != null
                    ? selection.SelectionDate
                    : (DateTime?)null;

            return View(
                "FinalApplicantVerification",
                model);
        }


        [HttpGet]
        public ActionResult OnboardTechnician(int? id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            if (!id.HasValue)
            {
                return RedirectToAction("ReviewApplications");
            }

            var application = db.TechnicianApplications
                .Include(a => a.Citizen)
                .Include(a => a.Opportunity)
                .FirstOrDefault(a => a.ApplicationID == id.Value);

            if (application == null)
            {
                return HttpNotFound();
            }

            // ------------------------------------------------------------
            // Application must be approved for onboarding or already onboarded
            // ------------------------------------------------------------

            if (application.Status != TechnicianApplicationStatus.ApprovedForOnboarding &&
                application.Status != TechnicianApplicationStatus.Onboarded)
            {
                TempData["ErrorMessage"] =
                    "This applicant is not currently approved for onboarding.";

                return RedirectToAction(
                    "Review",
                    new { id = application.ApplicationID }
                );
            }

            // ------------------------------------------------------------
            // Recruitment evidence
            // ------------------------------------------------------------

            var finalVerification =
                db.TechnicianApplicationFinalVerifications
                    .FirstOrDefault(v =>
                        v.ApplicationID == application.ApplicationID);

            var screening =
                db.TechnicianApplicationScreenings
                    .FirstOrDefault(s =>
                        s.ApplicationID == application.ApplicationID);

            var assessment =
                db.ApplicationAssessments
                    .Where(a =>
                        a.ApplicationID == application.ApplicationID &&
                        a.Status == ApplicationAssessmentStatus.Completed)
                    .OrderByDescending(a => a.AssessmentDate)
                    .FirstOrDefault();

            var interview =
                db.ApplicationInterviews
                    .Where(i =>
                        i.ApplicationID == application.ApplicationID &&
                        i.Status == ApplicationInterviewStatus.Completed)
                    .OrderByDescending(i => i.InterviewDate)
                    .FirstOrDefault();

            var selection =
                db.TechnicianApplicationSelections
                    .FirstOrDefault(s =>
                        s.ApplicationID == application.ApplicationID);

            var verifiedDocuments =
                db.ApplicationDocuments.Count(d =>
                    d.ApplicationID == application.ApplicationID &&
                    d.VerificationStatus ==
                        ApplicationDocumentVerificationStatus.Accepted);

            var unverifiedDocuments =
                db.ApplicationDocuments.Count(d =>
                    d.ApplicationID == application.ApplicationID &&
                    d.VerificationStatus !=
                        ApplicationDocumentVerificationStatus.Accepted);

            // ------------------------------------------------------------
            // Available technician skills
            // ------------------------------------------------------------

            var skills = db.Skills
                .OrderBy(s => s.SkillName)
                .ToList();

            // ------------------------------------------------------------
            // Build onboarding ViewModel
            // ------------------------------------------------------------

            var model = new TechnicianOnboardingViewModel
            {
                ApplicationID =
                    application.ApplicationID,

                ApplicationReference =
                    application.ApplicationReference,

                ApplicantName =
                    application.Citizen.FirstName + " " +
                    application.Citizen.LastName,

                PersonalEmail =
                    application.Citizen.EmailAddress,

                ApplicantPhoneNumber =
                    application.Citizen.PhoneNumber,

                ApplicationDate =
                    application.ApplicationDate,

                OpportunityTitle =
                    application.Opportunity.Title,

                OpportunityCode =
                    application.Opportunity.OpportunityCode,

                EmploymentType =
                    application.Opportunity.EmploymentType,

                ScreeningResult =
                    screening != null
                        ? screening.OverallResult.ToString()
                        : "Not available",

                VerifiedDocumentCount =
                    verifiedDocuments,

                UnverifiedDocumentCount =
                    unverifiedDocuments,

                AssessmentResult =
                    assessment != null
                        ? assessment.Result
                        : "Not available",

                AssessmentScore =
                    assessment != null
                        ? assessment.Score
                        : null,

                InterviewOutcome =
                    interview != null
                        ? interview.Outcome
                        : "Not available",

                SelectionComments =
                    selection != null
                        ? selection.Comments
                        : null,

                FinalVerificationResult =
                    finalVerification != null
                        ? finalVerification.Result.ToString()
                        : "Not available",

                FinalVerificationComments =
                    finalVerification != null
                        ? finalVerification.Comments
                        : null,

                // --------------------------------------------------------
                // Prepopulate technician account from Citizen
                // --------------------------------------------------------

                FirstName =
                    application.Citizen.FirstName,

                LastName =
                    application.Citizen.LastName,

                PhoneNumber =
                    application.Citizen.PhoneNumber,

                ApplicationStatus =
                    application.Status,

                // --------------------------------------------------------
                // Skill dropdown
                // --------------------------------------------------------

                AvailableSkills =
                    skills.Select(s => new SelectListItem
                    {
                        Value =
                            s.SkillID.ToString(),

                        Text =
                            s.SkillName
                    }).ToList()
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult OnboardTechnician(
    TechnicianOnboardingViewModel model)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction(
                    "Login",
                    "Administrators");
            }

            if (model == null)
            {
                TempData["ErrorMessage"] =
                    "Invalid onboarding request.";

                return RedirectToAction(
                    "ReviewApplications");
            }

            var application =
                db.TechnicianApplications
                    .Include(a => a.Citizen)
                    .Include(a => a.Opportunity)
                    .FirstOrDefault(a =>
                        a.ApplicationID ==
                        model.ApplicationID);

            if (application == null)
            {
                return HttpNotFound();
            }

            // ------------------------------------------------------------
            // Verify application state
            // ------------------------------------------------------------
            //
            // The application may be onboarded for the first time while
            // ApprovedForOnboarding, or onboarded again after it has already
            // been changed to Onboarded.
            //
            // ------------------------------------------------------------

            if (application.Status !=
                    TechnicianApplicationStatus.ApprovedForOnboarding &&
                application.Status !=
                    TechnicianApplicationStatus.Onboarded)
            {
                TempData["ErrorMessage"] =
                    "This applicant is no longer eligible for onboarding.";

                return RedirectToAction(
                    "Review",
                    new
                    {
                        id = application.ApplicationID
                    });
            }

            // ------------------------------------------------------------
            // No duplicate onboarding restriction
            // ------------------------------------------------------------
            //
            // The same application may create multiple Technician records.
            //
            // ------------------------------------------------------------

            // ------------------------------------------------------------
            // No duplicate Citizen → Technician restriction
            // ------------------------------------------------------------
            //
            // The same Citizen may be linked to multiple Technician records.
            //
            // ------------------------------------------------------------

            // ------------------------------------------------------------
            // Validate municipal email
            // ------------------------------------------------------------

            model.MunicipalEmail =
                (model.MunicipalEmail ?? string.Empty)
                    .Trim()
                    .ToLower();

            var existingEmail =
                db.Technicians
                    .FirstOrDefault(t =>
                        t.EmailAddress.ToLower() ==
                        model.MunicipalEmail);

            if (existingEmail != null)
            {
                ModelState.AddModelError(
                    "MunicipalEmail",
                    "This municipal email address is already assigned to another technician.");
            }

            // ------------------------------------------------------------
            // Validate names / phone
            // ------------------------------------------------------------

            model.FirstName =
                (model.FirstName ?? string.Empty).Trim();

            model.LastName =
                (model.LastName ?? string.Empty).Trim();

            model.PhoneNumber =
                (model.PhoneNumber ?? string.Empty).Trim();

            // ------------------------------------------------------------
            // Validate selected skill
            // ------------------------------------------------------------

            var selectedSkill =
                db.Skills
                    .FirstOrDefault(s =>
                        s.SkillID ==
                        model.SelectedSkillID);

            if (selectedSkill == null)
            {
                ModelState.AddModelError(
                    "SelectedSkillID",
                    "Please select a valid primary technician skill.");
            }

            // ------------------------------------------------------------
            // Validate onboarding confirmation
            // ------------------------------------------------------------

            if (!model.ConfirmOnboarding)
            {
                ModelState.AddModelError(
                    "ConfirmOnboarding",
                    "Please confirm that the onboarding information has been reviewed.");
            }

            // ------------------------------------------------------------
            // Return to page if validation fails
            // ------------------------------------------------------------

            if (!ModelState.IsValid)
            {
                // --------------------------------------------------------
                // Rebuild read-only recruitment information
                // --------------------------------------------------------

                var finalVerification =
                    db.TechnicianApplicationFinalVerifications
                        .FirstOrDefault(v =>
                            v.ApplicationID ==
                            application.ApplicationID);

                var screening =
                    db.TechnicianApplicationScreenings
                        .FirstOrDefault(s =>
                            s.ApplicationID ==
                            application.ApplicationID);

                var assessment =
                    db.ApplicationAssessments
                        .Where(a =>
                            a.ApplicationID ==
                            application.ApplicationID)
                        .OrderByDescending(a =>
                            a.AssessmentDate)
                        .FirstOrDefault();

                var interview =
                    db.ApplicationInterviews
                        .Where(i =>
                            i.ApplicationID ==
                            application.ApplicationID)
                        .OrderByDescending(i =>
                            i.InterviewDate)
                        .FirstOrDefault();

                var selection =
                    db.TechnicianApplicationSelections
                        .FirstOrDefault(s =>
                            s.ApplicationID ==
                            application.ApplicationID);

                model.ApplicationReference =
                    application.ApplicationReference;

                model.ApplicantName =
                    application.Citizen.FirstName + " " +
                    application.Citizen.LastName;

                model.PersonalEmail =
                    application.Citizen.EmailAddress;

                model.ApplicantPhoneNumber =
                    application.Citizen.PhoneNumber;

                model.ApplicationDate =
                    application.ApplicationDate;

                model.OpportunityTitle =
                    application.Opportunity.Title;

                model.OpportunityCode =
                    application.Opportunity.OpportunityCode;

                model.EmploymentType =
                    application.Opportunity.EmploymentType;

                model.ScreeningResult =
                    screening != null
                        ? screening.OverallResult.ToString()
                        : "Not available";

                model.AssessmentResult =
                    assessment != null
                        ? assessment.Result
                        : "Not available";

                model.AssessmentScore =
                    assessment != null
                        ? assessment.Score
                        : null;

                model.InterviewOutcome =
                    interview != null
                        ? interview.Outcome
                        : "Not available";

                model.SelectionComments =
                    selection != null
                        ? selection.Comments
                        : null;

                model.FinalVerificationResult =
                    finalVerification != null
                        ? finalVerification.Result.ToString()
                        : "Not available";

                model.FinalVerificationComments =
                    finalVerification != null
                        ? finalVerification.Comments
                        : null;

                model.ApplicationStatus =
                    application.Status;

                model.VerifiedDocumentCount =
                    db.ApplicationDocuments.Count(d =>
                        d.ApplicationID ==
                            application.ApplicationID &&
                        d.VerificationStatus ==
                            ApplicationDocumentVerificationStatus.Accepted);

                model.UnverifiedDocumentCount =
                    db.ApplicationDocuments.Count(d =>
                        d.ApplicationID ==
                            application.ApplicationID &&
                        d.VerificationStatus !=
                            ApplicationDocumentVerificationStatus.Accepted);

                // --------------------------------------------------------
                // Rebuild skill dropdown
                // --------------------------------------------------------

                model.AvailableSkills =
                    db.Skills
                        .OrderBy(s => s.SkillName)
                        .Select(s => new SelectListItem
                        {
                            Value =
                                s.SkillID.ToString(),

                            Text =
                                s.SkillName,

                            Selected =
                                s.SkillID ==
                                model.SelectedSkillID
                        })
                        .ToList();

                return View(model);
            }

            // ------------------------------------------------------------
            // Administrator
            // ------------------------------------------------------------

            int administratorID =
                (int)Session["AdministratorID"];

            // ------------------------------------------------------------
            // Database transaction
            // ------------------------------------------------------------

            using (var transaction =
                db.Database.BeginTransaction())
            {
                try
                {
                    // ====================================================
                    // CREATE TECHNICIAN
                    // ====================================================

                    var technician =
                        new Technician
                        {
                            FirstName =
                                model.FirstName,

                            LastName =
                                model.LastName,

                            EmailAddress =
                                model.MunicipalEmail,

                            PhoneNumber =
                                model.PhoneNumber,

                            Password =
                                model.Password,

                            // The administrator-created password is temporary.
                            // The technician must change it after first login.
                            MustChangePassword =
                                true,

                            AccountStatus =
                                AccountStatus.Active,

                            CitizenID =
                                application.CitizenID
                        };

                    db.Technicians.Add(
                        technician);

                    // ====================================================
                    // ASSIGN PRIMARY TECHNICIAN SKILL
                    // ====================================================

                    var technicianSkill =
                        new TechnicianSkill
                        {
                            TechnicianID =
                                technician.TechnicianID,

                            SkillID =
                                selectedSkill.SkillID
                        };

                    db.TechnicianSkills.Add(
                        technicianSkill);

                    // ====================================================
                    // CREATE ONBOARDING AUDIT RECORD
                    // ====================================================

                    var onboarding =
                        new TechnicianOnboarding
                        {
                            ApplicationID =
                                application.ApplicationID,

                            TechnicianID =
                                technician.TechnicianID,

                            OnboardedByAdministratorID =
                                administratorID,

                            OnboardingDate =
                                DateTime.Now,

                            MunicipalEmail =
                                technician.EmailAddress
                        };

                    db.TechnicianOnboardings.Add(
                        onboarding);

                    // ====================================================
                    // UPDATE APPLICATION
                    // ====================================================

                    application.Status =
                        TechnicianApplicationStatus.Onboarded;

                    application.LastUpdatedDate =
                        DateTime.Now;

                    // ====================================================
                    // CREATE CITIZEN NOTIFICATION
                    // ====================================================

                    var notificationService =
                        new CommunityServiceProject.Services
                            .TechnicianApplicationNotificationService(db);

                    notificationService.Create(
                        application,
                        TechnicianApplicationNotificationType
                            .OnboardingCompleted,
                        "Technician Onboarding Completed",
                        "Your application for " +
                        application.Opportunity.Title +
                        " has successfully completed the technician onboarding process. " +
                        "Your municipal technician account has been created and activated. " +
                        "You can now use your municipal technician login to access the system.");

                    // ====================================================
                    // SAVE EVERYTHING
                    // ====================================================

                    db.SaveChanges();

                    transaction.Commit();

                    // ====================================================
                    // SUCCESS INFORMATION
                    // ====================================================

                    TempData["OnboardingSuccess"] =
                        true;

                    TempData["TechnicianName"] =
                        technician.FirstName + " " +
                        technician.LastName;

                    TempData["TechnicianEmail"] =
                        technician.EmailAddress;

                    TempData["ApplicationReference"] =
                        application.ApplicationReference;

                    TempData["TechnicianID"] =
                        technician.TechnicianID;

                    return RedirectToAction(
                        "OnboardingSuccess",
                        new
                        {
                            id =
                                application.ApplicationID
                        });
                }
                catch
                {
                    transaction.Rollback();

                    ModelState.AddModelError(
                        "",
                        "The technician could not be onboarded. No changes were saved.");

                    // ----------------------------------------------------
                    // Rebuild dropdown in case transaction fails
                    // ----------------------------------------------------

                    model.AvailableSkills =
                        db.Skills
                            .OrderBy(s => s.SkillName)
                            .Select(s => new SelectListItem
                            {
                                Value =
                                    s.SkillID.ToString(),

                                Text =
                                    s.SkillName,

                                Selected =
                                    s.SkillID ==
                                    model.SelectedSkillID
                            })
                            .ToList();

                    return View(model);
                }
            }
        }

        public ActionResult OnboardingSuccess(int? id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            if (!id.HasValue)
            {
                return RedirectToAction("ReviewApplications");
            }

            var application = db.TechnicianApplications
                .Include(a => a.Citizen)
                .Include(a => a.Opportunity)
                .FirstOrDefault(a => a.ApplicationID == id.Value);

            if (application == null)
            {
                return HttpNotFound();
            }

            var onboarding = db.TechnicianOnboardings
                .FirstOrDefault(o =>
                    o.ApplicationID == application.ApplicationID);

            if (onboarding == null)
            {
                TempData["ErrorMessage"] =
                    "Technician onboarding information could not be found.";

                return RedirectToAction(
                    "Review",
                    new { id = application.ApplicationID }
                );
            }

            var technician = db.Technicians
                .FirstOrDefault(t =>
                    t.TechnicianID == onboarding.TechnicianID);

            if (technician == null)
            {
                TempData["ErrorMessage"] =
                    "The onboarded technician account could not be found.";

                return RedirectToAction(
                    "Review",
                    new { id = application.ApplicationID }
                );
            }

            // =========================================================
            // LOAD ASSIGNED TECHNICIAN SKILL
            // =========================================================

            var technicianSkill = db.TechnicianSkills
                .Include(ts => ts.Skill)
                .FirstOrDefault(ts =>
                    ts.TechnicianID == technician.TechnicianID);

            if (technicianSkill == null || technicianSkill.Skill == null)
            {
                TempData["ErrorMessage"] =
                    "The technician was onboarded, but the assigned skill could not be found.";

                return RedirectToAction(
                    "Review",
                    new { id = application.ApplicationID }
                );
            }

            var administrator = db.Administrators
                .FirstOrDefault(a =>
                    a.AdministratorID ==
                    onboarding.OnboardedByAdministratorID);

            var model = new TechnicianOnboardingSuccessViewModel
            {
                ApplicationID =
                    application.ApplicationID,

                ApplicationReference =
                    application.ApplicationReference,

                OpportunityCode =
                    application.Opportunity.OpportunityCode,

                OpportunityTitle =
                    application.Opportunity.Title,

                ApplicationStatus =
                    application.Status,

                TechnicianID =
                    technician.TechnicianID,

                TechnicianName =
                    technician.FirstName + " " +
                    technician.LastName,

                MunicipalEmail =
                    technician.EmailAddress,

                PhoneNumber =
                    technician.PhoneNumber,

                AccountStatus =
                    technician.AccountStatus,

                CitizenID =
                    application.CitizenID,

                CitizenName =
                    application.Citizen.FirstName + " " +
                    application.Citizen.LastName,

                OnboardingDate =
                    onboarding.OnboardingDate,

                AdministratorName =
                    administrator != null
                        ? administrator.FirstName + " " +
                          administrator.LastName
                        : "Administrator",

                SkillID =
                    technicianSkill.SkillID,

                SkillName =
                    technicianSkill.Skill.SkillName
            };

            return View(model);
        }

        
           
               [HttpGet]
public ActionResult TechnicianAccountDetails(int onboardingId)
        {
            if (Session["CitizenID"] == null)
            {
                return RedirectToAction("Login", "Citizens");
            }

            int citizenID = (int)Session["CitizenID"];

            var onboarding = db.TechnicianOnboardings
                .Include(o => o.Technician)
                .Include(o => o.Application)
                .Include(o => o.Application.Citizen)
                .FirstOrDefault(o =>
                    o.OnboardingID == onboardingId);

            if (onboarding == null)
            {
                return HttpNotFound();
            }

            if (onboarding.Application.CitizenID != citizenID)
            {
                return new HttpUnauthorizedResult();
            }

            var technician = onboarding.Technician;
            var citizen = onboarding.Application.Citizen;

            if (technician == null || citizen == null)
            {
                return HttpNotFound();
            }

            var model = new TechnicianAccountDetailsViewModel
            {
                OnboardingID = onboarding.OnboardingID,

                ApplicationID = onboarding.ApplicationID,

                TechnicianID = technician.TechnicianID,

                FirstName = citizen.FirstName,
                LastName = citizen.LastName,
                PhoneNumber = citizen.PhoneNumber,
                PersonalEmail = citizen.EmailAddress,
                ResidentialAddress = citizen.ResidentialAddress,

                MunicipalEmail = technician.EmailAddress,
                TemporaryPassword = technician.Password,
                AccountStatus = technician.AccountStatus.ToString(),
                MustChangePassword = technician.MustChangePassword
            };

            return View(model);
        }







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