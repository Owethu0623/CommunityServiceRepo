using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using CommunityServiceProject.Models;
using CommunityServiceProject.ViewModels;

namespace CommunityServiceProject.Controllers
{
    public class FinancePaymentController : Controller
    {
        private readonly Community db = new Community();

        // ============================================================
        // US141 - VIEW PAYMENTS AWAITING VERIFICATION
        // ============================================================

        public ActionResult Index()
        {
            if (Session["FinanceOfficerID"] == null)
                return RedirectToAction("Login", "Login");

            var payments = db.Payments
                .Include(p => p.Invoice)
                .Include(p => p.Invoice.MunicipalServiceRequest)
                .Include(p => p.Citizen)
                .Where(p =>
                    p.Status ==
                   Models.PaymentStatus.Processing)
                .OrderByDescending(p => p.PaymentDate)
                .ToList();

            return View(payments);
        }


        // ============================================================
        // US141 - REVIEW PAYMENT
        // ============================================================

        [HttpGet]
        public ActionResult Verify(int id)
        {
            if (Session["FinanceOfficerID"] == null)
                return RedirectToAction("Login", "Login");

            var payment = db.Payments
                .Include(p => p.Invoice)
                .Include(p => p.Invoice.MunicipalServiceRequest)
                .Include(p => p.Invoice.FeeSchedule)
                .Include(p => p.Invoice.FeeSchedule.ServiceType)
                .Include(p => p.Citizen)
                .FirstOrDefault(p =>
                    p.PaymentID == id);

            if (payment == null)
            {
                TempData["ErrorMessage"] =
                    "The payment could not be found.";

                return RedirectToAction("Index");
            }

            if (payment.Status !=
                 Models.PaymentStatus.Processing)
            {
                TempData["ErrorMessage"] =
                    "This payment has already been processed.";

                return RedirectToAction("Index");
            }

            var request =
                payment.Invoice != null
                    ? payment.Invoice.MunicipalServiceRequest
                    : null;

            var model = new VerifyPaymentViewModel
            {
                PaymentID = payment.PaymentID,

                TransactionReference =
                    payment.TransactionReference,

                InvoiceNumber =
                    payment.Invoice != null
                        ? payment.Invoice.InvoiceNumber
                        : "N/A",

                MunicipalServiceRequestID =
                    request != null
                        ? request.MunicipalServiceRequestID
                        : 0,

                RequestReference =
                    request != null
                        ? request.ReferenceNumber
                        : "N/A",

                CitizenName =
                    payment.Citizen != null
                        ? payment.Citizen.FirstName +
                          " " +
                          payment.Citizen.LastName
                        : "N/A",

                ServiceName =
                    payment.Invoice != null &&
                    payment.Invoice.FeeSchedule != null &&
                    payment.Invoice.FeeSchedule.ServiceType != null
                        ? payment.Invoice.FeeSchedule.ServiceType.ServiceName
                        : "Municipal Service",

                Amount =
                    payment.Amount,

                PaymentMethod =
                    payment.PaymentMethod,

                PaymentDate =
                    payment.PaymentDate,

                PaymentStatus =
                    payment.Status.ToString(),

                ProofOfPaymentPath =
                    payment.ProofOfPaymentPath,

                FailureReason =
                    payment.FailureReason
            };

            return View(model);
        }


