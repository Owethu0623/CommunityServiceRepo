using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using CommunityServiceProject.Models;
using CommunityServiceProject.ViewModels;

using CommunityServiceProject.Filters;

namespace CommunityServiceProject.Controllers
{
    [RoleAuthorize("FinanceOfficer")]
    public class FinancePaymentController : Controller
    {
        private readonly Community db = new Community();

        // ============================================================
        // US141 - VIEW PAYMENTS AWAITING VERIFICATION
        // ============================================================

        public ActionResult Index()
        {
            if (Session["FinanceOfficerID"] == null)
                return new HttpUnauthorizedResult();

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
                return new HttpUnauthorizedResult();

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
                return new HttpUnauthorizedResult();

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

            // Capture previous status for audit before updating.
            string previousStatus =
                payment.Status.ToString();

            payment.Status =
               Models.PaymentStatus.Successful;

            payment.ProcessedDate =
                DateTime.Now;

            payment.FailureReason = null;

            RecordFinanceAudit(
    "Payment Verified",
    "Payment",
    payment.PaymentID,
    payment.TransactionReference,
    previousStatus,
    payment.Status.ToString(),
    payment.Amount,
    "Finance Officer verified the submitted payment and recorded it as successful.");

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
                return new HttpUnauthorizedResult();

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
            string previousStatus =
    payment.Status.ToString();

            payment.Status =
                Models.PaymentStatus.Failed;
            
            RecordFinanceAudit(
            "Payment Rejected",
            "Payment",
            payment.PaymentID,
            payment.TransactionReference,
            previousStatus,
            payment.Status.ToString(),
            payment.Amount,
            "Finance Officer rejected the submitted payment. Reason: " +
            failureReason);

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
                return new HttpUnauthorizedResult();

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
            public ActionResult PaymentDetails(int? id)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(400);
                }

