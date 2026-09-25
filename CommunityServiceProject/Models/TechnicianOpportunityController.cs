using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using CommunityServiceProject.Models;
using CommunityServiceProject.ViewModels;

namespace CommunityServiceProject.Controllers
{
    public class TechnicianOpportunityController : Controller
    {
        private readonly Community db = new Community();

        // ============================================================
        // US105 - VIEW TECHNICIAN OPPORTUNITIES
        // ============================================================

        [HttpGet]
        public ActionResult Index(
            string searchTerm,
            string employmentType,
            string closingFilter)
        {
            var today = DateTime.Today;

            var query = db.TechnicianOpportunities
                .AsNoTracking()
                .Where(o =>
                    o.Status == TechnicianOpportunityStatus.Published &&
                    o.ApplicationStartDate <= today &&
                    o.ApplicationDeadline >= today);

            // --------------------------------------------------------
            // SEARCH
            // --------------------------------------------------------

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();

                query = query.Where(o =>
                    o.Title.Contains(searchTerm) ||
                    o.OpportunityCode.Contains(searchTerm) ||
                    o.Description.Contains(searchTerm) ||
                    o.Requirements.Contains(searchTerm) ||
                    o.RequiredQualifications.Contains(searchTerm));
            }

            // --------------------------------------------------------
            // EMPLOYMENT TYPE FILTER
            // --------------------------------------------------------

            if (!string.IsNullOrWhiteSpace(employmentType))
            {
                employmentType = employmentType.Trim();

                query = query.Where(o =>
                    o.EmploymentType == employmentType);
            }

            // --------------------------------------------------------
            // CLOSING FILTER
            // --------------------------------------------------------

            switch (closingFilter)
            {
                case "7":
                    query = query.Where(o =>
                        DbFunctions.DiffDays(
                            today,
                            o.ApplicationDeadline) <= 7);
                    break;

                case "14":
                    query = query.Where(o =>
                        DbFunctions.DiffDays(
                            today,
                            o.ApplicationDeadline) <= 14);
                    break;

                case "30":
                    query = query.Where(o =>
                        DbFunctions.DiffDays(
                            today,
                            o.ApplicationDeadline) <= 30);
                    break;
            }

            var opportunities = query
                .OrderBy(o => o.ApplicationDeadline)
                .ThenBy(o => o.Title)
                .ToList();

            var model =
                new TechnicianOpportunityListViewModel
                {
                    SearchTerm = searchTerm,
                    EmploymentType = employmentType,
                    ClosingFilter = closingFilter,
                    TotalOpportunities = opportunities.Count
                };

            foreach (var opportunity in opportunities)
            {
                var daysRemaining =
                    (opportunity.ApplicationDeadline.Date -
                     today).Days;

                model.Opportunities.Add(
                    new TechnicianOpportunityListItemViewModel
                    {
                        OpportunityID =
                            opportunity.OpportunityID,

                        OpportunityCode =
                            opportunity.OpportunityCode,

                        Title =
                            opportunity.Title,

                        EmploymentType =
                            string.IsNullOrWhiteSpace(
                                opportunity.EmploymentType)
                                ? "Not specified"
                                : opportunity.EmploymentType,

                        NumberOfPositions =
                            opportunity.NumberOfPositions,

                        ApplicationStartDate =
                            opportunity.ApplicationStartDate,

                        ApplicationDeadline =
                            opportunity.ApplicationDeadline,

                        Requirements =
                            opportunity.Requirements,

                        Status =
                            opportunity.Status,

                        IsOpenForApplications =
                            opportunity.Status ==
                            TechnicianOpportunityStatus.Published &&
                            opportunity.ApplicationStartDate.Date <= today &&
                            opportunity.ApplicationDeadline.Date >= today,

                        DaysRemaining =
                            Math.Max(0, daysRemaining)
                    });
            }

            PopulateEmploymentTypes(employmentType);

            return View(model);
        }

        // ============================================================
        // US106 - VIEW OPPORTUNITY DETAILS
        // ============================================================