        // ============================================================
        // US141 - VERIFY SUCCESSFUL PAYMENT
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmPayment(int id)
        {
            if (Session["FinanceOfficerID"] == null)
                return RedirectToAction("Login", "Login");

            var payment = db.Payments
                .Include(p => p.Invoice)
                .Include(p => p.Invoice.MunicipalServiceRequest)
                .FirstOrDefault(p =>
                    p.PaymentID == id);

            if (payment == null)
            {
                TempData["ErrorMessage"] =
                    "The payment could not be found.";

                return RedirectToAction("Index");
            }

            if (payment.Status !=
                 Models.PaymentStatus.Processing)
            {
                TempData["ErrorMessage"] =
                    "This payment has already been processed.";

                return RedirectToAction("Index");
            }

            if (payment.Invoice == null)
            {
                TempData["ErrorMessage"] =
                    "The invoice associated with this payment could not be found.";

                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(
                payment.ProofOfPaymentPath))
            {
                TempData["ErrorMessage"] =
                    "This payment cannot be verified because no proof of payment was submitted.";

                return RedirectToAction(
                    "Verify",
                    new { id = payment.PaymentID });
            }

            var invoice = payment.Invoice;

            var request =
                invoice.MunicipalServiceRequest;

            if (request == null)
            {
                TempData["ErrorMessage"] =
                    "The municipal service request associated with this payment could not be found.";

                return RedirectToAction("Index");
            }

            // --------------------------------------------------------
            // MARK PAYMENT AS SUCCESSFUL
            // --------------------------------------------------------

            payment.Status =
               Models.PaymentStatus.Successful;

            payment.ProcessedDate =
                DateTime.Now;

            payment.FailureReason = null;

            // --------------------------------------------------------
            // UPDATE INVOICE
            // --------------------------------------------------------

            invoice.AmountPaid =
                payment.Amount;

            invoice.Balance =
                Math.Max(
                    0m,
                    invoice.Amount - invoice.AmountPaid);

            if (invoice.Balance <= 0)
            {
                invoice.Balance = 0m;

                invoice.Status =
                    InvoiceStatus.Paid;

                invoice.PaidDate =
                    DateTime.Now;
            }
            else
            {
                invoice.Status =
                    InvoiceStatus.PartiallyPaid;
            }

            // --------------------------------------------------------
            // UPDATE MUNICIPAL SERVICE REQUEST
            // --------------------------------------------------------

            if (request.Status ==
                MunicipalServiceRequestStatus.PaymentProcessing)
            {
                request.Status =
                    MunicipalServiceRequestStatus.Paid;
            }

            db.SaveChanges();

            TempData["SuccessMessage"] =
                "The payment has been successfully verified and the invoice has been updated.";

            return RedirectToAction(
                "Verify",
                new { id = payment.PaymentID });
        }


        // ============================================================
        // US141 - REJECT PAYMENT
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RejectPayment(
            int id,
            string failureReason)
        {
            if (Session["FinanceOfficerID"] == null)
                return RedirectToAction("Login", "Login");

            if (string.IsNullOrWhiteSpace(failureReason))
            {
                TempData["ErrorMessage"] =
                    "Please provide a reason for rejecting the payment.";

                return RedirectToAction(
                    "Verify",
                    new { id = id });
            }

            var payment = db.Payments
                .Include(p => p.Invoice)
                .Include(p => p.Invoice.MunicipalServiceRequest)
                .FirstOrDefault(p =>
                    p.PaymentID == id);

            if (payment == null)
            {
                TempData["ErrorMessage"] =
                    "The payment could not be found.";

                return RedirectToAction("Index");
            }

            if (payment.Status !=
                Models.PaymentStatus.Processing)
            {
                TempData["ErrorMessage"] =
                    "This payment has already been processed.";

                return RedirectToAction("Index");
            }

            payment.Status =
                Models.PaymentStatus.Failed;

            payment.ProcessedDate =
                DateTime.Now;

            payment.FailureReason =
                failureReason.Trim();

            if (payment.Invoice != null &&
                payment.Invoice.MunicipalServiceRequest != null)
            {
                var request =
                    payment.Invoice.MunicipalServiceRequest;

                if (request.Status ==
                    MunicipalServiceRequestStatus.PaymentProcessing)
                {
                    request.Status =
                        MunicipalServiceRequestStatus.PaymentFailed;
                }
            }

            db.SaveChanges();

            TempData["SuccessMessage"] =
                "The payment has been rejected. The citizen can submit payment again.";

            return RedirectToAction("Index");
        }


        // ============================================================
        // VIEW PROOF OF PAYMENT
        // ============================================================

