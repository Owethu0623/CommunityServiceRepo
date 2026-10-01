
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using CommunityServiceProject.Models;
using CommunityServiceProject.ViewModels;

namespace CommunityServiceProject.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly Community db = new Community();

        private bool IsFinanceOfficer()
        {
            return Session["FinanceOfficerID"] != null &&
                   Session["UserRole"] != null &&
                   Session["UserRole"].ToString()
                       .Equals(
                           "FinanceOfficer",
                           StringComparison.OrdinalIgnoreCase);
        }

        private ActionResult FinanceLoginRedirect()
        {
            return RedirectToAction(
                "Index",
                "Login");
        }

        /*
         * ============================================================
         * US138
         * Generate Invoice
         * ============================================================
         */

        [HttpGet]
        public ActionResult Index()
        {
            if (!IsFinanceOfficer())
            {
                return FinanceLoginRedirect();
            }

            var today = DateTime.Today;

            var requests =
                db.MunicipalServiceRequests
                    .Include(r => r.Citizen)
                    .Include(r => r.ServiceType)
                    .Where(r =>
                        r.Status ==
                            MunicipalServiceRequestStatus.Approved &&
                        r.ServiceType != null &&
                        r.ServiceType.IsActive &&
                        r.ServiceType.IsChargeable &&
                        !r.Invoices.Any())
                    .ToList();

            var models =
                requests
                    .Select(r =>
                    {
                        var feeSchedule =
                            db.FeeSchedules
                                .Where(f =>
                                    f.ServiceTypeID ==
                                        r.ServiceTypeID &&
                                    f.IsActive &&
                                    f.EffectiveFrom <= today &&
                                    (!f.EffectiveTo.HasValue ||
                                     f.EffectiveTo.Value >= today))
                                .OrderByDescending(
                                    f => f.EffectiveFrom)
                                .FirstOrDefault();

                        if (feeSchedule == null)
                        {
                            return null;
                        }

                        return new GenerateInvoiceListViewModel
                        {
                            MunicipalServiceRequestID =
                                r.MunicipalServiceRequestID,

                            RequestReference =
                                r.ReferenceNumber,

                            CitizenName =
                                r.Citizen != null
                                    ? r.Citizen.FirstName +
                                      " " +
                                      r.Citizen.LastName
                                    : "Unknown Citizen",

                            ServiceCode =
                                r.ServiceType.ServiceCode,

                            ServiceName =
                                r.ServiceType.ServiceName,

                            FeeType =
                                feeSchedule.FeeType,

                            Amount =
                                feeSchedule.Amount,

                            DateApproved =
                                r.DateApproved,

                            RequestStatus =
                                r.Status.ToString(),

                            FeeScheduleID =
                                feeSchedule.FeeScheduleID
                        };
                    })
                    .Where(x => x != null)
                    .OrderByDescending(
                        x => x.DateApproved)
                    .ToList();

            ViewBag.PendingInvoiceCount =
                models.Count;

            ViewBag.TotalAmount =
                models.Sum(x => x.Amount);

            return View(models);
        }

        /*
         * ============================================================
         * REVIEW APPROVED REQUEST BEFORE INVOICE GENERATION
         * ============================================================
         */

        [HttpGet]
        public ActionResult Generate(
            int? id)
        {
            if (!IsFinanceOfficer())
            {
                return FinanceLoginRedirect();
            }

            if (!id.HasValue)
            {
                return RedirectToAction("Index");
            }

            var request =
                db.MunicipalServiceRequests
                    .Include(r => r.Citizen)
                    .Include(r => r.ServiceType)
                    .FirstOrDefault(r =>
                        r.MunicipalServiceRequestID ==
                            id.Value);

            if (request == null)
            {
                return HttpNotFound();
            }

            if (request.Status !=
                MunicipalServiceRequestStatus.Approved)
            {
                TempData["InvoiceError"] =
                    "Only approved municipal service requests can be invoiced.";

                return RedirectToAction("Index");
            }

            if (request.ServiceType == null ||
                !request.ServiceType.IsActive ||
                !request.ServiceType.IsChargeable)
            {
                TempData["InvoiceError"] =
                    "This municipal service is not currently chargeable.";

                return RedirectToAction("Index");
            }

            if (request.Invoices.Any())
            {
                TempData["InvoiceError"] =
                    "An invoice has already been generated for this request.";

                return RedirectToAction(
                    "Details",
                    new
                    {
                        id =
                            request.Invoices
                                .OrderByDescending(
                                    i => i.IssueDate)
                                .Select(
                                    i => i.InvoiceID)
                                .FirstOrDefault()
                    });
            }

            var today = DateTime.Today;

            var feeSchedule =
                db.FeeSchedules
                    .Where(f =>
                        f.ServiceTypeID ==
                            request.ServiceTypeID &&
                        f.IsActive &&
                        f.EffectiveFrom <= today &&
                        (!f.EffectiveTo.HasValue ||
                         f.EffectiveTo.Value >= today))
                    .OrderByDescending(
                        f => f.EffectiveFrom)
                    .FirstOrDefault();

            if (feeSchedule == null)
            {
                TempData["InvoiceError"] =
                    "No active fee schedule is available for this municipal service.";

                return RedirectToAction("Index");
            }

            var model =
                new GenerateInvoiceViewModel
                {
                    MunicipalServiceRequestID =
                        request.MunicipalServiceRequestID,

                    RequestReference =
                        request.ReferenceNumber,

                    CitizenName =
                        request.Citizen != null
                            ? request.Citizen.FirstName +
                              " " +
                              request.Citizen.LastName
                            : "Unknown Citizen",

                    CitizenEmail =
                        request.Citizen != null
                            ? request.Citizen.EmailAddress
                            : null,

                    ServiceCode =
                        request.ServiceType.ServiceCode,

                    ServiceName =
                        request.ServiceType.ServiceName,

                    ServiceDescription =
                        request.ServiceType.Description,

                    FeeType =
                        feeSchedule.FeeType,

                    Amount =
                        feeSchedule.Amount,

                    FeeScheduleID =
                        feeSchedule.FeeScheduleID,

                    EffectiveFrom =
                        feeSchedule.EffectiveFrom,

                    EffectiveTo =
                        feeSchedule.EffectiveTo,

                    DueDate =
                        null
                };

            return View(model);
        }

        /*
         * ============================================================
         * CREATE INVOICE
         * ============================================================
         */

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Generate(
            GenerateInvoiceViewModel model)
        {
            if (!IsFinanceOfficer())
            {
                return FinanceLoginRedirect();
            }

            if (!model.DueDate.HasValue)
            {
                ModelState.AddModelError(
                    "DueDate",
                    "Please select an invoice due date.");
            }
            else if (
                model.DueDate.Value.Date <
                DateTime.Today)
            {
                ModelState.AddModelError(
                    "DueDate",
                    "The invoice due date cannot be in the past.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var request =
                db.MunicipalServiceRequests
                    .Include(r => r.Citizen)
                    .Include(r => r.ServiceType)
                    .FirstOrDefault(r =>
                        r.MunicipalServiceRequestID ==
                            model.MunicipalServiceRequestID);

            if (request == null)
            {
                return HttpNotFound();
            }

            if (request.Status !=
                MunicipalServiceRequestStatus.Approved)
            {
                TempData["InvoiceError"] =
                    "Only approved municipal service requests can be invoiced.";

                return RedirectToAction("Index");
            }

            if (request.ServiceType == null ||
                !request.ServiceType.IsActive ||
                !request.ServiceType.IsChargeable)
            {
                TempData["InvoiceError"] =
                    "This municipal service is not currently chargeable.";

                return RedirectToAction("Index");
            }

            if (request.Invoices.Any())
            {
                var existingInvoice =
                    request.Invoices
                        .OrderByDescending(
                            i => i.IssueDate)
                        .First();

                return RedirectToAction(
                    "Details",
                    new
                    {
                        id =
                            existingInvoice.InvoiceID
                    });
            }

            var today = DateTime.Today;

            var feeSchedule =
                db.FeeSchedules
                    .Where(f =>
                        f.ServiceTypeID ==
                            request.ServiceTypeID &&
                        f.IsActive &&
                        f.EffectiveFrom <= today &&
                        (!f.EffectiveTo.HasValue ||
                         f.EffectiveTo.Value >= today))
                    .OrderByDescending(
                        f => f.EffectiveFrom)
                    .FirstOrDefault();

            if (feeSchedule == null)
            {
                TempData["InvoiceError"] =
                    "No active fee schedule is available for this municipal service.";

                return RedirectToAction("Index");
            }

            /*
             * Create the invoice first so that InvoiceID is available
             * for the system-generated invoice number.
             */
            var invoice =
                new Invoice
                {
                    MunicipalServiceRequestID =
                        request.MunicipalServiceRequestID,

                    CitizenID =
                        request.CitizenID,

                    FeeScheduleID =
                        feeSchedule.FeeScheduleID,

                    Amount =
                        feeSchedule.Amount,

                    AmountPaid =
                        0m,

                    Balance =
                        feeSchedule.Amount,

                    Status =
                        InvoiceStatus.Issued,

                    IssueDate =
                        DateTime.Now,

                    DueDate =
                        model.DueDate.Value.Date,

                    PaidDate =
                        null
                };

            /*
             * Temporary unique value is used because InvoiceNumber
             * is required and unique before the first SaveChanges.
             */
            invoice.InvoiceNumber =
                "TEMP-" +
                Guid.NewGuid()
                    .ToString("N")
                    .Substring(0, 20);

            db.Invoices.Add(invoice);

            db.SaveChanges();

            invoice.InvoiceNumber =
                "INV-" +
                DateTime.Now.Year +
                "-" +
                invoice.InvoiceID.ToString("D6");

            request.Status =
                MunicipalServiceRequestStatus.InvoiceIssued;

            db.SaveChanges();

            request.Status =
                MunicipalServiceRequestStatus.PaymentRequired;

            db.SaveChanges();

            return RedirectToAction(
                "Details",
                new
                {
                    id =
                        invoice.InvoiceID
                });
        }


        /*
         * ============================================================
         * INVOICE DETAILS
         * ============================================================
         */

        [HttpGet]
        public ActionResult Details(
            int? id)
        {
            if (!IsFinanceOfficer())
            {
                return FinanceLoginRedirect();
            }

            if (!id.HasValue)
            {
                return RedirectToAction("Index");
            }

            var invoice =
                db.Invoices
                    .Include(i => i.Citizen)
                    .Include(i => i.MunicipalServiceRequest)
                    .Include(i => i.FeeSchedule)
                    .Include(i => i.FeeSchedule.ServiceType)
                    .FirstOrDefault(i =>
                        i.InvoiceID == id.Value);

            if (invoice == null)
            {
                return HttpNotFound();
            }

            var model =
                new InvoiceDetailsViewModel
                {
                    InvoiceID =
                        invoice.InvoiceID,

                    InvoiceNumber =
                        invoice.InvoiceNumber,

                    RequestReference =
                        invoice.MunicipalServiceRequest != null
                            ? invoice.MunicipalServiceRequest.ReferenceNumber
                            : null,

                    MunicipalServiceRequestID =
                        invoice.MunicipalServiceRequestID,

                    CitizenName =
                        invoice.Citizen != null
                            ? invoice.Citizen.FirstName +
                              " " +
                              invoice.Citizen.LastName
                            : "Unknown Citizen",

                    CitizenEmail =
                        invoice.Citizen != null
                            ? invoice.Citizen.EmailAddress
                            : null,

                    ServiceCode =
                        invoice.FeeSchedule != null &&
                        invoice.FeeSchedule.ServiceType != null
                            ? invoice.FeeSchedule.ServiceType.ServiceCode
                            : null,

                    ServiceName =
                        invoice.FeeSchedule != null &&
                        invoice.FeeSchedule.ServiceType != null
                            ? invoice.FeeSchedule.ServiceType.ServiceName
                            : null,

                    FeeType =
                        invoice.FeeSchedule != null
                            ? invoice.FeeSchedule.FeeType
                            : null,

                    Amount =
                        invoice.Amount,

                    AmountPaid =
                        invoice.AmountPaid,

                    Balance =
                        invoice.Balance,

                    Status =
                        invoice.Status,

                    IssueDate =
                        invoice.IssueDate,

                    DueDate =
                        invoice.DueDate,

                    PaidDate =
                        invoice.PaidDate
                };

            return View(model);
        }

        
/*
 * ============================================================
 * US139
 * VIEW ISSUED INVOICES
 * ============================================================
 */

[HttpGet]
public ActionResult Issued()
        {
            if (!IsFinanceOfficer())
            {
                return FinanceLoginRedirect();
            }

            var invoices =
                db.Invoices
                    .Include(i => i.Citizen)
                    .Include(i => i.MunicipalServiceRequest)
                    .Include(i => i.FeeSchedule)
                    .Include(i => i.FeeSchedule.ServiceType)
                    .OrderByDescending(i => i.IssueDate)
                    .ToList();

            ViewBag.TotalInvoices =
                invoices.Count;

            ViewBag.TotalBilled =
                invoices.Sum(i => i.Amount);

            ViewBag.TotalPaid =
                invoices.Sum(i => i.AmountPaid);

            ViewBag.TotalOutstanding =
                invoices.Sum(i => i.Balance);

            return View(invoices);
        }


[HttpGet]
public ActionResult Outstanding()
        {
            if (!IsFinanceOfficer())
            {
                return FinanceLoginRedirect();
            }

            var outstandingStatuses = new[]
            {
        InvoiceStatus.Issued,
        InvoiceStatus.PartiallyPaid,
        InvoiceStatus.Overdue
    };

            var invoices =
                db.Invoices
                    .Include(i => i.Citizen)
                    .Include(i => i.MunicipalServiceRequest)
                    .Include(i => i.FeeSchedule)
                    .Include(i => i.FeeSchedule.ServiceType)
                    .Where(i =>
                        outstandingStatuses.Contains(i.Status) &&
                        i.Balance > 0)
                    .OrderBy(i => i.DueDate)
                    .ThenByDescending(i => i.IssueDate)
                    .ToList();

            ViewBag.TotalOutstandingInvoices =
                invoices.Count;

            ViewBag.TotalOutstandingBalance =
                invoices.Sum(i => i.Balance);

            ViewBag.TotalIssuedInvoices =
                invoices.Count(i =>
                    i.Status == InvoiceStatus.Issued);

            ViewBag.TotalPartiallyPaidInvoices =
                invoices.Count(i =>
                    i.Status == InvoiceStatus.PartiallyPaid);

            ViewBag.TotalOverdueInvoices =
                invoices.Count(i =>
                    i.Status == InvoiceStatus.Overdue);

            return View(invoices);
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

