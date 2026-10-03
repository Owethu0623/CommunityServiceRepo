using System;

namespace CommunityServiceProject.ViewModels
{
    public class PaymentReceiptViewModel
    {
        public int ReceiptID { get; set; }

        public string ReceiptNumber { get; set; }

        public DateTime IssueDate { get; set; }

        public string TransactionReference { get; set; }

        public string InvoiceNumber { get; set; }

        public string RequestReference { get; set; }

        public string CitizenName { get; set; }

        public string ServiceName { get; set; }

        public string PaymentMethod { get; set; }

        public DateTime PaymentDate { get; set; }

        public string PaymentStatus { get; set; }

        public decimal AmountPaid { get; set; }

        public decimal InvoiceAmount { get; set; }

        public decimal RemainingBalance { get; set; }

        public string InvoiceStatus { get; set; }
    }
}