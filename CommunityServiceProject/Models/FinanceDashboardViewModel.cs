using CommunityServiceProject.Models;
using System;
using System.Collections.Generic;

namespace CommunityServiceProject.ViewModels
{
    public class FinanceDashboardViewModel
    {
        public string FinanceOfficerName { get; set; }
        public string FinanceOfficerEmail { get; set; }

        public int TotalServiceRequests { get; set; }
        public int ServiceRequestsAwaitingReview { get; set; }
        public int ApprovedServiceRequests { get; set; }

        public int TotalInvoices { get; set; }
        public int OutstandingInvoices { get; set; }
        public decimal OutstandingInvoiceBalance { get; set; }

        public int SuccessfulPayments { get; set; }
        public int PendingPayments { get; set; }
        public decimal SuccessfulPaymentAmount { get; set; }

        public int PendingRefunds { get; set; }

        public List<FinanceServiceRequestSummaryViewModel> RecentServiceRequests { get; set; }
        public List<FinanceInvoiceSummaryViewModel> RecentInvoices { get; set; }
        public List<FinancePaymentSummaryViewModel> RecentPayments { get; set; }

        public FinanceDashboardViewModel()
        {
            RecentServiceRequests = new List<FinanceServiceRequestSummaryViewModel>();
            RecentInvoices = new List<FinanceInvoiceSummaryViewModel>();
            RecentPayments = new List<FinancePaymentSummaryViewModel>();
        }
    }

    public class FinanceServiceRequestSummaryViewModel
    {
        public int MunicipalServiceRequestID { get; set; }
        public string ReferenceNumber { get; set; }
        public string ServiceName { get; set; }
        public string CitizenName { get; set; }
        public MunicipalServiceRequestStatus Status { get; set; }
        public DateTime DateSubmitted { get; set; }
    }

    public class FinanceInvoiceSummaryViewModel
    {
        public int InvoiceID { get; set; }
        public string InvoiceNumber { get; set; }
        public string ServiceRequestReference { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public InvoiceStatus Status { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class FinancePaymentSummaryViewModel
    {
        public int PaymentID { get; set; }
        public string TransactionReference { get; set; }
        public string InvoiceNumber { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; }
    }
}