        [HttpGet]
        public ActionResult Details(int? id)
        {
            if (!id.HasValue)
                return HttpNotFound();

            var today = DateTime.Today;

            var opportunity =
                db.TechnicianOpportunities
                    .AsNoTracking()
                    .FirstOrDefault(o =>
                        o.OpportunityID == id.Value &&
                        o.Status == TechnicianOpportunityStatus.Published);

            if (opportunity == null)
                return HttpNotFound();

            var daysRemaining =
                (opportunity.ApplicationDeadline.Date - today).Days;

            var isOpenForApplications =
                opportunity.ApplicationStartDate.Date <= today &&
                opportunity.ApplicationDeadline.Date >= today;

            var model =
                new TechnicianOpportunityDetailsViewModel
                {
                    OpportunityID =
                        opportunity.OpportunityID,

                    OpportunityCode =
                        opportunity.OpportunityCode,

                    Title =
                        opportunity.Title,

                    Description =
                        opportunity.Description,

                    Responsibilities =
                        opportunity.Responsibilities,

                    Requirements =
                        opportunity.Requirements,

                    RequiredQualifications =
                        opportunity.RequiredQualifications,

                    RequiredExperience =
                        opportunity.RequiredExperience,

                    ApplicationInstructions =
                        opportunity.ApplicationInstructions,

                    EmploymentType =
                        string.IsNullOrWhiteSpace(
                            opportunity.EmploymentType)
                            ? "Not specified"
                            : opportunity.EmploymentType,

                    NumberOfPositions =
                        opportunity.NumberOfPositions,

                    ApplicationStartDate =
                        opportunity.ApplicationStartDate,

                    ApplicationDeadline =
                        opportunity.ApplicationDeadline,

                    PublishedDate =
                        opportunity.PublishedDate,

                    Status =
                        opportunity.Status,

                    IsOpenForApplications =
                        isOpenForApplications,

                    DaysRemaining =
                        Math.Max(0, daysRemaining)
                };

            return View(model);
        }

        // ============================================================
        // EMPLOYMENT TYPE DROPDOWN
        // ============================================================

        private void PopulateEmploymentTypes(
            string selectedEmploymentType)
        {
            var employmentTypes =
                db.TechnicianOpportunities
                    .AsNoTracking()
                    .Where(o =>
                        o.Status ==
                        TechnicianOpportunityStatus.Published &&
                        o.EmploymentType != null &&
                        o.EmploymentType != "")
                    .Select(o => o.EmploymentType)
                    .Distinct()
                    .OrderBy(e => e)
                    .ToList();

            ViewBag.EmploymentTypes =
                new SelectList(
                    employmentTypes,
                    selectedEmploymentType);
        }

        // ============================================================
        // US107 - CREATE TECHNICIAN OPPORTUNITY
        // ============================================================

        [HttpGet]
        public ActionResult Create()
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            var model = new TechnicianOpportunityCreateViewModel
            {
                ApplicationStartDate = DateTime.Today,
                ApplicationDeadline = DateTime.Today.AddDays(30)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
    TechnicianOpportunityCreateViewModel model)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            if (model == null)
                return HttpNotFound();

            // --------------------------------------------------------
            // NORMALISE TEXT INPUT
            // --------------------------------------------------------

            model.Title =
                model.Title?.Trim();

            model.Description =
                model.Description?.Trim();

            model.Responsibilities =
                model.Responsibilities?.Trim();

            model.Requirements =
                model.Requirements?.Trim();

            model.RequiredQualifications =
                model.RequiredQualifications?.Trim();

            model.RequiredExperience =
                model.RequiredExperience?.Trim();

            model.ApplicationInstructions =
                model.ApplicationInstructions?.Trim();

            model.EmploymentType =
                model.EmploymentType?.Trim();

            // --------------------------------------------------------
            // ADMINISTRATOR
            // --------------------------------------------------------

            var administratorID =
                GetAdministratorID();

            if (!administratorID.HasValue)
                return new HttpUnauthorizedResult();

            // --------------------------------------------------------
            // DATE VALIDATION
            // --------------------------------------------------------

            var today = DateTime.Today;

