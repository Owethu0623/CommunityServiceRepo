
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using CommunityServiceProject.Models;
using CommunityServiceProject.ViewModels;

namespace CommunityServiceProject.Controllers
{
    [CommunityServiceProject.Filters.RoleAuthorize("FinanceOfficer")]
    public class ServiceTypeController : Controller
    {
        private readonly Community db = new Community();

        // =========================================================
        // US132 - VIEW / MANAGE SERVICE TYPES
        // =========================================================

        public ActionResult Index(
            string searchTerm,
            string statusFilter,
            string chargeableFilter)
        {
            if (Session["FinanceOfficerID"] == null)
            {
                return RedirectToAction("Login", "Login");
            }

            var query = db.ServiceTypes
                .Include(s => s.MunicipalServiceRequests)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();

                query = query.Where(s =>
                    s.ServiceCode.Contains(searchTerm) ||
                    s.ServiceName.Contains(searchTerm) ||
                    s.Description.Contains(searchTerm));
            }

            if (statusFilter == "Active")
            {
                query = query.Where(s => s.IsActive);
            }
            else if (statusFilter == "Inactive")
            {
                query = query.Where(s => !s.IsActive);
            }

            if (chargeableFilter == "Chargeable")
            {
                query = query.Where(s => s.IsChargeable);
            }
            else if (chargeableFilter == "NonChargeable")
            {
                query = query.Where(s => !s.IsChargeable);
            }

            var allServices = db.ServiceTypes.AsQueryable();

            var model = new ServiceTypeManagementViewModel
            {
                TotalServices = allServices.Count(),

                ActiveServices = allServices.Count(s => s.IsActive),

                InactiveServices = allServices.Count(s => !s.IsActive),

                ChargeableServices = allServices.Count(s => s.IsChargeable),

                NonChargeableServices = allServices.Count(s => !s.IsChargeable),

                SearchTerm = searchTerm,

                StatusFilter = statusFilter,

                ChargeableFilter = chargeableFilter,

                Services = query
                    .OrderBy(s => s.ServiceName)
                    .Select(s => new ServiceTypeListItemViewModel
                    {
                        ServiceTypeID = s.ServiceTypeID,
                        ServiceCode = s.ServiceCode,
                        ServiceName = s.ServiceName,
                        Description = s.Description,
                        IsChargeable = s.IsChargeable,
                        IsActive = s.IsActive,
                        ServiceRequestCount = s.MunicipalServiceRequests.Count()
                    })
                    .ToList()
            };

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];

            return View(model);
        }

        // =========================================================
        // CREATE
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ServiceTypeFormViewModel model)
        {
            if (Session["FinanceOfficerID"] == null)
            {
                return RedirectToAction("Login", "Login");
            }

            if (string.IsNullOrWhiteSpace(model.ServiceCode))
            {
                ModelState.AddModelError(
                    "ServiceCode",
                    "Service code is required.");
            }

            if (string.IsNullOrWhiteSpace(model.ServiceName))
            {
                ModelState.AddModelError(
                    "ServiceName",
                    "Service name is required.");
            }

            string normalizedCode = model.ServiceCode == null
                ? null
                : model.ServiceCode.Trim().ToUpper();

            if (!string.IsNullOrWhiteSpace(normalizedCode))
            {
                bool duplicateCode = db.ServiceTypes.Any(s =>
                    s.ServiceCode.ToUpper() == normalizedCode);

                if (duplicateCode)
                {
                    ModelState.AddModelError(
                        "ServiceCode",
                        "A service with this code already exists.");
                }
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] =
                    "Please correct the service information before saving.";

                return RedirectToAction("Index");
            }

            var serviceType = new ServiceType
            {
                ServiceCode = normalizedCode,
                ServiceName = model.ServiceName.Trim(),
                Description = string.IsNullOrWhiteSpace(model.Description)
                    ? null
                    : model.Description.Trim(),
                IsChargeable = model.IsChargeable,
                IsActive = model.IsActive
            };

            db.ServiceTypes.Add(serviceType);
            db.SaveChanges();

            TempData["SuccessMessage"] =
                "Municipal service type was created successfully.";

            return RedirectToAction("Index");
        }

        // =========================================================
        // EDIT
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ServiceTypeFormViewModel model)
        {
            if (Session["FinanceOfficerID"] == null)
            {
                return RedirectToAction("Login", "Login");
            }

            var serviceType = db.ServiceTypes
                .FirstOrDefault(s => s.ServiceTypeID == model.ServiceTypeID);

            if (serviceType == null)
            {
                return HttpNotFound();
            }

            string normalizedCode = model.ServiceCode == null
                ? null
                : model.ServiceCode.Trim().ToUpper();

            bool duplicateCode = !string.IsNullOrWhiteSpace(normalizedCode)
                && db.ServiceTypes.Any(s =>
                    s.ServiceTypeID != model.ServiceTypeID &&
                    s.ServiceCode.ToUpper() == normalizedCode);

            if (duplicateCode)
            {
                ModelState.AddModelError(
                    "ServiceCode",
                    "A different service already uses this code.");
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] =
                    "Please correct the service information before saving.";

                return RedirectToAction("Index");
            }

            serviceType.ServiceCode = normalizedCode;
            serviceType.ServiceName = model.ServiceName.Trim();
            serviceType.Description =
                string.IsNullOrWhiteSpace(model.Description)
                    ? null
                    : model.Description.Trim();

            serviceType.IsChargeable = model.IsChargeable;
            serviceType.IsActive = model.IsActive;

            db.SaveChanges();

            TempData["SuccessMessage"] =
                "Municipal service type was updated successfully.";

            return RedirectToAction("Index");
        }

        // =========================================================
        // ACTIVATE / DEACTIVATE
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleStatus(int id)
        {
            if (Session["FinanceOfficerID"] == null)
            {
                return RedirectToAction("Login", "Login");
            }

            var serviceType = db.ServiceTypes
                .FirstOrDefault(s => s.ServiceTypeID == id);

            if (serviceType == null)
            {
                return HttpNotFound();
            }

            serviceType.IsActive = !serviceType.IsActive;

            db.SaveChanges();

            TempData["SuccessMessage"] = serviceType.IsActive
                ? "Service type has been activated."
                : "Service type has been deactivated.";

            return RedirectToAction("Index");
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