        [HttpGet]
        public ActionResult ViewProofOfPayment(int id)
        {
            if (Session["FinanceOfficerID"] == null)
                return RedirectToAction("Login", "Login");

            var payment = db.Payments
                .FirstOrDefault(p =>
                    p.PaymentID == id);

            if (payment == null ||
                string.IsNullOrWhiteSpace(
                    payment.ProofOfPaymentPath))
            {
                TempData["ErrorMessage"] =
                    "Proof of payment could not be found.";

                return RedirectToAction(
                    "Verify",
                    new { id = id });
            }

            var relativePath =
                payment.ProofOfPaymentPath
                    .TrimStart('~', '/')
                    .Replace("/", Path.DirectorySeparatorChar.ToString());

            var physicalPath =
                Server.MapPath("~/" + relativePath);

            if (string.IsNullOrWhiteSpace(physicalPath) ||
                !System.IO.File.Exists(physicalPath))
            {
                TempData["ErrorMessage"] =
                    "The proof of payment file could not be found.";

                return RedirectToAction(
                    "Verify",
                    new { id = id });
            }

            var extension =
                Path.GetExtension(physicalPath)
                    .ToLowerInvariant();

            var contentType =
                "application/octet-stream";

            if (extension == ".pdf")
                contentType = "application/pdf";

            else if (extension == ".jpg" ||
                     extension == ".jpeg")
                contentType = "image/jpeg";

            else if (extension == ".png")
                contentType = "image/png";

            return File(
                physicalPath,
                contentType);
        }
        

      


            [HttpGet]
            public ActionResult Review(int id)
            {
                if (Session["FinanceOfficerID"] == null)
                    return RedirectToAction("Login", "Login");

                var payment = db.Payments
                    .Include(p => p.Invoice)
                    .Include(p => p.Invoice.MunicipalServiceRequest)
                    .Include(p => p.Invoice.FeeSchedule)
                    .Include(p => p.Invoice.FeeSchedule.ServiceType)
                    .Include(p => p.Citizen)
                    .FirstOrDefault(p =>
                        p.PaymentID == id);

                if (payment == null)
                {
                    TempData["ErrorMessage"] =
                        "The payment could not be found.";

                    return RedirectToAction("Index");
                }

                if (payment.Status !=
                    Models.PaymentStatus.Processing)
                {
                    TempData["ErrorMessage"] =
                        "This payment has already been processed.";

                    return RedirectToAction("Index");
                }

                if (payment.Invoice == null)
                {
                    TempData["ErrorMessage"] =
                        "The invoice associated with this payment could not be found.";

                    return RedirectToAction("Index");
                }

                var request =
                    payment.Invoice.MunicipalServiceRequest;

                if (request == null)
                {
                    TempData["ErrorMessage"] =
                        "The municipal service request associated with this payment could not be found.";

                    return RedirectToAction("Index");
                }

                var model = new VerifyPaymentViewModel
                {
                    PaymentID =
                        payment.PaymentID,

                    TransactionReference =
                        payment.TransactionReference,

                    InvoiceNumber =
                        payment.Invoice.InvoiceNumber,

                    MunicipalServiceRequestID =
                        request.MunicipalServiceRequestID,

                    RequestReference =
                        request.ReferenceNumber,

                    CitizenName =
                        payment.Citizen != null
                            ? payment.Citizen.FirstName +
                              " " +
                              payment.Citizen.LastName
                            : "N/A",

                    ServiceName =
                        payment.Invoice.FeeSchedule != null &&
                        payment.Invoice.FeeSchedule.ServiceType != null
                            ? payment.Invoice.FeeSchedule.ServiceType.ServiceName
                            : "Municipal Service",

                    Amount =
                        payment.Amount,

                    PaymentMethod =
                        payment.PaymentMethod,

                    PaymentDate =
                        payment.PaymentDate,

                    PaymentStatus =
                        payment.Status.ToString(),

                    ProofOfPaymentPath =
                        payment.ProofOfPaymentPath,

                    FailureReason =
                        payment.FailureReason
                };

                return View(model);
            }


        // ============================================================
        // US142 - VIEW SUCCESSFUL PAYMENTS
        // ============================================================