            if (model.ApplicationStartDate.Date < today)
            {
                ModelState.AddModelError(
                    "ApplicationStartDate",
                    "The application start date cannot be in the past."
                );
            }

            if (model.ApplicationDeadline.Date <
                model.ApplicationStartDate.Date)
            {
                ModelState.AddModelError(
                    "ApplicationDeadline",
                    "The application deadline cannot be before the application start date."
                );
            }

            if (model.ApplicationDeadline.Date ==
                model.ApplicationStartDate.Date)
            {
                ModelState.AddModelError(
                    "ApplicationDeadline",
                    "The application deadline must be after the application start date."
                );
            }

            // --------------------------------------------------------
            // EMPLOYMENT TYPE
            // --------------------------------------------------------

            if (string.IsNullOrWhiteSpace(model.EmploymentType))
            {
                ModelState.AddModelError(
                    "EmploymentType",
                    "Please select an employment type."
                );
            }

            // --------------------------------------------------------
            // MODEL VALIDATION
            // --------------------------------------------------------

            if (!ModelState.IsValid)
                return View(model);

            // --------------------------------------------------------
            // CREATE OPPORTUNITY
            // --------------------------------------------------------

            var opportunity =
                new TechnicianOpportunity
                {
                    OpportunityCode = "TEMP",
                    Title = model.Title,
                    Description = model.Description,
                    Responsibilities = model.Responsibilities,
                    Requirements = model.Requirements,
                    RequiredQualifications =
                        model.RequiredQualifications,
                    RequiredExperience =
                        model.RequiredExperience,
                    ApplicationInstructions =
                        model.ApplicationInstructions,
                    EmploymentType =
                        model.EmploymentType,
                    NumberOfPositions =
                        model.NumberOfPositions,
                    ApplicationStartDate =
                        model.ApplicationStartDate.Date,
                    ApplicationDeadline =
                        model.ApplicationDeadline.Date,

                    // New opportunities begin as Draft.
                    Status =
                        TechnicianOpportunityStatus.Draft,

                    DateCreated =
                        DateTime.Now,

                    CreatedByAdministratorID =
                        administratorID.Value
                };

            db.TechnicianOpportunities.Add(opportunity);

            db.SaveChanges();

            // --------------------------------------------------------
            // GENERATE OPPORTUNITY CODE
            // --------------------------------------------------------

            opportunity.OpportunityCode =
                "TECH-" +
                DateTime.Now.Year +
                "-" +
                opportunity.OpportunityID.ToString("D5");

            db.SaveChanges();

            TempData["SuccessMessage"] =
                "Technician opportunity has been created successfully as a draft.";

