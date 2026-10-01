
using CommunityServiceProject.Models;
using CommunityServiceProject.ViewModels;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace CommunityServiceProject.Controllers
{
    public class FeeScheduleController : Controller
    {
        private readonly Community db = new Community();

        // =========================================================
        // INDEX
        // =========================================================

        [HttpGet]
        public ActionResult Index(
            string searchTerm,
            string statusFilter,
            string serviceTypeFilter)
        {
            if (Session["FinanceOfficerID"] == null)
            {
                return RedirectToAction("Login", "Login");
            }

            var query = db.FeeSchedules
                .Include(f => f.ServiceType)
                .Include(f => f.CreatedByFinanceOfficer)
                .AsQueryable();

            // -----------------------------------------------------
            // SEARCH
            // -----------------------------------------------------

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();

                query = query.Where(f =>
                    f.ServiceType.ServiceCode.Contains(searchTerm) ||
                    f.ServiceType.ServiceName.Contains(searchTerm) ||
                    f.FeeType.Contains(searchTerm)
                );
            }

            // -----------------------------------------------------
            // STATUS FILTER
            // -----------------------------------------------------

            if (statusFilter == "Active")
            {
                query = query.Where(f => f.IsActive);
            }
            else if (statusFilter == "Inactive")
            {
                query = query.Where(f => !f.IsActive);
            }

            // -----------------------------------------------------
            // SERVICE TYPE FILTER
            // -----------------------------------------------------

            int serviceTypeId;

            if (int.TryParse(serviceTypeFilter, out serviceTypeId))
            {
                query = query.Where(f =>
                    f.ServiceTypeID == serviceTypeId
                );
            }

            var fees = query
                .OrderBy(f => f.ServiceType.ServiceName)
                .ThenBy(f => f.FeeType)
                .ThenByDescending(f => f.EffectiveFrom)
                .Select(f => new FeeScheduleListItemViewModel
                {
                    FeeScheduleID = f.FeeScheduleID,
                    ServiceTypeID = f.ServiceTypeID,
                    ServiceCode = f.ServiceType.ServiceCode,
                    ServiceName = f.ServiceType.ServiceName,
                    FeeType = f.FeeType,
                    Amount = f.Amount,
                    EffectiveFrom = f.EffectiveFrom,
                    EffectiveTo = f.EffectiveTo,
                    IsActive = f.IsActive,
                    CreatedByName =
                        f.CreatedByFinanceOfficer.FirstName +
                        " " +
                        f.CreatedByFinanceOfficer.LastName
                })
                .ToList();

            // -----------------------------------------------------
            // SUMMARY
            // -----------------------------------------------------

            var allFees = db.FeeSchedules.AsQueryable();

            var model = new FeeScheduleManagementViewModel
            {
                TotalFees = allFees.Count(),
                ActiveFees = allFees.Count(f => f.IsActive),
                InactiveFees = allFees.Count(f => !f.IsActive),

                TotalActiveFeeValue = allFees
                    .Where(f => f.IsActive)
                    .Select(f => (decimal?)f.Amount)
                    .Sum() ?? 0m,

                SearchTerm = searchTerm,
                StatusFilter = statusFilter,
                ServiceTypeFilter = serviceTypeFilter,

                Fees = fees
            };

            ViewBag.ServiceTypes = db.ServiceTypes
                .Where(s => s.IsActive)
                .OrderBy(s => s.ServiceName)
                .Select(s => new SelectListItem
                {
                    Value = s.ServiceTypeID.ToString(),
                    Text = s.ServiceName + " (" + s.ServiceCode + ")"
                })
                .ToList();

            return View(model);
        }

        // =========================================================
        // CREATE
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FeeScheduleFormViewModel model)
        {
            if (Session["FinanceOfficerID"] == null)
            {
                return RedirectToAction("Login", "Login");
            }

            if (model.EffectiveTo.HasValue &&
                model.EffectiveTo.Value.Date < model.EffectiveFrom.Date)
            {
                ModelState.AddModelError(
                    "EffectiveTo",
                    "Effective To cannot be earlier than Effective From."
                );
            }

            var serviceType = db.ServiceTypes
                .FirstOrDefault(s =>
                    s.ServiceTypeID == model.ServiceTypeID
                );

            if (serviceType == null)
            {
                ModelState.AddModelError(
                    "ServiceTypeID",
                    "The selected service type could not be found."
                );
            }
            else if (!serviceType.IsActive)
            {
                ModelState.AddModelError(
                    "ServiceTypeID",
                    "An inactive service type cannot receive a new fee schedule."
                );
            }

            if (ModelState.IsValid)
            {
                var financeOfficerId =
                    (int)Session["FinanceOfficerID"];

                var fee = new FeeSchedule
                {
                    ServiceTypeID = model.ServiceTypeID,
                    FeeType = model.FeeType.Trim(),
                    Amount = model.Amount,
                    EffectiveFrom = model.EffectiveFrom.Date,
                    EffectiveTo = model.EffectiveTo.HasValue
                        ? model.EffectiveTo.Value.Date
                        : (DateTime?)null,
                    IsActive = model.IsActive,
                    CreatedByFinanceOfficerID = financeOfficerId
                };

                db.FeeSchedules.Add(fee);
                db.SaveChanges();

                TempData["SuccessMessage"] =
                    "Fee schedule created successfully.";

                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] =
                "The fee schedule could not be created. Please correct the highlighted information.";

            return RedirectToAction("Index");
        }

        // =========================================================
        // EDIT
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(FeeScheduleFormViewModel model)
        {
            if (Session["FinanceOfficerID"] == null)
            {
                return RedirectToAction("Login", "Login");
            }

            if (model.EffectiveTo.HasValue &&
                model.EffectiveTo.Value.Date < model.EffectiveFrom.Date)
            {
                ModelState.AddModelError(
                    "EffectiveTo",
                    "Effective To cannot be earlier than Effective From."
                );
            }

            var fee = db.FeeSchedules
                .FirstOrDefault(f =>
                    f.FeeScheduleID == model.FeeScheduleID
                );

            if (fee == null)
            {
                TempData["ErrorMessage"] =
                    "The selected fee schedule could not be found.";

                return RedirectToAction("Index");
            }

            var serviceType = db.ServiceTypes
                .FirstOrDefault(s =>
                    s.ServiceTypeID == model.ServiceTypeID
                );

            if (serviceType == null)
            {
                ModelState.AddModelError(
                    "ServiceTypeID",
                    "The selected service type could not be found."
                );
            }

            if (ModelState.IsValid)
            {
                fee.ServiceTypeID = model.ServiceTypeID;
                fee.FeeType = model.FeeType.Trim();
                fee.Amount = model.Amount;
                fee.EffectiveFrom = model.EffectiveFrom.Date;
                fee.EffectiveTo = model.EffectiveTo.HasValue
                    ? model.EffectiveTo.Value.Date
                    : (DateTime?)null;
                fee.IsActive = model.IsActive;

                db.SaveChanges();

                TempData["SuccessMessage"] =
                    "Fee schedule updated successfully.";

                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] =
                "The fee schedule could not be updated. Please correct the information.";

            return RedirectToAction("Index");
        }

        // =========================================================
        // TOGGLE STATUS
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleStatus(int id)
        {
            if (Session["FinanceOfficerID"] == null)
            {
                return RedirectToAction("Login", "Login");
            }

            var fee = db.FeeSchedules
                .FirstOrDefault(f =>
                    f.FeeScheduleID == id
                );

            if (fee == null)
            {
                TempData["ErrorMessage"] =
                    "The selected fee schedule could not be found.";

                return RedirectToAction("Index");
            }

            fee.IsActive = !fee.IsActive;

            db.SaveChanges();

            TempData["SuccessMessage"] =
                fee.IsActive
                    ? "Fee schedule activated successfully."
                    : "Fee schedule deactivated successfully.";

            return RedirectToAction("Index");
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

