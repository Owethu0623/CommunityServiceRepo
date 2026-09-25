using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using CommunityServiceProject.Models;
using CommunityServiceProject.ViewModels;

namespace CommunityServiceProject.Controllers
{
    public class MunicipalProjectController : Controller
    {
        private readonly Community db = new Community();


        // =========================================================
        // ADMINISTRATOR AUTHENTICATION
        // =========================================================

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


        // =========================================================
        // US88 — REGISTER MUNICIPAL PROJECT
        // =========================================================

        [HttpGet]
        public ActionResult Create()
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            var model =
                new MunicipalProjectCreateViewModel
                {
                    StartDate = DateTime.Today,
                    Priority = MunicipalProjectPriority.Medium
                };

            PopulateCreateOptions(model);

            return View(model);
        }


        // =========================================================
        // CREATE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            MunicipalProjectCreateViewModel model)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();


            // =========================================================
            // AUTHENTICATION
            // =========================================================

            var administratorID =
                GetAdministratorID();

            if (!administratorID.HasValue)
                return new HttpUnauthorizedResult();


            // =========================================================
            // READ COORDINATES DIRECTLY FROM FORM
            // =========================================================
            /*
             * Latitude and Longitude are strings in the ViewModel.
             *
             * The hidden fields post:
             *
             * name="Latitude"
             * name="Longitude"
             *
             * Read the values directly from Request.Form and
             * convert them manually after validation.
             */

            var postedLatitude =
                Request.Form["Latitude"];

            var postedLongitude =
                Request.Form["Longitude"];


            model.Latitude =
                string.IsNullOrWhiteSpace(postedLatitude)
                    ? null
                    : postedLatitude.Trim();

            model.Longitude =
                string.IsNullOrWhiteSpace(postedLongitude)
                    ? null
                    : postedLongitude.Trim();


            // =========================================================
            // CLEAN INPUT
            // =========================================================

            model.ProjectName =
                model.ProjectName?.Trim();

            model.ProjectType =
                model.ProjectType?.Trim();

            model.Description =
                model.Description?.Trim();

            model.ProjectLocation =
                model.ProjectLocation?.Trim();

            model.LocationDescription =
                model.LocationDescription?.Trim();


            // =========================================================
            // LOCATION VALIDATION
            // =========================================================

            double selectedLatitude = 0;

            double selectedLongitude = 0;

            bool latitudeIsValid =
                double.TryParse(
                    model.Latitude,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out selectedLatitude
                );

            bool longitudeIsValid =
                double.TryParse(
                    model.Longitude,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out selectedLongitude
                );


            if (!latitudeIsValid ||
                !longitudeIsValid)
            {
                ModelState.AddModelError(
                    "",
                    "Please select the project location on the map."
                );
            }
            else
            {
                // -----------------------------------------------------
                // LATITUDE
                // -----------------------------------------------------

                if (selectedLatitude < -90 ||
                    selectedLatitude > 90)
                {
                    ModelState.AddModelError(
                        "Latitude",
                        "The selected latitude is invalid."
                    );
                }


                // -----------------------------------------------------
                // LONGITUDE
                // -----------------------------------------------------

                if (selectedLongitude < -180 ||
                    selectedLongitude > 180)
                {
                    ModelState.AddModelError(
                        "Longitude",
                        "The selected longitude is invalid."
                    );
                }


                // -----------------------------------------------------
                // DEFAULT 0,0 CHECK
                // -----------------------------------------------------

                if (Math.Abs(selectedLatitude) < 0.000001 &&
                    Math.Abs(selectedLongitude) < 0.000001)
                {
                    ModelState.AddModelError(
                        "",
                        "Please select a valid municipal project location rather than the default map position."
                    );
                }
            }


            // =========================================================
            // DATE VALIDATION
            // =========================================================

            if (model.StartDate.HasValue &&
                model.ExpectedCompletionDate.HasValue &&
                model.ExpectedCompletionDate.Value.Date <
                model.StartDate.Value.Date)
            {
                ModelState.AddModelError(
                    "ExpectedCompletionDate",
                    "Expected completion date cannot be earlier than the project start date."
                );
            }


            // =========================================================
            // VALIDATE WARD
            // =========================================================

            Ward ward = null;

            if (model.WardID.HasValue)
            {
                ward =
                    db.Wards
                        .FirstOrDefault(w =>
                            w.WardID ==
                            model.WardID.Value &&
                            w.IsActive);

                if (ward == null)
                {
                    ModelState.AddModelError(
                        "WardID",
                        "Please select a valid active municipal ward."
                    );
                }
            }
            else
            {
                ModelState.AddModelError(
                    "WardID",
                    "Please select a municipal ward."
                );
            }


            // =========================================================
            // VALIDATE RESPONSIBLE ADMINISTRATOR
            // =========================================================

            Administrator responsibleAdministrator = null;

            if (model.ResponsibleAdministratorID.HasValue)
            {
                responsibleAdministrator =
                    db.Administrators
                        .FirstOrDefault(a =>
                            a.AdministratorID ==
                            model.ResponsibleAdministratorID.Value &&
                            a.AccountStatus ==
                            AccountStatus.Active);

                if (responsibleAdministrator == null)
                {
                    ModelState.AddModelError(
                        "ResponsibleAdministratorID",
                        "Please select a valid active administrator."
                    );
                }
            }
            else
            {
                ModelState.AddModelError(
                    "ResponsibleAdministratorID",
                    "Please select a responsible administrator."
                );
            }


            // =========================================================
            // VALIDATE PROJECT TYPE
            // =========================================================

            var projectTypes =
                GetProjectTypes();

            if (string.IsNullOrWhiteSpace(model.ProjectType) ||
                !projectTypes.Contains(model.ProjectType))
            {
                ModelState.AddModelError(
                    "ProjectType",
                    "Please select a valid project type."
                );
            }


            // =========================================================
            // VALIDATE PRIORITY
            // =========================================================

            if (!model.Priority.HasValue ||
                !Enum.IsDefined(
                    typeof(MunicipalProjectPriority),
                    model.Priority.Value))
            {
                ModelState.AddModelError(
                    "Priority",
                    "Please select a valid project priority."
                );
            }


            // =========================================================
            // VALIDATE REQUIRED PROJECT INFORMATION
            // =========================================================

            if (string.IsNullOrWhiteSpace(model.ProjectName))
            {
                ModelState.AddModelError(
                    "ProjectName",
                    "Please enter the project name."
                );
            }

            if (string.IsNullOrWhiteSpace(model.ProjectLocation))
            {
                ModelState.AddModelError(
                    "ProjectLocation",
                    "Please provide the project location."
                );
            }

            if (!model.StartDate.HasValue)
            {
                ModelState.AddModelError(
                    "StartDate",
                    "Please select the project start date."
                );
            }


            // =========================================================
            // RETURN WITH VALIDATION ERRORS
            // =========================================================

            if (!ModelState.IsValid)
            {
                PopulateCreateOptions(model);

                return View(model);
            }


            // =========================================================
            // CREATE PROJECT
            // =========================================================

