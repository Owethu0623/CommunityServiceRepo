using System;

namespace CommunityServiceProject.ViewModels
{
    public class PaymentStatusViewModel
    {
        public int PaymentID { get; set; }

        public string TransactionReference { get; set; }

        public string InvoiceNumber { get; set; }

        public int MunicipalServiceRequestID { get; set; }

        public string RequestReference { get; set; }

        public string ServiceName { get; set; }

        public decimal Amount { get; set; }

        public string PaymentMethod { get; set; }

        public string PaymentStatus { get; set; }

        public DateTime PaymentDate { get; set; }

        public DateTime? ProcessedDate { get; set; }

        public string FailureReason { get; set; }

        public bool HasProofOfPayment { get; set; }
    }
}