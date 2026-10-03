using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using System.Collections.Generic;
using CommunityServiceProject.Models;
using CommunityServiceProject.ViewModels;

namespace CommunityServiceProject.Controllers
{
    [CommunityServiceProject.Filters.RoleAuthorize("FinanceOfficer")]
    public class FinanceReportsController : Controller
    {
        private readonly Community db = new Community();


        // ============================================================
        // US - FINANCIAL REPORTS
        // ============================================================

        [HttpGet]
        public ActionResult FinancialReports(
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
                new FinancialReportsViewModel
                {
                    SearchTerm = searchTerm,
                    TransactionType = transactionType,
                    StatusFilter = statusFilter,
                    FromDate = fromDate,
                    ToDate = toDate,
                    ReportGeneratedAt = DateTime.Now
                };


            var transactions =
                new List<FinancialReportTransactionViewModel>();


            // ========================================================
            // PAYMENTS
            // ========================================================

            if (string.IsNullOrWhiteSpace(transactionType) ||
                transactionType == "Payment")
            {
                var payments =
                    db.Payments
                        .Include(p => p.Citizen)
                        .Include(p => p.Invoice)
                        .AsNoTracking()
                        .ToList();

                foreach (var payment in payments)
                {
                    var citizenName =
                        payment.Citizen != null
                            ? payment.Citizen.FirstName +
                              " " +
                              payment.Citizen.LastName
                            : "Unknown Citizen";

                    var invoiceNumber =
                        payment.Invoice != null
                            ? payment.Invoice.InvoiceNumber
                            : "N/A";


                    if (!string.IsNullOrWhiteSpace(searchTerm))
                    {
                        var matches =
                            (payment.TransactionReference ?? "")
                                .IndexOf(
                                    searchTerm,
                                    StringComparison.OrdinalIgnoreCase) >= 0

                            ||

                            (invoiceNumber ?? "")
                                .IndexOf(
                                    searchTerm,
                                    StringComparison.OrdinalIgnoreCase) >= 0

                            ||

                            (citizenName ?? "")
                                .IndexOf(
                                    searchTerm,
                                    StringComparison.OrdinalIgnoreCase) >= 0;

                        if (!matches)
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


                    transactions.Add(
                        new FinancialReportTransactionViewModel
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

            if (string.IsNullOrWhiteSpace(transactionType) ||
                transactionType == "Refund")
            {
                var refunds =
                    db.Refunds
                        .Include(r => r.Payment.Citizen)
                        .Include(r => r.Invoice)
                        .AsNoTracking()
                        .ToList();

                foreach (var refund in refunds)
                {
                    var citizenName =
                        refund.Payment != null &&
                        refund.Payment.Citizen != null
                            ? refund.Payment.Citizen.FirstName +
                              " " +
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
                        var matches =
                            (refund.RefundReference ?? "")
                                .IndexOf(
                                    searchTerm,
                                    StringComparison.OrdinalIgnoreCase) >= 0

                            ||

                            (invoiceNumber ?? "")
                                .IndexOf(
                                    searchTerm,
                                    StringComparison.OrdinalIgnoreCase) >= 0

                            ||

                            (citizenName ?? "")
                                .IndexOf(
                                    searchTerm,
                                    StringComparison.OrdinalIgnoreCase) >= 0;

                        if (!matches)
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


                    transactions.Add(
                        new FinancialReportTransactionViewModel
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

            transactions =
                transactions
                    .OrderByDescending(t => t.TransactionDate)
                    .ToList();

            model.Transactions = transactions;

            model.TotalRecords =
                transactions.Count;


            // ========================================================
            // CORE FINANCIAL CALCULATIONS
            // ========================================================

            model.SuccessfulPaymentCount =
                transactions.Count(t =>
                    t.Type == "Payment" &&
                    t.Status == "Successful");

            model.FailedPaymentCount =
                transactions.Count(t =>
                    t.Type == "Payment" &&
                    t.Status == "Failed");

            model.PendingPaymentCount =
                transactions.Count(t =>
                    t.Type == "Payment" &&
                    (t.Status == "Pending" ||
                     t.Status == "Processing"));

            model.RefundedCount =
                transactions.Count(t =>
                    t.Type == "Refund" &&
                    t.Status == "Refunded");

            model.PendingRefundCount =
                transactions.Count(t =>
                    t.Type == "Refund" &&
                    (t.Status == "RefundRequested" ||
                     t.Status == "UnderReview" ||
                     t.Status == "Approved" ||
                     t.Status == "RefundProcessing"));


            model.GrossSuccessfulPayments =
                transactions
                    .Where(t =>
                        t.Type == "Payment" &&
                        t.Status == "Successful")
                    .Sum(t => t.Amount);


            model.TotalRefunded =
                transactions
                    .Where(t =>
                        t.Type == "Refund" &&
                        t.Status == "Refunded")
                    .Sum(t => t.Amount);


            model.NetFinancialMovement =
                model.GrossSuccessfulPayments -
                model.TotalRefunded;


            model.PendingPaymentAmount =
                transactions
                    .Where(t =>
                        t.Type == "Payment" &&
                        (t.Status == "Pending" ||
                         t.Status == "Processing"))
                    .Sum(t => t.Amount);


            model.ApprovedRefundExposure =
                transactions
                    .Where(t =>
                        t.Type == "Refund" &&
                        (t.Status == "Approved" ||
                         t.Status == "RefundProcessing"))
                    .Sum(t => t.Amount);


            model.FailedPaymentAmount =
                transactions
                    .Where(t =>
                        t.Type == "Payment" &&
                        t.Status == "Failed")
                    .Sum(t => t.Amount);


            model.RefundRatePercentage =
                model.GrossSuccessfulPayments > 0
                    ? Math.Round(
                        (model.TotalRefunded /
                         model.GrossSuccessfulPayments) * 100m,
                        2)
                    : 0m;


            // ========================================================
            // STATUS SUMMARY
            // ========================================================

            model.StatusSummary =
                transactions
                    .GroupBy(t => new
                    {
                        t.Type,
                        t.Status
                    })
                    .Select(g =>
                        new FinancialReportStatusSummaryViewModel
                        {
                            Type =
                                g.Key.Type,

                            Status =
                                g.Key.Status,

                            Count =
                                g.Count(),

                            Amount =
                                g.Sum(x => x.Amount)
                        })
                    .OrderBy(x => x.Type)
                    .ThenBy(x => x.Status)
                    .ToList();


            // ========================================================
            // DAILY SUMMARY
            // ========================================================

            model.DailySummary =
                transactions
                    .GroupBy(t => t.TransactionDate.Date)
                    .OrderByDescending(g => g.Key)
                    .Select(g =>
                        new FinancialReportDailySummaryViewModel
                        {
                            Date =
                                g.Key,

                            PaymentRecords =
                                g.Count(t =>
                                    t.Type == "Payment"),

                            SuccessfulPayments =
                                g.Count(t =>
                                    t.Type == "Payment" &&
                                    t.Status == "Successful"),

                            SuccessfulPaymentAmount =
                                g.Where(t =>
                                        t.Type == "Payment" &&
                                        t.Status == "Successful")
                                  .Sum(t => t.Amount),

                            RefundRecords =
                                g.Count(t =>
                                    t.Type == "Refund"),

                            CompletedRefunds =
                                g.Count(t =>
                                    t.Type == "Refund" &&
                                    t.Status == "Refunded"),

                            RefundedAmount =
                                g.Where(t =>
                                        t.Type == "Refund" &&
                                        t.Status == "Refunded")
                                  .Sum(t => t.Amount),

                            NetMovement =
                                g.Where(t =>
                                    t.Type == "Payment" &&
                                    t.Status == "Successful")
                                 .Sum(t => t.Amount)

                                -

                                g.Where(t =>
                                    t.Type == "Refund" &&
                                    t.Status == "Refunded")
                                 .Sum(t => t.Amount)
                        })
                    .ToList();


            return View(model);
        }


        // ============================================================
        // US - FINANCIAL AUDIT HISTORY
        // ============================================================

        [HttpGet]
        public ActionResult AuditHistory(
            string searchTerm,
            string actionFilter,
            string entityFilter,
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


            var query =
                db.FinancialAuditRecords
                    .AsNoTracking()
                    .AsQueryable();


            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query =
                    query.Where(a =>
                        a.Reference.Contains(searchTerm) ||
                        a.Action.Contains(searchTerm) ||
                        a.PerformedBy.Contains(searchTerm) ||
                        a.Details.Contains(searchTerm));
            }


            if (!string.IsNullOrWhiteSpace(actionFilter))
            {
                query =
                    query.Where(a =>
                        a.Action == actionFilter);
            }


            if (!string.IsNullOrWhiteSpace(entityFilter))
            {
                query =
                    query.Where(a =>
                        a.EntityType == entityFilter);
            }


            if (fromDate.HasValue)
            {
                query =
                    query.Where(a =>
                        a.AuditDate >=
                        fromDate.Value.Date);
            }


            if (toDate.HasValue)
            {
                var endDate =
                    toDate.Value.Date.AddDays(1);

                query =
                    query.Where(a =>
                        a.AuditDate < endDate);
            }


            var records =
                query
                    .OrderByDescending(a => a.AuditDate)
                    .ToList();


            var model =
                new FinancialAuditHistoryViewModel
                {
                    SearchTerm = searchTerm,
                    ActionFilter = actionFilter,
                    EntityFilter = entityFilter,
                    FromDate = fromDate,
                    ToDate = toDate,

                    TotalEvents =
                        records.Count,

                    PaymentEvents =
                        records.Count(a =>
                            a.EntityType == "Payment"),

                    RefundEvents =
                        records.Count(a =>
                            a.EntityType == "Refund"),

                    ReceiptEvents =
                        records.Count(a =>
                            a.EntityType == "Receipt")
                };


            foreach (var record in records)
            {
                model.Records.Add(
                    new FinancialAuditRowViewModel
                    {
                        FinancialAuditRecordID =
                            record.FinancialAuditRecordID,

                        AuditDate =
                            record.AuditDate,

                        PerformedBy =
                            record.PerformedBy,

                        Action =
                            record.Action,

                        EntityType =
                            record.EntityType,

                        EntityID =
                            record.EntityID,

                        Reference =
                            record.Reference,

                        PreviousStatus =
                            record.PreviousStatus,

                        NewStatus =
                            record.NewStatus,

                        Amount =
                            record.Amount,

                        Details =
                            record.Details,

                        IPAddress =
                            record.IPAddress
                    });
            }


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