            try
            {
                var project =
                    new MunicipalProject
                    {
                        // -------------------------------------------------
                        // PROJECT INFORMATION
                        // -------------------------------------------------

                        ProjectCode =
                            "TEMP",

                        ProjectName =
                            model.ProjectName,

                        ProjectType =
                            model.ProjectType,

                        Description =
                            model.Description,


                        // -------------------------------------------------
                        // LOCATION
                        // -------------------------------------------------

                        ProjectLocation =
                            model.ProjectLocation,

                        Latitude =
                            selectedLatitude,

                        Longitude =
                            selectedLongitude,

                        LocationDescription =
                            string.IsNullOrWhiteSpace(
                                model.LocationDescription)
                                ? model.ProjectLocation
                                : model.LocationDescription,


                        // -------------------------------------------------
                        // WARD
                        // -------------------------------------------------

                        WardID =
                            model.WardID.Value,


                        // -------------------------------------------------
                        // SCHEDULE
                        // -------------------------------------------------

                        StartDate =
                            model.StartDate.Value,

                        ExpectedCompletionDate =
                            model.ExpectedCompletionDate,


                        // -------------------------------------------------
                        // STATUS / PRIORITY
                        // -------------------------------------------------

                        Status =
                            MunicipalProjectStatus.Planned,

                        Priority =
                            model.Priority.Value,


                        // -------------------------------------------------
                        // FINANCIAL
                        // -------------------------------------------------

                        EstimatedBudget =
                            model.EstimatedBudget,


                        // -------------------------------------------------
                        // RESPONSIBILITY
                        // -------------------------------------------------

                        ResponsibleAdministratorID =
                            model.ResponsibleAdministratorID.Value,


                        // -------------------------------------------------
                        // SYSTEM REGISTRATION
                        // -------------------------------------------------

                        DateRegistered =
                            DateTime.Now,

                        CreatedByAdministratorID =
                            administratorID.Value,

                        LastUpdatedDate =
                            null,

                        LastUpdatedByAdministratorID =
                            null
                    };


                db.MunicipalProjects.Add(project);

                db.SaveChanges();


                // =========================================================
                // GENERATE PERMANENT PROJECT CODE
                // =========================================================

                project.ProjectCode =
                    "PRJ-" +
                    project.ProjectID.ToString("D6");

                db.SaveChanges();


                // =========================================================
                // SUCCESS
                // =========================================================

                TempData["SuccessMessage"] =
                    "Municipal project " +
                    project.ProjectCode +
                    " was successfully registered.";


                return RedirectToAction(
                    "Details",
                    new
                    {
                        id = project.ProjectID
                    }
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "MunicipalProject Create Error:"
                );

                System.Diagnostics.Debug.WriteLine(
                    ex.ToString()
                );

                ModelState.AddModelError(
                    "",
                    "The municipal project could not be registered. Please try again."
                );

                PopulateCreateOptions(model);

                return View(model);
            }
        }


        // =========================================================
        // DROPDOWN OPTIONS
        // =========================================================

        private void PopulateCreateOptions(
            MunicipalProjectCreateViewModel model)
        {
            // =========================================================
            // PROJECT TYPES
            // =========================================================

            model.ProjectTypeOptions =
                GetProjectTypes()
                    .Select(type => new SelectListItem
                    {
                        Text =
                            type,

                        Value =
                            type,

                        Selected =
                            type == model.ProjectType
                    })
                    .ToList();


            // =========================================================
            // WARDS
            // =========================================================

            var wards =
                db.Wards
                    .AsNoTracking()
                    .Where(w => w.IsActive)
                    .ToList()
                    .OrderBy(w =>
                    {
                        int number;

                        return int.TryParse(
                            w.WardNumber,
                            out number)
                            ? number
                            : int.MaxValue;
                    })
                    .ToList();


            model.WardOptions =
                wards
                    .Select(w => new SelectListItem
                    {
                        Text =
                            "Ward " +
                            w.WardNumber +
                            " – " +
                            w.WardName,

                        Value =
                            w.WardID.ToString(),

                        Selected =
                            model.WardID.HasValue &&
                            w.WardID ==
                            model.WardID.Value
                    })
                    .ToList();


            // =========================================================
            // PRIORITY
            // =========================================================

            model.PriorityOptions =
                Enum.GetValues(
                    typeof(MunicipalProjectPriority))
                .Cast<MunicipalProjectPriority>()
                .Select(p => new SelectListItem
                {
                    Text =
                        p.ToString(),

                    Value =
                        p.ToString(),

                    Selected =
                        model.Priority.HasValue &&
                        model.Priority.Value == p
                })
                .ToList();


            // =========================================================
            // ADMINISTRATORS
            // =========================================================

            model.AdministratorOptions =
                db.Administrators
                    .AsNoTracking()
                    .Where(a =>
                        a.AccountStatus ==
                        AccountStatus.Active)
                    .OrderBy(a => a.FirstName)
                    .ThenBy(a => a.LastName)
                    .Select(a => new SelectListItem
                    {
                        Text =
                            a.FirstName +
                            " " +
                            a.LastName +
                            " — " +
                            a.EmailAddress,

                        Value =
                            a.AdministratorID.ToString(),

                        Selected =
                            model.ResponsibleAdministratorID.HasValue &&
                            a.AdministratorID ==
                            model.ResponsibleAdministratorID.Value
                    })
                    .ToList();
        }


        // =========================================================
        // PROJECT TYPES
        // =========================================================

        private List<string> GetProjectTypes()
        {
            return new List<string>
            {
                "Road Infrastructure",
                "Stormwater Infrastructure",
                "Bridge Infrastructure",
                "Public Building",
                "Community Facility",
                "Sports Facility",
                "Public Park",
                "Transport Infrastructure",
                "Water Infrastructure",
                "Wastewater Infrastructure",
                "Environmental Project",
                "Urban Development",
                "Rehabilitation",
                "Upgrade",
                "Other"
            };
        }


        // =========================================================
        // US89 — VIEW MUNICIPAL PROJECTS
        // =========================================================

        [HttpGet]
        public ActionResult Index(
            string searchTerm,
            string status,
            string priority,
            string ward,
            string projectType)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();


            var projects =
                db.MunicipalProjects
                    .AsNoTracking()
                    .Include(p => p.Ward)
                    .Include(p => p.ResponsibleAdministrator)
                    .OrderByDescending(p => p.DateRegistered)
                    .ToList();


            searchTerm =
                searchTerm?.Trim();

            status =
                status?.Trim();

            priority =
                priority?.Trim();

            ward =
                ward?.Trim();

            projectType =
                projectType?.Trim();


            // =========================================================
            // FILTER OPTIONS
            // =========================================================

            var statusOptions =
                projects
                    .Select(p =>
                        p.Status.ToString())
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();


            var priorityOptions =
                projects
                    .Select(p =>
                        p.Priority.ToString())
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();


            var projectTypeOptions =
                projects
                    .Select(p =>
                        p.ProjectType)
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();


            var wardOptions =
                projects
                    .Where(p =>
                        p.Ward != null)
                    .Select(p =>
                        "Ward " +
                        p.Ward.WardNumber +
                        " – " +
                        p.Ward.WardName)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();


            // =========================================================
            // SEARCH
            // =========================================================

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term =
                    searchTerm.ToLower();

                projects =
                    projects
                        .Where(p =>
                            (p.ProjectCode ?? "")
                                .ToLower()
                                .Contains(term)
                            ||
                            (p.ProjectName ?? "")
                                .ToLower()
                                .Contains(term)
                            ||
                            (p.ProjectType ?? "")
                                .ToLower()
                                .Contains(term)
                            ||
                            (p.Description ?? "")
                                .ToLower()
                                .Contains(term)
                            ||
                            (p.ProjectLocation ?? "")
                                .ToLower()
                                .Contains(term)
                            ||
                            (
                                p.Ward != null &&
                                (
                                    "Ward " +
                                    p.Ward.WardNumber +
                                    " – " +
                                    p.Ward.WardName
                                )
                                .ToLower()
                                .Contains(term)
                            )
                            ||
                            (
                                p.ResponsibleAdministrator != null &&
                                (
                                    (p.ResponsibleAdministrator.FirstName ?? "") +
                                    " " +
                                    (p.ResponsibleAdministrator.LastName ?? "")
                                )
                                .ToLower()
                                .Contains(term)
                            )
                        )
                        .ToList();
            }


            // =========================================================
            // STATUS
            // =========================================================

            if (!string.IsNullOrWhiteSpace(status))
            {
                projects =
                    projects
                        .Where(p =>
                            p.Status.ToString() ==
                            status)
                        .ToList();
            }


            // =========================================================
            // PRIORITY
            // =========================================================

            if (!string.IsNullOrWhiteSpace(priority))
            {
                projects =
                    projects
                        .Where(p =>
                            p.Priority.ToString() ==
                            priority)
                        .ToList();
            }


            // =========================================================
            // WARD
            // =========================================================

            if (!string.IsNullOrWhiteSpace(ward))
            {
                projects =
                    projects
                        .Where(p =>
                            p.Ward != null &&
                            (
                                "Ward " +
                                p.Ward.WardNumber +
                                " – " +
                                p.Ward.WardName
                            ) == ward)
                        .ToList();
            }


            // =========================================================
            // PROJECT TYPE
            // =========================================================

            if (!string.IsNullOrWhiteSpace(projectType))
            {
                projects =
                    projects
                        .Where(p =>
                            p.ProjectType ==
                            projectType)
                        .ToList();
            }


            // =========================================================
            // VIEW MODEL
            // =========================================================

            var model =
                new MunicipalProjectIndexViewModel
                {
                    SearchTerm =
                        searchTerm,

                    SelectedStatus =
                        status,

                    SelectedPriority =
                        priority,

                    SelectedWard =
                        ward,

                    SelectedProjectType =
                        projectType,

                    StatusOptions =
                        statusOptions,

                    PriorityOptions =
                        priorityOptions,

                    WardOptions =
                        wardOptions,

                    ProjectTypeOptions =
                        projectTypeOptions,

                    TotalProjects =
                        projects.Count,

                    PlannedProjects =
                        projects.Count(p =>
                            p.Status ==
                            MunicipalProjectStatus.Planned),

                    ApprovedProjects =
                        projects.Count(p =>
                            p.Status ==
                            MunicipalProjectStatus.Approved),

                    InProgressProjects =
                        projects.Count(p =>
                            p.Status ==
                            MunicipalProjectStatus.InProgress),

                    OnHoldProjects =
                        projects.Count(p =>
                            p.Status ==
                            MunicipalProjectStatus.OnHold),

                    CompletedProjects =
                        projects.Count(p =>
                            p.Status ==
                            MunicipalProjectStatus.Completed),

                    CancelledProjects =
                        projects.Count(p =>
                            p.Status ==
                            MunicipalProjectStatus.Cancelled)
                };


            // =========================================================
            // PROJECT RECORDS
            // =========================================================

            model.Projects =
                projects
                    .Select(p =>
                        new MunicipalProjectIndexItemViewModel
                        {
                            ProjectID =
                                p.ProjectID,

                            ProjectCode =
                                p.ProjectCode,

                            ProjectName =
                                p.ProjectName,

                            ProjectType =
                                p.ProjectType,

                            WardName =
                                p.Ward != null
                                    ? "Ward " +
                                      p.Ward.WardNumber +
                                      " – " +
                                      p.Ward.WardName
                                    : "Ward not specified",

                            Status =
                                p.Status.ToString(),

                            Priority =
                                p.Priority.ToString(),

                            EstimatedBudget =
                                p.EstimatedBudget,

                            StartDate =
                                p.StartDate,

                            ExpectedCompletionDate =
                                p.ExpectedCompletionDate,

                            DateRegistered =
                                p.DateRegistered,

                            ResponsibleAdministratorName =
                                p.ResponsibleAdministrator != null
                                    ? p.ResponsibleAdministrator.FirstName +
                                      " " +
                                      p.ResponsibleAdministrator.LastName
                                    : "Administrator unavailable"
                        })
                    .ToList();


            return View(model);
        }


        // =========================================================
        // ADDRESS SEARCH
        // =========================================================

        [HttpGet]
        public async Task<ActionResult> SearchAddress(
            string query)
        {
            if (!IsAdministrator())
            {
                return Json(
                    new
                    {
                        success = false,
                        message =
                            "You are not authorised to perform this action."
                    },
                    JsonRequestBehavior.AllowGet
                );
            }


            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(
                    new
                    {
                        success = false,
                        message =
                            "Please enter an address or location."
                    },
                    JsonRequestBehavior.AllowGet
                );
            }


            try
            {
                using (var client =
                    new HttpClient())
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(
                        "CommunityServiceProject/1.0"
                    );


                    var encodedQuery =
                        Uri.EscapeDataString(
                            query.Trim());


                    var url =
                        "https://nominatim.openstreetmap.org/search" +
                        "?q=" +
                        encodedQuery +
                        "&format=json" +
                        "&addressdetails=1" +
                        "&limit=5";


                    var response =
                        await client.GetAsync(url);


                    if (!response.IsSuccessStatusCode)
                    {
                        return Json(
                            new
                            {
                                success = false,
                                message =
                                    "The location search service is currently unavailable."
                            },
                            JsonRequestBehavior.AllowGet
                        );
                    }


                    var json =
                        await response.Content
                            .ReadAsStringAsync();


                    return Content(
                        json,
                        "application/json"
                    );
                }
            }
            catch
            {
                return Json(
                    new
                    {
                        success = false,
                        message =
                            "Unable to search for the requested location."
                    },
                    JsonRequestBehavior.AllowGet
                );
            }
        }


        // =========================================================
        // REVERSE GEOCODING
        // =========================================================

        [HttpGet]
        public async Task<ActionResult> ReverseGeocode(
            double lat,
            double lon)
        {
            if (!IsAdministrator())
            {
                return Json(
                    new
                    {
                        success = false,
                        message =
                            "You are not authorised to perform this action."
                    },
                    JsonRequestBehavior.AllowGet
                );
            }


            if (lat < -90 ||
                lat > 90 ||
                lon < -180 ||
                lon > 180)
            {
                return Json(
                    new
                    {
                        success = false,
                        message =
                            "Invalid coordinates."
                    },
                    JsonRequestBehavior.AllowGet
                );
            }


            try
            {
                using (var client =
                    new HttpClient())
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(
                        "CommunityServiceProject/1.0"
                    );


                    var url =
                        "https://nominatim.openstreetmap.org/reverse" +
                        "?lat=" +
                        lat.ToString(
                            CultureInfo.InvariantCulture
                        ) +
                        "&lon=" +
                        lon.ToString(
                            CultureInfo.InvariantCulture
                        ) +
                        "&format=json" +
                        "&addressdetails=1";


                    var response =
                        await client.GetAsync(url);


                    if (!response.IsSuccessStatusCode)
                    {
                        return Json(
                            new
                            {
                                success = false,
                                message =
                                    "The location identification service is currently unavailable."
                            },
                            JsonRequestBehavior.AllowGet
                        );
                    }


                    var json =
                        await response.Content
                            .ReadAsStringAsync();


                    return Content(
                        json,
                        "application/json"
                    );
                }
            }
            catch
            {
                return Json(
                    new
                    {
                        success = false,
                        message =
                            "Unable to identify the selected location."
                    },
                    JsonRequestBehavior.AllowGet
                );
            }
        }


        // =========================================================
        // US90 — VIEW MUNICIPAL PROJECT DETAILS
        // =========================================================

        [HttpGet]
        public ActionResult Details(
            int? id)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();


            if (!id.HasValue)
                return HttpNotFound();


            var project =
                db.MunicipalProjects
                    .AsNoTracking()
                    .Include(p => p.Ward)
                    .Include(p => p.ResponsibleAdministrator)
                    .Include(p => p.CreatedByAdministrator)
                    .Include(p => p.LastUpdatedByAdministrator)
                    .FirstOrDefault(
                        p =>
                            p.ProjectID ==
                            id.Value
                    );


            if (project == null)
                return HttpNotFound();


            var model =
                new MunicipalProjectDetailsViewModel
                {
                    ProjectID =
                        project.ProjectID,

                    ProjectCode =
                        project.ProjectCode,

                    ProjectName =
                        project.ProjectName,

                    ProjectType =
                        project.ProjectType,

                    ProjectScope =
                        project.ProjectScope,

                    Description =
                        project.Description,

                    ProjectLocation =
                        project.ProjectLocation,

                    WardName =
                        project.Ward != null
                            ? "Ward " +
                              project.Ward.WardNumber +
                              " – " +
                              project.Ward.WardName
                            : "Ward not specified",

                    StartDate =
                        project.StartDate,

                    ExpectedCompletionDate =
                        project.ExpectedCompletionDate,

                    Status =
                        project.Status.ToString(),

                    Priority =
                        project.Priority.ToString(),

                    EstimatedBudget =
                        project.EstimatedBudget,

                    ResponsibleAdministratorName =
                        project.ResponsibleAdministrator != null
                            ? project.ResponsibleAdministrator.FirstName +
                              " " +
                              project.ResponsibleAdministrator.LastName
                            : "Administrator unavailable",

                    ResponsibleAdministratorEmail =
                        project.ResponsibleAdministrator != null
                            ? project.ResponsibleAdministrator.EmailAddress
                            : null,

                    DateRegistered =
                        project.DateRegistered,

                    CreatedByAdministratorName =
                        project.CreatedByAdministrator != null
                            ? project.CreatedByAdministrator.FirstName +
                              " " +
                              project.CreatedByAdministrator.LastName
                            : "Administrator unavailable",

                    LastUpdatedDate =
                        project.LastUpdatedDate,

                    LastUpdatedByAdministratorName =
                        project.LastUpdatedByAdministrator != null
                            ? project.LastUpdatedByAdministrator.FirstName +
                              " " +
                              project.LastUpdatedByAdministrator.LastName
                            : null
                };


            // =========================================================
            // US94 — PROJECT LOCATION
            // Pass the coordinates selected during project registration
            // to the read-only project details map.
            // =========================================================

            ViewBag.MapLatitude =
                project.Latitude;

            ViewBag.MapLongitude =
                project.Longitude;


            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DefineScope(MunicipalProjectScopeViewModel model)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            if (model == null)
                return HttpNotFound();

            model.ProjectScope = model.ProjectScope?.Trim();

            if (string.IsNullOrWhiteSpace(model.ProjectScope))
            {
                ModelState.AddModelError(
                    "ProjectScope",
                    "Please define the project scope."
                );
            }

            if (model.ProjectScope != null && model.ProjectScope.Length < 20)
            {
                ModelState.AddModelError(
                    "ProjectScope",
                    "Project scope must contain at least 20 characters."
                );
            }

            if (model.ProjectScope != null && model.ProjectScope.Length > 4000)
            {
                ModelState.AddModelError(
                    "ProjectScope",
                    "Project scope cannot exceed 4000 characters."
                );
            }

            var administratorID = GetAdministratorID();

            if (!administratorID.HasValue)
                return new HttpUnauthorizedResult();

            var project = db.MunicipalProjects
                .FirstOrDefault(p => p.ProjectID == model.ProjectID);

            if (project == null)
                return HttpNotFound();

            model.ProjectCode = project.ProjectCode;
            model.ProjectName = project.ProjectName;
            model.ProjectType = project.ProjectType;

            if (!ModelState.IsValid)
                return View(model);

            project.ProjectScope = model.ProjectScope;
            project.LastUpdatedDate = DateTime.Now;
            project.LastUpdatedByAdministratorID = administratorID.Value;

            db.SaveChanges();

            TempData["SuccessMessage"] =
                "Project scope has been saved successfully.";

            return RedirectToAction(
                "Details",
                new { id = project.ProjectID }
            );
        }

        // ============================================================
        // US92 - DEFINE / EDIT PROJECT SCOPE
        // ============================================================

        [HttpGet]
        public ActionResult DefineScope(int? id)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            if (!id.HasValue)
                return HttpNotFound();

            var project = db.MunicipalProjects
                .AsNoTracking()
                .FirstOrDefault(
                    p => p.ProjectID == id.Value
                );

            if (project == null)
                return HttpNotFound();

            var model =
                new MunicipalProjectScopeViewModel
                {
                    ProjectID =
                        project.ProjectID,

                    ProjectCode =
                        project.ProjectCode,

                    ProjectName =
                        project.ProjectName,

                    ProjectType =
                        project.ProjectType,

                    ProjectScope =
                        project.ProjectScope
                };

            return View(model);
        }

        [HttpGet]
        public ActionResult Objectives(int? id)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            if (!id.HasValue)
                return HttpNotFound();

            var project = db.MunicipalProjects
                .AsNoTracking()
                .FirstOrDefault(p => p.ProjectID == id.Value);

            if (project == null)
                return HttpNotFound();

            var model = new MunicipalProjectObjectivesViewModel
            {
                ProjectID = project.ProjectID,
                ProjectCode = project.ProjectCode,
                ProjectName = project.ProjectName,
                ProjectType = project.ProjectType,
                Objectives = db.ProjectObjectives
                    .AsNoTracking()
                    .Where(o => o.ProjectID == project.ProjectID)
                    .OrderBy(o => o.Priority)
                    .ThenBy(o => o.TargetDate)
                    .ThenBy(o => o.ObjectiveTitle)
                    .Select(o => new ProjectObjectiveItemViewModel
                    {
                        ProjectObjectiveID = o.ProjectObjectiveID,
                        ObjectiveTitle = o.ObjectiveTitle,
                        ObjectiveDescription = o.ObjectiveDescription,
                        Priority = o.Priority,
                        TargetDate = o.TargetDate,
                        Status = o.Status
                    })
                    .ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddObjective(
    MunicipalProjectObjectivesViewModel model,
    ProjectObjectiveItemViewModel objective)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            if (objective == null)
                return HttpNotFound();

            var administratorID = GetAdministratorID();

            if (!administratorID.HasValue)
                return new HttpUnauthorizedResult();

            objective.ObjectiveTitle =
                objective.ObjectiveTitle?.Trim();

            objective.ObjectiveDescription =
                objective.ObjectiveDescription?.Trim();

            if (objective.TargetDate.HasValue)
            {
                var project = db.MunicipalProjects
                    .AsNoTracking()
                    .FirstOrDefault(p => p.ProjectID == model.ProjectID);

                if (project == null)
                    return HttpNotFound();

                if (objective.TargetDate.Value.Date < project.StartDate.Date)
                {
                    ModelState.AddModelError(
                        "",
                        "The objective target date cannot be before the project start date."
                    );
                }
            }

            if (!Enum.IsDefined(
                typeof(MunicipalProjectPriority),
                objective.Priority))
            {
                ModelState.AddModelError(
                    "",
                    "Please select a valid objective priority."
                );
            }

            if (!Enum.IsDefined(
                typeof(ProjectObjectiveStatus),
                objective.Status))
            {
                ModelState.AddModelError(
                    "",
                    "Please select a valid objective status."
                );
            }

            var projectExists = db.MunicipalProjects
                .Any(p => p.ProjectID == model.ProjectID);

            if (!projectExists)
                return HttpNotFound();

            if (!ModelState.IsValid)
            {
                return RedirectToAction(
                    "Objectives",
                    new { id = model.ProjectID }
                );
            }

            var objectiveEntity = new ProjectObjective
            {
                ProjectID = model.ProjectID,
                ObjectiveTitle = objective.ObjectiveTitle,
                ObjectiveDescription = objective.ObjectiveDescription,
                Priority = objective.Priority,
                TargetDate = objective.TargetDate,
                Status = objective.Status,
                DateCreated = DateTime.Now,
                CreatedByAdministratorID = administratorID.Value
            };

            db.ProjectObjectives.Add(objectiveEntity);
            db.SaveChanges();

            TempData["SuccessMessage"] =
                "Project objective has been added successfully.";

            return RedirectToAction(
                "Objectives",
                new { id = model.ProjectID }
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteObjective(
    int? id,
    int? projectID)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            if (!id.HasValue || !projectID.HasValue)
                return HttpNotFound();

            var objective = db.ProjectObjectives
                .FirstOrDefault(o =>
                    o.ProjectObjectiveID == id.Value &&
                    o.ProjectID == projectID.Value);

            if (objective == null)
                return HttpNotFound();

            db.ProjectObjectives.Remove(objective);
            db.SaveChanges();

            TempData["SuccessMessage"] =
                "Project objective has been removed.";

            return RedirectToAction(
                "Objectives",
                new { id = projectID.Value }
            );
        }

        // =========================================================
        // US95 — LINK SERVICE REQUESTS TO MUNICIPAL PROJECT
        // =========================================================

        [HttpGet]
        public ActionResult Requests(
            int? id,
            string searchTerm,
            string status,
            string priority,
            int? ward)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            if (!id.HasValue)
                return HttpNotFound();

            var project =
                db.MunicipalProjects
                    .AsNoTracking()
                    .Include(p => p.Ward)
                    .FirstOrDefault(p => p.ProjectID == id.Value);

            if (project == null)
                return HttpNotFound();

            var linkedRequestIds =
                db.ProjectRequests
                    .Where(pr => pr.ProjectID == project.ProjectID)
                    .Select(pr => pr.RequestID)
                    .ToList();

            var linkedRequests =
                db.ProjectRequests
                    .AsNoTracking()
                    .Include(pr => pr.Request)
                    .Include(pr => pr.LinkedByAdministrator)
                    .Where(pr => pr.ProjectID == project.ProjectID)
                    .ToList();

            var linkedRequestItems =
                linkedRequests
                    .Select(pr => new ProjectRequestItemViewModel
                    {
                        ProjectRequestID =
                            pr.ProjectRequestID,

                        RequestID =
                            pr.RequestID,

                        ReferenceNumber =
                            pr.Request != null
                                ? pr.Request.ReferenceNumber
                                : "Reference unavailable",

                        RequestTitle =
                            pr.Request != null
                                ? pr.Request.Title
                                : "Request unavailable",

                        CategoryName =
                            pr.Request != null &&
                            pr.Request.Category != null
                                ? pr.Request.Category.CategoryName
                                : "Category unavailable",

                        WardName =
                            pr.Request != null &&
                            pr.Request.Ward != null
                                ? "Ward " +
                                  pr.Request.Ward.WardNumber +
                                  " – " +
                                  pr.Request.Ward.WardName
                                : "Ward unavailable",

                        Priority =
                            pr.Request != null
                                ? pr.Request.Priority.ToString()
                                : "Unknown",

                        Status =
                            pr.Request != null
                                ? pr.Request.Status.ToString()
                                : "Unknown",

                        DateSubmitted =
                            pr.Request != null
                                ? pr.Request.DateSubmitted
                                : DateTime.MinValue,

                        RelationshipType =
                            pr.RelationshipType,

                        Notes =
                            pr.Notes,

                        DateLinked =
                            pr.DateLinked,

                        LinkedByAdministratorName =
                            pr.LinkedByAdministrator != null
                                ? pr.LinkedByAdministrator.FirstName +
                                  " " +
                                  pr.LinkedByAdministrator.LastName
                                : "Administrator unavailable"
                    })
                    .OrderByDescending(x => x.DateLinked)
                    .ToList();

            var availableQuery =
                db.Requests
                    .AsNoTracking()
                    .Include(r => r.Category)
                    .Include(r => r.Ward)
                    .Where(r => !linkedRequestIds.Contains(r.RequestID));

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();

                availableQuery =
                    availableQuery.Where(r =>
                        r.ReferenceNumber.Contains(searchTerm) ||
                        r.Title.Contains(searchTerm) ||
                        r.Description.Contains(searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                RequestStatus selectedStatus;

                if (Enum.TryParse(status, true, out selectedStatus))
                {
                    availableQuery =
                        availableQuery.Where(r =>
                            r.Status == selectedStatus);
                }
            }

            if (!string.IsNullOrWhiteSpace(priority))
            {
                Priority selectedPriority;

                if (Enum.TryParse(priority, true, out selectedPriority))
                {
                    availableQuery =
                        availableQuery.Where(r =>
                            r.Priority == selectedPriority);
                }
            }

            if (ward.HasValue)
            {
                availableQuery =
                    availableQuery.Where(r =>
                        r.WardID == ward.Value);
            }

            var availableRequests =
                availableQuery
                    .OrderByDescending(r => r.DateSubmitted)
                    .Take(100)
                    .ToList();

            var availableRequestItems =
                availableRequests
                    .Select(r => new ProjectRequestAvailableItemViewModel
                    {
                        RequestID =
                            r.RequestID,

                        ReferenceNumber =
                            r.ReferenceNumber,

                        RequestTitle =
                            r.Title,

                        CategoryName =
                            r.Category != null
                                ? r.Category.CategoryName
                                : "Category unavailable",

                        WardName =
                            r.Ward != null
                                ? "Ward " +
                                  r.Ward.WardNumber +
                                  " – " +
                                  r.Ward.WardName
                                : "Ward unavailable",

                        Priority =
                            r.Priority.ToString(),

                        Status =
                            r.Status.ToString(),

                        DateSubmitted =
                            r.DateSubmitted
                    })
                    .ToList();

            var model =
                new MunicipalProjectRequestsViewModel
                {
                    ProjectID =
                        project.ProjectID,

                    ProjectCode =
                        project.ProjectCode,

                    ProjectName =
                        project.ProjectName,

                    ProjectType =
                        project.ProjectType,

                    ProjectLocation =
                        project.ProjectLocation,

                    WardName =
                        project.Ward != null
                            ? "Ward " +
                              project.Ward.WardNumber +
                              " – " +
                              project.Ward.WardName
                            : "Ward unavailable",

                    LinkedRequests =
                        linkedRequestItems,

                    AvailableRequests =
                        availableRequestItems,

                    LinkedRequestCount =
                        linkedRequestItems.Count,

                    SearchTerm =
                        searchTerm,

                    StatusFilter =
                        status,

                    PriorityFilter =
                        priority,

                    WardFilter =
                        ward
                };

            PopulateProjectRequestOptions(model);

            return View(model);
        }

        // =========================================================
        // US95 — LINK SERVICE REQUEST TO MUNICIPAL PROJECT
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkRequest(LinkProjectRequestViewModel model)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            var administratorID = GetAdministratorID();

            if (!administratorID.HasValue)
                return new HttpUnauthorizedResult();

            if (model.ProjectID <= 0)
                return HttpNotFound();

            if (model.RequestID <= 0)
            {
                TempData["ErrorMessage"] =
                    "Please select a service request to link.";

                return RedirectToAction(
                    "Requests",
                    new { id = model.ProjectID });
            }

            var project =
                db.MunicipalProjects
                    .FirstOrDefault(p =>
                        p.ProjectID == model.ProjectID);

            if (project == null)
                return HttpNotFound();

            var request =
                db.Requests
                    .FirstOrDefault(r =>
                        r.RequestID == model.RequestID);

            if (request == null)
            {
                TempData["ErrorMessage"] =
                    "The selected service request could not be found.";

                return RedirectToAction(
                    "Requests",
                    new { id = model.ProjectID });
            }

            var alreadyLinked =
                db.ProjectRequests.Any(pr =>
                    pr.ProjectID == model.ProjectID &&
                    pr.RequestID == model.RequestID);

            if (alreadyLinked)
            {
                TempData["ErrorMessage"] =
                    "This service request is already linked to the project.";

                return RedirectToAction(
                    "Requests",
                    new { id = model.ProjectID });
            }

            var relationshipType =
                string.IsNullOrWhiteSpace(model.RelationshipType)
                    ? null
                    : model.RelationshipType.Trim();

            var allowedRelationshipTypes =
                new[]
                {
            "Primary Project Request",
            "Related Service Request",
            "Supporting Request",
            "Scope-Related Request",
            "Community Issue"
                };

            if (!string.IsNullOrWhiteSpace(relationshipType) &&
                !allowedRelationshipTypes.Contains(relationshipType))
            {
                TempData["ErrorMessage"] =
                    "The selected relationship type is invalid.";

                return RedirectToAction(
                    "Requests",
                    new { id = model.ProjectID });
            }

            var notes =
                string.IsNullOrWhiteSpace(model.Notes)
                    ? null
                    : model.Notes.Trim();

            var projectRequest =
                new ProjectRequest
                {
                    ProjectID =
                        project.ProjectID,

                    RequestID =
                        request.RequestID,

                    RelationshipType =
                        relationshipType,

                    DateLinked =
                        DateTime.Now,

                    LinkedByAdministratorID =
                        administratorID.Value,

                    Notes =
                        notes
                };

            db.ProjectRequests.Add(projectRequest);
            db.SaveChanges();

            TempData["SuccessMessage"] =
                "Service request " +
                request.ReferenceNumber +
                " was successfully linked to the project.";

            return RedirectToAction(
                "Requests",
                new { id = project.ProjectID });
        }

        // =========================================================
        // US95 — UNLINK SERVICE REQUEST FROM MUNICIPAL PROJECT
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UnlinkRequest(
            int? id,
            int? projectID)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            if (!id.HasValue || !projectID.HasValue)
                return HttpNotFound();

            var projectRequest =
                db.ProjectRequests
                    .Include(pr => pr.Request)
                    .FirstOrDefault(pr =>
                        pr.ProjectRequestID == id.Value &&
                        pr.ProjectID == projectID.Value);

            if (projectRequest == null)
                return HttpNotFound();

            var referenceNumber =
                projectRequest.Request != null
                    ? projectRequest.Request.ReferenceNumber
                    : "the selected request";

            db.ProjectRequests.Remove(projectRequest);
            db.SaveChanges();

            TempData["SuccessMessage"] =
                "Service request " +
                referenceNumber +
                " was successfully unlinked from the project.";

            return RedirectToAction(
                "Requests",
                new { id = projectID.Value });
        }


        private void PopulateProjectRequestOptions(
    MunicipalProjectRequestsViewModel model)
        {
            model.RelationshipTypeOptions =
                new List<SelectListItem>
                {
            new SelectListItem
            {
                Text = "Primary Project Request",
                Value = "Primary Project Request",
                Selected =
                    model.RelationshipType == "Primary Project Request"
            },
            new SelectListItem
            {
                Text = "Related Service Request",
                Value = "Related Service Request",
                Selected =
                    model.RelationshipType == "Related Service Request"
            },
            new SelectListItem
            {
                Text = "Supporting Request",
                Value = "Supporting Request",
                Selected =
                    model.RelationshipType == "Supporting Request"
            },
            new SelectListItem
            {
                Text = "Scope-Related Request",
                Value = "Scope-Related Request",
                Selected =
                    model.RelationshipType == "Scope-Related Request"
            },
            new SelectListItem
            {
                Text = "Community Issue",
                Value = "Community Issue",
                Selected =
                    model.RelationshipType == "Community Issue"
            }
                };

            model.StatusOptions =
                Enum.GetValues(typeof(RequestStatus))
                    .Cast<RequestStatus>()
                    .Select(s => new SelectListItem
                    {
                        Text = s.ToString(),
                        Value = s.ToString(),
                        Selected =
                            s.ToString() == model.StatusFilter
                    })
                    .ToList();

            model.PriorityOptions =
                Enum.GetValues(typeof(Priority))
                    .Cast<Priority>()
                    .Select(p => new SelectListItem
                    {
                        Text = p.ToString(),
                        Value = p.ToString(),
                        Selected =
                            p.ToString() == model.PriorityFilter
                    })
                    .ToList();

            model.WardOptions =
                db.Wards
                    .AsNoTracking()
                    .Where(w => w.IsActive)
                    .ToList()
                    .OrderBy(w =>
                    {
                        int number;

                        return int.TryParse(
                            w.WardNumber,
                            out number)
                            ? number
                            : int.MaxValue;
                    })
                    .Select(w => new SelectListItem
                    {
                        Text =
                            "Ward " +
                            w.WardNumber +
                            " – " +
                            w.WardName,

                        Value =
                            w.WardID.ToString(),

                        Selected =
                            model.WardFilter.HasValue &&
                            model.WardFilter.Value == w.WardID
                    })
                    .ToList();
        }

        // =========================================================
        // US96 — LINK MUNICIPAL ASSETS TO PROJECT
        // =========================================================

        [HttpGet]
        public ActionResult Assets(
            int? id,
            string searchTerm,
            string assetType,
            string condition,
            string status,
            int? ward)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            if (!id.HasValue)
                return HttpNotFound();

            var project =
                db.MunicipalProjects
                    .AsNoTracking()
                    .Include(p => p.Ward)
                    .FirstOrDefault(p =>
                        p.ProjectID == id.Value);

            if (project == null)
                return HttpNotFound();

            var linkedAssetIds =
                db.AssetProjects
                    .Where(ap =>
                        ap.ProjectID == project.ProjectID)
                    .Select(ap => ap.AssetID)
                    .ToList();

            var linkedAssets =
                db.AssetProjects
                    .AsNoTracking()
                    .Include(ap => ap.Asset)
                    .Include(ap => ap.LinkedByAdministrator)
                    .Where(ap =>
                        ap.ProjectID == project.ProjectID)
                    .ToList();

            var linkedAssetItems =
                linkedAssets
                    .Where(ap => ap.Asset != null)
                    .Select(ap => new ProjectAssetItemViewModel
                    {
                        AssetProjectID =
                            ap.AssetProjectID,

                        AssetID =
                            ap.AssetID,

                        AssetCode =
                            ap.Asset.AssetCode,

                        AssetName =
                            ap.Asset.AssetName,

                        AssetType =
                            ap.Asset.AssetType,

                        AssetCategory =
                            ap.Asset.AssetCategory,

                        WardName =
                            ap.Asset.Ward != null
                                ? "Ward " +
                                  ap.Asset.Ward.WardNumber +
                                  " – " +
                                  ap.Asset.Ward.WardName
                                : "Ward unavailable",

                        Condition =
                            ap.Asset.Condition.ToString(),

                        Status =
                            ap.Asset.Status.ToString(),

                        LastInspectionDate =
                            ap.Asset.LastInspectionDate,

                        LastMaintenanceDate =
                            ap.Asset.LastMaintenanceDate,

                        LinkDate =
                            ap.LinkDate,

                        Notes =
                            ap.Notes,

                        LinkedByAdministratorName =
                            ap.LinkedByAdministrator != null
                                ? ap.LinkedByAdministrator.FirstName +
                                  " " +
                                  ap.LinkedByAdministrator.LastName
                                : "Administrator unavailable"
                    })
                    .OrderByDescending(x => x.LinkDate)
                    .ToList();

            var availableQuery =
                db.MunicipalAssets
                    .AsNoTracking()
                    .Include(a => a.Ward)
                    .Where(a =>
                        !linkedAssetIds.Contains(a.AssetID));

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();

                availableQuery =
                    availableQuery.Where(a =>
                        a.AssetCode.Contains(searchTerm) ||
                        a.AssetName.Contains(searchTerm) ||
                        a.AssetType.Contains(searchTerm) ||
                        a.AssetCategory.Contains(searchTerm) ||
                        a.LocationDescription.Contains(searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(assetType))
            {
                availableQuery =
                    availableQuery.Where(a =>
                        a.AssetType == assetType);
            }

            if (!string.IsNullOrWhiteSpace(condition))
            {
                AssetCondition selectedCondition;

                if (Enum.TryParse(
                    condition,
                    true,
                    out selectedCondition))
                {
                    availableQuery =
                        availableQuery.Where(a =>
                            a.Condition == selectedCondition);
                }
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                AssetStatus selectedStatus;

                if (Enum.TryParse(
                    status,
                    true,
                    out selectedStatus))
                {
                    availableQuery =
                        availableQuery.Where(a =>
                            a.Status == selectedStatus);
                }
            }

            if (ward.HasValue)
            {
                availableQuery =
                    availableQuery.Where(a =>
                        a.WardID == ward.Value);
            }

            var availableAssets =
                availableQuery
                    .OrderBy(a => a.AssetCode)
                    .Take(100)
                    .ToList();

            var availableAssetItems =
                availableAssets
                    .Select(a => new ProjectAssetAvailableItemViewModel
                    {
                        AssetID =
                            a.AssetID,

                        AssetCode =
                            a.AssetCode,

                        AssetName =
                            a.AssetName,

                        AssetType =
                            a.AssetType,

                        AssetCategory =
                            a.AssetCategory,

                        WardName =
                            a.Ward != null
                                ? "Ward " +
                                  a.Ward.WardNumber +
                                  " – " +
                                  a.Ward.WardName
                                : "Ward unavailable",

                        Condition =
                            a.Condition.ToString(),

                        Status =
                            a.Status.ToString(),

                        DateRegistered =
                            a.DateRegistered,

                        LastInspectionDate =
                            a.LastInspectionDate,

                        LastMaintenanceDate =
                            a.LastMaintenanceDate
                    })
                    .ToList();

            var model =
                new MunicipalProjectAssetsViewModel
                {
                    ProjectID =
                        project.ProjectID,

                    ProjectCode =
                        project.ProjectCode,

                    ProjectName =
                        project.ProjectName,

                    ProjectType =
                        project.ProjectType,

                    ProjectLocation =
                        project.ProjectLocation,

                    WardName =
                        project.Ward != null
                            ? "Ward " +
                              project.Ward.WardNumber +
                              " – " +
                              project.Ward.WardName
                            : "Ward unavailable",

                    LinkedAssets =
                        linkedAssetItems,

                    AvailableAssets =
                        availableAssetItems,

                    LinkedAssetCount =
                        linkedAssetItems.Count,

                    SearchTerm =
                        searchTerm,

                    AssetTypeFilter =
                        assetType,

                    ConditionFilter =
                        condition,

                    StatusFilter =
                        status,

                    WardFilter =
                        ward
                };

            PopulateProjectAssetOptions(model);

            return View(model);
        }

        private void PopulateProjectAssetOptions(
    MunicipalProjectAssetsViewModel model)
        {
            model.AssetTypeOptions =
                db.MunicipalAssets
                    .AsNoTracking()
                    .Where(a => a.AssetType != null)
                    .Select(a => a.AssetType)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList()
                    .Select(type => new SelectListItem
                    {
                        Text = type,
                        Value = type,
                        Selected =
                            type == model.AssetTypeFilter
                    })
                    .ToList();

            model.ConditionOptions =
                Enum.GetValues(typeof(AssetCondition))
                    .Cast<AssetCondition>()
                    .Select(c => new SelectListItem
                    {
                        Text = c.ToString(),
                        Value = c.ToString(),
                        Selected =
                            c.ToString() ==
                            model.ConditionFilter
                    })
                    .ToList();

            model.StatusOptions =
                Enum.GetValues(typeof(AssetStatus))
                    .Cast<AssetStatus>()
                    .Select(s => new SelectListItem
                    {
                        Text = s.ToString(),
                        Value = s.ToString(),
                        Selected =
                            s.ToString() ==
                            model.StatusFilter
                    })
                    .ToList();

            model.WardOptions =
                db.Wards
                    .AsNoTracking()
                    .Where(w => w.IsActive)
                    .ToList()
                    .OrderBy(w =>
                    {
                        int number;

                        return int.TryParse(
                            w.WardNumber,
                            out number)
                            ? number
                            : int.MaxValue;
                    })
                    .Select(w => new SelectListItem
                    {
                        Text =
                            "Ward " +
                            w.WardNumber +
                            " – " +
                            w.WardName,

                        Value =
                            w.WardID.ToString(),

                        Selected =
                            model.WardFilter.HasValue &&
                            model.WardFilter.Value ==
                            w.WardID
                    })
                    .ToList();
        }

        // =========================================================
        // US96 — LINK ASSET
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkAsset(
            LinkProjectAssetViewModel model)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            var administratorID =
                GetAdministratorID();

            if (!administratorID.HasValue)
                return new HttpUnauthorizedResult();

            if (model.ProjectID <= 0)
                return HttpNotFound();

            if (model.AssetID <= 0)
            {
                TempData["ErrorMessage"] =
                    "Please select a municipal asset to link.";

                return RedirectToAction(
                    "Assets",
                    new { id = model.ProjectID });
            }

            var project =
                db.MunicipalProjects
                    .FirstOrDefault(p =>
                        p.ProjectID == model.ProjectID);

            if (project == null)
                return HttpNotFound();

            var asset =
                db.MunicipalAssets
                    .FirstOrDefault(a =>
                        a.AssetID == model.AssetID);

            if (asset == null)
            {
                TempData["ErrorMessage"] =
                    "The selected municipal asset could not be found.";

                return RedirectToAction(
                    "Assets",
                    new { id = model.ProjectID });
            }

            var alreadyLinked =
                db.AssetProjects.Any(ap =>
                    ap.ProjectID == model.ProjectID &&
                    ap.AssetID == model.AssetID);

            if (alreadyLinked)
            {
                TempData["ErrorMessage"] =
                    "This asset is already linked to the project.";

                return RedirectToAction(
                    "Assets",
                    new { id = model.ProjectID });
            }

            var notes =
                string.IsNullOrWhiteSpace(model.Notes)
                    ? null
                    : model.Notes.Trim();

            var assetProject =
                new AssetProject
                {
                    AssetID =
                        asset.AssetID,

                    ProjectID =
                        project.ProjectID,

                    LinkedByAdministratorID =
                        administratorID.Value,

                    LinkDate =
                        DateTime.Now,

                    Notes =
                        notes
                };

            db.AssetProjects.Add(assetProject);
            db.SaveChanges();

            TempData["SuccessMessage"] =
                "Asset " +
                asset.AssetCode +
                " was successfully linked to the project.";

            return RedirectToAction(
                "Assets",
                new { id = project.ProjectID });
        }

        // =========================================================
        // US96 — UNLINK ASSET
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UnlinkAsset(
            int? id,
            int? projectID)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            if (!id.HasValue ||
                !projectID.HasValue)
                return HttpNotFound();

            var assetProject =
                db.AssetProjects
                    .Include(ap => ap.Asset)
                    .FirstOrDefault(ap =>
                        ap.AssetProjectID == id.Value &&
                        ap.ProjectID == projectID.Value);

            if (assetProject == null)
                return HttpNotFound();

            var assetCode =
                assetProject.Asset != null
                    ? assetProject.Asset.AssetCode
                    : "the selected asset";

            db.AssetProjects.Remove(assetProject);
            db.SaveChanges();

            TempData["SuccessMessage"] =
                "Asset " +
                assetCode +
                " was successfully unlinked from the project.";

            return RedirectToAction(
                "Assets",
                new { id = projectID.Value });
        }

        // =========================================================
        // US97 — DEFINE PROJECT MILESTONES
        // =========================================================

        [HttpGet]
        public ActionResult Milestones(int? id)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            if (!id.HasValue)
                return HttpNotFound();

            var project =
                db.MunicipalProjects
                    .AsNoTracking()
                    .Include(p => p.Ward)
                    .FirstOrDefault(p =>
                        p.ProjectID == id.Value);

            if (project == null)
                return HttpNotFound();


            var milestones =
                db.ProjectMilestones
                    .AsNoTracking()
                    .Where(m =>
                        m.ProjectID == project.ProjectID)
                    .OrderBy(m => m.SequenceNumber)
                    .ThenBy(m => m.PlannedStartDate)
                    .ToList();


            var milestoneItems =
                milestones
                    .Select(m =>
                        new ProjectMilestoneItemViewModel
                        {
                            ProjectMilestoneID =
                                m.ProjectMilestoneID,

                            MilestoneName =
                                m.MilestoneName,

                            Description =
                                m.Description,

                            PlannedStartDate =
                                m.PlannedStartDate,

                            PlannedCompletionDate =
                                m.PlannedCompletionDate,

                            ActualCompletionDate =
                                m.ActualCompletionDate,

                            Status =
                                m.Status,

                            ProgressPercentage =
                                m.ProgressPercentage,

                            Priority =
                                m.Priority,

                            SequenceNumber =
                                m.SequenceNumber
                        })
                    .ToList();


            var model =
                new MunicipalProjectMilestonesViewModel
                {
                    ProjectID =
                        project.ProjectID,

                    ProjectCode =
                        project.ProjectCode,

                    ProjectName =
                        project.ProjectName,

                    ProjectType =
                        project.ProjectType,

                    ProjectLocation =
                        project.ProjectLocation,

                    WardName =
                        project.Ward != null
                            ? "Ward " +
                              project.Ward.WardNumber +
                              " – " +
                              project.Ward.WardName
                            : "Ward unavailable",

                    ProjectStartDate =
                        project.StartDate,

                    ExpectedCompletionDate =
                        project.ExpectedCompletionDate,

                    Milestones =
                        milestoneItems,

                    MilestoneCount =
                        milestoneItems.Count,

                    CompletedMilestoneCount =
                        milestoneItems.Count(m =>
                            m.Status ==
                            ProjectMilestoneStatus.Completed),

                    InProgressMilestoneCount =
                        milestoneItems.Count(m =>
                            m.Status ==
                            ProjectMilestoneStatus.InProgress),

                    DelayedMilestoneCount =
                        milestoneItems.Count(m =>
                            m.Status ==
                            ProjectMilestoneStatus.Delayed)
                };


            return View(model);
        }


        // =========================================================
        // US97 — ADD PROJECT MILESTONE
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddMilestone(
            MunicipalProjectMilestonesViewModel model,
            ProjectMilestoneItemViewModel milestone)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            if (model == null ||
                milestone == null)
                return HttpNotFound();


            var administratorID =
                GetAdministratorID();

            if (!administratorID.HasValue)
                return new HttpUnauthorizedResult();


            var project =
                db.MunicipalProjects
                    .FirstOrDefault(p =>
                        p.ProjectID == model.ProjectID);

            if (project == null)
                return HttpNotFound();


            // =========================================================
            // CLEAN INPUT
            // =========================================================

            milestone.MilestoneName =
                milestone.MilestoneName?.Trim();

            milestone.Description =
                milestone.Description?.Trim();


            // =========================================================
            // REQUIRED VALIDATION
            // =========================================================

            if (string.IsNullOrWhiteSpace(
                milestone.MilestoneName))
            {
                ModelState.AddModelError(
                    "MilestoneName",
                    "Please enter a milestone name."
                );
            }


            if (milestone.MilestoneName != null &&
                milestone.MilestoneName.Length < 3)
            {
                ModelState.AddModelError(
                    "MilestoneName",
                    "Milestone name must contain at least 3 characters."
                );
            }


            // =========================================================
            // DATE VALIDATION
            // =========================================================

            if (milestone.PlannedStartDate.Date <
                project.StartDate.Date)
            {
                ModelState.AddModelError(
                    "PlannedStartDate",
                    "Milestone start date cannot be before the project start date."
                );
            }


            if (milestone.PlannedCompletionDate.Date <
                milestone.PlannedStartDate.Date)
            {
                ModelState.AddModelError(
                    "PlannedCompletionDate",
                    "Milestone completion date cannot be earlier than its planned start date."
                );
            }


            if (project.ExpectedCompletionDate.HasValue &&
                milestone.PlannedCompletionDate.Date >
                project.ExpectedCompletionDate.Value.Date)
            {
                ModelState.AddModelError(
                    "PlannedCompletionDate",
                    "Milestone completion date cannot be later than the project's expected completion date."
                );
            }


            if (milestone.ActualCompletionDate.HasValue &&
                milestone.ActualCompletionDate.Value.Date <
                milestone.PlannedStartDate.Date)
            {
                ModelState.AddModelError(
                    "ActualCompletionDate",
                    "Actual completion date cannot be before the milestone start date."
                );
            }


            // =========================================================
            // PROGRESS VALIDATION
            // =========================================================

            if (milestone.ProgressPercentage < 0 ||
                milestone.ProgressPercentage > 100)
            {
                ModelState.AddModelError(
                    "ProgressPercentage",
                    "Progress must be between 0 and 100 percent."
                );
            }


            // =========================================================
            // STATUS / PROGRESS CONSISTENCY
            // =========================================================

            if (milestone.Status ==
                ProjectMilestoneStatus.NotStarted &&
                milestone.ProgressPercentage != 0)
            {
                ModelState.AddModelError(
                    "ProgressPercentage",
                    "A milestone marked Not Started must have 0% progress."
                );
            }


            if (milestone.Status ==
                ProjectMilestoneStatus.Completed &&
                milestone.ProgressPercentage != 100)
            {
                ModelState.AddModelError(
                    "ProgressPercentage",
                    "A completed milestone must have 100% progress."
                );
            }


            if (milestone.Status ==
                ProjectMilestoneStatus.Cancelled &&
                milestone.ProgressPercentage == 100)
            {
                ModelState.AddModelError(
                    "Status",
                    "A cancelled milestone cannot have 100% progress."
                );
            }


            if (milestone.Status ==
                ProjectMilestoneStatus.Completed &&
                !milestone.ActualCompletionDate.HasValue)
            {
                ModelState.AddModelError(
                    "ActualCompletionDate",
                    "Please provide the actual completion date for a completed milestone."
                );
            }


            // =========================================================
            // ENUM VALIDATION
            // =========================================================

            if (!Enum.IsDefined(
                typeof(ProjectMilestoneStatus),
                milestone.Status))
            {
                ModelState.AddModelError(
                    "Status",
                    "Please select a valid milestone status."
                );
            }


            if (!Enum.IsDefined(
                typeof(MunicipalProjectPriority),
                milestone.Priority))
            {
                ModelState.AddModelError(
                    "Priority",
                    "Please select a valid milestone priority."
                );
            }


            // =========================================================
            // SEQUENCE VALIDATION
            // =========================================================

            if (milestone.SequenceNumber < 1 ||
                milestone.SequenceNumber > 9999)
            {
                ModelState.AddModelError(
                    "SequenceNumber",
                    "Sequence number must be between 1 and 9999."
                );
            }


            var sequenceExists =
                db.ProjectMilestones.Any(m =>
                    m.ProjectID == project.ProjectID &&
                    m.SequenceNumber ==
                        milestone.SequenceNumber);

            if (sequenceExists)
            {
                ModelState.AddModelError(
                    "SequenceNumber",
                    "This sequence number is already assigned to another milestone in this project."
                );
            }


            // =========================================================
            // RETURN VALIDATION ERRORS
            // =========================================================

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] =
                    "The milestone could not be added. Please correct the highlighted information.";

                return RedirectToAction(
                    "Milestones",
                    new
                    {
                        id = model.ProjectID
                    });
            }


            // =========================================================
            // CREATE MILESTONE
            // =========================================================

            var milestoneEntity =
                new ProjectMilestone
                {
                    ProjectID =
                        project.ProjectID,

                    MilestoneName =
                        milestone.MilestoneName,

                    Description =
                        milestone.Description,

                    PlannedStartDate =
                        milestone.PlannedStartDate,

                    PlannedCompletionDate =
                        milestone.PlannedCompletionDate,

                    ActualCompletionDate =
                        milestone.ActualCompletionDate,

                    Status =
                        milestone.Status,

                    ProgressPercentage =
                        milestone.ProgressPercentage,

                    Priority =
                        milestone.Priority,

                    SequenceNumber =
                        milestone.SequenceNumber,

                    DateCreated =
                        DateTime.Now,

                    CreatedByAdministratorID =
                        administratorID.Value
                };


            db.ProjectMilestones.Add(
                milestoneEntity);

            db.SaveChanges();


            // =========================================================
            // UPDATE PROJECT AUDIT INFORMATION
            // =========================================================

            project.LastUpdatedDate =
                DateTime.Now;

            project.LastUpdatedByAdministratorID =
                administratorID.Value;

            db.SaveChanges();


            TempData["SuccessMessage"] =
                "Milestone \"" +
                milestoneEntity.MilestoneName +
                "\" was successfully added to the project.";


            return RedirectToAction(
                "Milestones",
                new
                {
                    id = project.ProjectID
                });
        }


        // =========================================================
        // US97 — EDIT PROJECT MILESTONE
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditMilestone(
            MunicipalProjectMilestonesViewModel model,
            ProjectMilestoneItemViewModel milestone)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            if (model == null ||
                milestone == null)
                return HttpNotFound();


            var administratorID =
                GetAdministratorID();

            if (!administratorID.HasValue)
                return new HttpUnauthorizedResult();


            var project =
                db.MunicipalProjects
                    .FirstOrDefault(p =>
                        p.ProjectID == model.ProjectID);

            if (project == null)
                return HttpNotFound();


            var milestoneEntity =
                db.ProjectMilestones
                    .FirstOrDefault(m =>
                        m.ProjectMilestoneID ==
                            milestone.ProjectMilestoneID &&
                        m.ProjectID ==
                            project.ProjectID);

            if (milestoneEntity == null)
                return HttpNotFound();


            milestone.MilestoneName =
                milestone.MilestoneName?.Trim();

            milestone.Description =
                milestone.Description?.Trim();


            // =========================================================
            // DATE VALIDATION
            // =========================================================

            if (milestone.PlannedStartDate.Date <
                project.StartDate.Date)
            {
                TempData["ErrorMessage"] =
                    "Milestone start date cannot be before the project start date.";

                return RedirectToAction(
                    "Milestones",
                    new { id = project.ProjectID });
            }


            if (milestone.PlannedCompletionDate.Date <
                milestone.PlannedStartDate.Date)
            {
                TempData["ErrorMessage"] =
                    "Milestone completion date cannot be earlier than its planned start date.";

                return RedirectToAction(
                    "Milestones",
                    new { id = project.ProjectID });
            }


            if (project.ExpectedCompletionDate.HasValue &&
                milestone.PlannedCompletionDate.Date >
                project.ExpectedCompletionDate.Value.Date)
            {
                TempData["ErrorMessage"] =
                    "Milestone completion date cannot be later than the project's expected completion date.";

                return RedirectToAction(
                    "Milestones",
                    new { id = project.ProjectID });
            }


            if (milestone.Status ==
                ProjectMilestoneStatus.NotStarted &&
                milestone.ProgressPercentage != 0)
            {
                TempData["ErrorMessage"] =
                    "A milestone marked Not Started must have 0% progress.";

                return RedirectToAction(
                    "Milestones",
                    new { id = project.ProjectID });
            }


            if (milestone.Status ==
                ProjectMilestoneStatus.Completed &&
                milestone.ProgressPercentage != 100)
            {
                TempData["ErrorMessage"] =
                    "A completed milestone must have 100% progress.";

                return RedirectToAction(
                    "Milestones",
                    new { id = project.ProjectID });
            }


            if (milestone.Status ==
                ProjectMilestoneStatus.Completed &&
                !milestone.ActualCompletionDate.HasValue)
            {
                TempData["ErrorMessage"] =
                    "Please provide the actual completion date for a completed milestone.";

                return RedirectToAction(
                    "Milestones",
                    new { id = project.ProjectID });
            }


            // =========================================================
            // SEQUENCE VALIDATION
            // =========================================================

            var sequenceExists =
                db.ProjectMilestones.Any(m =>
                    m.ProjectID == project.ProjectID &&
                    m.SequenceNumber ==
                        milestone.SequenceNumber &&
                    m.ProjectMilestoneID !=
                        milestone.ProjectMilestoneID);

            if (sequenceExists)
            {
                TempData["ErrorMessage"] =
                    "This sequence number is already assigned to another milestone.";

                return RedirectToAction(
                    "Milestones",
                    new { id = project.ProjectID });
            }


            // =========================================================
            // UPDATE
            // =========================================================

            milestoneEntity.MilestoneName =
                milestone.MilestoneName;

            milestoneEntity.Description =
                milestone.Description;

            milestoneEntity.PlannedStartDate =
                milestone.PlannedStartDate;

            milestoneEntity.PlannedCompletionDate =
                milestone.PlannedCompletionDate;

            milestoneEntity.ActualCompletionDate =
                milestone.ActualCompletionDate;

            milestoneEntity.Status =
                milestone.Status;

            milestoneEntity.ProgressPercentage =
                milestone.ProgressPercentage;

            milestoneEntity.Priority =
                milestone.Priority;

            milestoneEntity.SequenceNumber =
                milestone.SequenceNumber;


            project.LastUpdatedDate =
                DateTime.Now;

            project.LastUpdatedByAdministratorID =
                administratorID.Value;


            db.SaveChanges();


            TempData["SuccessMessage"] =
                "Milestone \"" +
                milestoneEntity.MilestoneName +
                "\" was successfully updated.";


            return RedirectToAction(
                "Milestones",
                new { id = project.ProjectID });
        }


        // =========================================================
        // US97 — DELETE PROJECT MILESTONE
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteMilestone(
            int? id,
            int? projectID)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            if (!id.HasValue ||
                !projectID.HasValue)
                return HttpNotFound();


            var administratorID =
                GetAdministratorID();

            if (!administratorID.HasValue)
                return new HttpUnauthorizedResult();


            var milestone =
                db.ProjectMilestones
                    .FirstOrDefault(m =>
                        m.ProjectMilestoneID ==
                            id.Value &&
                        m.ProjectID ==
                            projectID.Value);

            if (milestone == null)
                return HttpNotFound();


            var milestoneName =
                milestone.MilestoneName;


            var project =
                db.MunicipalProjects
                    .FirstOrDefault(p =>
                        p.ProjectID ==
                            projectID.Value);

            if (project == null)
                return HttpNotFound();


            db.ProjectMilestones.Remove(
                milestone);


            project.LastUpdatedDate =
                DateTime.Now;

            project.LastUpdatedByAdministratorID =
                administratorID.Value;


            db.SaveChanges();


            TempData["SuccessMessage"] =
                "Milestone \"" +
                milestoneName +
                "\" was removed from the project.";


            return RedirectToAction(
                "Milestones",
                new
                {
                    id = projectID.Value
                });
        }




        // ===============================================================
        // US98 - RECORD PROJECT PROGRESS
        // ===============================================================

        [HttpGet]
        public ActionResult Progress(int? id)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            if (!id.HasValue)
                return HttpNotFound();

            var model = BuildProgressViewModel(id.Value);

            if (model == null)
                return HttpNotFound();

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RecordProgress(
            MunicipalProjectProgressViewModel model)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            if (model == null || model.Entry == null)
                return HttpNotFound();

            var administratorID = GetAdministratorID();

            if (!administratorID.HasValue)
                return new HttpUnauthorizedResult();

            var project = db.MunicipalProjects
                .FirstOrDefault(p => p.ProjectID == model.ProjectID);

            if (project == null)
                return HttpNotFound();


            // -----------------------------------------------------------
            // Clean submitted values
            // -----------------------------------------------------------

            model.Entry.ProgressSummary =
                model.Entry.ProgressSummary?.Trim();

            model.Entry.CurrentActivity =
                model.Entry.CurrentActivity?.Trim();

            model.Entry.IssuesEncountered =
                model.Entry.IssuesEncountered?.Trim();

            model.Entry.NextPlannedActivity =
                model.Entry.NextPlannedActivity?.Trim();


            // -----------------------------------------------------------
            // Project lifecycle validation
            // -----------------------------------------------------------

            if (project.Status == MunicipalProjectStatus.Cancelled)
            {
                ModelState.AddModelError(
                    "",
                    "Progress cannot be recorded for a cancelled project."
                );
            }

            if (project.Status == MunicipalProjectStatus.Closed)
            {
                ModelState.AddModelError(
                    "",
                    "Progress cannot be recorded for a closed project."
                );
            }

            if (project.Status == MunicipalProjectStatus.Completed)
            {
                ModelState.AddModelError(
                    "",
                    "Progress cannot be recorded for a completed project."
                );
            }


            // -----------------------------------------------------------
            // Progress date validation
            // -----------------------------------------------------------

            if (model.Entry.ProgressDate.HasValue)
            {
                DateTime progressDate =
                    model.Entry.ProgressDate.Value.Date;

                DateTime today =
                    DateTime.Today;

                DateTime projectStartDate =
                    project.StartDate.Date;


                if (progressDate < projectStartDate)
                {
                    ModelState.AddModelError(
                        "Entry.ProgressDate",
                        "Progress date cannot be earlier than the project start date."
                    );
                }

                if (progressDate > today)
                {
                    ModelState.AddModelError(
                        "Entry.ProgressDate",
                        "Progress date cannot be in the future."
                    );
                }
            }


            // -----------------------------------------------------------
            // Get latest progress
            // -----------------------------------------------------------

            var latestProgress = db.ProjectProgressRecords
                .Where(p => p.ProjectID == project.ProjectID)
                .OrderByDescending(p => p.ProgressDate)
                .ThenByDescending(p => p.DateRecorded)
                .FirstOrDefault();


            // -----------------------------------------------------------
            // Prevent progress from moving backwards
            // -----------------------------------------------------------

            if (latestProgress != null &&
                model.Entry.ProgressPercentage <
                latestProgress.ProgressPercentage)
            {
                ModelState.AddModelError(
                    "Entry.ProgressPercentage",
                    "New progress cannot be lower than the project's latest recorded progress of "
                    + latestProgress.ProgressPercentage + "%."
                );
            }


            // -----------------------------------------------------------
            // Return page with validation errors
            // -----------------------------------------------------------

            if (!ModelState.IsValid)
            {
                var invalidModel =
                    BuildProgressViewModel(
                        project.ProjectID,
                        model.Entry
                    );

                return View(
                    "Progress",
                    invalidModel
                );
            }


            // -----------------------------------------------------------
            // Create progress record
            // -----------------------------------------------------------

            var progress = new ProjectProgress
            {
                ProjectID =
                    project.ProjectID,

                ProgressPercentage =
                    model.Entry.ProgressPercentage,

                ProgressSummary =
                    model.Entry.ProgressSummary,

                CurrentActivity =
                    model.Entry.CurrentActivity,

                IssuesEncountered =
                    model.Entry.IssuesEncountered,

                NextPlannedActivity =
                    model.Entry.NextPlannedActivity,

                ProgressDate =
                    model.Entry.ProgressDate.Value.Date,

                DateRecorded =
                    DateTime.Now,

                RecordedByAdministratorID =
                    administratorID.Value
            };


            db.ProjectProgressRecords.Add(progress);


            // -----------------------------------------------------------
            // Update project audit information
            // -----------------------------------------------------------

            project.LastUpdatedDate =
                DateTime.Now;

            project.LastUpdatedByAdministratorID =
                administratorID.Value;


            db.SaveChanges();


            TempData["SuccessMessage"] =
                "Project progress has been recorded successfully.";


            return RedirectToAction(
                "Progress",
                new
                {
                    id = project.ProjectID
                }
            );
        }


        private MunicipalProjectProgressViewModel BuildProgressViewModel(
            int projectID,
            ProjectProgressEntryViewModel entry = null)
        {
            var project = db.MunicipalProjects
                .AsNoTracking()
                .Include(p => p.Ward)
                .FirstOrDefault(
                    p => p.ProjectID == projectID
                );

            if (project == null)
                return null;


            var progressRecords = db.ProjectProgressRecords
                .AsNoTracking()
                .Include(p => p.RecordedByAdministrator)
                .Where(
                    p => p.ProjectID == projectID
                )
                .OrderByDescending(
                    p => p.ProgressDate
                )
                .ThenByDescending(
                    p => p.DateRecorded
                )
                .ToList();


            var latestProgress =
                progressRecords.FirstOrDefault();


            var model =
                new MunicipalProjectProgressViewModel
                {
                    ProjectID =
                        project.ProjectID,

                    ProjectCode =
                        project.ProjectCode,

                    ProjectName =
                        project.ProjectName,

                    ProjectType =
                        project.ProjectType,

                    ProjectLocation =
                        project.ProjectLocation,

                    WardName =
                        project.Ward != null
                            ? "Ward " + project.Ward.WardNumber
                            : "Not specified",

                    Status =
                        project.Status.ToString(),

                    Priority =
                        project.Priority.ToString(),

                    StartDate =
                        project.StartDate,

                    ExpectedCompletionDate =
                        project.ExpectedCompletionDate,

                    CurrentProgressPercentage =
                        latestProgress != null
                            ? latestProgress.ProgressPercentage
                            : 0,

                    LatestProgressDate =
                        latestProgress != null
                            ? latestProgress.ProgressDate
                            : (DateTime?)null,

                    LatestProgressSummary =
                        latestProgress != null
                            ? latestProgress.ProgressSummary
                            : null,

                    LatestCurrentActivity =
                        latestProgress != null
                            ? latestProgress.CurrentActivity
                            : null,

                    LatestNextPlannedActivity =
                        latestProgress != null
                            ? latestProgress.NextPlannedActivity
                            : null,

                    LatestRecordedBy =
                        latestProgress != null &&
                        latestProgress.RecordedByAdministrator != null
                            ? latestProgress.RecordedByAdministrator.FirstName
                                + " "
                                + latestProgress.RecordedByAdministrator.LastName
                            : null
                };


            // -----------------------------------------------------------
            // Progress history
            // -----------------------------------------------------------

            foreach (var record in progressRecords)
            {
                model.ProgressRecords.Add(
                    new ProjectProgressItemViewModel
                    {
                        ProjectProgressID =
                            record.ProjectProgressID,

                        ProgressPercentage =
                            record.ProgressPercentage,

                        ProgressSummary =
                            record.ProgressSummary,

                        CurrentActivity =
                            record.CurrentActivity,

                        IssuesEncountered =
                            record.IssuesEncountered,

                        NextPlannedActivity =
                            record.NextPlannedActivity,

                        ProgressDate =
                            record.ProgressDate,

                        DateRecorded =
                            record.DateRecorded,

                        RecordedByAdministratorName =
                            record.RecordedByAdministrator != null
                                ? record.RecordedByAdministrator.FirstName
                                    + " "
                                    + record.RecordedByAdministrator.LastName
                                : "Administrator"
                    }
                );
            }


            // -----------------------------------------------------------
            // New progress entry
            // -----------------------------------------------------------

            if (entry != null)
            {
                model.Entry = entry;

                model.Entry.ProjectID =
                    projectID;
            }
            else
            {
                model.Entry.ProjectID =
                    projectID;

                model.Entry.ProgressPercentage =
                    latestProgress != null
                        ? latestProgress.ProgressPercentage
                        : 0;

                model.Entry.ProgressDate =
                    DateTime.Today;
            }


            return model;
        }

        [HttpGet]
        public ActionResult UpdateStatus(int? id)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            if (!id.HasValue)
                return HttpNotFound();

            var project = db.MunicipalProjects
                .Include(p => p.Ward)
                .Include(p => p.LastUpdatedByAdministrator)
                .FirstOrDefault(p => p.ProjectID == id.Value);

            if (project == null)
                return HttpNotFound();

            var model = BuildProjectStatusViewModel(project);

            return View(model);
        }

        private bool IsValidProjectStatusTransition(
    MunicipalProjectStatus currentStatus,
    MunicipalProjectStatus newStatus)
        {
            switch (currentStatus)
            {
                case MunicipalProjectStatus.Planned:

                    return newStatus ==
                               MunicipalProjectStatus.Approved
                           ||
                           newStatus ==
                               MunicipalProjectStatus.Cancelled;


                case MunicipalProjectStatus.Approved:

                    return newStatus ==
                               MunicipalProjectStatus.InProgress
                           ||
                           newStatus ==
                               MunicipalProjectStatus.OnHold
                           ||
                           newStatus ==
                               MunicipalProjectStatus.Cancelled;


                case MunicipalProjectStatus.InProgress:

                    return newStatus ==
                               MunicipalProjectStatus.OnHold
                           ||
                           newStatus ==
                               MunicipalProjectStatus.Cancelled;


                case MunicipalProjectStatus.OnHold:

                    return newStatus ==
                               MunicipalProjectStatus.InProgress
                           ||
                           newStatus ==
                               MunicipalProjectStatus.Cancelled;


                case MunicipalProjectStatus.Completed:

                    return false;


                case MunicipalProjectStatus.Cancelled:

                    return false;


                case MunicipalProjectStatus.Closed:

                    return false;


                default:

                    return false;
            }
        }

        private MunicipalProjectStatusViewModel BuildProjectStatusViewModel(
    MunicipalProject project,
    string statusChangeReason = null)
        {
            var model =
                new MunicipalProjectStatusViewModel
                {
                    ProjectID =
                        project.ProjectID,

                    ProjectCode =
                        project.ProjectCode,

                    ProjectName =
                        project.ProjectName,

                    ProjectType =
                        project.ProjectType,

                    ProjectLocation =
                        project.ProjectLocation,

                    WardName =
                        project.Ward != null
                            ? "Ward " + project.Ward.WardNumber
                            : "Not specified",

                    CurrentStatus =
                        project.Status,

                    NewStatus =
                        project.Status,

                    StatusChangeReason =
                        statusChangeReason,

                    LastUpdatedDate =
                        project.LastUpdatedDate,

                    LastUpdatedByAdministratorName =
                        project.LastUpdatedByAdministrator != null
                            ? project.LastUpdatedByAdministrator.FirstName
                                + " "
                                + project.LastUpdatedByAdministrator.LastName
                            : null
                };


            var availableStatuses =
                GetAvailableProjectStatuses(
                    project.Status);


            model.AvailableStatuses =
                availableStatuses;


            return model;
        }

        private List<MunicipalProjectStatusOptionViewModel>
    GetAvailableProjectStatuses(
        MunicipalProjectStatus currentStatus)
        {
            var statuses =
                new List<MunicipalProjectStatusOptionViewModel>();


            switch (currentStatus)
            {
                case MunicipalProjectStatus.Planned:

                    statuses.Add(
                        new MunicipalProjectStatusOptionViewModel
                        {
                            Status =
                                MunicipalProjectStatus.Approved,

                            DisplayName =
                                "Approved",

                            Description =
                                "The project has been formally approved and can proceed to implementation."
                        });

                    statuses.Add(
                        new MunicipalProjectStatusOptionViewModel
                        {
                            Status =
                                MunicipalProjectStatus.Cancelled,

                            DisplayName =
                                "Cancelled",

                            Description =
                                "The project has been cancelled and will not proceed."
                        });

                    break;


                case MunicipalProjectStatus.Approved:

                    statuses.Add(
                        new MunicipalProjectStatusOptionViewModel
                        {
                            Status =
                                MunicipalProjectStatus.InProgress,

                            DisplayName =
                                "In Progress",

                            Description =
                                "Project activities are actively being carried out."
                        });

                    statuses.Add(
                        new MunicipalProjectStatusOptionViewModel
                        {
                            Status =
                                MunicipalProjectStatus.OnHold,

                            DisplayName =
                                "On Hold",

                            Description =
                                "Project activities are temporarily paused."
                        });

                    statuses.Add(
                        new MunicipalProjectStatusOptionViewModel
                        {
                            Status =
                                MunicipalProjectStatus.Cancelled,

                            DisplayName =
                                "Cancelled",

                            Description =
                                "The project has been cancelled and will not proceed."
                        });

                    break;


                case MunicipalProjectStatus.InProgress:

                    statuses.Add(
                        new MunicipalProjectStatusOptionViewModel
                        {
                            Status =
                                MunicipalProjectStatus.OnHold,

                            DisplayName =
                                "On Hold",

                            Description =
                                "Project activities are temporarily paused."
                        });

                    statuses.Add(
                        new MunicipalProjectStatusOptionViewModel
                        {
                            Status =
                                MunicipalProjectStatus.Cancelled,

                            DisplayName =
                                "Cancelled",

                            Description =
                                "The project has been cancelled and will not proceed."
                        });

                    break;


                case MunicipalProjectStatus.OnHold:

                    statuses.Add(
                        new MunicipalProjectStatusOptionViewModel
                        {
                            Status =
                                MunicipalProjectStatus.InProgress,

                            DisplayName =
                                "In Progress",

                            Description =
                                "Project activities are resuming."
                        });

                    statuses.Add(
                        new MunicipalProjectStatusOptionViewModel
                        {
                            Status =
                                MunicipalProjectStatus.Cancelled,

                            DisplayName =
                                "Cancelled",

                            Description =
                                "The project has been cancelled and will not proceed."
                        });

                    break;
            }


            return statuses;
        }

        // ================================================================
        // US99 - UPDATE PROJECT STATUS
        // POST
        // ================================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(
            MunicipalProjectStatusViewModel model)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            if (model == null)
                return HttpNotFound();

            var administratorID = GetAdministratorID();

            if (!administratorID.HasValue)
                return new HttpUnauthorizedResult();

            var project = db.MunicipalProjects
                .Include(p => p.Ward)
                .Include(p => p.LastUpdatedByAdministrator)
                .FirstOrDefault(
                    p => p.ProjectID == model.ProjectID
                );

            if (project == null)
                return HttpNotFound();

            // ------------------------------------------------------------
            // Clean submitted values
            // ------------------------------------------------------------

            model.StatusChangeReason =
                model.StatusChangeReason?.Trim();

            // Always use the actual database status.
            model.CurrentStatus =
                project.Status;


            // ------------------------------------------------------------
            // Validate that a different status was selected
            // ------------------------------------------------------------

            if (model.NewStatus == project.Status)
            {
                ModelState.AddModelError(
                    "NewStatus",
                    "The selected status is already the project's current status."
                );
            }


            // ------------------------------------------------------------
            // Validate lifecycle transition
            // ------------------------------------------------------------

            if (!IsValidProjectStatusTransition(
                project.Status,
                model.NewStatus))
            {
                ModelState.AddModelError(
                    "NewStatus",
                    "The project cannot be changed from "
                    + project.Status
                    + " to "
                    + model.NewStatus
                    + " using the current project lifecycle."
                );
            }


            // ------------------------------------------------------------
            // Prevent updates to terminal statuses
            // ------------------------------------------------------------

            if (project.Status == MunicipalProjectStatus.Cancelled)
            {
                ModelState.AddModelError(
                    "",
                    "A cancelled project cannot be given another status."
                );
            }

            if (project.Status == MunicipalProjectStatus.Completed)
            {
                ModelState.AddModelError(
                    "",
                    "A completed project cannot be changed using this function."
                );
            }

            if (project.Status == MunicipalProjectStatus.Closed)
            {
                ModelState.AddModelError(
                    "",
                    "A closed project cannot be changed."
                );
            }


            // ------------------------------------------------------------
            // Validate reason
            // ------------------------------------------------------------

            if (string.IsNullOrWhiteSpace(
                model.StatusChangeReason))
            {
                ModelState.AddModelError(
                    "StatusChangeReason",
                    "Please provide a reason for the status change."
                );
            }
            else if (
                model.StatusChangeReason.Length < 10)
            {
                ModelState.AddModelError(
                    "StatusChangeReason",
                    "The status change reason must contain at least 10 characters."
                );
            }


            // ------------------------------------------------------------
            // If invalid, rebuild the page and return it
            // ------------------------------------------------------------

            if (!ModelState.IsValid)
            {
                var invalidModel =
                    BuildProjectStatusViewModel(
                        project,
                        model.StatusChangeReason
                    );

                invalidModel.NewStatus =
                    model.NewStatus;

                return View(
                    "UpdateStatus",
                    invalidModel
                );
            }


            // ------------------------------------------------------------
            // Store previous status before changing it
            // ------------------------------------------------------------

            var previousStatus =
                project.Status;


            // ------------------------------------------------------------
            // Update project
            // ------------------------------------------------------------

            project.Status =
                model.NewStatus;

            project.LastUpdatedDate =
                DateTime.Now;

            project.LastUpdatedByAdministratorID =
                administratorID.Value;


            // ------------------------------------------------------------
            // Record status change in project history
            // ------------------------------------------------------------

            var history =
                new ProjectHistory
                {
                    ProjectID =
                        project.ProjectID,

                    ActionType =
                        "Status Updated",

                    Description =
                        model.StatusChangeReason,

                    PreviousStatus =
                        previousStatus.ToString(),

                    NewStatus =
                        model.NewStatus.ToString(),

                    ActionDate =
                        DateTime.Now,

                    PerformedByAdministratorID =
                        administratorID.Value
                };

            db.ProjectHistoryRecords.Add(history);


            // ------------------------------------------------------------
            // Save everything
            // ------------------------------------------------------------

            db.SaveChanges();


            // ------------------------------------------------------------
            // Success message
            // ------------------------------------------------------------

            TempData["SuccessMessage"] =
                "Project status has been updated successfully from "
                + previousStatus
                + " to "
                + model.NewStatus
                + ".";


            // ------------------------------------------------------------
            // Return to Project Details
            // ------------------------------------------------------------

            return RedirectToAction(
                "Details",
                new
                {
                    id = project.ProjectID
                }
            );
        }

        // ================================================================
        // US100 - RECORD PROJECT EVIDENCE
        // GET
        // ================================================================

        [HttpGet]
        public ActionResult RecordEvidence(int? id)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            if (!id.HasValue)
                return HttpNotFound();

            var project = db.MunicipalProjects
                .Include(p => p.Ward)
                .FirstOrDefault(
                    p => p.ProjectID == id.Value
                );

            if (project == null)
                return HttpNotFound();

            var model =
                BuildProjectEvidenceViewModel(
                    project.ProjectID
                );

            return View(model);
        }

        // ================================================================
        // US100 - RECORD PROJECT EVIDENCE
        // POST
        // ================================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RecordEvidence(
            MunicipalProjectEvidenceViewModel model)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            if (model == null || model.Entry == null)
                return HttpNotFound();

            var administratorID =
                GetAdministratorID();

            if (!administratorID.HasValue)
                return new HttpUnauthorizedResult();


            // ------------------------------------------------------------
            // Get project
            // ------------------------------------------------------------

            var project = db.MunicipalProjects
                .Include(p => p.Ward)
                .FirstOrDefault(
                    p => p.ProjectID == model.ProjectID
                );

            if (project == null)
                return HttpNotFound();


            // ------------------------------------------------------------
            // Clean submitted values
            // ------------------------------------------------------------

            model.Entry.EvidenceTitle =
                model.Entry.EvidenceTitle?.Trim();

            model.Entry.Description =
                model.Entry.Description?.Trim();

            model.Entry.EvidenceType =
                model.Entry.EvidenceType?.Trim();


            // ------------------------------------------------------------
            // Validate project status
            // ------------------------------------------------------------

            if (project.Status ==
                MunicipalProjectStatus.Cancelled)
            {
                ModelState.AddModelError(
                    "",
                    "Evidence cannot be recorded for a cancelled project."
                );
            }

            if (project.Status ==
                MunicipalProjectStatus.Closed)
            {
                ModelState.AddModelError(
                    "",
                    "Evidence cannot be recorded for a closed project."
                );
            }


            // ------------------------------------------------------------
            // Validate evidence type
            // ------------------------------------------------------------

            var allowedEvidenceTypes =
                new[]
                {
            "Site Photograph",
            "Progress Report",
            "Inspection Report",
            "Completion Certificate",
            "Contractor Document",
            "Meeting Record",
            "Other"
                };


            if (!allowedEvidenceTypes.Contains(
                model.Entry.EvidenceType))
            {
                ModelState.AddModelError(
                    "Entry.EvidenceType",
                    "Please select a valid evidence type."
                );
            }


            // ------------------------------------------------------------
            // Get uploaded file
            // ------------------------------------------------------------

            var file =
                Request.Files["EvidenceFile"];


            // ------------------------------------------------------------
            // Validate uploaded file
            // ------------------------------------------------------------

            if (file == null ||
                file.ContentLength <= 0)
            {
                ModelState.AddModelError(
                    "EvidenceFile",
                    "Please select an evidence file."
                );
            }
            else
            {
                const int maxFileSize =
                    10 * 1024 * 1024;

                if (file.ContentLength >
                    maxFileSize)
                {
                    ModelState.AddModelError(
                        "EvidenceFile",
                        "Evidence files cannot exceed 10 MB."
                    );
                }


                var extension =
                    System.IO.Path
                        .GetExtension(file.FileName)
                        ?.ToLowerInvariant();


                var allowedExtensions =
                    new[]
                    {
                ".jpg",
                ".jpeg",
                ".png",
                ".pdf",
                ".doc",
                ".docx",
                ".xls",
                ".xlsx"
                    };


                if (string.IsNullOrWhiteSpace(extension) ||
                    !allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        "EvidenceFile",
                        "Only JPG, JPEG, PNG, PDF, DOC, DOCX, XLS and XLSX files are allowed."
                    );
                }
            }


            // ------------------------------------------------------------
            // Return page if validation fails
            // ------------------------------------------------------------

            if (!ModelState.IsValid)
            {
                var invalidModel =
                    BuildProjectEvidenceViewModel(
                        project.ProjectID,
                        model.Entry
                    );

                return View(
                    "RecordEvidence",
                    invalidModel
                );
            }


            // ------------------------------------------------------------
            // Create upload directory
            // ------------------------------------------------------------

            var uploadDirectory =
                Server.MapPath(
                    "~/Uploads/ProjectEvidence"
                );

            if (!System.IO.Directory.Exists(
                uploadDirectory))
            {
                System.IO.Directory.CreateDirectory(
                    uploadDirectory
                );
            }


            // ------------------------------------------------------------
            // Generate safe stored filename
            // ------------------------------------------------------------

            var originalFileName =
                System.IO.Path.GetFileName(
                    file.FileName
                );

            var extensionName =
                System.IO.Path.GetExtension(
                    originalFileName
                );


            var storedFileName =
                "PROJECT_"
                + project.ProjectID
                + "_"
                + Guid.NewGuid().ToString("N")
                + extensionName;


            var physicalFilePath =
                System.IO.Path.Combine(
                    uploadDirectory,
                    storedFileName
                );


            // ------------------------------------------------------------
            // Save physical file
            // ------------------------------------------------------------

            file.SaveAs(
                physicalFilePath
            );


            // ------------------------------------------------------------
            // Create database evidence record
            // ------------------------------------------------------------

            var evidence =
                new ProjectEvidence
                {
                    ProjectID =
                        project.ProjectID,

                    EvidenceTitle =
                        model.Entry.EvidenceTitle,

                    Description =
                        model.Entry.Description,

                    EvidenceType =
                        model.Entry.EvidenceType,

                    FileName =
                        originalFileName,

                    FilePath =
                        "~/Uploads/ProjectEvidence/"
                        + storedFileName,

                    ContentType =
                        file.ContentType,

                    FileSize =
                        file.ContentLength,

                    DateRecorded =
                        DateTime.Now,

                    RecordedByAdministratorID =
                        administratorID.Value
                };


            db.ProjectEvidenceRecords.Add(
                evidence
            );


            // ------------------------------------------------------------
            // Update project audit fields
            // ------------------------------------------------------------

            project.LastUpdatedDate =
                DateTime.Now;

            project.LastUpdatedByAdministratorID =
                administratorID.Value;


            // ------------------------------------------------------------
            // Save
            // ------------------------------------------------------------

            db.SaveChanges();


            // ------------------------------------------------------------
            // Success
            // ------------------------------------------------------------

            TempData["SuccessMessage"] =
                "Project evidence has been recorded successfully.";


            return RedirectToAction(
                "RecordEvidence",
                new
                {
                    id = project.ProjectID
                }
            );
        }

        

        private MunicipalProjectEvidenceViewModel
    BuildProjectEvidenceViewModel(
        int projectID,
        ProjectEvidenceEntryViewModel entry = null)
        {
            var project = db.MunicipalProjects
                .AsNoTracking()
                .Include(p => p.Ward)
                .FirstOrDefault(
                    p => p.ProjectID == projectID
                );

            if (project == null)
                return null;

            var records =
                db.ProjectEvidenceRecords
                    .AsNoTracking()
                    .Include(e => e.RecordedByAdministrator)
                    .Where(e => e.ProjectID == projectID)
                    .OrderByDescending(e => e.DateRecorded)
                    .ToList();

            var model =
                new MunicipalProjectEvidenceViewModel
                {
                    ProjectID = project.ProjectID,
                    ProjectCode = project.ProjectCode,
                    ProjectName = project.ProjectName,
                    ProjectType = project.ProjectType,
                    ProjectLocation = project.ProjectLocation,

                    WardName = project.Ward != null
                        ? "Ward " + project.Ward.WardNumber
                        : "Not specified",

                    Status = project.Status.ToString(),
                    Priority = project.Priority.ToString()
                };

            foreach (var record in records)
            {
                model.EvidenceRecords.Add(
                    new ProjectEvidenceItemViewModel
                    {
                        ProjectEvidenceID =
                            record.ProjectEvidenceID,

                        EvidenceTitle =
                            record.EvidenceTitle,

                        Description =
                            record.Description,

                        EvidenceType =
                            record.EvidenceType,

                        FileName =
                            record.FileName,

                        FilePath =
                            record.FilePath,

                        ContentType =
                            record.ContentType,

                        FileSize =
                            record.FileSize,

                        DateRecorded =
                            record.DateRecorded,

                        RecordedByAdministratorName =
                            record.RecordedByAdministrator != null
                                ? record.RecordedByAdministrator.FirstName
                                    + " "
                                    + record.RecordedByAdministrator.LastName
                                : "Administrator"
                    }
                );
            }

            if (entry != null)
            {
                model.Entry = entry;
                model.Entry.ProjectID = projectID;
            }
            else
            {
                model.Entry.ProjectID = projectID;
            }

            return model;
        }

        // ============================================================
        // US101 - MONITOR PROJECT MILESTONES
        // ============================================================

        [HttpGet]
        public ActionResult MonitorMilestones(int? id)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            if (!id.HasValue)
                return HttpNotFound();

            var model =
                BuildMilestoneMonitoringViewModel(
                    id.Value
                );

            if (model == null)
                return HttpNotFound();

            return View(model);
        }

        private MunicipalProjectMilestoneMonitoringViewModel
    BuildMilestoneMonitoringViewModel(int projectID)
        {
            var project = db.MunicipalProjects
                .AsNoTracking()
                .Include(p => p.Ward)
                .FirstOrDefault(
                    p => p.ProjectID == projectID
                );

            if (project == null)
                return null;

            var milestones = db.ProjectMilestones
                .AsNoTracking()
                .Where(
                    m => m.ProjectID == projectID
                )
                .OrderBy(m => m.SequenceNumber)
                .ThenBy(m => m.PlannedCompletionDate)
                .ToList();

            var today = DateTime.Today;

            var model =
                new MunicipalProjectMilestoneMonitoringViewModel
                {
                    ProjectID =
                        project.ProjectID,

                    ProjectCode =
                        project.ProjectCode,

                    ProjectName =
                        project.ProjectName,

                    ProjectType =
                        project.ProjectType,

                    ProjectLocation =
                        project.ProjectLocation,

                    WardName =
                        project.Ward != null
                            ? "Ward " + project.Ward.WardNumber
                            : "Not specified",

                    ProjectStatus =
                        project.Status.ToString(),

                    ProjectPriority =
                        project.Priority.ToString(),

                    TotalMilestones =
                        milestones.Count,

                    CompletedMilestones =
                        milestones.Count(
                            m => m.Status ==
                                 ProjectMilestoneStatus.Completed
                        ),

                    InProgressMilestones =
                        milestones.Count(
                            m => m.Status ==
                                 ProjectMilestoneStatus.InProgress
                        ),

                    NotStartedMilestones =
                        milestones.Count(
                            m => m.Status ==
                                 ProjectMilestoneStatus.NotStarted
                        ),

                    DelayedMilestones =
                        milestones.Count(
                            m => m.Status ==
                                 ProjectMilestoneStatus.Delayed
                        )
                };

            model.IncompleteMilestones =
                milestones.Count(
                    m =>
                        m.Status !=
                        ProjectMilestoneStatus.Completed
                        &&
                        m.Status !=
                        ProjectMilestoneStatus.Cancelled
                );

            model.OverdueMilestones =
                milestones.Count(
                    m =>
                        m.Status !=
                        ProjectMilestoneStatus.Completed
                        &&
                        m.Status !=
                        ProjectMilestoneStatus.Cancelled
                        &&
                        m.PlannedCompletionDate.Date < today
                );

            if (milestones.Any())
            {
                model.OverallProgress =
                    Math.Round(
                        (decimal)milestones
                            .Average(
                                m => m.ProgressPercentage
                            ),
                        1
                    );
            }
            else
            {
                model.OverallProgress = 0;
            }

            foreach (var milestone in milestones)
            {
                bool isCompleted =
                    milestone.Status ==
                    ProjectMilestoneStatus.Completed;

                bool isCancelled =
                    milestone.Status ==
                    ProjectMilestoneStatus.Cancelled;

                bool isOverdue =
                    !isCompleted
                    &&
                    !isCancelled
                    &&
                    milestone.PlannedCompletionDate.Date < today;

                bool isIncomplete =
                    !isCompleted
                    &&
                    !isCancelled;

                int daysRemaining = 0;
                int daysOverdue = 0;

                if (isCompleted)
                {
                    daysRemaining = 0;
                    daysOverdue = 0;
                }
                else if (isOverdue)
                {
                    daysOverdue =
                        (today -
                         milestone.PlannedCompletionDate.Date)
                        .Days;
                }
                else
                {
                    daysRemaining =
                        (milestone.PlannedCompletionDate.Date -
                         today)
                        .Days;
                }

                string monitoringStatus;

                if (isCancelled)
                {
                    monitoringStatus = "Cancelled";
                }
                else if (isCompleted)
                {
                    monitoringStatus = "Completed";
                }
                else if (milestone.Status ==
                         ProjectMilestoneStatus.Delayed)
                {
                    monitoringStatus = "Delayed";
                }
                else if (isOverdue)
                {
                    monitoringStatus = "Overdue";
                }
                else if (milestone.Status ==
                         ProjectMilestoneStatus.InProgress)
                {
                    monitoringStatus = "On Track";
                }
                else
                {
                    monitoringStatus = "Not Started";
                }

                model.Milestones.Add(
                    new ProjectMilestoneMonitoringItemViewModel
                    {
                        ProjectMilestoneID =
                            milestone.ProjectMilestoneID,

                        MilestoneName =
                            milestone.MilestoneName,

                        Description =
                            milestone.Description,

                        SequenceNumber =
                            milestone.SequenceNumber,

                        PlannedStartDate =
                            milestone.PlannedStartDate,

                        PlannedCompletionDate =
                            milestone.PlannedCompletionDate,

                        ActualCompletionDate =
                            milestone.ActualCompletionDate,

                        Status =
                            milestone.Status.ToString(),

                        ProgressPercentage =
                            milestone.ProgressPercentage,

                        Priority =
                            milestone.Priority.ToString(),

                        IsOverdue =
                            isOverdue,

                        IsIncomplete =
                            isIncomplete,

                        MonitoringStatus =
                            monitoringStatus,

                        DaysRemaining =
                            daysRemaining,

                        DaysOverdue =
                            daysOverdue
                    }
                );
            }

            return model;
        }

        // ============================================================
        // US102 - COMPLETE MUNICIPAL PROJECT
        // ============================================================

        [HttpGet]
        public ActionResult CompleteProject(int? id)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            if (!id.HasValue)
                return HttpNotFound();

            var project = db.MunicipalProjects
                .AsNoTracking()
                .Include(p => p.Ward)
                .FirstOrDefault(
                    p => p.ProjectID == id.Value
                );

            if (project == null)
                return HttpNotFound();

            if (project.Status == MunicipalProjectStatus.Cancelled)
            {
                TempData["ErrorMessage"] =
                    "A cancelled project cannot be completed.";

                return RedirectToAction(
                    "Details",
                    new { id = project.ProjectID }
                );
            }

            if (project.Status == MunicipalProjectStatus.Completed)
            {
                TempData["ErrorMessage"] =
                    "This project has already been completed.";

                return RedirectToAction(
                    "Details",
                    new { id = project.ProjectID }
                );
            }

            if (project.Status == MunicipalProjectStatus.Closed)
            {
                TempData["ErrorMessage"] =
                    "A closed project cannot be completed.";

                return RedirectToAction(
                    "Details",
                    new { id = project.ProjectID }
                );
            }

            if (project.Status != MunicipalProjectStatus.InProgress)
            {
                TempData["ErrorMessage"] =
                    "Only projects currently in progress can be completed.";

                return RedirectToAction(
                    "Details",
                    new { id = project.ProjectID }
                );
            }

            var milestones = db.ProjectMilestones
                .AsNoTracking()
                .Where(
                    m => m.ProjectID == project.ProjectID
                )
                .ToList();

            var model =
                new MunicipalProjectCompletionViewModel
                {
                    ProjectID =
                        project.ProjectID,

                    ProjectCode =
                        project.ProjectCode,

                    ProjectName =
                        project.ProjectName,

                    ProjectType =
                        project.ProjectType,

                    ProjectLocation =
                        project.ProjectLocation,

                    WardName =
                        project.Ward != null
                            ? "Ward " + project.Ward.WardNumber
                            : "Not specified",

                    CurrentStatus =
                        project.Status.ToString(),

                    Priority =
                        project.Priority.ToString(),

                    TotalMilestones =
                        milestones.Count,

                    CompletedMilestones =
                        milestones.Count(
                            m =>
                                m.Status ==
                                ProjectMilestoneStatus.Completed
                        ),

                    IncompleteMilestones =
                        milestones.Count(
                            m =>
                                m.Status !=
                                ProjectMilestoneStatus.Completed
                                &&
                                m.Status !=
                                ProjectMilestoneStatus.Cancelled
                        ),

                    OverallProgress =
                        milestones.Any()
                            ? Math.Round(
                                (decimal)milestones.Average(
                                    m => m.ProgressPercentage
                                ),
                                1
                            )
                            : 0,

                    ActualCompletionDate =
                        DateTime.Today
                };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CompleteProject(
    MunicipalProjectCompletionViewModel model)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            if (model == null)
                return HttpNotFound();

            var administratorID = GetAdministratorID();

            if (!administratorID.HasValue)
                return new HttpUnauthorizedResult();

            model.CompletionSummary =
                model.CompletionSummary?.Trim();

            var project = db.MunicipalProjects
                .Include(p => p.Ward)
                .FirstOrDefault(
                    p => p.ProjectID == model.ProjectID
                );

            if (project == null)
                return HttpNotFound();

            if (project.Status == MunicipalProjectStatus.Cancelled)
            {
                ModelState.AddModelError(
                    "",
                    "A cancelled project cannot be completed."
                );
            }

            if (project.Status == MunicipalProjectStatus.Completed)
            {
                ModelState.AddModelError(
                    "",
                    "This project has already been completed."
                );
            }

            if (project.Status == MunicipalProjectStatus.Closed)
            {
                ModelState.AddModelError(
                    "",
                    "A closed project cannot be completed."
                );
            }

            if (project.Status != MunicipalProjectStatus.InProgress)
            {
                ModelState.AddModelError(
                    "",
                    "Only projects currently in progress can be completed."
                );
            }

            if (model.ActualCompletionDate.Date > DateTime.Today)
            {
                ModelState.AddModelError(
                    "ActualCompletionDate",
                    "Actual completion date cannot be in the future."
                );
            }

            if (model.ActualCompletionDate.Date <
                project.StartDate.Date)
            {
                ModelState.AddModelError(
                    "ActualCompletionDate",
                    "Actual completion date cannot be before the project start date."
                );
            }

            var milestones = db.ProjectMilestones
                .AsNoTracking()
                .Where(
                    m => m.ProjectID == project.ProjectID
                )
                .ToList();

            model.ProjectCode =
                project.ProjectCode;

            model.ProjectName =
                project.ProjectName;

            model.ProjectType =
                project.ProjectType;

            model.ProjectLocation =
                project.ProjectLocation;

            model.WardName =
                project.Ward != null
                    ? "Ward " + project.Ward.WardNumber
                    : "Not specified";

            model.CurrentStatus =
                project.Status.ToString();

            model.Priority =
                project.Priority.ToString();

            model.TotalMilestones =
                milestones.Count;

            model.CompletedMilestones =
                milestones.Count(
                    m =>
                        m.Status ==
                        ProjectMilestoneStatus.Completed
                );

            model.IncompleteMilestones =
                milestones.Count(
                    m =>
                        m.Status !=
                        ProjectMilestoneStatus.Completed
                        &&
                        m.Status !=
                        ProjectMilestoneStatus.Cancelled
                );

            model.OverallProgress =
                milestones.Any()
                    ? Math.Round(
                        (decimal)milestones.Average(
                            m => m.ProgressPercentage
                        ),
                        1
                    )
                    : 0;

            if (!ModelState.IsValid)
                return View(model);

            var previousStatus =
                project.Status.ToString();

            project.Status =
                MunicipalProjectStatus.Completed;

            project.ActualCompletionDate =
                model.ActualCompletionDate.Date;

            project.LastUpdatedDate =
                DateTime.Now;

            project.LastUpdatedByAdministratorID =
                administratorID.Value;

            var history =
                new ProjectHistory
                {
                    ProjectID =
                        project.ProjectID,

                    ActionType =
                        "Project Completed",

                    Description =
                        model.CompletionSummary,

                    PreviousStatus =
                        previousStatus,

                    NewStatus =
                        MunicipalProjectStatus.Completed.ToString(),

                    ActionDate =
                        DateTime.Now,

                    PerformedByAdministratorID =
                        administratorID.Value
                };

            db.ProjectHistoryRecords.Add(history);

            db.SaveChanges();

            TempData["SuccessMessage"] =
                "Project has been marked as completed successfully.";

            return RedirectToAction(
                "Details",
                new
                {
                    id = project.ProjectID
                }
            );
        }


        // ============================================================
        // US103 - CLOSE MUNICIPAL PROJECT
        // ============================================================

        [HttpGet]
        public ActionResult CloseProject(int? id)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            if (!id.HasValue)
                return HttpNotFound();

            var project = db.MunicipalProjects
                .AsNoTracking()
                .Include(p => p.Ward)
                .FirstOrDefault(
                    p => p.ProjectID == id.Value
                );

            if (project == null)
                return HttpNotFound();

            if (project.Status == MunicipalProjectStatus.Closed)
            {
                TempData["ErrorMessage"] =
                    "This project has already been closed.";

                return RedirectToAction(
                    "Details",
                    new { id = project.ProjectID }
                );
            }

            if (project.Status != MunicipalProjectStatus.Completed)
            {
                TempData["ErrorMessage"] =
                    "Only completed projects can be closed.";

                return RedirectToAction(
                    "Details",
                    new { id = project.ProjectID }
                );
            }

            var model =
                new MunicipalProjectClosureViewModel
                {
                    ProjectID =
                        project.ProjectID,

                    ProjectCode =
                        project.ProjectCode,

                    ProjectName =
                        project.ProjectName,

                    ProjectType =
                        project.ProjectType,

                    ProjectLocation =
                        project.ProjectLocation,

                    WardName =
                        project.Ward != null
                            ? "Ward " + project.Ward.WardNumber
                            : "Not specified",

                    CurrentStatus =
                        project.Status.ToString(),

                    Priority =
                        project.Priority.ToString(),

                    ActualCompletionDate =
                        project.ActualCompletionDate
                };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CloseProject(
    MunicipalProjectClosureViewModel model)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            if (model == null)
                return HttpNotFound();

            var administratorID =
                GetAdministratorID();

            if (!administratorID.HasValue)
                return new HttpUnauthorizedResult();

            model.ClosureSummary =
                model.ClosureSummary?.Trim();

            var project = db.MunicipalProjects
                .Include(p => p.Ward)
                .FirstOrDefault(
                    p => p.ProjectID == model.ProjectID
                );

            if (project == null)
                return HttpNotFound();

            if (project.Status == MunicipalProjectStatus.Closed)
            {
                ModelState.AddModelError(
                    "",
                    "This project has already been closed."
                );
            }

            if (project.Status != MunicipalProjectStatus.Completed)
            {
                ModelState.AddModelError(
                    "",
                    "Only completed projects can be closed."
                );
            }

            model.ProjectCode =
                project.ProjectCode;

            model.ProjectName =
                project.ProjectName;

            model.ProjectType =
                project.ProjectType;

            model.ProjectLocation =
                project.ProjectLocation;

            model.WardName =
                project.Ward != null
                    ? "Ward " + project.Ward.WardNumber
                    : "Not specified";

            model.CurrentStatus =
                project.Status.ToString();

            model.Priority =
                project.Priority.ToString();

            model.ActualCompletionDate =
                project.ActualCompletionDate;

            if (!ModelState.IsValid)
                return View(model);

            var previousStatus =
                project.Status.ToString();

            project.Status =
                MunicipalProjectStatus.Closed;

            project.LastUpdatedDate =
                DateTime.Now;

            project.LastUpdatedByAdministratorID =
                administratorID.Value;

            var history =
                new ProjectHistory
                {
                    ProjectID =
                        project.ProjectID,

                    ActionType =
                        "Project Closed",

                    Description =
                        model.ClosureSummary,

                    PreviousStatus =
                        previousStatus,

                    NewStatus =
                        MunicipalProjectStatus.Closed.ToString(),

                    ActionDate =
                        DateTime.Now,

                    PerformedByAdministratorID =
                        administratorID.Value
                };

            db.ProjectHistoryRecords.Add(history);

            db.SaveChanges();

            TempData["SuccessMessage"] =
                "Project has been closed successfully.";

            return RedirectToAction(
                "Details",
                new
                {
                    id = project.ProjectID
                }
            );
        }

        // ============================================================
        // US104 - VIEW PROJECT HISTORY
        // ============================================================

        [HttpGet]
        public ActionResult History(int? id)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            if (!id.HasValue)
                return HttpNotFound();

            var model =
                BuildProjectHistoryViewModel(
                    id.Value
                );

            if (model == null)
                return HttpNotFound();

            return View(model);
        }

        private MunicipalProjectHistoryViewModel
    BuildProjectHistoryViewModel(int projectID)
        {
            var project = db.MunicipalProjects
                .AsNoTracking()
                .Include(p => p.Ward)
                .FirstOrDefault(
                    p => p.ProjectID == projectID
                );

            if (project == null)
                return null;

            var historyRecords =
                db.ProjectHistoryRecords
                    .AsNoTracking()
                    .Include(h => h.PerformedByAdministrator)
                    .Where(
                        h => h.ProjectID == projectID
                    )
                    .OrderByDescending(
                        h => h.ActionDate
                    )
                    .ThenByDescending(
                        h => h.ProjectHistoryID
                    )
                    .ToList();

            var model =
                new MunicipalProjectHistoryViewModel
                {
                    ProjectID =
                        project.ProjectID,

                    ProjectCode =
                        project.ProjectCode,

                    ProjectName =
                        project.ProjectName,

                    ProjectType =
                        project.ProjectType,

                    ProjectLocation =
                        project.ProjectLocation,

                    WardName =
                        project.Ward != null
                            ? "Ward " + project.Ward.WardNumber
                            : "Not specified",

                    CurrentStatus =
                        project.Status.ToString(),

                    Priority =
                        project.Priority.ToString(),

                    TotalHistoryRecords =
                        historyRecords.Count
                };

            foreach (var record in historyRecords)
            {
                model.HistoryRecords.Add(
                    new ProjectHistoryItemViewModel
                    {
                        ProjectHistoryID =
                            record.ProjectHistoryID,

                        ActionType =
                            record.ActionType,

                        Description =
                            record.Description,

                        PreviousStatus =
                            record.PreviousStatus,

                        NewStatus =
                            record.NewStatus,

                        ActionDate =
                            record.ActionDate,

                        PerformedByAdministratorName =
                            record.PerformedByAdministrator != null
                                ? record.PerformedByAdministrator.FirstName
                                    + " "
                                    + record.PerformedByAdministrator.LastName
                                : "Administrator"
                    }
                );
            }

            return model;
        }

        // =========================================================
        // DISPOSE
        // =========================================================

        protected override void Dispose(
            bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}