        [HttpGet]
        public ActionResult SuccessfulPayments()
        {
            if (Session["FinanceOfficerID"] == null)
                return RedirectToAction("Login", "Login");

            var payments = db.Payments
                .Include(p => p.Invoice)
                .Include(p => p.Invoice.MunicipalServiceRequest)
                .Include(p => p.Citizen)
                .Include(p => p.Receipts)
                .Where(p =>
                    p.Status ==
                    Models.PaymentStatus.Successful)
                .OrderByDescending(p =>
                    p.ProcessedDate ?? p.PaymentDate)
                .ToList();

            return View(payments);
        }

        // ============================================================
        // US142 - GENERATE / VIEW PAYMENT RECEIPT
        // ============================================================

        [HttpGet]
        public ActionResult GenerateReceipt(int id)
        {
            if (Session["FinanceOfficerID"] == null)
                return RedirectToAction("Login", "Login");

            var payment = db.Payments
                .Include(p => p.Invoice)
                .Include(p => p.Invoice.MunicipalServiceRequest)
                .Include(p => p.Invoice.FeeSchedule)
                .Include(p => p.Invoice.FeeSchedule.ServiceType)
                .Include(p => p.Citizen)
                .Include(p => p.Receipts)
                .FirstOrDefault(p =>
                    p.PaymentID == id);

            if (payment == null)
            {
                TempData["ErrorMessage"] =
                    "The payment could not be found.";

                return RedirectToAction("SuccessfulPayments");
            }

            if (payment.Status !=
                Models.PaymentStatus.Successful)
            {
                TempData["ErrorMessage"] =
                    "A payment receipt can only be generated for a successful payment.";

                return RedirectToAction("SuccessfulPayments");
            }

            if (payment.Invoice == null)
            {
                TempData["ErrorMessage"] =
                    "The invoice associated with this payment could not be found.";

                return RedirectToAction("SuccessfulPayments");
            }

            // ------------------------------------------------------------
            // CHECK WHETHER A RECEIPT ALREADY EXISTS
            // ------------------------------------------------------------

            var receipt = payment.Receipts
                .FirstOrDefault();

            // ------------------------------------------------------------
            // CREATE RECEIPT ONLY ONCE
            // ------------------------------------------------------------

            if (receipt == null)
            {
                receipt = new Receipt
                {
                    PaymentID = payment.PaymentID,
                    IssueDate = DateTime.Now,
                    Amount = payment.Amount,

                    // Temporary value so the required field can be saved.
                    ReceiptNumber =
                        "TEMP-" +
                        Guid.NewGuid()
                            .ToString("N")
                            .Substring(0, 12)
                            .ToUpperInvariant()
                };

                db.Receipts.Add(receipt);

                db.SaveChanges();

                // --------------------------------------------------------
                // GENERATE FORMAL RECEIPT NUMBER USING RECEIPT ID
                // --------------------------------------------------------

                receipt.ReceiptNumber =
                    $"REC-{receipt.IssueDate.Year}-{receipt.ReceiptID:D6}";

                db.SaveChanges();

                TempData["SuccessMessage"] =
                    "The payment receipt has been generated successfully.";
            }

            var request =
                payment.Invoice.MunicipalServiceRequest;

            var model = new PaymentReceiptViewModel
            {
                ReceiptID =
                    receipt.ReceiptID,

                ReceiptNumber =
                    receipt.ReceiptNumber,

                IssueDate =
                    receipt.IssueDate,

                TransactionReference =
                    payment.TransactionReference,

                InvoiceNumber =
                    payment.Invoice.InvoiceNumber,

                RequestReference =
                    request != null
                        ? request.ReferenceNumber
                        : "N/A",

                CitizenName =
                    payment.Citizen != null
                        ? payment.Citizen.FirstName +
                          " " +
                          payment.Citizen.LastName
                        : "N/A",

                ServiceName =
                    payment.Invoice.FeeSchedule != null &&
                    payment.Invoice.FeeSchedule.ServiceType != null
                        ? payment.Invoice.FeeSchedule.ServiceType.ServiceName
                        : "Municipal Service",

                PaymentMethod =
                    string.IsNullOrWhiteSpace(payment.PaymentMethod)
                        ? "N/A"
                        : payment.PaymentMethod,

                PaymentDate =
                    payment.PaymentDate,

                PaymentStatus =
                    payment.Status.ToString(),

                AmountPaid =
                    receipt.Amount,

                InvoiceAmount =
                    payment.Invoice.Amount,

                RemainingBalance =
                    Math.Max(
                        0m,
                        payment.Invoice.Balance),

                InvoiceStatus =
                    payment.Invoice.Status.ToString()
            };

            return View("Receipt", model);
        }

