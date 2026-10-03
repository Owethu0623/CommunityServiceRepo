using System;
using System.Collections.Generic;

namespace CommunityServiceProject.ViewModels
{
    public class FinancialReportTransactionViewModel
    {
        public string Reference { get; set; }

        public string Type { get; set; }

        public DateTime TransactionDate { get; set; }

        public string CitizenName { get; set; }

        public string InvoiceNumber { get; set; }

        public string Status { get; set; }

        public decimal Amount { get; set; }

        public string PaymentMethod { get; set; }

        public string Description { get; set; }
    }


    public class FinancialReportStatusSummaryViewModel
    {
        public string Type { get; set; }

        public string Status { get; set; }

        public int Count { get; set; }

        public decimal Amount { get; set; }
    }


    public class FinancialReportDailySummaryViewModel
    {
        public DateTime Date { get; set; }

        public int PaymentRecords { get; set; }

        public int SuccessfulPayments { get; set; }

        public decimal SuccessfulPaymentAmount { get; set; }

        public int RefundRecords { get; set; }

        public int CompletedRefunds { get; set; }

        public decimal RefundedAmount { get; set; }

        public decimal NetMovement { get; set; }
    }


    public class FinancialReportsViewModel
    {
        public string SearchTerm { get; set; }

        public string TransactionType { get; set; }

        public string StatusFilter { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public DateTime ReportGeneratedAt { get; set; }

        public int TotalRecords { get; set; }

        public int SuccessfulPaymentCount { get; set; }

        public int FailedPaymentCount { get; set; }

        public int RefundedCount { get; set; }

        public int PendingPaymentCount { get; set; }

        public int PendingRefundCount { get; set; }

        public decimal GrossSuccessfulPayments { get; set; }

        public decimal TotalRefunded { get; set; }

        public decimal NetFinancialMovement { get; set; }

        public decimal PendingPaymentAmount { get; set; }

        public decimal ApprovedRefundExposure { get; set; }

        public decimal FailedPaymentAmount { get; set; }

        public decimal RefundRatePercentage { get; set; }

        public List<FinancialReportTransactionViewModel> Transactions { get; set; }

        public List<FinancialReportStatusSummaryViewModel> StatusSummary { get; set; }

        public List<FinancialReportDailySummaryViewModel> DailySummary { get; set; }

        public FinancialReportsViewModel()
        {
            Transactions =
                new List<FinancialReportTransactionViewModel>();

            StatusSummary =
                new List<FinancialReportStatusSummaryViewModel>();

            DailySummary =
                new List<FinancialReportDailySummaryViewModel>();
        }
    }


    public class FinancialAuditHistoryViewModel
    {
        public string SearchTerm { get; set; }

        public string ActionFilter { get; set; }

        public string EntityFilter { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public int TotalEvents { get; set; }

        public int PaymentEvents { get; set; }

        public int RefundEvents { get; set; }

        public int ReceiptEvents { get; set; }

        public List<FinancialAuditRowViewModel> Records { get; set; }

        public FinancialAuditHistoryViewModel()
        {
            Records =
                new List<FinancialAuditRowViewModel>();
        }
    }


    public class FinancialAuditRowViewModel
    {
        public int FinancialAuditRecordID { get; set; }

        public DateTime AuditDate { get; set; }

        public string PerformedBy { get; set; }

        public string Action { get; set; }

        public string EntityType { get; set; }

        public int EntityID { get; set; }

        public string Reference { get; set; }

        public string PreviousStatus { get; set; }

        public string NewStatus { get; set; }

        public decimal? Amount { get; set; }

        public string Details { get; set; }

        public string IPAddress { get; set; }
    }
}