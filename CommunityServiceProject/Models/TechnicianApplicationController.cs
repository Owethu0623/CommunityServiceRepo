using System;
using System.Data.Entity;
using System.Linq;
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
        // US109 - APPLY
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

            var opportunity =
                db.TechnicianOpportunities
                    .AsNoTracking()
                    .FirstOrDefault(o =>
                        o.OpportunityID ==
                            opportunityID.Value &&
                        o.Status ==
                            TechnicianOpportunityStatus.Published);

            if (opportunity == null)
                return HttpNotFound();


            // --------------------------------------------------------
            // APPLICATION PERIOD
            // --------------------------------------------------------

            if (opportunity.ApplicationStartDate.Date >
                today)
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

            if (opportunity.ApplicationDeadline.Date <
                today)
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
            // DUPLICATE APPLICATION CHECK
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
                TempData["ErrorMessage"] =
                    "You have already submitted an application for this opportunity.";

                return RedirectToAction(
                    "Status",
                    new
                    {
                        id =
                            existingApplication.ApplicationID
                    });
            }


            // --------------------------------------------------------
            // CREATE MODEL
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
        // US109 - SUBMIT APPLICATION
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

            var citizenID =
                GetCitizenID();

            if (!citizenID.HasValue)
                return new HttpUnauthorizedResult();


            model.CoverLetter =
                model.CoverLetter?.Trim();


            // --------------------------------------------------------
            // LOAD OPPORTUNITY
            // --------------------------------------------------------

            var opportunity =
                db.TechnicianOpportunities
                    .FirstOrDefault(o =>
                        o.OpportunityID ==
                            model.OpportunityID);

            if (opportunity == null)
                return HttpNotFound();


            // --------------------------------------------------------
            // VALIDATE OPPORTUNITY STATUS
            // --------------------------------------------------------

            if (opportunity.Status !=
                TechnicianOpportunityStatus.Published)
            {
                ModelState.AddModelError(
                    "",
                    "This opportunity is no longer available for applications."
                );
            }


            // --------------------------------------------------------
            // VALIDATE APPLICATION PERIOD
            // --------------------------------------------------------

            var today = DateTime.Today;

            if (opportunity.ApplicationStartDate.Date >
                today)
            {
                ModelState.AddModelError(
                    "",
                    "Applications for this opportunity have not opened yet."
                );
            }

            if (opportunity.ApplicationDeadline.Date <
                today)
            {
                ModelState.AddModelError(
                    "",
                    "The application deadline for this opportunity has passed."
                );
            }


            // --------------------------------------------------------
            // DUPLICATE CHECK
            // --------------------------------------------------------

            var alreadyApplied =
                db.TechnicianApplications
                    .Any(a =>
                        a.OpportunityID ==
                            opportunity.OpportunityID &&
                        a.CitizenID ==
                            citizenID.Value);

            if (alreadyApplied)
            {
                ModelState.AddModelError(
                    "",
                    "You have already submitted an application for this opportunity."
                );
            }


            // --------------------------------------------------------
            // VALIDATE MODEL
            // --------------------------------------------------------

            if (!ModelState.IsValid)
            {
                model.OpportunityCode =
                    opportunity.OpportunityCode;

                model.OpportunityTitle =
                    opportunity.Title;

                model.EmploymentType =
                    opportunity.EmploymentType;

                model.ApplicationDeadline =
                    opportunity.ApplicationDeadline;

                model.NumberOfPositions =
                    opportunity.NumberOfPositions;

                return View(model);
            }


            // --------------------------------------------------------
            // CREATE APPLICATION
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
                        TechnicianApplicationStatus.Submitted,

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
                application.ApplicationID
                    .ToString("D6");

            db.SaveChanges();


            TempData["SuccessMessage"] =
                "Your technician application has been submitted successfully.";

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
                    .FirstOrDefault(a =>
                        a.ApplicationID == id.Value &&
                        a.CitizenID == citizenID.Value);

            if (application == null)
                return HttpNotFound();

            var isSuccessful =
                application.Status == TechnicianApplicationStatus.Selected ||
                application.Status == TechnicianApplicationStatus.ApprovedForOnboarding ||
                application.Status == TechnicianApplicationStatus.Onboarded;

            var isUnsuccessful =
                application.Status == TechnicianApplicationStatus.NotSelected;

            var model =
                new TechnicianApplicationDetailsViewModel
                {
                    ApplicationID = application.ApplicationID,

                    OpportunityID = application.OpportunityID,

                    ApplicationReference = application.ApplicationReference,

                    OpportunityCode = application.Opportunity.OpportunityCode,

                    OpportunityTitle = application.Opportunity.Title,

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
                        isUnsuccessful
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