        // ============================================================
        // US143 - VIEW ALL PAYMENTS
        // ============================================================

        [HttpGet]
        public ActionResult Payments()
        {
            if (Session["FinanceOfficerID"] == null)
                return RedirectToAction("Login", "Login");

            var payments = db.Payments
                .Include(p => p.Invoice)
                .Include(p => p.Invoice.MunicipalServiceRequest)
                .Include(p => p.Invoice.FeeSchedule)
                .Include(p => p.Invoice.FeeSchedule.ServiceType)
                .Include(p => p.Citizen)
                .Include(p => p.Receipts)
                .OrderByDescending(p => p.PaymentDate)
                .ToList();

            return View(payments);
        }

        // ============================================================
        // US143A - VIEW REFUND REQUESTS
        // ============================================================

        [HttpGet]
        public ActionResult RefundRequests()
        {
            if (Session["FinanceOfficerID"] == null)
                return RedirectToAction("Login", "Login");

            var refunds = db.Refunds
                .Include(r => r.Payment)
                .Include(r => r.Payment.Citizen)
                .Include(r => r.Invoice)
                .Include(r => r.Invoice.MunicipalServiceRequest)
                .OrderByDescending(r => r.RequestDate)
                .ToList();

            return View(refunds);
        }


        // ============================================================
        // US143A - REVIEW REFUND
        // ============================================================

        [HttpGet]
        public ActionResult ReviewRefund(int id)
        {
            if (Session["FinanceOfficerID"] == null)
                return RedirectToAction("Login", "Login");

            var refund = db.Refunds
                .Include(r => r.Payment)
                .Include(r => r.Payment.Citizen)
                .Include(r => r.Invoice)
                .Include(r => r.Invoice.MunicipalServiceRequest)
                .Include(r => r.Invoice.FeeSchedule)
                .Include(r => r.Invoice.FeeSchedule.ServiceType)
                .FirstOrDefault(r =>
                    r.RefundID == id);

            if (refund == null)
                return HttpNotFound();

            if (refund.Status ==
                RefundStatus.RefundRequested)
            {
                refund.Status =
                    RefundStatus.UnderReview;

                refund.ReviewDate =
                    DateTime.Now;

                refund.ReviewedByFinanceOfficerID =
                    Convert.ToInt32(
                        Session["FinanceOfficerID"]);

                db.SaveChanges();
            }

            var request =
                refund.Invoice != null
                    ? refund.Invoice.MunicipalServiceRequest
                    : null;

            var model = new RefundReviewViewModel
            {
                RefundID =
                    refund.RefundID,

                RefundReference =
                    refund.RefundReference,

                TransactionReference =
                    refund.Payment != null
                        ? refund.Payment.TransactionReference
                        : "N/A",

                InvoiceNumber =
                    refund.Invoice != null
                        ? refund.Invoice.InvoiceNumber
                        : "N/A",

                RequestReference =
                    request != null
                        ? request.ReferenceNumber
                        : "N/A",

                CitizenName =
                    refund.Payment != null &&
                    refund.Payment.Citizen != null
                        ? refund.Payment.Citizen.FirstName +
                          " " +
                          refund.Payment.Citizen.LastName
                        : "N/A",

                ServiceName =
                    refund.Invoice != null &&
                    refund.Invoice.FeeSchedule != null &&
                    refund.Invoice.FeeSchedule.ServiceType != null
                        ? refund.Invoice.FeeSchedule.ServiceType.ServiceName
                        : "Municipal Service",

                OriginalPaymentAmount =
                    refund.Payment != null
                        ? refund.Payment.Amount
                        : 0m,

                RefundAmount =
                    refund.Amount,

                RequestDate =
                    refund.RequestDate,

                Status =
                    refund.Status,

                Reason =
                    refund.Reason,

                SupportingDocumentPath =
                    refund.SupportingDocumentPath,

                SupportingDocumentName =
                    refund.SupportingDocumentName,

                ReviewComments =
                    refund.ReviewComments,

                CanDecide =
                    refund.Status ==
                        RefundStatus.UnderReview
            };

            return View(model);
        }