                // Ensure only finance officers can view payment details.
                if (Session["FinanceOfficerID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }

                var payment = db.Payments
                    .Include(p => p.Invoice)
                    .Include(p => p.Citizen)
                    .FirstOrDefault(p => p.PaymentID == id.Value);

                if (payment == null)
                {
                    TempData["ErrorMessage"] = "The payment could not be found.";
                    return RedirectToAction("Index");
                }

                return View(payment);
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
                return new HttpUnauthorizedResult();

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
                return new HttpUnauthorizedResult();

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
                    PaymentID =
                        payment.PaymentID,

                    IssueDate =
                        DateTime.Now,

                    Amount =
                        payment.Amount,

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
                // GENERATE FORMAL RECEIPT NUMBER
                // --------------------------------------------------------

                receipt.ReceiptNumber =
                    $"REC-{receipt.IssueDate.Year}-{receipt.ReceiptID:D6}";

                // --------------------------------------------------------
                // FINANCE NOTIFICATION - RECEIPT GENERATED
                // --------------------------------------------------------

                var receiptNotification =
                    new FinanceNotification
                    {
                        CitizenID =
                            payment.CitizenID,

                        InvoiceID =
                            payment.InvoiceID,

                        PaymentID =
                            payment.PaymentID,

                        RefundID =
                            null,

                        ReceiptID =
                            receipt.ReceiptID,

                        NotificationType =
                            FinanceNotificationType.ReceiptGenerated,

                        Title =
                            "Payment Receipt Generated",

                        Message =
                            "Your payment receipt " +
                            receipt.ReceiptNumber +
                            " has been generated for your successful payment of R" +
                            receipt.Amount.ToString("N2") +
                            ".",

                        DateCreated =
                            DateTime.Now,

                        IsRead =
                            false,

                        ReadDate =
                            null
                    };

                db.FinanceNotifications.Add(
                    receiptNotification);

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

        // ============================================================
        // US144 - DECIDE REFUND REQUEST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DecideRefund(
            int id,
            RefundStatus decision,
            string reviewComments)
        {
            if (Session["FinanceOfficerID"] == null)
                return RedirectToAction(
                    "Login",
                    "FinanceOfficer");

            var refund = db.Refunds
                .Include(r => r.Payment)
                .Include(r => r.Invoice)
                .FirstOrDefault(r =>
                    r.RefundID == id);

            if (refund == null)
                return HttpNotFound();

            string previousStatus =
                refund.Status.ToString();

            // --------------------------------------------------------
            // REFUND MUST BE UNDER REVIEW
            // --------------------------------------------------------

            if (refund.Status !=
                RefundStatus.UnderReview)
            {
                TempData["Error"] =
                    "This refund is no longer available for review.";

                return RedirectToAction(
                    "RefundRequests");
            }

            // --------------------------------------------------------
            // ONLY APPROVED OR REJECTED ARE VALID
            // --------------------------------------------------------

            if (decision != RefundStatus.Approved &&
                decision != RefundStatus.Rejected)
            {
                TempData["Error"] =
                    "Invalid refund decision.";

                return RedirectToAction(
                    "ReviewRefund",
                    new
                    {
                        id =
                            refund.RefundID
                    });
            }

            // --------------------------------------------------------
            // REQUIRE COMMENTS WHEN REJECTING
            // --------------------------------------------------------

            if (decision == RefundStatus.Rejected &&
                string.IsNullOrWhiteSpace(reviewComments))
            {
                TempData["Error"] =
                    "Please provide a reason for rejecting the refund.";

                return RedirectToAction(
                    "ReviewRefund",
                    new
                    {
                        id =
                            refund.RefundID
                    });
            }

            refund.Status =
                decision;

            refund.ReviewDate =
                DateTime.Now;

            refund.ReviewedByFinanceOfficerID =
                (int)Session["FinanceOfficerID"];

            refund.ReviewComments =
                string.IsNullOrWhiteSpace(reviewComments)
                    ? null
                    : reviewComments.Trim();

            string action =
                decision == RefundStatus.Approved
                    ? "Refund Approved"
                    : "Refund Rejected";

            RecordFinanceAudit(
                action,
                "Refund",
                refund.RefundID,
                refund.RefundReference,
                previousStatus,
                refund.Status.ToString(),
                refund.Amount,
                decision == RefundStatus.Approved
                    ? "Finance Officer approved the refund request."
                    : "Finance Officer rejected the refund request. Reason: " +
                      refund.ReviewComments);

            // --------------------------------------------------------
            // CREATE CORRESPONDING CITIZEN NOTIFICATION
            // --------------------------------------------------------

            var refundNotification =
                new FinanceNotification
                {
                    CitizenID =
                        refund.Payment.CitizenID,

                    InvoiceID =
                        refund.InvoiceID,

                    PaymentID =
                        refund.PaymentID,

                    RefundID =
                        refund.RefundID,

                    ReceiptID =
                        null,

                    NotificationType =
                        decision == RefundStatus.Approved
                            ? FinanceNotificationType.RefundApproved
                            : FinanceNotificationType.RefundRejected,

                    Title =
                        decision == RefundStatus.Approved
                            ? "Refund Approved"
                            : "Refund Rejected",

                    Message =
                        decision == RefundStatus.Approved
                            ? "Your refund request " +
                              refund.RefundReference +
                              " for R" +
                              refund.Amount.ToString("N2") +
                              " has been approved and can now be processed."
                            : "Your refund request " +
                              refund.RefundReference +
                              " for R" +
                              refund.Amount.ToString("N2") +
                              " has been rejected. Reason: " +
                              refund.ReviewComments,

                    DateCreated =
                        DateTime.Now,

                    IsRead =
                        false,

                    ReadDate =
                        null
                };

            db.FinanceNotifications.Add(
                refundNotification);

            db.SaveChanges();

            return RedirectToAction(
                "RefundDecisionResult",
                new
                {
                    id =
                        refund.RefundID
                });
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
                return RedirectToAction(
                    "Login",
                    "Login");

            int financeOfficerID =
                Convert.ToInt32(
                    Session["FinanceOfficerID"]);

            var refund =
                db.Refunds
                    .Include(r => r.Payment)
                    .Include(r => r.Invoice)
                    .FirstOrDefault(r =>
                        r.RefundID == id);

            // --------------------------------------------------------
            // CHECK REFUND EXISTS BEFORE USING IT
            // --------------------------------------------------------

            if (refund == null)
                return HttpNotFound();

            // --------------------------------------------------------
            // VALIDATE REFUND STATUS
            // --------------------------------------------------------

            if (refund.Status !=
                RefundStatus.Approved)
            {
                TempData["ErrorMessage"] =
                    "Only approved refunds can be processed.";

                return RedirectToAction(
                    "ProcessRefunds");
            }

            // --------------------------------------------------------
            // VALIDATE PAYMENT
            // --------------------------------------------------------

            if (refund.Payment == null ||
                refund.Payment.Status !=
                PaymentStatus.Successful)
            {
                TempData["ErrorMessage"] =
                    "The refund is linked to an invalid payment.";

                return RedirectToAction(
                    "ProcessRefunds");
            }

            // --------------------------------------------------------
            // VALIDATE INVOICE
            // --------------------------------------------------------

            if (refund.Invoice == null)
            {
                TempData["ErrorMessage"] =
                    "The invoice associated with this refund could not be found.";

                return RedirectToAction(
                    "ProcessRefunds");
            }

            // --------------------------------------------------------
            // VALIDATE REFUND AMOUNT
            // --------------------------------------------------------

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

                    RecordFinanceAudit(
                        "Refund Processing Started",
                        "Refund",
                        refund.RefundID,
                        refund.RefundReference,
                        "Approved",
                        "RefundProcessing",
                        refund.Amount,
                        "Finance Officer started processing the approved refund.");

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
                        invoice.AmountPaid =
                            0m;

                        invoice.Balance =
                            invoice.Amount;

                        invoice.PaidDate =
                            null;

                        invoice.Status =
                            InvoiceStatus.Refunded;
                    }
                    else
                    {
                        invoice.Status =
                            InvoiceStatus.PartiallyPaid;

                        invoice.PaidDate =
                            null;
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

                    RecordFinanceAudit(
                        "Refund Processed",
                        "Refund",
                        refund.RefundID,
                        refund.RefundReference,
                        "RefundProcessing",
                        "Refunded",
                        refund.Amount,
                        "Finance Officer completed the refund process and recorded the refund as refunded.");

                    // ----------------------------------------------------
                    // FINANCE NOTIFICATION - REFUND PROCESSED
                    // ----------------------------------------------------

                    var refundNotification =
                        new FinanceNotification
                        {
                            CitizenID =
                                refund.Payment.CitizenID,

                            InvoiceID =
                                refund.InvoiceID,

                            PaymentID =
                                refund.PaymentID,

                            RefundID =
                                refund.RefundID,

                            ReceiptID =
                                null,

                            NotificationType =
                                FinanceNotificationType.RefundProcessed,

                            Title =
                                "Refund Processed",

                            Message =
                                "Your refund " +
                                refund.RefundReference +
                                " for R" +
                                refund.Amount.ToString("N2") +
                                " has been successfully processed.",

                            DateCreated =
                                DateTime.Now,

                            IsRead =
                                false,

                            ReadDate =
                                null
                        };

                    db.FinanceNotifications.Add(
                        refundNotification);

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

        // ============================================================
        // US145 - VIEW FINANCIAL TRANSACTIONS
        // ============================================================

        [HttpGet]
        public ActionResult FinancialTransactions(
            string searchTerm,
            string transactionType,
            string statusFilter,
            DateTime? fromDate,
            DateTime? toDate)
        {
            if (Session["FinanceOfficerID"] == null)
            {
                return RedirectToAction(
                    "Login",
                    "FinanceOfficer");
            }

            searchTerm =
                string.IsNullOrWhiteSpace(searchTerm)
                    ? null
                    : searchTerm.Trim();

            var model =
                new FinancialTransactionsViewModel
                {
                    SearchTerm = searchTerm,
                    TransactionType = transactionType,
                    StatusFilter = statusFilter,
                    FromDate = fromDate,
                    ToDate = toDate
                };


            // ========================================================
            // PAYMENTS
            // ========================================================

            if (transactionType == null ||
                transactionType == "Payment")
            {
                var payments =
                    db.Payments
                        .Include(p => p.Invoice)
                        .Include(p => p.Citizen)
                        .AsNoTracking()
                        .ToList();

                foreach (var payment in payments)
                {
                    var citizenName =
                        payment.Citizen != null
                            ? payment.Citizen.FirstName + " " +
                              payment.Citizen.LastName
                            : "Unknown Citizen";

                    var invoiceNumber =
                        payment.Invoice != null
                            ? payment.Invoice.InvoiceNumber
                            : "N/A";

                    if (!string.IsNullOrWhiteSpace(searchTerm))
                    {
                        var matchesSearch =
                            payment.TransactionReference
                                .Contains(searchTerm) ||

                            invoiceNumber
                                .Contains(searchTerm) ||

                            citizenName
                                .Contains(searchTerm);

                        if (!matchesSearch)
                        {
                            continue;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(statusFilter) &&
                        payment.Status.ToString() != statusFilter)
                    {
                        continue;
                    }

                    if (fromDate.HasValue &&
                        payment.PaymentDate.Date <
                        fromDate.Value.Date)
                    {
                        continue;
                    }

                    if (toDate.HasValue &&
                        payment.PaymentDate.Date >
                        toDate.Value.Date)
                    {
                        continue;
                    }

                    model.Transactions.Add(
                        new FinancialTransactionRowViewModel
                        {
                            Reference =
                                payment.TransactionReference,

                            Type =
                                "Payment",

                            TransactionDate =
                                payment.PaymentDate,

                            CitizenName =
                                citizenName,

                            InvoiceNumber =
                                invoiceNumber,

                            Status =
                                payment.Status.ToString(),

                            Amount =
                                payment.Amount,

                            PaymentMethod =
                                payment.PaymentMethod,

                            Description =
                                "Municipal payment"
                        });
                }
            }


            // ========================================================
            // REFUNDS
            // ========================================================

            if (transactionType == null ||
                transactionType == "Refund")
            {
                var refunds =
                    db.Refunds
                        .Include(r => r.Payment)
                        .Include(r => r.Invoice)
                        .AsNoTracking()
                        .ToList();

                foreach (var refund in refunds)
                {
                    var citizenName =
                        refund.Payment != null &&
                        refund.Payment.Citizen != null
                            ? refund.Payment.Citizen.FirstName + " " +
                              refund.Payment.Citizen.LastName
                            : "Unknown Citizen";

                    var invoiceNumber =
                        refund.Invoice != null
                            ? refund.Invoice.InvoiceNumber
                            : "N/A";

                    var transactionDate =
                        refund.ProcessedDate ??
                        refund.RequestDate;

                    if (!string.IsNullOrWhiteSpace(searchTerm))
                    {
                        var matchesSearch =
                            refund.RefundReference
                                .Contains(searchTerm) ||

                            invoiceNumber
                                .Contains(searchTerm) ||

                            citizenName
                                .Contains(searchTerm);

                        if (!matchesSearch)
                        {
                            continue;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(statusFilter) &&
                        refund.Status.ToString() != statusFilter)
                    {
                        continue;
                    }

                    if (fromDate.HasValue &&
                        transactionDate.Date <
                        fromDate.Value.Date)
                    {
                        continue;
                    }

                    if (toDate.HasValue &&
                        transactionDate.Date >
                        toDate.Value.Date)
                    {
                        continue;
                    }

                    model.Transactions.Add(
                        new FinancialTransactionRowViewModel
                        {
                            Reference =
                                refund.RefundReference,

                            Type =
                                "Refund",

                            TransactionDate =
                                transactionDate,

                            CitizenName =
                                citizenName,

                            InvoiceNumber =
                                invoiceNumber,

                            Status =
                                refund.Status.ToString(),

                            Amount =
                                refund.Amount,

                            PaymentMethod =
                                null,

                            Description =
                                "Municipal refund"
                        });
                }
            }


            // ========================================================
            // SORT
            // ========================================================

            model.Transactions =
                model.Transactions
                    .OrderByDescending(t => t.TransactionDate)
                    .ToList();


            // ========================================================
            // SUMMARY
            // ========================================================

            model.TotalTransactions =
                model.Transactions.Count;

            model.TotalPayments =
                model.Transactions
                    .Where(t => t.Type == "Payment")
                    .Sum(t => t.Amount);

            model.TotalRefunds =
                model.Transactions
                    .Where(t => t.Type == "Refund")
                    .Sum(t => t.Amount);


            return View(model);
        }

        // ============================================================
        // FINANCIAL AUDIT HELPER
        // ============================================================

        private void RecordFinanceAudit(
            string action,
            string entityType,
            int entityId,
            string reference,
            string previousStatus,
            string newStatus,
            decimal? amount,
            string details)
        {
            if (Session["FinanceOfficerID"] == null)
            {
                return;
            }

            int financeOfficerID =
                Convert.ToInt32(
                    Session["FinanceOfficerID"]);


            var audit =
                new FinancialAuditRecord
                {
                    AuditDate =
                        DateTime.Now,

                    FinanceOfficerID =
                        financeOfficerID,

                    PerformedBy =
                        "Finance Officer #" +
                        financeOfficerID,

                    Action =
                        action,

                    EntityType =
                        entityType,

                    EntityID =
                        entityId,

                    Reference =
                        reference,

                    PreviousStatus =
                        previousStatus,

                    NewStatus =
                        newStatus,

                    Amount =
                        amount,

                    Details =
                        details,

                    IPAddress =
                        Request != null
                            ? Request.UserHostAddress
                            : null
                };


            db.FinancialAuditRecords.Add(audit);
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
    
  