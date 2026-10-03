using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using CommunityServiceProject.Models;
using CommunityServiceProject.ViewModels;

namespace CommunityServiceProject.Controllers
{
    public class CitizenRefundController : Controller
    {
        private readonly Community db = new Community();

        // ============================================================
        // REFUND REQUESTS
        // ============================================================

        [HttpGet]
        public ActionResult Index()
        {
            if (Session["CitizenID"] == null)
                return RedirectToAction("Login", "Login");

            int citizenID =
                Convert.ToInt32(Session["CitizenID"]);

            var payments = db.Payments
                .Include(p => p.Invoice)
                .Include(p => p.Invoice.MunicipalServiceRequest)
                .Include(p => p.Invoice.FeeSchedule)
                .Include(p => p.Invoice.FeeSchedule.ServiceType)
                .Where(p =>
                    p.CitizenID == citizenID &&
                    p.Status == PaymentStatus.Successful)
                .OrderByDescending(p => p.PaymentDate)
                .ToList();

            return View(payments);
        }

        // ============================================================
        // REQUEST REFUND - GET
        // ============================================================

        [HttpGet]
        public ActionResult RequestRefund(int id)
        {
            if (Session["CitizenID"] == null)
                return RedirectToAction("Login", "Login");

            int citizenID =
                Convert.ToInt32(Session["CitizenID"]);

            var payment = db.Payments
                .Include(p => p.Invoice)
                .Include(p => p.Invoice.MunicipalServiceRequest)
                .Include(p => p.Invoice.FeeSchedule)
                .Include(p => p.Invoice.FeeSchedule.ServiceType)
                .FirstOrDefault(p =>
                    p.PaymentID == id &&
                    p.CitizenID == citizenID);

            if (payment == null)
                return HttpNotFound();

            if (payment.Status != PaymentStatus.Successful)
            {
                TempData["ErrorMessage"] =
                    "Only successful payments can be submitted for a refund.";

                return RedirectToAction("Index");
            }

            // An active or completed refund already exists.
            bool existingRefund =
                db.Refunds.Any(r =>
                    r.PaymentID == payment.PaymentID &&
                    r.Status != RefundStatus.Rejected);

            if (existingRefund)
            {
                TempData["ErrorMessage"] =
                    "A refund request already exists for this payment.";

                return RedirectToAction("Index");
            }

            var request =
                payment.Invoice != null
                    ? payment.Invoice.MunicipalServiceRequest
                    : null;

            var model = new RefundRequestViewModel
            {
                PaymentID =
                    payment.PaymentID,

                InvoiceID =
                    payment.InvoiceID,

                TransactionReference =
                    payment.TransactionReference,

                InvoiceNumber =
                    payment.Invoice != null
                        ? payment.Invoice.InvoiceNumber
                        : "N/A",

                RequestReference =
                    request != null
                        ? request.ReferenceNumber
                        : "N/A",

                ServiceName =
                    payment.Invoice != null &&
                    payment.Invoice.FeeSchedule != null &&
                    payment.Invoice.FeeSchedule.ServiceType != null
                        ? payment.Invoice.FeeSchedule.ServiceType.ServiceName
                        : "Municipal Service",

                PaymentDate =
                    payment.PaymentDate,

                PaymentAmount =
                    payment.Amount,

                RefundAmount =
                    payment.Amount
            };

            return View(model);
        }