        // US144 - Decide Refund Request
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DecideRefund(
            int id,
            RefundStatus decision,
            string reviewComments)
        {
            if (Session["FinanceOfficerID"] == null)
                return RedirectToAction("Login", "FinanceOfficer");

            var refund = db.Refunds
                .Include(r => r.Payment)
                .Include(r => r.Invoice)
                .FirstOrDefault(r => r.RefundID == id);

            if (refund == null)
                return HttpNotFound();

            // Refund must be under review before a decision can be made.
            if (refund.Status != RefundStatus.UnderReview)
            {
                TempData["Error"] = "This refund is no longer available for review.";
                return RedirectToAction("RefundRequests");
            }

            // Only Approved or Rejected are valid decisions.
            if (decision != RefundStatus.Approved &&
                decision != RefundStatus.Rejected)
            {
                TempData["Error"] = "Invalid refund decision.";
                return RedirectToAction("ReviewRefund", new { id = refund.RefundID });
            }

            // Require comments when rejecting.
            if (decision == RefundStatus.Rejected &&
                string.IsNullOrWhiteSpace(reviewComments))
            {
                TempData["Error"] = "Please provide a reason for rejecting the refund.";
                return RedirectToAction("ReviewRefund", new { id = refund.RefundID });
            }

            refund.Status = decision;
            refund.ReviewDate = DateTime.Now;
            refund.ReviewedByFinanceOfficerID =
                (int)Session["FinanceOfficerID"];

            refund.ReviewComments = string.IsNullOrWhiteSpace(reviewComments)
                ? null
                : reviewComments.Trim();

            db.SaveChanges();

            // APPROVED
            if (decision == RefundStatus.Approved)
            {
                return RedirectToAction(
                    "RefundDecisionResult",
                    new { id = refund.RefundID });
            }

            // REJECTED
            return RedirectToAction(
                "RefundDecisionResult",
                new { id = refund.RefundID });
        }


        // US144 - Refund Decision Result
        [HttpGet]
        public ActionResult RefundDecisionResult(int id)
        {
            if (Session["FinanceOfficerID"] == null)
                return RedirectToAction("Login", "FinanceOfficer");

            var refund = db.Refunds
                .Include(r => r.Payment)
                .Include(r => r.Invoice)
                .FirstOrDefault(r => r.RefundID == id);

            if (refund == null)
                return HttpNotFound();

            var model = new RefundReviewViewModel
            {
                RefundID = refund.RefundID,
                RefundReference = refund.RefundReference,

                TransactionReference =
                    refund.Payment != null
                        ? refund.Payment.TransactionReference
                        : null,

                InvoiceNumber =
                    refund.Invoice != null
                        ? refund.Invoice.InvoiceNumber
                        : null,

                RefundAmount = refund.Amount,
                RequestDate = refund.RequestDate,
                Status = refund.Status,
                Reason = refund.Reason,
                SupportingDocumentPath = refund.SupportingDocumentPath,
                SupportingDocumentName = refund.SupportingDocumentName,
                ReviewComments = refund.ReviewComments
            };

            return View(model);
        }

        // ============================================================
        // US144 - PROCESS APPROVED REFUNDS
        // ============================================================

        [HttpGet]
        public ActionResult ProcessRefunds()
        {
            if (Session["FinanceOfficerID"] == null)
                return RedirectToAction("Login", "Login");

            var refunds = db.Refunds
                .Include(r => r.Payment)
                .Include(r => r.Payment.Citizen)
                .Include(r => r.Invoice)
                .Where(r =>
                    r.Status == RefundStatus.Approved ||
                    r.Status == RefundStatus.RefundProcessing ||
                    r.Status == RefundStatus.Refunded)
                .OrderByDescending(r => r.RequestDate)
                .ToList();

            return View(refunds);
        }


