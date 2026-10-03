using CommunityServiceProject.Models;
using System;

namespace CommunityServiceProject.ViewModels
{
    public class RefundReviewViewModel
    {
        public int RefundID { get; set; }

        public string RefundReference { get; set; }

        public string TransactionReference { get; set; }

        public string InvoiceNumber { get; set; }

        public string RequestReference { get; set; }

        public string CitizenName { get; set; }

        public string ServiceName { get; set; }

        public decimal OriginalPaymentAmount { get; set; }

        public decimal RefundAmount { get; set; }

        public DateTime RequestDate { get; set; }

        public RefundStatus Status { get; set; }

        public string Reason { get; set; }

        public string SupportingDocumentPath { get; set; }

        public string SupportingDocumentName { get; set; }

        public string ReviewComments { get; set; }

        public bool CanDecide { get; set; }
    }
}