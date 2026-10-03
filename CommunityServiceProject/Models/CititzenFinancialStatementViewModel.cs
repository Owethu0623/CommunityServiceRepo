using System;
using System.Collections.Generic;

namespace CommunityServiceProject.ViewModels
{
    public class CitizenFinancialStatementRowViewModel
    {
        public DateTime Date { get; set; }

        public string Type { get; set; }

        public string Reference { get; set; }

        public string Description { get; set; }

        public string Status { get; set; }

        public decimal Debit { get; set; }

        public decimal Credit { get; set; }

        public decimal RunningBalance { get; set; }

        public int? PaymentID { get; set; }

        public bool HasReceipt { get; set; }
    }


    public class CitizenFinancialStatementViewModel
    {
        public DateTime StatementGeneratedDate { get; set; }

        public string CitizenName { get; set; }

        public int InvoiceCount { get; set; }

        public int PaymentCount { get; set; }

        public int RefundCount { get; set; }

        public decimal TotalInvoiced { get; set; }

        public decimal TotalSuccessfulPayments { get; set; }

        public decimal TotalRefunded { get; set; }

        public decimal CurrentBalance { get; set; }

        public List<CitizenFinancialStatementRowViewModel> Transactions { get; set; }

        public CitizenFinancialStatementViewModel()
        {
            Transactions =
                new List<CitizenFinancialStatementRowViewModel>();
        }
    }
}