        // ============================================================
        // US144 - PROCESS REFUND
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProcessRefund(int id)
        {
            if (Session["FinanceOfficerID"] == null)
                return RedirectToAction("Login", "Login");

            int financeOfficerID =
                Convert.ToInt32(
                    Session["FinanceOfficerID"]);

            var refund =
                db.Refunds
                    .Include(r => r.Payment)
                    .Include(r => r.Invoice)
                    .FirstOrDefault(r =>
                        r.RefundID == id);

            if (refund == null)
                return HttpNotFound();

            if (refund.Status !=
                RefundStatus.Approved)
            {
                TempData["ErrorMessage"] =
                    "Only approved refunds can be processed.";

                return RedirectToAction(
                    "ProcessRefunds");
            }

            if (refund.Payment == null ||
                refund.Payment.Status !=
                PaymentStatus.Successful)
            {
                TempData["ErrorMessage"] =
                    "The refund is linked to an invalid payment.";

                return RedirectToAction(
                    "ProcessRefunds");
            }

            if (refund.Invoice == null)
            {
                TempData["ErrorMessage"] =
                    "The invoice associated with this refund could not be found.";

                return RedirectToAction(
                    "ProcessRefunds");
            }

            if (refund.Amount <= 0)
            {
                TempData["ErrorMessage"] =
                    "The refund amount must be greater than zero.";

                return RedirectToAction(
                    "ProcessRefunds");
            }

            if (refund.Amount >
                refund.Invoice.AmountPaid)
            {
                TempData["ErrorMessage"] =
                    "The refund amount cannot exceed the amount currently recorded as paid on the invoice.";

                return RedirectToAction(
                    "ProcessRefunds");
            }

            using (var transaction =
                db.Database.BeginTransaction())
            {
                try
                {
                    // ----------------------------------------------------
                    // BEGIN REFUND PROCESSING
                    // ----------------------------------------------------

                    refund.Status =
                        RefundStatus.RefundProcessing;

                    db.SaveChanges();

                    // ----------------------------------------------------
                    // REVERSE THE REFUNDED AMOUNT
                    // ----------------------------------------------------

                    var invoice =
                        refund.Invoice;

                    invoice.AmountPaid =
                        Math.Max(
                            0m,
                            invoice.AmountPaid -
                            refund.Amount);

                    invoice.Balance =
                        Math.Max(
                            0m,
                            invoice.Amount -
                            invoice.AmountPaid);

                    // ----------------------------------------------------
                    // UPDATE INVOICE STATUS
                    // ----------------------------------------------------

                    if (invoice.AmountPaid <= 0m)
                    {
                        invoice.AmountPaid = 0m;

                        invoice.Balance =
                            invoice.Amount;

                        invoice.PaidDate = null;

                        invoice.Status =
                            InvoiceStatus.Refunded;
                    }
                    else
                    {
                        invoice.Status =
                            InvoiceStatus.PartiallyPaid;

                        invoice.PaidDate = null;
                    }

                    // ----------------------------------------------------
                    // COMPLETE REFUND
                    // ----------------------------------------------------

                    refund.Status =
                        RefundStatus.Refunded;

                    refund.ProcessedDate =
                        DateTime.Now;

                    refund.ProcessedByFinanceOfficerID =
                        financeOfficerID;

                    db.SaveChanges();

                    transaction.Commit();

                    TempData["SuccessMessage"] =
                        "Refund " +
                        refund.RefundReference +
                        " has been processed successfully.";

                    return RedirectToAction(
                        "ProcessRefunds");
                }
                catch
                {
                    transaction.Rollback();

                    TempData["ErrorMessage"] =
                        "The refund could not be processed. No financial changes were saved.";

                    return RedirectToAction(
                        "ProcessRefunds");
                }
            }
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
    
  