            return RedirectToAction("Manage");
        }

        private bool IsAdministrator()
        {
            return Session["AdministratorID"] != null;
        }

        private int? GetAdministratorID()
        {
            if (Session["AdministratorID"] == null)
                return null;

            int administratorID;

            if (int.TryParse(
                Session["AdministratorID"].ToString(),
                out administratorID))
            {
                return administratorID;
            }

            return null;
        }

        // ============================================================
        // US108 - MANAGE TECHNICIAN OPPORTUNITIES
        // ============================================================

        [HttpGet]
        public ActionResult Manage(
            string searchTerm,
            string statusFilter)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            var query =
                db.TechnicianOpportunities
                    .AsNoTracking()
                    .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();

                query = query.Where(o =>
                    o.Title.Contains(searchTerm) ||
                    o.OpportunityCode.Contains(searchTerm));
            }

            TechnicianOpportunityStatus selectedStatus;

            if (!string.IsNullOrWhiteSpace(statusFilter) &&
                Enum.TryParse(
                    statusFilter,
                    true,
                    out selectedStatus))
            {
                query = query.Where(
                    o => o.Status == selectedStatus
                );
            }

            var opportunities =
                query
                    .OrderByDescending(o => o.DateCreated)
                    .ThenBy(o => o.Title)
                    .ToList();

            var today = DateTime.Today;

            var model =
                new TechnicianOpportunityManagementViewModel
                {
                    SearchTerm = searchTerm,
                    StatusFilter = statusFilter,
                    TotalOpportunities =
                        opportunities.Count,
                    DraftCount =
                        opportunities.Count(
                            o => o.Status ==
                                TechnicianOpportunityStatus.Draft),
                    PublishedCount =
                        opportunities.Count(
                            o => o.Status ==
                                TechnicianOpportunityStatus.Published),
                    ClosedCount =
                        opportunities.Count(
                            o => o.Status ==
                                TechnicianOpportunityStatus.Closed),
                    CancelledCount =
                        opportunities.Count(
                            o => o.Status ==
                                TechnicianOpportunityStatus.Cancelled)
                };

            foreach (var opportunity in opportunities)
            {
                var daysUntilDeadline =
                    (opportunity.ApplicationDeadline.Date -
                     today).Days;

                model.Opportunities.Add(
                    new TechnicianOpportunityManagementItemViewModel
                    {
                        OpportunityID =
                            opportunity.OpportunityID,

                        OpportunityCode =
                            opportunity.OpportunityCode,

                        Title =
                            opportunity.Title,

                        EmploymentType =
                            string.IsNullOrWhiteSpace(
                                opportunity.EmploymentType)
                                ? "Not specified"
                                : opportunity.EmploymentType,

                        NumberOfPositions =
                            opportunity.NumberOfPositions,

                        ApplicationStartDate =
                            opportunity.ApplicationStartDate,

                        ApplicationDeadline =
                            opportunity.ApplicationDeadline,

                        Status =
                            opportunity.Status,

                        CanEdit =
                            opportunity.Status !=
                            TechnicianOpportunityStatus.Closed &&
                            opportunity.Status !=
                            TechnicianOpportunityStatus.Cancelled,

                        CanPublish =
                            opportunity.Status ==
                            TechnicianOpportunityStatus.Draft,

                        CanClose =
                            opportunity.Status ==
                            TechnicianOpportunityStatus.Published,

                        CanCancel =
                            opportunity.Status ==
                            TechnicianOpportunityStatus.Draft ||
                            opportunity.Status ==
                            TechnicianOpportunityStatus.Published,

                        DaysUntilDeadline =
                            opportunity.Status ==
                            TechnicianOpportunityStatus.Published
                                ? (int?)daysUntilDeadline
                                : null
                    }
                );
            }

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
     int id,
     TechnicianOpportunityCreateViewModel model)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            if (model == null)
                return HttpNotFound();

            var administratorID =
                GetAdministratorID();

            if (!administratorID.HasValue)
                return new HttpUnauthorizedResult();

            var opportunity =
                db.TechnicianOpportunities
                    .FirstOrDefault(
                        o => o.OpportunityID == id
                    );

            if (opportunity == null)
                return HttpNotFound();

            if (opportunity.Status ==
                TechnicianOpportunityStatus.Closed ||
                opportunity.Status ==
                TechnicianOpportunityStatus.Cancelled)
            {
                TempData["ErrorMessage"] =
                    "Closed or cancelled opportunities cannot be edited.";

                return RedirectToAction("Manage");
            }

            model.Title = model.Title?.Trim();
            model.Description = model.Description?.Trim();
            model.Responsibilities = model.Responsibilities?.Trim();
            model.Requirements = model.Requirements?.Trim();
            model.RequiredQualifications =
                model.RequiredQualifications?.Trim();
            model.RequiredExperience =
                model.RequiredExperience?.Trim();
            model.ApplicationInstructions =
                model.ApplicationInstructions?.Trim();
            model.EmploymentType =
                model.EmploymentType?.Trim();

            var today = DateTime.Today;

            if (opportunity.Status ==
                TechnicianOpportunityStatus.Draft &&
                model.ApplicationStartDate.Date < today)
            {
                ModelState.AddModelError(
                    "ApplicationStartDate",
                    "The application start date cannot be in the past."
                );
            }

            if (model.ApplicationDeadline.Date <=
                model.ApplicationStartDate.Date)
            {
                ModelState.AddModelError(
                    "ApplicationDeadline",
                    "The application deadline must be after the application start date."
                );
            }

            if (!ModelState.IsValid)
            {
                ViewBag.OpportunityID = id;
                ViewBag.OpportunityCode =
                    opportunity.OpportunityCode;
                ViewBag.CurrentStatus =
                    opportunity.Status.ToString();

                return View(model);
            }

            opportunity.Title = model.Title;
            opportunity.Description = model.Description;
            opportunity.Responsibilities =
                model.Responsibilities;
            opportunity.Requirements =
                model.Requirements;
            opportunity.RequiredQualifications =
                model.RequiredQualifications;
            opportunity.RequiredExperience =
                model.RequiredExperience;
            opportunity.ApplicationInstructions =
                model.ApplicationInstructions;
            opportunity.EmploymentType =
                model.EmploymentType;
            opportunity.NumberOfPositions =
                model.NumberOfPositions;
            opportunity.ApplicationStartDate =
                model.ApplicationStartDate.Date;
            opportunity.ApplicationDeadline =
                model.ApplicationDeadline.Date;

            opportunity.LastUpdatedDate =
                DateTime.Now;

            opportunity.LastUpdatedByAdministratorID =
                administratorID.Value;

            db.SaveChanges();

            TempData["SuccessMessage"] =
                "Technician opportunity has been updated successfully.";

            return RedirectToAction(
                "Manage"
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPost(
    int id,
    TechnicianOpportunityCreateViewModel model)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            if (model == null)
                return HttpNotFound();

            var administratorID =
                GetAdministratorID();

            if (!administratorID.HasValue)
                return new HttpUnauthorizedResult();

            var opportunity =
                db.TechnicianOpportunities
                    .FirstOrDefault(
                        o => o.OpportunityID == id
                    );

            if (opportunity == null)
                return HttpNotFound();

            if (opportunity.Status ==
                TechnicianOpportunityStatus.Closed ||
                opportunity.Status ==
                TechnicianOpportunityStatus.Cancelled)
            {
                TempData["ErrorMessage"] =
                    "Closed or cancelled opportunities cannot be edited.";

                return RedirectToAction("Manage");
            }

            model.Title = model.Title?.Trim();
            model.Description = model.Description?.Trim();
            model.Responsibilities = model.Responsibilities?.Trim();
            model.Requirements = model.Requirements?.Trim();
            model.RequiredQualifications =
                model.RequiredQualifications?.Trim();
            model.RequiredExperience =
                model.RequiredExperience?.Trim();
            model.ApplicationInstructions =
                model.ApplicationInstructions?.Trim();
            model.EmploymentType =
                model.EmploymentType?.Trim();

            var today = DateTime.Today;

            if (opportunity.Status ==
                TechnicianOpportunityStatus.Draft &&
                model.ApplicationStartDate.Date < today)
            {
                ModelState.AddModelError(
                    "ApplicationStartDate",
                    "The application start date cannot be in the past."
                );
            }

            if (model.ApplicationDeadline.Date <=
                model.ApplicationStartDate.Date)
            {
                ModelState.AddModelError(
                    "ApplicationDeadline",
                    "The application deadline must be after the application start date."
                );
            }

            if (!ModelState.IsValid)
            {
                ViewBag.OpportunityID = id;
                ViewBag.OpportunityCode =
                    opportunity.OpportunityCode;
                ViewBag.CurrentStatus =
                    opportunity.Status.ToString();

                return View(model);
            }

            opportunity.Title = model.Title;
            opportunity.Description = model.Description;
            opportunity.Responsibilities =
                model.Responsibilities;
            opportunity.Requirements =
                model.Requirements;
            opportunity.RequiredQualifications =
                model.RequiredQualifications;
            opportunity.RequiredExperience =
                model.RequiredExperience;
            opportunity.ApplicationInstructions =
                model.ApplicationInstructions;
            opportunity.EmploymentType =
                model.EmploymentType;
            opportunity.NumberOfPositions =
                model.NumberOfPositions;
            opportunity.ApplicationStartDate =
                model.ApplicationStartDate.Date;
            opportunity.ApplicationDeadline =
                model.ApplicationDeadline.Date;

            opportunity.LastUpdatedDate =
                DateTime.Now;

            opportunity.LastUpdatedByAdministratorID =
                administratorID.Value;

            db.SaveChanges();

            TempData["SuccessMessage"] =
                "Technician opportunity has been updated successfully.";

            return RedirectToAction(
                "Manage"
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Publish(int id)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            var administratorID =
                GetAdministratorID();

            if (!administratorID.HasValue)
                return new HttpUnauthorizedResult();

            var opportunity =
                db.TechnicianOpportunities
                    .FirstOrDefault(
                        o => o.OpportunityID == id
                    );

            if (opportunity == null)
                return HttpNotFound();

            if (opportunity.Status !=
                TechnicianOpportunityStatus.Draft)
            {
                TempData["ErrorMessage"] =
                    "Only draft opportunities can be published.";

                return RedirectToAction("Manage");
            }

            var today = DateTime.Today;

            if (opportunity.ApplicationStartDate.Date < today)
            {
                TempData["ErrorMessage"] =
                    "The opportunity cannot be published because its application start date is in the past.";

                return RedirectToAction("Manage");
            }

            if (opportunity.ApplicationDeadline.Date <=
                opportunity.ApplicationStartDate.Date)
            {
                TempData["ErrorMessage"] =
                    "The opportunity cannot be published because the application deadline is invalid.";

                return RedirectToAction("Manage");
            }

            opportunity.Status =
                TechnicianOpportunityStatus.Published;

            opportunity.PublishedDate =
                DateTime.Now;

            opportunity.LastUpdatedDate =
                DateTime.Now;

            opportunity.LastUpdatedByAdministratorID =
                administratorID.Value;

            db.SaveChanges();

            TempData["SuccessMessage"] =
                "Technician opportunity has been published successfully.";

            return RedirectToAction("Manage");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Close(int id)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            var administratorID =
                GetAdministratorID();

            if (!administratorID.HasValue)
                return new HttpUnauthorizedResult();

            var opportunity =
                db.TechnicianOpportunities
                    .FirstOrDefault(
                        o => o.OpportunityID == id
                    );

            if (opportunity == null)
                return HttpNotFound();

            if (opportunity.Status !=
                TechnicianOpportunityStatus.Published)
            {
                TempData["ErrorMessage"] =
                    "Only published opportunities can be closed.";

                return RedirectToAction("Manage");
            }

            opportunity.Status =
                TechnicianOpportunityStatus.Closed;

            opportunity.ClosedDate =
                DateTime.Now;

            opportunity.LastUpdatedDate =
                DateTime.Now;

            opportunity.LastUpdatedByAdministratorID =
                administratorID.Value;

            db.SaveChanges();

            TempData["SuccessMessage"] =
                "Technician opportunity has been closed successfully.";

            return RedirectToAction("Manage");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CancelOpportunity(int id)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            var administratorID =
                GetAdministratorID();

            if (!administratorID.HasValue)
                return new HttpUnauthorizedResult();

            var opportunity =
                db.TechnicianOpportunities
                    .FirstOrDefault(
                        o => o.OpportunityID == id
                    );

            if (opportunity == null)
                return HttpNotFound();

            if (opportunity.Status !=
                    TechnicianOpportunityStatus.Draft &&
                opportunity.Status !=
                    TechnicianOpportunityStatus.Published)
            {
                TempData["ErrorMessage"] =
                    "This opportunity cannot be cancelled in its current status.";

                return RedirectToAction("Manage");
            }

            opportunity.Status =
                TechnicianOpportunityStatus.Cancelled;

            opportunity.ClosedDate =
                DateTime.Now;

            opportunity.LastUpdatedDate =
                DateTime.Now;

            opportunity.LastUpdatedByAdministratorID =
                administratorID.Value;

            db.SaveChanges();

            TempData["SuccessMessage"] =
                "Technician opportunity has been cancelled successfully.";

            return RedirectToAction("Manage");
        }



        // ============================================================
        // DISPOSE
        // ============================================================

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