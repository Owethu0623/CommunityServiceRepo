
using CommunityServiceProject.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace CommunityServiceProject.Controllers
{
    public class MunicipalServiceRequestController : Controller
    {
        private readonly Community db = new Community();

        // =========================================================
        // CREATE
        // =========================================================

        [HttpGet]
        public ActionResult Create(int serviceTypeId)
        {
            if (Session["CitizenID"] == null)
                return RedirectToAction("Index", "Login");

            var citizenId = (int)Session["CitizenID"];

            var service = db.ServiceTypes
                .FirstOrDefault(s =>
                    s.ServiceTypeID == serviceTypeId &&
                    s.IsActive);

            if (service == null)
            {
                TempData["ErrorMessage"] =
                    "The selected municipal service is not available.";

                return RedirectToAction(
                    "Index",
                    "MunicipalServices"
                );
            }

            // -----------------------------------------------------
            // Prevent duplicate active requests for same service
            // -----------------------------------------------------

            var existingRequest = db.MunicipalServiceRequests
                .FirstOrDefault(r =>
                    r.CitizenID == citizenId &&
                    r.ServiceTypeID == serviceTypeId &&
                    r.Status != MunicipalServiceRequestStatus.Completed &&
                    r.Status != MunicipalServiceRequestStatus.Rejected &&
                    r.Status != MunicipalServiceRequestStatus.Cancelled);

            if (existingRequest != null)
            {
                TempData["ErrorMessage"] =
                    "You already have an active request for this municipal service. " +
                    "Please wait for the existing request to be processed before submitting another request.";

                return RedirectToAction(
                    "Details",
                    new
                    {
                        id = existingRequest.MunicipalServiceRequestID
                    }
                );
            }

            var model = new MunicipalServiceRequest
            {
                ServiceTypeID = service.ServiceTypeID,
                ServiceType = service
            };

            return View(model);
        }


        // =========================================================
        // CREATE POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MunicipalServiceRequest model)
        {
            if (Session["CitizenID"] == null)
                return RedirectToAction("Login", "Login");

            var citizenId = (int)Session["CitizenID"];

            var service = db.ServiceTypes
                .FirstOrDefault(s =>
                    s.ServiceTypeID == model.ServiceTypeID &&
                    s.IsActive);

            if (service == null)
            {
                ModelState.AddModelError(
                    "ServiceTypeID",
                    "The selected municipal service is not available."
                );
            }

            // -----------------------------------------------------
            // Prevent duplicate active requests
            // -----------------------------------------------------

            var existingRequest = db.MunicipalServiceRequests
                .FirstOrDefault(r =>
                    r.CitizenID == citizenId &&
                    r.ServiceTypeID == model.ServiceTypeID &&
                    r.Status != MunicipalServiceRequestStatus.Completed &&
                    r.Status != MunicipalServiceRequestStatus.Rejected &&
                    r.Status != MunicipalServiceRequestStatus.Cancelled);

            if (existingRequest != null)
            {
                TempData["ErrorMessage"] =
                    "You already have an active request for this municipal service.";

                return RedirectToAction(
                    "Details",
                    new
                    {
                        id = existingRequest.MunicipalServiceRequestID
                    }
                );
            }

            ModelState.Remove("ReferenceNumber");
            ModelState.Remove("CitizenID");
            ModelState.Remove("DateSubmitted");
            ModelState.Remove("Status");

            if (ModelState.IsValid)
            {
                var request = new MunicipalServiceRequest
                {
                    CitizenID = citizenId,
                    ServiceTypeID = model.ServiceTypeID,
                    Title = model.Title.Trim(),
                    Description = model.Description.Trim(),

                    AdditionalInformation =
                        string.IsNullOrWhiteSpace(model.AdditionalInformation)
                            ? null
                            : model.AdditionalInformation.Trim(),

                    // Temporary value must be <= 30 characters.
                    ReferenceNumber =
                        "TMP-" +
                        DateTime.Now.ToString("yyyyMMddHHmmssfff"),

                    DateSubmitted = DateTime.Now,

                    Status =
                        MunicipalServiceRequestStatus.Submitted
                };

                db.MunicipalServiceRequests.Add(request);

                try
                {
                    // -------------------------------------------------
                    // FIRST SAVE
                    // Generates MunicipalServiceRequestID
                    // -------------------------------------------------

                    db.SaveChanges();

                    // -------------------------------------------------
                    // Generate final reference number
                    // -------------------------------------------------

                    request.ReferenceNumber =
                        "MSR-" +
                        DateTime.Now.Year +
                        "-" +
                        request.MunicipalServiceRequestID.ToString("D6");

                    // -------------------------------------------------
                    // SECOND SAVE
                    // Stores final reference number
                    // -------------------------------------------------

                    db.SaveChanges();
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                {
                    var errors = new System.Text.StringBuilder();

                    foreach (var entityError in ex.EntityValidationErrors)
                    {
                        errors.AppendLine(
                            "Entity: " +
                            entityError.Entry.Entity.GetType().Name
                        );

                        errors.AppendLine(
                            "State: " +
                            entityError.Entry.State
                        );

                        foreach (var validationError in entityError.ValidationErrors)
                        {
                            errors.AppendLine(
                                "Property: " +
                                validationError.PropertyName
                            );

                            errors.AppendLine(
                                "Error: " +
                                validationError.ErrorMessage
                            );

                            errors.AppendLine();
                        }
                    }

                    return Content(
                        "<html>" +
                        "<head>" +
                        "<title>Municipal Service Request Validation Error</title>" +
                        "<style>" +
                        "body{font-family:Arial,sans-serif;background:#f4f6f9;padding:40px;color:#222;}" +
                        ".error-box{max-width:900px;margin:auto;background:white;border:1px solid #ddd;border-radius:10px;padding:30px;}" +
                        "h1{color:#b42318;margin-top:0;}" +
                        "pre{background:#f8f9fa;border:1px solid #ddd;border-radius:8px;padding:20px;white-space:pre-wrap;}" +
                        "</style>" +
                        "</head>" +
                        "<body>" +
                        "<div class='error-box'>" +
                        "<h1>Municipal Service Request Validation Error</h1>" +
                        "<pre>" +
                        System.Web.HttpUtility.HtmlEncode(errors.ToString()) +
                        "</pre>" +
                        "</div>" +
                        "</body>" +
                        "</html>",
                        "text/html"
                    );
                }

                TempData["SuccessMessage"] =
                    "Municipal service request submitted successfully.";

                return RedirectToAction(
                    "Details",
                    new
                    {
                        id = request.MunicipalServiceRequestID
                    }
                );
            }

            model.ServiceType = service;

            return View(model);
        }


        // =========================================================
        // DETAILS
        // =========================================================

        [HttpGet]
        public ActionResult Details(int id)
        {
            if (Session["CitizenID"] == null)
                return RedirectToAction("Login", "Login");

            var citizenId = (int)Session["CitizenID"];

            var request = db.MunicipalServiceRequests
                .Include("ServiceType")
                .FirstOrDefault(r =>
                    r.MunicipalServiceRequestID == id &&
                    r.CitizenID == citizenId);

            if (request == null)
            {
                TempData["ErrorMessage"] =
                    "The municipal service request could not be found.";

                return RedirectToAction(
                    "Index",
                    "MunicipalServices"
                );
            }

            return View(request);
        }
            
            
// =========================================================
// VIEW INVOICE
// US150 - Citizen View Invoice
// =========================================================

[HttpGet]
public ActionResult Invoice(int id)
        {
            if (Session["CitizenID"] == null)
                return RedirectToAction("Login", "Login");

            var citizenId = (int)Session["CitizenID"];

            // -----------------------------------------------------
            // Verify that the service request belongs to the citizen
            // -----------------------------------------------------

            var request = db.MunicipalServiceRequests
                .Include("ServiceType")
                .FirstOrDefault(r =>
                    r.MunicipalServiceRequestID == id &&
                    r.CitizenID == citizenId);

            if (request == null)
            {
                TempData["ErrorMessage"] =
                    "The municipal service request could not be found.";

                return RedirectToAction(
                    "Index",
                    "MunicipalServices"
                );
            }

            // -----------------------------------------------------
            // Find the invoice belonging to this request
            // -----------------------------------------------------

            var invoice = db.Invoices
                .Include("MunicipalServiceRequest")
                .Include("FeeSchedule")
                .Include("FeeSchedule.ServiceType")
                .FirstOrDefault(i =>
                    i.MunicipalServiceRequestID == request.MunicipalServiceRequestID &&
                    i.CitizenID == citizenId);

            if (invoice == null)
            {
                TempData["ErrorMessage"] =
                    "An invoice has not yet been generated for this municipal service request.";

                return RedirectToAction(
                    "Details",
                    new
                    {
                        id = request.MunicipalServiceRequestID
                    }
                );
            }

            return View(invoice);
        }

      

   [HttpGet]
public ActionResult MakePayment(int id)
        {
            if (Session["CitizenID"] == null)
                return RedirectToAction("Login", "Login");

            var citizenId = (int)Session["CitizenID"];

            // ---------------------------------------------------------
            // Find the invoice and verify ownership through the request
            // ---------------------------------------------------------

            var invoice = db.Invoices
                .Include("MunicipalServiceRequest")
                .Include("FeeSchedule")
                .Include("FeeSchedule.ServiceType")
                .FirstOrDefault(i =>
                    i.InvoiceID == id &&
                    i.CitizenID == citizenId &&
                    i.MunicipalServiceRequest != null &&
                    i.MunicipalServiceRequest.CitizenID == citizenId);

            if (invoice == null)
            {
                TempData["ErrorMessage"] =
                    "The invoice could not be found.";

                return RedirectToAction(
                    "Index",
                    "MunicipalServices"
                );
            }

            // ---------------------------------------------------------
            // Prevent payment when there is no outstanding balance
            // ---------------------------------------------------------

            if (invoice.Balance <= 0)
            {
                TempData["ErrorMessage"] =
                    "This invoice has no outstanding balance.";

                return RedirectToAction(
                    "Invoice",
                    new
                    {
                        id = invoice.MunicipalServiceRequestID
                    }
                );
            }

            // ---------------------------------------------------------
            // Only invoices that require payment can be paid
            // ---------------------------------------------------------

            if (invoice.Status != InvoiceStatus.Issued &&
                invoice.Status != InvoiceStatus.PartiallyPaid &&
                invoice.Status != InvoiceStatus.Overdue)
            {
                TempData["ErrorMessage"] =
                    "This invoice is not currently available for payment.";

                return RedirectToAction(
                    "Invoice",
                    new
                    {
                        id = invoice.MunicipalServiceRequestID
                    }
                );
            }

            // ---------------------------------------------------------
            // Check whether another payment is currently processing
            // ---------------------------------------------------------

            var paymentProcessing = db.Payments.Any(p =>
                p.InvoiceID == invoice.InvoiceID &&
                p.CitizenID == citizenId &&
                p.Status == PaymentStatus.Processing);

            if (paymentProcessing)
            {
                TempData["ErrorMessage"] =
                    "A payment for this invoice is already being processed.";

                return RedirectToAction(
                    "Details",
                    new
                    {
                        id = invoice.MunicipalServiceRequestID
                    }
                );
            }

            var model = new MakePaymentViewModel
            {
                InvoiceID = invoice.InvoiceID,

                MunicipalServiceRequestID =
                    invoice.MunicipalServiceRequestID,

                InvoiceNumber =
                    invoice.InvoiceNumber,

                RequestReference =
                    invoice.MunicipalServiceRequest != null
                        ? invoice.MunicipalServiceRequest.ReferenceNumber
                        : null,

                ServiceName =
                    invoice.FeeSchedule != null &&
                    invoice.FeeSchedule.ServiceType != null
                        ? invoice.FeeSchedule.ServiceType.ServiceName
                        : "Municipal Service",

                Amount =
                    invoice.Amount,

                AmountPaid =
                    invoice.AmountPaid,

                Balance =
                    invoice.Balance,

                DueDate =
                    invoice.DueDate
            };

            return View(model);
        }


        // =========================================================
        // PROCESS MUNICIPAL PAYMENT
        // US151 - Citizen Makes Municipal Payment
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MakePayment(
            MakePaymentViewModel model)
        {
            if (Session["CitizenID"] == null)
                return RedirectToAction("Login", "Login");

            var citizenId = (int)Session["CitizenID"];

            // ---------------------------------------------------------
            // Validate payment method
            // ---------------------------------------------------------

            if (string.IsNullOrWhiteSpace(model.PaymentMethod))
            {
                ModelState.AddModelError(
                    "PaymentMethod",
                    "Please select a payment method."
                );
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // ---------------------------------------------------------
            // Reload invoice from database
            // NEVER trust invoice amount from submitted form
            // ---------------------------------------------------------

            var invoice = db.Invoices
                .Include("MunicipalServiceRequest")
                .Include("FeeSchedule")
                .Include("FeeSchedule.ServiceType")
                .FirstOrDefault(i =>
                    i.InvoiceID == model.InvoiceID &&
                    i.CitizenID == citizenId &&
                    i.MunicipalServiceRequest != null &&
                    i.MunicipalServiceRequest.CitizenID == citizenId);

            if (invoice == null)
            {
                TempData["ErrorMessage"] =
                    "The invoice could not be found.";

                return RedirectToAction(
                    "Index",
                    "MunicipalServices"
                );
            }

            // ---------------------------------------------------------
            // Verify outstanding balance
            // ---------------------------------------------------------

            if (invoice.Balance <= 0)
            {
                TempData["ErrorMessage"] =
                    "This invoice has already been fully paid.";

                return RedirectToAction(
                    "Invoice",
                    new
                    {
                        id = invoice.MunicipalServiceRequestID
                    }
                );
            }

            // ---------------------------------------------------------
            // Verify invoice status
            // ---------------------------------------------------------

            if (invoice.Status != InvoiceStatus.Issued &&
                invoice.Status != InvoiceStatus.PartiallyPaid &&
                invoice.Status != InvoiceStatus.Overdue)
            {
                TempData["ErrorMessage"] =
                    "This invoice is not currently available for payment.";

                return RedirectToAction(
                    "Invoice",
                    new
                    {
                        id = invoice.MunicipalServiceRequestID
                    }
                );
            }

            // ---------------------------------------------------------
            // Prevent duplicate processing payments
            // ---------------------------------------------------------

            var existingProcessingPayment = db.Payments.Any(p =>
                p.InvoiceID == invoice.InvoiceID &&
                p.CitizenID == citizenId &&
                p.Status == PaymentStatus.Processing);

            if (existingProcessingPayment)
            {
                TempData["ErrorMessage"] =
                    "A payment for this invoice is already being processed.";

                return RedirectToAction(
                    "Details",
                    new
                    {
                        id = invoice.MunicipalServiceRequestID
                    }
                );
            }

            // ---------------------------------------------------------
            // Create payment
            // ---------------------------------------------------------

            var payment = new Payment
            {
                InvoiceID =
                    invoice.InvoiceID,

                CitizenID =
                    citizenId,

                Amount =
                    invoice.Balance,

                Status =
                    PaymentStatus.Processing,

                PaymentDate =
                    DateTime.Now,

                ProcessedDate =
                    null,

                PaymentMethod =
                    model.PaymentMethod.Trim(),

                FailureReason =
                    null,

                TransactionReference =
                    "TEMP-" +
                    Guid.NewGuid()
                        .ToString("N")
                        .Substring(0, 20)
            };

            db.Payments.Add(payment);

            // ---------------------------------------------------------
            // First save
            // Generates PaymentID
            // ---------------------------------------------------------

            db.SaveChanges();

            // ---------------------------------------------------------
            // Generate final transaction reference
            // ---------------------------------------------------------

            payment.TransactionReference =
                "PAY-" +
                DateTime.Now.Year +
                "-" +
                payment.PaymentID.ToString("D6");

            // ---------------------------------------------------------
            // Update municipal service request status
            // ---------------------------------------------------------

            invoice.MunicipalServiceRequest.Status =
                MunicipalServiceRequestStatus.PaymentProcessing;

            db.SaveChanges();

            TempData["SuccessMessage"] =
                "Your payment has been submitted successfully and is currently being processed.";

            return RedirectToAction(
                "Details",
                new
                {
                    id = invoice.MunicipalServiceRequestID
                }
            );
        }







        // =========================================================
        // DISPOSE
        // =========================================================

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}