        // ============================================================
        // REQUEST REFUND - POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RequestRefund(
            RefundRequestViewModel model)
        {
            if (Session["CitizenID"] == null)
                return RedirectToAction("Login", "Login");

            int citizenID =
                Convert.ToInt32(Session["CitizenID"]);

            var payment = db.Payments
                .Include(p => p.Invoice)
                .Include(p => p.Invoice.MunicipalServiceRequest)
                .FirstOrDefault(p =>
                    p.PaymentID == model.PaymentID &&
                    p.CitizenID == citizenID);

            if (payment == null)
                return HttpNotFound();

            if (payment.Status != PaymentStatus.Successful)
            {
                TempData["ErrorMessage"] =
                    "Only successful payments can be refunded.";

                return RedirectToAction("Index");
            }

            if (model.RefundAmount > payment.Amount)
            {
                ModelState.AddModelError(
                    "RefundAmount",
                    "The refund amount cannot exceed the original payment amount.");
            }

            if (model.RefundAmount <= 0)
            {
                ModelState.AddModelError(
                    "RefundAmount",
                    "The refund amount must be greater than zero.");
            }

            if (model.SupportingDocument == null ||
                model.SupportingDocument.ContentLength == 0)
            {
                ModelState.AddModelError(
                    "SupportingDocument",
                    "Please attach supporting documentation.");
            }

            if (model.SupportingDocument != null &&
                model.SupportingDocument.ContentLength >
                5 * 1024 * 1024)
            {
                ModelState.AddModelError(
                    "SupportingDocument",
                    "The supporting document cannot exceed 5 MB.");
            }

            if (model.SupportingDocument != null)
            {
                string extension =
                    Path.GetExtension(
                        model.SupportingDocument.FileName)
                        .ToLowerInvariant();

                string[] allowedExtensions =
                {
                    ".pdf",
                    ".jpg",
                    ".jpeg",
                    ".png"
                };

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        "SupportingDocument",
                        "Only PDF, JPG, JPEG and PNG files are allowed.");
                }
            }

            bool existingRefund =
                db.Refunds.Any(r =>
                    r.PaymentID == payment.PaymentID &&
                    r.Status != RefundStatus.Rejected);

            if (existingRefund)
            {
                TempData["ErrorMessage"] =
                    "A refund request already exists for this payment.";

                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                model.PaymentAmount =
                    payment.Amount;

                return View(model);
            }

            string uploadFolder =
                Server.MapPath("~/Uploads/RefundDocuments");

            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            string extensionName =
                Path.GetExtension(
                    model.SupportingDocument.FileName)
                    .ToLowerInvariant();

            string uniqueFileName =
                Guid.NewGuid()
                    .ToString("N") +
                extensionName;

            string physicalPath =
                Path.Combine(
                    uploadFolder,
                    uniqueFileName);

            model.SupportingDocument.SaveAs(
                physicalPath);

            var refund = new Refund
            {
                RefundReference =
                    "TEMP-" +
                    Guid.NewGuid()
                        .ToString("N")
                        .Substring(0, 12)
                        .ToUpperInvariant(),

                PaymentID =
                    payment.PaymentID,

                InvoiceID =
                    payment.InvoiceID,

                Amount =
                    model.RefundAmount,

                Status =
                    RefundStatus.RefundRequested,

                RequestDate =
                    DateTime.Now,

                Reason =
                    model.Reason.Trim(),

                SupportingDocumentPath =
                    "~/Uploads/RefundDocuments/" +
                    uniqueFileName,

                SupportingDocumentName =
                    Path.GetFileName(
                        model.SupportingDocument.FileName)
            };

            db.Refunds.Add(refund);

            db.SaveChanges();

            refund.RefundReference =
                "REF-" +
                refund.RequestDate.Year +
                "-" +
                refund.RefundID.ToString("D6");

            db.SaveChanges();

            TempData["SuccessMessage"] =
                "Your refund request " +
                refund.RefundReference +
                " has been submitted successfully.";

            return RedirectToAction("Index");
        }

        // ============================================================
        // REQUEST REFUND FROM MUNICIPAL SERVICE REQUEST
        // ============================================================

        [HttpGet]
        public ActionResult RequestRefundFromRequest(int id)
        {
            if (Session["CitizenID"] == null)
                return RedirectToAction("Login", "Login");

            int citizenID =
                Convert.ToInt32(Session["CitizenID"]);

            // --------------------------------------------------------
            // FIND A SUCCESSFUL PAYMENT FOR THIS REQUEST
            // --------------------------------------------------------

            var payment = db.Payments
                .Include(p => p.Invoice)
                .Include(p => p.Invoice.MunicipalServiceRequest)
                .Include(p => p.Invoice.FeeSchedule)
                .Include(p => p.Invoice.FeeSchedule.ServiceType)
                .FirstOrDefault(p =>
                    p.CitizenID == citizenID &&
                    p.Status == PaymentStatus.Successful &&
                    p.Invoice != null &&
                    p.Invoice.MunicipalServiceRequest != null &&
                    p.Invoice.MunicipalServiceRequest.MunicipalServiceRequestID == id);

            if (payment == null)
            {
                TempData["ErrorMessage"] =
                    "A successful payment could not be found for this service request.";

                return RedirectToAction(
                    "Details",
                    "MunicipalServiceRequest",
                    new { id = id });
            }

            // --------------------------------------------------------
            // PREVENT DUPLICATE ACTIVE REFUND REQUESTS
            // --------------------------------------------------------

            bool existingRefund =
                db.Refunds.Any(r =>
                    r.PaymentID == payment.PaymentID &&
                    r.Status != RefundStatus.Rejected);

            if (existingRefund)
            {
                TempData["ErrorMessage"] =
                    "A refund request already exists for this payment.";

                return RedirectToAction(
                    "Details",
                    "MunicipalServiceRequest",
                    new { id = id });
            }

            var request =
                payment.Invoice.MunicipalServiceRequest;

            var model = new RefundRequestViewModel
            {
                PaymentID =
                    payment.PaymentID,

                InvoiceID =
                    payment.InvoiceID,

                TransactionReference =
                    payment.TransactionReference,

                InvoiceNumber =
                    payment.Invoice.InvoiceNumber,

                RequestReference =
                    request.ReferenceNumber,

                ServiceName =
                    payment.Invoice.FeeSchedule != null &&
                    payment.Invoice.FeeSchedule.ServiceType != null
                        ? payment.Invoice.FeeSchedule.ServiceType.ServiceName
                        : "Municipal Service",

                PaymentDate =
                    payment.PaymentDate,

                PaymentAmount =
                    payment.Amount,

                RefundAmount =
                    payment.Amount
            };

            return View(
                "RequestRefund",
                model);
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