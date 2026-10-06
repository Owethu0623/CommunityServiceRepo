using CommunityServiceProject.Models;
using CommunityServiceProject.ViewModels;
using System;
using System.Linq;
using System.Web.Mvc;
using CommunityServiceProject.Filters;

namespace CommunityServiceProject.Models
{
    [RoleAuthorize("Citizen")]
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
                // Previously set an ErrorMessage and redirected to the existing request details.
                // Remove the message so it does not appear in views per user request.
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
                // Previously set an ErrorMessage so it displayed in the view; remove the message.
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
            try
            {
                // ---------------------------------------------------------
                // AUTHENTICATION
                // ---------------------------------------------------------

                if (Session["CitizenID"] == null)
                    return RedirectToAction("Login", "Login");

                var citizenId = (int)Session["CitizenID"];

                // ---------------------------------------------------------
                // LOAD MUNICIPAL SERVICE REQUEST
                // ---------------------------------------------------------

                var request = db.MunicipalServiceRequests
                    .Include("ServiceType")
                    .FirstOrDefault(r =>
                        r.MunicipalServiceRequestID == id &&
                        r.CitizenID == citizenId);

                // ---------------------------------------------------------
                // CHECK THAT REQUEST EXISTS
                // ---------------------------------------------------------

                if (request == null)
                {
                    return Content(
                        "<html>" +
                        "<head>" +
                        "<title>Municipal Service Request - Not Found</title>" +
                        "<style>" +
                        "body{font-family:Arial,sans-serif;background:#f4f6f9;padding:40px;color:#222;}" +
                        ".error-box{max-width:900px;margin:auto;background:white;border:1px solid #ddd;border-radius:10px;padding:30px;}" +
                        "h1{color:#b42318;margin-top:0;}" +
                        ".info{background:#f8f9fa;border:1px solid #ddd;border-radius:8px;padding:20px;margin-top:20px;}" +
                        "strong{color:#16324f;}" +
                        "</style>" +
                        "</head>" +
                        "<body>" +
                        "<div class='error-box'>" +
                        "<h1>Municipal Service Request Not Found</h1>" +
                        "<div class='info'>" +
                        "<p><strong>ID received:</strong> " +
                        id +
                        "</p>" +
                        "<p><strong>Citizen ID:</strong> " +
                        citizenId +
                        "</p>" +
                        "<p>The controller could not find a municipal service request matching both the ID and the logged-in citizen.</p>" +
                        "</div>" +
                        "</div>" +
                        "</body>" +
                        "</html>",
                        "text/html"
                    );
                }

                // ---------------------------------------------------------
                // CHECK SERVICE TYPE
                // ---------------------------------------------------------

                if (request.ServiceType == null)
                {
                    return Content(
                        "<html>" +
                        "<head>" +
                        "<title>Municipal Service Request Error</title>" +
                        "<style>" +
                        "body{font-family:Arial,sans-serif;background:#f4f6f9;padding:40px;color:#222;}" +
                        ".error-box{max-width:900px;margin:auto;background:white;border:1px solid #ddd;border-radius:10px;padding:30px;}" +
                        "h1{color:#b42318;margin-top:0;}" +
                        "pre{background:#f8f9fa;border:1px solid #ddd;border-radius:8px;padding:20px;white-space:pre-wrap;}" +
                        "</style>" +
                        "</head>" +
                        "<body>" +
                        "<div class='error-box'>" +
                        "<h1>Service Type Is Missing</h1>" +
                        "<pre>" +
                        "MunicipalServiceRequestID: " +
                        request.MunicipalServiceRequestID +
                        "\nServiceTypeID: " +
                        request.ServiceTypeID +
                        "\nReferenceNumber: " +
                        System.Web.HttpUtility.HtmlEncode(request.ReferenceNumber) +
                        "</pre>" +
                        "</div>" +
                        "</body>" +
                        "</html>",
                        "text/html"
                    );
                }

                // ---------------------------------------------------------
                // COMPLETION NOTICE
                // ---------------------------------------------------------

                bool showCompletionNotice = false;

                if (request.Status ==
                    MunicipalServiceRequestStatus.Completed)
                {
                    string completionNoticeKey =
                        "CompletionNoticeShown_" +
                        request.MunicipalServiceRequestID;

                    if (Session[completionNoticeKey] == null)
                    {
                        showCompletionNotice = true;
                        Session[completionNoticeKey] = true;
                    }
                }

                ViewBag.ShowCompletionNotice =
                    showCompletionNotice;

                // ---------------------------------------------------------
                // RETURN VIEW
                // ---------------------------------------------------------

                return View(request);
            }
            catch (Exception ex)
            {
                // =========================================================
                // DISPLAY FULL ERROR IN BROWSER
                // TEMPORARY DEBUGGING ONLY
                // =========================================================

                var errorDetails =
                    "MESSAGE:\n" +
                    ex.Message +
                    "\n\n" +

                    "EXCEPTION TYPE:\n" +
                    ex.GetType().FullName +
                    "\n\n" +

                    "STACK TRACE:\n" +
                    ex.StackTrace;

                if (ex.InnerException != null)
                {
                    errorDetails +=
                        "\n\nINNER EXCEPTION:\n" +
                        ex.InnerException.Message +
                        "\n\nINNER EXCEPTION TYPE:\n" +
                        ex.InnerException.GetType().FullName +
                        "\n\nINNER STACK TRACE:\n" +
                        ex.InnerException.StackTrace;
                }

                return Content(
                    "<html>" +
                    "<head>" +
                    "<title>Municipal Service Request Error</title>" +
                    "<style>" +
                    "body{" +
                        "font-family:Arial,sans-serif;" +
                        "background:#f4f6f9;" +
                        "padding:40px;" +
                        "color:#222;" +
                    "}" +

                    ".error-box{" +
                        "max-width:1100px;" +
                        "margin:auto;" +
                        "background:white;" +
                        "border:1px solid #ddd;" +
                        "border-radius:10px;" +
                        "padding:30px;" +
                        "box-shadow:0 4px 15px rgba(0,0,0,.08);" +
                    "}" +

                    "h1{" +
                        "color:#b42318;" +
                        "margin-top:0;" +
                    "}" +

                    ".warning{" +
                        "background:#fff3cd;" +
                        "border:1px solid #ffe69c;" +
                        "border-radius:8px;" +
                        "padding:15px;" +
                        "margin-bottom:20px;" +
                        "color:#664d03;" +
                    "}" +

                    "pre{" +
                        "background:#f8f9fa;" +
                        "border:1px solid #ddd;" +
                        "border-radius:8px;" +
                        "padding:20px;" +
                        "white-space:pre-wrap;" +
                        "word-break:break-word;" +
                        "overflow-x:auto;" +
                        "line-height:1.5;" +
                        "font-size:13px;" +
                    "}" +

                    ".label{" +
                        "font-weight:bold;" +
                        "color:#16324f;" +
                    "}" +

                    "</style>" +
                    "</head>" +

                    "<body>" +

                    "<div class='error-box'>" +

                    "<h1>Municipal Service Request Error</h1>" +

                    "<div class='warning'>" +
                    "<strong>Temporary debugging page.</strong><br />" +
                    "This error is being displayed so we can identify exactly what is failing when opening the municipal service request details page." +
                    "</div>" +

                    "<p>" +
                    "<span class='label'>Request ID received:</span> " +
                    id +
                    "</p>" +

                    "<pre>" +
                    System.Web.HttpUtility.HtmlEncode(errorDetails) +
                    "</pre>" +

                    "</div>" +

                    "</body>" +
                    "</html>",
                    "text/html"
                );
            }
        }

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


        // =========================================================
        // MAKE MUNICIPAL PAYMENT
        // US151 - Display Payment Page
        // =========================================================

        [HttpGet]
        public ActionResult MakePayment(int id)
        {
            if (Session["CitizenID"] == null)
                return RedirectToAction("Login", "Login");

            var citizenId = (int)Session["CitizenID"];

            // ---------------------------------------------------------
            // Find the municipal service request belonging to the citizen
            // ---------------------------------------------------------

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

            // ---------------------------------------------------------
            // Payment is only available for these request statuses
            // ---------------------------------------------------------

            if (request.Status != MunicipalServiceRequestStatus.PaymentRequired &&
                request.Status != MunicipalServiceRequestStatus.PaymentFailed)
            {
                TempData["ErrorMessage"] =
                    "Payment is not currently available for this municipal service request.";

                return RedirectToAction(
                    "Details",
                    new
                    {
                        id = request.MunicipalServiceRequestID
                    }
                );
            }

            // ---------------------------------------------------------
            // Find the invoice belonging to THIS request and citizen
            // ---------------------------------------------------------

            var invoice = db.Invoices
                .Include("MunicipalServiceRequest")
                .Include("FeeSchedule")
                .Include("FeeSchedule.ServiceType")
                .FirstOrDefault(i =>
                    i.MunicipalServiceRequestID ==
                        request.MunicipalServiceRequestID &&
                    i.CitizenID == citizenId &&
                    i.MunicipalServiceRequest != null &&
                    i.MunicipalServiceRequest.CitizenID == citizenId);

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
                        id = request.MunicipalServiceRequestID
                    }
                );
            }

            // ---------------------------------------------------------
            // Only payable invoice statuses are allowed
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
                        id = request.MunicipalServiceRequestID
                    }
                );
            }

            // ---------------------------------------------------------
            // Check ONLY this invoice for an existing processing payment
            // ---------------------------------------------------------

            var paymentProcessing = db.Payments.Any(p =>
                p.InvoiceID == invoice.InvoiceID &&
                p.CitizenID == citizenId &&
                p.Status ==
                   Models.PaymentStatus.Processing);

            if (paymentProcessing)
            {
                TempData["ErrorMessage"] =
                    "A payment for this invoice is already being processed.";

                return RedirectToAction(
                    "Details",
                    new
                    {
                        id = request.MunicipalServiceRequestID
                    }
                );
            }

            // ---------------------------------------------------------
            // Build payment model
            // ---------------------------------------------------------

            var model = new MakePaymentViewModel
            {
                InvoiceID =
                    invoice.InvoiceID,

                MunicipalServiceRequestID =
                    request.MunicipalServiceRequestID,

                InvoiceNumber =
                    invoice.InvoiceNumber,

                RequestReference =
                    request.ReferenceNumber,

                ServiceName =
                    invoice.FeeSchedule != null &&
                    invoice.FeeSchedule.ServiceType != null
                        ? invoice.FeeSchedule.ServiceType.ServiceName
                        : request.ServiceType != null
                            ? request.ServiceType.ServiceName
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
            // Reload invoice from database.
            // NEVER trust invoice information submitted by the browser.
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

            var request = invoice.MunicipalServiceRequest;

            // ---------------------------------------------------------
            // Verify that payment is currently allowed for this request
            // ---------------------------------------------------------

            if (request.Status != MunicipalServiceRequestStatus.PaymentRequired &&
                request.Status != MunicipalServiceRequestStatus.PaymentFailed)
            {
                TempData["ErrorMessage"] =
                    "Payment is not currently available for this municipal service request.";

                return RedirectToAction(
                    "Details",
                    new
                    {
                        id = request.MunicipalServiceRequestID
                    }
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
            // Validate payment method
            // ---------------------------------------------------------

            if (string.IsNullOrWhiteSpace(model.PaymentMethod))
            {
                ModelState.AddModelError(
                    "PaymentMethod",
                    "Please select a payment method."
                );
            }

            // ---------------------------------------------------------
            // Validate Proof of Payment
            // ---------------------------------------------------------

            if (model.ProofOfPayment == null ||
                model.ProofOfPayment.ContentLength <= 0)
            {
                ModelState.AddModelError(
                    "ProofOfPayment",
                    "Please upload proof of payment."
                );
            }
            else
            {
                // -----------------------------------------------------
                // Validate file extension
                // -----------------------------------------------------

                var allowedExtensions = new[]
                {
            ".pdf",
            ".jpg",
            ".jpeg",
            ".png"
        };

                var extension = System.IO.Path
                    .GetExtension(model.ProofOfPayment.FileName)
                    ?.ToLowerInvariant();

                if (string.IsNullOrWhiteSpace(extension) ||
                    !allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        "ProofOfPayment",
                        "Only PDF, JPG, JPEG and PNG files are allowed."
                    );
                }

                // -----------------------------------------------------
                // Validate file size
                // Maximum = 5 MB
                // -----------------------------------------------------

                const int maxFileSize = 5 * 1024 * 1024;

                if (model.ProofOfPayment.ContentLength > maxFileSize)
                {
                    ModelState.AddModelError(
                        "ProofOfPayment",
                        "The proof of payment file cannot exceed 5 MB."
                    );
                }
            }

            // ---------------------------------------------------------
            // Prevent duplicate processing payments
            // ---------------------------------------------------------

            var existingProcessingPayment = db.Payments.Any(p =>
                p.InvoiceID == invoice.InvoiceID &&
                p.CitizenID == citizenId &&
                p.Status ==
                    Models.PaymentStatus.Processing);

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
            // If validation failed, rebuild display information before
            // returning to the payment page.
            // ---------------------------------------------------------

            if (!ModelState.IsValid)
            {
                model.MunicipalServiceRequestID =
                    invoice.MunicipalServiceRequestID;

                model.InvoiceNumber =
                    invoice.InvoiceNumber;

                model.RequestReference =
                    request.ReferenceNumber;

                model.ServiceName =
                    invoice.FeeSchedule != null &&
                    invoice.FeeSchedule.ServiceType != null
                        ? invoice.FeeSchedule.ServiceType.ServiceName
                        : "Municipal Service";

                model.Amount =
                    invoice.Amount;

                model.AmountPaid =
                    invoice.AmountPaid;

                model.Balance =
                    invoice.Balance;

                model.DueDate =
                    invoice.DueDate;

                return View(model);
            }

            // ---------------------------------------------------------
            // Prepare upload directory
            // ---------------------------------------------------------

            var uploadDirectory =
                Server.MapPath("~/Uploads/PaymentProof");

            if (string.IsNullOrWhiteSpace(uploadDirectory))
            {
                TempData["ErrorMessage"] =
                    "The payment proof upload location could not be accessed.";

                return RedirectToAction(
                    "Details",
                    new
                    {
                        id = invoice.MunicipalServiceRequestID
                    }
                );
            }

            if (!System.IO.Directory.Exists(uploadDirectory))
            {
                System.IO.Directory.CreateDirectory(uploadDirectory);
            }

            // ---------------------------------------------------------
            // Generate a safe server-side filename
            // ---------------------------------------------------------

            var fileExtension = System.IO.Path
                .GetExtension(model.ProofOfPayment.FileName)
                .ToLowerInvariant();

            var generatedFileName =
                "PAYMENTPROOF_" +
                Guid.NewGuid().ToString("N") +
                fileExtension;

            var physicalFilePath =
                System.IO.Path.Combine(
                    uploadDirectory,
                    generatedFileName);

            var proofOfPaymentPath =
                "~/Uploads/PaymentProof/" +
                generatedFileName;

            try
            {
                // -----------------------------------------------------
                // Save proof of payment
                // -----------------------------------------------------

                model.ProofOfPayment.SaveAs(
                    physicalFilePath);

                // -----------------------------------------------------
                // Create payment
                // -----------------------------------------------------

                var payment = new Payment
                {
                    InvoiceID =
                        invoice.InvoiceID,

                    CitizenID =
                        citizenId,

                    // Always use the database balance.
                    Amount =
                        invoice.Balance,

                    Status =
                        Models.PaymentStatus.Processing,

                    PaymentDate =
                        DateTime.Now,

                    ProcessedDate =
                        null,

                    PaymentMethod =
                        model.PaymentMethod.Trim(),

                    FailureReason =
                        null,

                    ProofOfPaymentPath =
                        proofOfPaymentPath,

                    // Temporary value.
                    // Final reference is generated after PaymentID exists.
                    TransactionReference =
                        "TEMP-" +
                        Guid.NewGuid()
                            .ToString("N")
                            .Substring(0, 20)
                };

                db.Payments.Add(payment);

                // -----------------------------------------------------
                // First save generates PaymentID
                // -----------------------------------------------------

                db.SaveChanges();

                // -----------------------------------------------------
                // Generate final transaction reference
                // -----------------------------------------------------

                payment.TransactionReference =
                    "PAY-" +
                    DateTime.Now.Year +
                    "-" +
                    payment.PaymentID.ToString("D6");

                // -----------------------------------------------------
                // Move municipal request into payment processing
                // -----------------------------------------------------

                request.Status =
                    MunicipalServiceRequestStatus.PaymentProcessing;

                db.SaveChanges();

                // -----------------------------------------------------
                // Payment successfully recorded
                // -----------------------------------------------------

                TempData["SuccessMessage"] =
                    "Your payment and proof of payment have been submitted and are now awaiting Finance Officer verification.";

                return RedirectToAction(
                    "PaymentConfirmation",
                    new
                    {
                        id = payment.PaymentID
                    }
                );
            }
            catch
            {
                // -----------------------------------------------------
                // If database processing fails after the file was saved,
                // remove the uploaded file so we do not leave an orphaned
                // document on the server.
                // -----------------------------------------------------

                if (System.IO.File.Exists(physicalFilePath))
                {
                    try
                    {
                        System.IO.File.Delete(physicalFilePath);
                    }
                    catch
                    {
                        // Do not hide the original failure.
                    }
                }

                TempData["ErrorMessage"] =
                    "The payment could not be submitted. Please try again.";

                return RedirectToAction(
                    "Details",
                    new
                    {
                        id = invoice.MunicipalServiceRequestID
                    }
                );
            }
        }

        // =========================================================
        // US151 - PAYMENT CONFIRMATION
        // =========================================================

        [HttpGet]
        public ActionResult PaymentConfirmation(int id)
        {
            if (Session["CitizenID"] == null)
                return RedirectToAction("Login", "Login");

            var citizenId = (int)Session["CitizenID"];

            var payment = db.Payments
                .Include("Invoice")
                .Include("Invoice.MunicipalServiceRequest")
                .Include("Invoice.FeeSchedule")
                .Include("Invoice.FeeSchedule.ServiceType")
                .FirstOrDefault(p =>
                    p.PaymentID == id &&
                    p.CitizenID == citizenId);

            if (payment == null)
            {
                TempData["ErrorMessage"] =
                    "The payment could not be found.";

                return RedirectToAction(
                    "Index",
                    "MunicipalServices"
                );
            }

            var invoice = payment.Invoice;

            if (invoice == null)
            {
                TempData["ErrorMessage"] =
                    "The invoice associated with this payment could not be found.";

                return RedirectToAction(
                    "Index",
                    "MunicipalServices"
                );
            }

            var request = invoice.MunicipalServiceRequest;

            if (request == null ||
                request.CitizenID != citizenId)
            {
                TempData["ErrorMessage"] =
                    "You are not authorised to view this payment.";

                return RedirectToAction(
                    "Index",
                    "MunicipalServices"
                );
            }

            var model = new PaymentConfirmationViewModel
            {
                PaymentID = payment.PaymentID,

                TransactionReference =
                    payment.TransactionReference,

                InvoiceNumber =
                    invoice.InvoiceNumber,

                MunicipalServiceRequestID =
                    request.MunicipalServiceRequestID,

                RequestReference =
                    request.ReferenceNumber,

                ServiceName =
                    invoice.FeeSchedule != null &&
                    invoice.FeeSchedule.ServiceType != null
                        ? invoice.FeeSchedule.ServiceType.ServiceName
                        : request.ServiceType != null
                            ? request.ServiceType.ServiceName
                            : "Municipal Service",

                Amount =
                    payment.Amount,

                PaymentMethod =
                    payment.PaymentMethod,

                PaymentStatus =
                    payment.Status.ToString(),

                PaymentDate =
                    payment.PaymentDate,

                DueDate =
                    invoice.DueDate,

                HasProofOfPayment =
                    !string.IsNullOrWhiteSpace(
                        payment.ProofOfPaymentPath)
            };

            return View(model);
        }

        [HttpGet]
        public ActionResult PaymentStatus(int id)
        {
            // ============================================================
            // 1. AUTHENTICATION
            // ============================================================
            if (Session["CitizenID"] == null)
                return RedirectToAction("Login", "Login");

            var citizenId = (int)Session["CitizenID"];

            // ============================================================
            // 2. FIND THE CITIZEN'S MUNICIPAL SERVICE REQUEST
            // ============================================================
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

            // ============================================================
            // 3. FIND THE INVOICE
            // ============================================================
            var invoice = db.Invoices
                .Include("MunicipalServiceRequest")
                .Include("FeeSchedule")
                .Include("FeeSchedule.ServiceType")
                .FirstOrDefault(i =>
                    i.MunicipalServiceRequestID ==
                        request.MunicipalServiceRequestID &&
                    i.CitizenID == citizenId &&
                    i.MunicipalServiceRequest != null &&
                    i.MunicipalServiceRequest.CitizenID == citizenId);

            if (invoice == null)
            {
                TempData["ErrorMessage"] =
                    "No invoice has been generated for this municipal service request.";

                return RedirectToAction(
                    "Details",
                    new
                    {
                        id = request.MunicipalServiceRequestID
                    }
                );
            }

            // ============================================================
            // 4. FIND THE MOST RECENT PAYMENT
            // ============================================================
            var payment = db.Payments
                .Where(p =>
                    p.InvoiceID == invoice.InvoiceID &&
                    p.CitizenID == citizenId)
                .OrderByDescending(p => p.PaymentDate)
                .ThenByDescending(p => p.PaymentID)
                .FirstOrDefault();

            if (payment == null)
            {
                TempData["ErrorMessage"] =
                    "No payment has been recorded for this municipal service request.";

                return RedirectToAction(
                    "Details",
                    new
                    {
                        id = request.MunicipalServiceRequestID
                    }
                );
            }

            // ============================================================
            // 5. BUILD PAYMENT STATUS MODEL
            // ============================================================
            var model = new PaymentStatusViewModel
            {
                PaymentID = payment.PaymentID,

                TransactionReference =
                    payment.TransactionReference,

                InvoiceNumber =
                    invoice.InvoiceNumber,

                MunicipalServiceRequestID =
                    request.MunicipalServiceRequestID,

                RequestReference =
                    request.ReferenceNumber,

                ServiceName =
                    invoice.FeeSchedule != null &&
                    invoice.FeeSchedule.ServiceType != null
                        ? invoice.FeeSchedule.ServiceType.ServiceName
                        : request.ServiceType != null
                            ? request.ServiceType.ServiceName
                            : "Municipal Service",

                Amount =
                    payment.Amount,

                PaymentMethod =
                    payment.PaymentMethod,

                PaymentStatus =
                    payment.Status.ToString(),

                PaymentDate =
                    payment.PaymentDate,

                ProcessedDate =
                    payment.ProcessedDate,

                FailureReason =
                    payment.FailureReason,

                HasProofOfPayment =
                    !string.IsNullOrWhiteSpace(
                        payment.ProofOfPaymentPath)
            };

            return View(model);
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

