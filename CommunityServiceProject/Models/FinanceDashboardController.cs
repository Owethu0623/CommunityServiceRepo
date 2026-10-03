using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using CommunityServiceProject.Models;
using CommunityServiceProject.ViewModels;

namespace CommunityServiceProject.Controllers
{
    [CommunityServiceProject.Filters.RoleAuthorize("FinanceOfficer")]
    public class FinanceDashboardController : Controller
    {
        private readonly Community db = new Community();

        public ActionResult Index()
        {
            if (Session["FinanceOfficerID"] == null)
            {
                return RedirectToAction("Login", "Login");
            }

            int financeOfficerId = (int)Session["FinanceOfficerID"];

            var financeOfficer = db.FinanceOfficers
                .FirstOrDefault(f => f.FinanceOfficerID == financeOfficerId);

            if (financeOfficer == null)
            {
                Session.Clear();
                return RedirectToAction("Login", "Login");
            }

            var outstandingInvoiceStatuses = new[]
            {
                InvoiceStatus.Issued,
                InvoiceStatus.PartiallyPaid,
                InvoiceStatus.Overdue
            };

            var model = new FinanceDashboardViewModel
            {
                FinanceOfficerName = financeOfficer.FirstName + " " + financeOfficer.LastName,
                FinanceOfficerEmail = financeOfficer.EmailAddress,

                TotalServiceRequests = db.MunicipalServiceRequests.Count(),

                ServiceRequestsAwaitingReview = db.MunicipalServiceRequests
                    .Count(r => r.Status == MunicipalServiceRequestStatus.Submitted ||
                                r.Status == MunicipalServiceRequestStatus.UnderReview),

                ApprovedServiceRequests = db.MunicipalServiceRequests
                    .Count(r => r.Status == MunicipalServiceRequestStatus.Approved),

                TotalInvoices = db.Invoices.Count(),

                OutstandingInvoices = db.Invoices
                    .Count(i => outstandingInvoiceStatuses.Contains(i.Status) &&
                                i.Balance > 0),

                OutstandingInvoiceBalance = db.Invoices
                    .Where(i => outstandingInvoiceStatuses.Contains(i.Status) &&
                                i.Balance > 0)
                    .Select(i => (decimal?)i.Balance)
                    .Sum() ?? 0m,

                SuccessfulPayments = db.Payments
                    .Count(p => p.Status == PaymentStatus.Successful),

                PendingPayments = db.Payments
                    .Count(p => p.Status == PaymentStatus.Pending ||
                                p.Status == PaymentStatus.Processing),

                SuccessfulPaymentAmount = db.Payments
                    .Where(p => p.Status == PaymentStatus.Successful)
                    .Select(p => (decimal?)p.Amount)
                    .Sum() ?? 0m,

                PendingRefunds = db.Refunds
                    .Count(r => r.Status == RefundStatus.RefundRequested ||
                                r.Status == RefundStatus.UnderReview ||
                                r.Status == RefundStatus.Approved)
            };

            model.RecentServiceRequests = db.MunicipalServiceRequests
                .Include(r => r.Citizen)
                .Include(r => r.ServiceType)
                .OrderByDescending(r => r.DateSubmitted)
                .Take(6)
                .Select(r => new FinanceServiceRequestSummaryViewModel
                {
                    MunicipalServiceRequestID = r.MunicipalServiceRequestID,
                    ReferenceNumber = r.ReferenceNumber,
                    ServiceName = r.ServiceType.ServiceName,
                    CitizenName = r.Citizen.FirstName + " " + r.Citizen.LastName,
                    Status = r.Status,
                    DateSubmitted = r.DateSubmitted
                })
                .ToList();

            model.RecentInvoices = db.Invoices
                .Include(i => i.MunicipalServiceRequest)
                .OrderByDescending(i => i.IssueDate)
                .Take(6)
                .Select(i => new FinanceInvoiceSummaryViewModel
                {
                    InvoiceID = i.InvoiceID,
                    InvoiceNumber = i.InvoiceNumber,
                    ServiceRequestReference = i.MunicipalServiceRequest.ReferenceNumber,
                    Amount = i.Amount,
                    Balance = i.Balance,
                    Status = i.Status,
                    IssueDate = i.IssueDate,
                    DueDate = i.DueDate
                })
                .ToList();

            model.RecentPayments = db.Payments
                .Include(p => p.Invoice)
                .OrderByDescending(p => p.PaymentDate)
                .Take(6)
                .Select(p => new FinancePaymentSummaryViewModel
                {
                    PaymentID = p.PaymentID,
                    TransactionReference = p.TransactionReference,
                    InvoiceNumber = p.Invoice.InvoiceNumber,
                    Amount = p.Amount,
                    Status = p.Status,
                    PaymentDate = p.PaymentDate,
                    PaymentMethod = p.PaymentMethod
                })
                .ToList();

            return View(model);
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
